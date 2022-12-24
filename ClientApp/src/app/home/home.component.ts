import { Component } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { forEachChild } from 'typescript';
import { PDFEditAPI } from '../../api/PDFEditAPI';
import { MessageBoxComponent } from '../message-box/message-box.component';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {

  documents: Doc[] = [];

  // Options
  size = 'medium';
  pageHeight = 400;
  pageWidth = 282;

  sort = 'Name';
  sortDirection = 'Asc';

  constructor(private api: PDFEditAPI.DocumentClient, private modalService: NgbModal) {

    // Load the document list
    api.getDocuments().subscribe(result => {

      this.loadDocuments(result!);

      this.sortDocuments();

    }, error => console.error(error));
  }

  setActive(doc: Doc, page: Page, $event: MouseEvent) {

    let activatePage = true;

    // If the page is already active, deactivate it
    if (page.active) {
      activatePage = false;
    }

    // Deactivate all pages in the doc (unless the ctrl key is pressed - multiselect)
    if (!$event.ctrlKey) {
      doc.pages.forEach(p => {
        p.active = false;
      });
    }

    // Set the state of the selected page
    page.active = activatePage;

    // Check if any pages are active and update the doc flag
    doc.hasSelectedPages = false;

    doc.pages.forEach(p => {
      if (p.active) {
        doc.hasSelectedPages = true;
      }
    });
  }

  // Sizing
  setSize($event: Event) {

    let size = ($event.target as HTMLInputElement).value;

    switch (size) {

      default:
      case 'large':
        this.pageHeight = 800;
        this.pageWidth = 565;
        break;

      case 'medium':
        this.pageHeight = 400;
        this.pageWidth = 282;
        break;

      case 'small':
        this.pageHeight = 200;
        this.pageWidth = 141;
        break;
    }

    this.reloadDocumentsPages(this.documents);
  }

  // Sorting
  setSort(sort: string) {
    this.sort = sort;
    this.sortDocuments();
  }

  setSortDirection(direction: string) {
    this.sortDirection = direction;
    this.sortDocuments();
  }

  sortDocuments() {

    switch (this.sort) {
      case 'Name':

        if (this.sortDirection === 'Desc') {
          this.documents = this.documents.sort((objA, objB) => objB.name.toLowerCase() > objA.name.toLowerCase() ? 1 : -1);
        } else {
          this.documents = this.documents.sort((objA, objB) => objA.name.toLowerCase() > objB.name.toLowerCase() ? 1 : -1);
        }

        break;

      case 'Created':

        if (this.sortDirection === 'Desc') {
          this.documents = this.documents.sort((objA, objB) => objB.created.getTime() - objA.created.getTime());
        } else {
          this.documents = this.documents.sort((objA, objB) => objA.created.getTime() - objB.created.getTime());
        }

        break;

      case 'Last Modified':

        if (this.sortDirection === 'Desc') {
          this.documents = this.documents.sort((objA, objB) => objB.lastModified.getTime() - objA.lastModified.getTime());
        } else {
          this.documents = this.documents.sort((objA, objB) => objA.lastModified.getTime() - objB.lastModified.getTime());
        }

        break;
    }
  }

  selectAll(doc: Doc) {
    doc.pages.forEach(page => {
      page.active = true;
      doc.hasSelectedPages = true;
    });
  }

  unselect(doc: Doc) {
    doc.pages.forEach(page => {
      page.active = false;
    });

    doc.hasSelectedPages = false;
  }

  // Manipulation
  rotate(doc: Doc) {

    let pagesToRotate = this.getSelectedPageNumbers(doc);

    if (pagesToRotate.length > 0) {

      this.setPagesUnloaded(doc, pagesToRotate);

      this.api.rotatePages(doc.name, pagesToRotate).subscribe(() => {
        this.loadDocumentPages(doc, pagesToRotate);
      }, error => this.showMessageBox(error));

    } else {
      this.showMessageBox('No pages selected. Please select a page to rotate.');
    }
  }

  remove(doc: Doc) {

    let pagesToRemove = this.getSelectedPageNumbers(doc);

    if (pagesToRemove.length > 0) {

      this.setPagesUnloaded(doc, pagesToRemove);

      this.api.deletePages(doc.name, pagesToRemove).subscribe(() => {
        this.loadDocumentPages(doc);
      }, error => this.showMessageBox(error));

    } else {
      this.showMessageBox('No pages selected. Please select a page to remove.');
    }
  }

  reorder(doc: Doc, $event: Page[]) {

    let newPageOrder: number[] = [];

    $event.forEach(page => {
      newPageOrder.push(page.number);
    });

    this.setPagesUnloaded(doc, newPageOrder);

    this.api.reorderPages(doc.name, newPageOrder).subscribe(() => {
      this.loadDocumentPages(doc);
    }, error => this.showMessageBox(error));
  }

  revert(doc: Doc) {
    this.api.revertChanges(doc.name).subscribe(() => {
      this.loadDocumentPages(doc);
    }, error => this.showMessageBox(error));
  }

  delete(doc: Doc) {
    this.api.delete(doc.name).subscribe(() => {
      this.documents = this.documents.filter(item => item.name !== doc.name);
    }, error => this.showMessageBox(error));
  }

  save(doc: Doc) {
    this.api.save(doc.name).subscribe(() => {
      this.documents = this.documents.filter(item => item.name !== doc.name);
    }, error => this.showMessageBox(error));
  }

  // Helpers

  loadDocuments(files: PDFEditAPI.Document[]) {

    this.documents = [];

    files.forEach((file) => {

      let doc = this.loadDocument(file);

      this.documents.push(doc);
    });
  }

  loadDocument(file: PDFEditAPI.Document): Doc {

    let doc: Doc = {
      name: file.name,
      created: file.created,
      lastModified: file.lastModified,
      pages: [],
      hasSelectedPages: false,
      downloadUrl: this.getDownloadUrl(file.name)
    };

    this.loadDocumentPages(doc);

    return doc;
  }

  loadDocumentPages(doc: Doc, pages?: number[] ) {

    let api = this.api;

    if (pages == undefined) {

      // Load all pages
      doc.pages = [];

      api.getPageCount(doc.name).subscribe((pageCount) => {

        // Get page previews
        for (let i = 1; i <= pageCount; i++) {

          let page: Page = {
            number: i,
            active: false,
            url: this.getPagePreviewUrl(doc, i, this.pageWidth, this.pageHeight),
            loaded: false
          };

          doc.pages.push(page);
        }
      }, error => this.showMessageBox(error));

    } else {

      // Load defined pages
      pages.forEach(page => {

        doc.pages[page - 1].url = this.getPagePreviewUrl(doc, page, this.pageWidth, this.pageHeight);
      })
    }
  }

  getPagePreviewUrl(doc: Doc, page: number, width: number, height: number) {
    return '/api/documents/' + doc.name + '/preview/' + page + '?width=' + width + '&height=' + height + '&t=' + new Date().getTime();
  }

  getDownloadUrl(name: string) {
    return '/api/documents/' + name + '/download';
  }

  reloadDocumentsPages(documents: Doc[]) {
    documents.forEach(doc => {
      this.loadDocumentPages(doc);
    });
  }

  getSelectedPageNumbers(doc: Doc): number[] {
    let selectedPagesNumbers: number[] = [];

    doc.pages.forEach(page => {
      if (page.active) {
        selectedPagesNumbers.push(page.number);
      }
    });

    return selectedPagesNumbers;
  }

  setPagesUnloaded(doc: Doc, pages: number[]) {

    doc.pages.forEach(page => {
      if (pages.indexOf(page.number) > -1) {
        page.loaded = false;
      }
    })
  }

  showMessageBox(message: string, title = 'An Error Occurred') {
    const modalRef = this.modalService.open(MessageBoxComponent);
    modalRef.componentInstance.title = title;
    modalRef.componentInstance.message = message;
  }
}

interface Doc {
  name: string;
  created: Date,
  lastModified: Date,
  pages: Page[];
  hasSelectedPages: boolean;
  downloadUrl?: string;
}

interface Page {
  number: number;
  url?: string;
  loaded: boolean;
  active: boolean;
}

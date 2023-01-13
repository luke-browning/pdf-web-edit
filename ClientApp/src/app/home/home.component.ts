import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { PDFWebEditAPI } from '../../api/PDFWebEditAPI';
import { InputBoxComponent } from '../input-box/input-box.component';
import { MessageBoxComponent } from '../message-box/message-box.component';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {

  documents: Doc[] = [];

  // Options
  directory = PDFWebEditAPI.TargetDirectory.Input;
  targetDirectories = PDFWebEditAPI.TargetDirectory;

  size = 'medium';
  pageHeight = 400;
  pageWidth = 282;

  sort = 'Name';
  sortDirection = 'Asc';

  constructor(private api: PDFWebEditAPI.DocumentClient, private modalService: NgbModal, private route: ActivatedRoute, private router: Router) {

    switch (router.url) {
      default:
        this.directory = PDFWebEditAPI.TargetDirectory.Input
        break;

      case '/output':
        this.directory = PDFWebEditAPI.TargetDirectory.Output;
        break;

      case '/trash':
        this.directory = PDFWebEditAPI.TargetDirectory.Trash;
        break;
    }

    // Load the document list
    api.getDocuments(this.directory).subscribe(result => {

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

  //
  // Sizing
  // 

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

  //
  // Sorting
  // 

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

  // 
  // Selection
  // 

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

  //
  // Manipulation
  // 

  rotate(doc: Doc) {

    let pagesToRotate = this.getSelectedPageNumbers(doc);

    if (pagesToRotate.length > 0) {

      this.setPagesUnloaded(doc, pagesToRotate);

      this.api.rotatePages(this.directory, doc.name, pagesToRotate).subscribe(() => {

        this.loadDocumentPages(doc, pagesToRotate);
        this.setDocumentModifiedState(doc, true);

      }, error => {
        this.loadDocumentPages(doc);
        this.showMessageBox(error);
      });

    } else {
      this.showMessageBox('No pages selected. Please select a page to rotate.');
    }
  }

  remove(doc: Doc) {

    let pagesToRemove = this.getSelectedPageNumbers(doc);

    if (pagesToRemove.length > 0) {

      this.setPagesUnloaded(doc, pagesToRemove);

      this.api.deletePages(this.directory, doc.name, pagesToRemove).subscribe(() => {

        this.loadDocumentPages(doc);
        this.setDocumentModifiedState(doc, true);

      }, error => {
        this.loadDocumentPages(doc);
        this.showMessageBox(error);
      });

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

    this.api.reorderPages(this.directory, doc.name, newPageOrder).subscribe(() => {

      this.loadDocumentPages(doc);
      this.setDocumentModifiedState(doc, true);
      
    }, error => {
      this.loadDocumentPages(doc);
      this.showMessageBox(error);
    });
  }

  revert(doc: Doc) {
    this.api.revertChanges(this.directory, doc.name).subscribe(() => {
      this.api.getDocument(this.directory, doc.name).subscribe((updatedDocument) => {

        // Make sure the document isn't null
        if (updatedDocument != null) {
          this.replaceDoc(doc, updatedDocument);
        } else {
          this.showMessageBox('File does not exist!');
        }
      });
    }, error => this.showMessageBox(error));
  }

  delete(doc: Doc) {
    this.api.delete(this.directory, doc.name).subscribe(() => {
      this.documents = this.documents.filter(item => item.name !== doc.name);
    }, error => this.showMessageBox(error));
  }

  save(doc: Doc) {
    this.api.save(doc.name).subscribe(() => {
      this.documents = this.documents.filter(item => item.name !== doc.name);
    }, error => this.showMessageBox(error));
  }

  restore(doc: Doc) {
    this.api.restore(this.directory, doc.name).subscribe(() => {
      this.documents = this.documents.filter(item => item.name !== doc.name);
    }, error => this.showMessageBox(error));
  }

  rename(doc: Doc) {

    this.showInputBox('Enter new name.', 'Rename Document', false, doc.name).then(result => {
      if (result) {
        this.api.rename(this.directory, doc.name, result).subscribe(() => {
          this.api.getDocument(this.directory, result).subscribe((updatedDocument) => {

            // Make sure the document isn't null
            if (updatedDocument != null) {
              this.replaceDoc(doc, updatedDocument);
            } else {
              this.showMessageBox('File does not exist!');
            }
          });
        }, error => this.showMessageBox(error.detail));
      }
    }, () => {

      // Dismissed
      return;
    });
  }

  unlock(doc: Doc) {

    this.showInputBox('This document is password protected. Please enter the password to unlock it.', 'Unlock Document', true).then(result => {
      if (result) {
        this.api.unlock(this.directory, doc.name, result).subscribe(() => {
          this.api.getDocument(this.directory, doc.name).subscribe((updatedDocument) => {

            // Make sure the document isn't null
            if (updatedDocument != null) {
              this.replaceDoc(doc, updatedDocument);
            } else {
              this.showMessageBox('File does not exist!');
            }
          });
        }, error => this.showMessageBox(error.detail));
      }
    }, () => {

      // Dismissed
      return;
    });
  }

  //
  // Helpers
  //

  replaceDoc(originalDoc: Doc, newDocument: PDFWebEditAPI.Document) {

    let index = this.documents.findIndex(item => item.name === originalDoc.name);

    if (index > -1) {
      let newDoc = this.loadDocument(newDocument);
      this.documents[index] = newDoc;
    }
  }

  loadDocuments(files: PDFWebEditAPI.Document[]) {

    this.documents = [];

    files.forEach((file) => {

      let doc = this.loadDocument(file);

      this.documents.push(doc);
    });
  }

  loadDocument(file: PDFWebEditAPI.Document): Doc {

    let doc: Doc = {
      name: file.name,
      created: file.created,
      lastModified: file.lastModified,
      pages: [],
      hasSelectedPages: false,
      downloadUrl: this.getDownloadUrl(file.name),
      canRevertChanges: file.hasChanges,
      corrupt: file.status == PDFWebEditAPI.DocumentStatus.Corrupted,
      passwordProtected: file.status == PDFWebEditAPI.DocumentStatus.PasswordProtected,
    };

    this.loadDocumentPages(doc);

    return doc;
  }

  loadDocumentPages(doc: Doc, pages?: number[] ) {

    if ((!doc.corrupt) && (!doc.passwordProtected)) {

      if (pages == undefined) {

        // Load all pages
        doc.pages = [];

        this.api.getPageCount(this.directory, doc.name).subscribe((pageCount) => {

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
  }

  getPagePreviewUrl(doc: Doc, page: number, width: number, height: number) {
    return '/api/documents/' + this.directory + '/' + doc.name + '/preview/' + page + '?width=' + width + '&height=' + height + '&t=' + new Date().getTime();
  }

  getDownloadUrl(name: string) {
    return '/api/documents/' + this.directory + '/' + name + '/download';
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

  setDocumentModifiedState(doc: Doc, state: boolean) {
    doc.canRevertChanges = state;
  }

  pageLoaded(page: Page) {
    page.loaded = true;
  }

  showMessageBox(message: any, title = 'An Error Occurred') {

    let error: string | undefined | null;

    if (typeof message === 'string') {
      error = message;
    }

    if (typeof message === 'object') {

      if (message instanceof PDFWebEditAPI.ProblemDetails) {
        error = message.detail;
      }
      else {
        error = 'Unhandled exception!';
      }
    }

    const modalRef = this.modalService.open(MessageBoxComponent);
    modalRef.componentInstance.title = title;
    modalRef.componentInstance.message = error;
  }

  showInputBox(message: string, title: string, password: boolean, defaultValue?: string): Promise<string> {
    const modalRef = this.modalService.open(InputBoxComponent);
    modalRef.componentInstance.title = title;
    modalRef.componentInstance.message = message;
    modalRef.componentInstance.password = password;
    modalRef.componentInstance.value = defaultValue;

    return modalRef.result;
  }
}

interface Doc {
  name: string;
  created: Date,
  lastModified: Date,
  pages: Page[];
  hasSelectedPages: boolean;
  downloadUrl?: string;
  canRevertChanges: boolean;
  corrupt: boolean;
  passwordProtected: boolean;
}

interface Page {
  number: number;
  url?: string;
  loaded: boolean;
  active: boolean;
}

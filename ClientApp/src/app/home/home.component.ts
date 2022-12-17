import { Component } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { PDFEditAPI } from '../../api/PDFEditAPI';
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {

  documents: Doc[] = [];

  // Options
  size = 'medium';

  sort = 'Name';
  sortDirection = 'Asc';

  constructor(private api: PDFEditAPI.DocumentClient, private sanitizer: DomSanitizer) {

    let docs: Doc[] = [];

    // Load the document list
    api.getDocuments().subscribe(result => {

      let files = result;

      files?.forEach(function (file) {

        let doc: Doc = {
          name: file.name,
          created: file.created,
          lastModified: file.lastModified,
          pages: []
        };

        api.getPageCount(file.name).subscribe(function (pageCount) {

          // Get page previews
          for (let i = 1; i <= pageCount; i++) {

            let page: Page = { number: i, active: false };
            doc.pages.push(page);

            api.getPagePreview(file.name, i).subscribe(function (preview) {

              let objectURL = URL.createObjectURL(preview!.data);
              let image = sanitizer.bypassSecurityTrustUrl(objectURL);

              page.data = image;
            });
          }
        });

        docs.push(doc);
      });

      this.documents = docs;

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
}

interface Doc {
  name: string;
  created: Date,
  lastModified: Date,
  pages: Page[];
}

interface Page {
  number: number;
  data?: SafeUrl;
  active: boolean;
}

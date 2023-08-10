import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { PDFWebEditAPI } from '../../../api/PDFWebEditAPI';
import { Doc } from '../../shared/models/doc';
import { ConfigService } from '../../services/config/config.service';
import { ReplaceDocument } from '../../shared/components/elements/document/document.component';
import { BehaviorSubject } from 'rxjs';
import { SessionService } from '../../services/session/session.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent {

  // Documents
  documentsLoaded = false;
  documents: Doc[] = [];
  filteredDocuments: Doc[] = [];
  filterQuery = '';

  // Config
  config!: PDFWebEditAPI.Config;

  // Options
  directory = PDFWebEditAPI.TargetDirectory.Inbox;
  targetDirectories = PDFWebEditAPI.TargetDirectory;
  directoryStructure!: PDFWebEditAPI.Folder;

  size = new BehaviorSubject<string>('medium');

  sort: string = 'Name';
  sortDirection: string = 'Asc';

  alwaysShowFooter: boolean = false;

  constructor(private api: PDFWebEditAPI.DocumentClient, private modalService: NgbModal, private route: ActivatedRoute,
    private router: Router, private configService: ConfigService, private sessionService: SessionService) {

    // Load the app config
    configService.getConfig().subscribe((config: PDFWebEditAPI.Config | null | undefined) => {
      this.config = config!;

      this.size.next(this.config?.previewConfig?.defaultSize!);

      this.sort = this.config?.generalConfig?.defaultSortColumn!;
      this.sortDirection = this.config?.generalConfig?.defaultSortDirection!;

      this.alwaysShowFooter = this.config?.footerConfig.showAlways!;

      // Work out the current directory
      switch (router.url) {
        case '/inbox':
          this.directory = PDFWebEditAPI.TargetDirectory.Inbox
          break;

        case '/outbox':
          this.directory = PDFWebEditAPI.TargetDirectory.Outbox;
          break;

        case '/archive':
          this.directory = PDFWebEditAPI.TargetDirectory.Archive;
          break;
      }

      this.refreshDocuments();
    });

    // Keep track of the preview size
    sessionService.previewSize.subscribe((previewSize: string) => this.size.next(previewSize));

    // Keep track of the sorting
    sessionService.sortBy.subscribe((sortBy: string) => this.setSort(sortBy));
    sessionService.sortDirection.subscribe((sortDirection: string) => this.setSortDirection(sortDirection));
  }

  //
  // Sorting
  // 

  setSort(sort: string) {
    this.sort = sort;
    this.sortDocuments();
    this.updatedFilteredDocuments();
  }

  setSortDirection(direction: string) {
    this.sortDirection = direction;
    this.sortDocuments();
    this.updatedFilteredDocuments();
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
  // Events
  // 

  newDocumentEvent(newDocument: PDFWebEditAPI.Document) {

    // Load the doc and push it to the screen
    let newDoc = this.loadDocument(newDocument);
    this.documents.push(newDoc);
    this.sortDocuments();
    this.updatedFilteredDocuments();
  }

  replaceDocEvent($event: ReplaceDocument) {
    this.replaceDoc($event.originalDoc, $event.newDocument);
    this.updatedFilteredDocuments();
  }

  removeDocEvent(name: string) {
    this.documents = this.documents.filter(item => item.name !== name);
    this.updatedFilteredDocuments();
  }

  batchDocumentChanges(changes: PDFWebEditAPI.DocumentResult[]) {

    // Remove documents that succeeded
    changes.forEach(change => {
      if (change.statusCode == 200) {
        this.documents = this.documents.filter(x => x.name !== change.document);
      }
    });

    this.updatedFilteredDocuments();
  }

  //
  // Helpers
  //

  refreshDocuments() {

    // Load the document list
    this.api.getDocuments(this.directory).subscribe(result => {

      this.loadDocuments(result!);

      this.sortDocuments();

      this.sessionService.search.subscribe((query: string) => {
        this.filterQuery = query;
        this.updatedFilteredDocuments();
      });

    }, error => console.error(error));
  }

  updatedFilteredDocuments() {
    this.filteredDocuments = this.documents.filter(doc => doc.name.toLowerCase().indexOf(this.filterQuery.toLowerCase()) > -1);
  }

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

    this.documentsLoaded = true;
  }

  loadDocument(file: PDFWebEditAPI.Document): Doc {

    let doc: Doc = {
      name: file.name,
      directory: file.directory,
      created: file.created,
      lastModified: file.lastModified,
      pages: [],
      hasSelectedPages: new BehaviorSubject<boolean>(false),
      downloadUrl: this.getDownloadUrl(file.name, file.directory),
      canRevertChanges: new BehaviorSubject<boolean>(file.hasChanges),
      corrupt: new BehaviorSubject<boolean>(file.status == PDFWebEditAPI.DocumentStatus.Corrupted),
      passwordProtected: new BehaviorSubject<boolean>(file.status == PDFWebEditAPI.DocumentStatus.PasswordProtected),
      selected: false
    };

    return doc;
  }

  getDownloadUrl(name: string, directory: string) {
    return '/api/documents/' + this.directory + '/' + name + '/download?subdirectory=' + encodeURIComponent(directory || '');
  }

  hasDocumentsSelected(): boolean {
    return (this.documents.filter(x => x.selected).length || 0) > 0;
  }
}

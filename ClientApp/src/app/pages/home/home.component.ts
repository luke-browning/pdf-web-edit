import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { PDFWebEditAPI } from '../../../api/PDFWebEditAPI';
import { DirectoryPickerComponent } from '../../shared/components/modals/directory-picker/directory-picker.component';
import { InputBoxComponent } from '../../shared/components/modals/input-box/input-box.component';
import { MergeDocmentComponent } from '../../shared/components/modals/merge-docment/merge-docment.component';
import { MessageBoxComponent } from '../../shared/components/modals/message-box/message-box.component';
import { Doc } from '../../shared/models/doc';
import { Page } from '../../shared/models/page';
import { ConfigService } from '../../services/config/config.service';
import { ToolbarButton } from '../../shared/models/toolbar-button';
import { ReplaceDocument } from '../../shared/components/elements/document/document.component';
import { BehaviorSubject } from 'rxjs';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {

  documents: Doc[] = [];

  // Config
  config!: PDFWebEditAPI.Config;

  // Options
  directory = PDFWebEditAPI.TargetDirectory.Input;
  targetDirectories = PDFWebEditAPI.TargetDirectory;
  directoryStructure: PDFWebEditAPI.Folder[] = [];

  size = new BehaviorSubject<string>('medium');

  sort: string = 'Name';
  sortDirection: string = 'Asc';

  constructor(private api: PDFWebEditAPI.DocumentClient, private modalService: NgbModal, private route: ActivatedRoute,
    private router: Router, private configService: ConfigService) {

    // Load the app config
    configService.getConfig().subscribe(config => {
      this.config = config!;

      this.size.next(this.config?.previewConfig?.defaultSize!);

      this.sort = this.config?.generalConfig?.defaultSortColumn!;
      this.sortDirection = this.config?.generalConfig?.defaultSortDirection!;

      // Work out the current directory
      switch (router.url) {
        default:
          router.navigateByUrl('/' + this.config?.generalConfig?.defaultFolder.toLowerCase() || 'input');
          break;

        case '/input':
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

      // List directories
      api.getDirectories().subscribe(result => {
        if (result != null) {
          this.directoryStructure = result;
        }
      });
    });
  }

  //
  // Sizing
  // 

  setSize($event: Event) {
    this.size.next(($event.target as HTMLInputElement).value);
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
  // Events
  // 

  newDocumentEvent(newDocument: PDFWebEditAPI.Document) {

    // Load the doc and push it to the screen
    let newDoc = this.loadDocument(newDocument);
    this.documents.push(newDoc);
    this.sortDocuments();
  }

  replaceDocEvent($event: ReplaceDocument) {
    this.replaceDoc($event.originalDoc, $event.newDocument);
  }

  removeDocEvent(name: string) {
    this.documents = this.documents.filter(item => item.name !== name);
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
      directory: file.directory,
      created: file.created,
      lastModified: file.lastModified,
      pages: [],
      hasSelectedPages: new BehaviorSubject<boolean>(false),
      downloadUrl: this.getDownloadUrl(file.name, file.directory),
      canRevertChanges: new BehaviorSubject<boolean>(file.hasChanges),
      corrupt: new BehaviorSubject<boolean>(file.status == PDFWebEditAPI.DocumentStatus.Corrupted),
      passwordProtected: new BehaviorSubject<boolean>(file.status == PDFWebEditAPI.DocumentStatus.PasswordProtected),
    };

    return doc;
  }

  getDownloadUrl(name: string, directory: string) {
    return '/api/documents/' + this.directory + '/' + name + '/download?subdirectory=' + encodeURIComponent(directory || '');
  }
}

import { Component, EventEmitter, Input, Output } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateService } from '@ngx-translate/core';
import { BehaviorSubject } from 'rxjs';
import { PDFWebEditAPI } from '../../../../../api/PDFWebEditAPI';
import { SessionService } from '../../../../services/session/session.service';
import { Doc } from '../../../models/doc';
import { ToolbarButton } from '../../../models/toolbar-button';
import { BatchResultComponent } from '../../modals/batch-result/batch-result.component';
import { DirectoryPickerComponent } from '../../modals/directory-picker/directory-picker.component';

@Component({
  selector: 'footer',
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.scss']
})
export class FooterComponent {

  @Input() documents!: Doc[];
  @Input() config!: PDFWebEditAPI.Config;
  @Input() directoryStructure: PDFWebEditAPI.Folder[] = [];
  @Input() directory!: PDFWebEditAPI.TargetDirectory;

  @Output() onDocumentChanges: EventEmitter<PDFWebEditAPI.DocumentResult[]> = new EventEmitter();

  // Toolbar buttons
  inboxButtons: Array<ToolbarButton> = [];
  outboxButtons: Array<ToolbarButton> = [];
  archiveButtons: Array<ToolbarButton> = [];

  // Colour modes
  colourMode!: string;
  targetDirectories = PDFWebEditAPI.TargetDirectory;

  // Button enabled observables
  allowSave = new BehaviorSubject<boolean>(false);
  allowSaveAs = new BehaviorSubject<boolean>(false);

  constructor(private api: PDFWebEditAPI.DocumentClient, private sessionService: SessionService,
    private translateService: TranslateService, private modalService: NgbModal) { }

  ngOnInit(): void {

    // Colour mode
    this.sessionService.colourMode.subscribe((colourMode: string) => this.colourMode = colourMode);

    // Generate the toolbar
    this.generateToolbar();
  }

  ngOnDestroy(): void {
    
  }

  getSelectedDocuments(): Doc[] {

    let selectedDocs = this.documents?.filter(x => x.selected);

    let hasCorruptSelection = selectedDocs.some(x => x.corrupt.value);
    let hasPasswordProtectedSelection = selectedDocs.some(x => x.passwordProtected.value);

    // Update button enabled observables
    this.allowSaveAs.next(!hasCorruptSelection && !hasPasswordProtectedSelection);
    this.allowSave.next(!hasCorruptSelection && !hasPasswordProtectedSelection);

    return selectedDocs;
  }

  getNumberOfSelectedDocuments(): number {
    return this.getSelectedDocuments().length;
  }

  getSelectedDocumentParameters() {
    return {
      selectedDocuments: this.getNumberOfSelectedDocuments() || 0,
      documents: this.documents?.length || 0
    };
  }

  generateToolbar() {

    // Inbox
    this.inboxButtons.push({
      label: "documents.buttons.selectAll",
      icon: "bi bi-check2-square",
      separator: false,
      if: this.config.footerConfig.showSelectAll,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.selectAll.bind(this)
    }, {
      label: "documents.buttons.deselectAll",
      icon: "bi bi-square",
      separator: true,
      if: this.config.footerConfig.showDeselectAll,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.unselect.bind(this)
    }, {
      label: "documents.buttons.download",
      icon: "bi bi-download",
      separator: false,
      if: this.config.inboxConfig.batchShowDownload,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.download.bind(this)
    }, {
      label: "documents.buttons.archive",
      icon: "bi bi-archive-fill",
      separator: false,
      if: this.config.inboxConfig.batchShowArchive,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.archive.bind(this)
    }, {
      label: "documents.buttons.saveAs",
      icon: "bi bi-check2-all",
      separator: false,
      if: this.config.inboxConfig.batchShowSaveAs,
      enabled: this.allowSaveAs,
      function: this.saveAs.bind(this)
    }, {
      label: "documents.buttons.save",
      icon: "bi bi-check2",
      separator: false,
      if: this.config.inboxConfig.batchShowSave,
      enabled: this.allowSave,
      function: this.save.bind(this)
    });

    // Outbox
    this.outboxButtons.push({
      label: "documents.buttons.selectAll",
      icon: "bi bi-check2-square",
      separator: false,
      if: this.config.footerConfig.showSelectAll,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.selectAll.bind(this)
    }, {
      label: "documents.buttons.deselectAll",
      icon: "bi bi-square",
      separator: true,
      if: this.config.footerConfig.showDeselectAll,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.unselect.bind(this)
    }, {
      label: "documents.buttons.download",
      icon: "bi bi-download",
      separator: false,
      if: this.config.outboxConfig.batchShowDownload,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.download.bind(this)
    }, {
      label: "documents.buttons.moveToInbox",
      icon: "bi bi-folder-symlink",
      separator: false,
      if: this.config.outboxConfig.batchShowRestore,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.restore.bind(this)
    });

    // Archive
    this.archiveButtons.push({
      label: "documents.buttons.selectAll",
      icon: "bi bi-check2-square",
      separator: false,
      if: this.config.footerConfig.showSelectAll,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.selectAll.bind(this)
    }, {
      label: "documents.buttons.deselectAll",
      icon: "bi bi-square",
      separator: true,
      if: this.config.footerConfig.showDeselectAll,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.unselect.bind(this)
    }, {
      label: "documents.buttons.permanentlyDelete",
      icon: "bi bi-trash3",
      separator: false,
      if: this.config.archiveConfig.batchShowDelete,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.permenentlyDeleteFromArchive.bind(this)
    }, {
      label: "documents.buttons.download",
      icon: "bi bi-download",
      separator: true,
      if: this.config.archiveConfig.batchShowDownload,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.download.bind(this)
    }, {
      label: "documents.buttons.moveToInbox",
      icon: "bi bi-folder-symlink",
      separator: false,
      if: this.config.archiveConfig.batchShowRestore,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.restore.bind(this)
    });
  }

  inboxSelectedDocumentsValid(): boolean {

    let valid = true;

    this.documents.forEach(doc => {
      if (doc.selected && (doc.corrupt || doc.passwordProtected)) {
        valid = false;
      }
    });

    return valid;
  }

  selectAll() {
    this.documents.forEach(doc => {
      doc.selected = true;
    });
  }

  unselect() {
    this.documents.forEach(doc => {
      doc.selected = false;
    });
  }

  archive() {

    let selected: PDFWebEditAPI.Archive[] = [];
    let selectedDocuments = this.getSelectedDocuments();

    // Get selected docs
    selectedDocuments.forEach(selectedDocument => {
      selected.push({
        document: selectedDocument.name,
        targetDirectory: this.directory,
        subDirectory: selectedDocument.directory
      } as PDFWebEditAPI.Archive);
    });

    // Make the batch request
    this.api.archiveBatch(selected).subscribe(results => this.handleResults(results!), error => this.handleErrorResult(error!));
  }

  saveAs() {

    const modalRef = this.modalService.open(DirectoryPickerComponent, {
      size: 'lg'
    });

    modalRef.componentInstance.folders = this.directoryStructure;
    modalRef.componentInstance.showNameEditor = false;

    modalRef.result.then(result => {

      let path = result.path;
      
      let selected: PDFWebEditAPI.SaveAs[] = [];
      let selectedDocuments = this.getSelectedDocuments();

      // Get selected docs
      selectedDocuments.forEach(selectedDocument => {
        selected.push({
          document: selectedDocument.name,
          sourceSubDirectory: selectedDocument.directory,
          targetSubDirectory: path,
          newName: selectedDocument.name
        } as PDFWebEditAPI.SaveAs);
      });

      this.api.saveAsBatch(selected).subscribe(results => this.handleResults(results!), error => this.handleErrorResult(error!));
    }, () => {

      // Dismissed
      return;
    });
  }

  save() {

    let selected: PDFWebEditAPI.Save[] = [];
    let selectedDocuments = this.getSelectedDocuments();

    // Get selected docs
    selectedDocuments.forEach(selectedDocument => {
      selected.push({
        document: selectedDocument.name,
        sourceSubDirectory: selectedDocument.directory
      } as PDFWebEditAPI.Save);
    });

    // Make the batch request
    this.api.saveBatch(selected).subscribe(results => this.handleResults(results!), error => this.handleErrorResult(error!));
  }

  permenentlyDeleteFromArchive() {

    let selected: PDFWebEditAPI.Delete[] = [];
    let selectedDocuments = this.getSelectedDocuments();

    // Get selected docs
    selectedDocuments.forEach(selectedDocument => {
      selected.push({
        document: selectedDocument.name,
        subDirectory: selectedDocument.directory
      } as PDFWebEditAPI.Delete);
    });

    // Make the batch request
    this.api.deleteFromArchiveBatch(selected).subscribe(results => this.handleResults(results!), error => this.handleErrorResult(error!));
  }

  download() {

    let downloadUrl = "/api/documents/batch/download?";
    let selectedDocuments = this.getSelectedDocuments();

    selectedDocuments.forEach((selectedDocument, index) => {
      downloadUrl += "batch[" + index + "].targetDirectory=" + encodeURIComponent(this.directory) + "&";
      downloadUrl += "batch[" + index + "].document=" + encodeURIComponent(selectedDocument.name) + "&";
      downloadUrl += "batch[" + index + "].subDirectory=" + encodeURIComponent(selectedDocument.directory) + "&";
    });

    var a = document.createElement("a");
    a.href = downloadUrl;
    a.target = '_blank';
    a.click();
  }

  restore() {

    let selected: PDFWebEditAPI.Restore[] = [];
    let selectedDocuments = this.getSelectedDocuments();

    // Get selected docs
    selectedDocuments.forEach(selectedDocument => {
      selected.push({
        document: selectedDocument.name,
        targetDirectory: this.directory,
        subDirectory: selectedDocument.directory
      } as PDFWebEditAPI.Restore);
    });

    // Make the batch request
    this.api.restoreBatch(selected).subscribe(results => this.handleResults(results!), error => this.handleErrorResult(error!));
  }

  //
  // Helpers
  //  

  handleErrorResult(error: any) {

    // Is it an array of results?
    if ((error instanceof Array) && (error.every(err => err instanceof PDFWebEditAPI.DocumentResult))) {
      this.handleResults(error!);
    }
  }

  handleResults(results: PDFWebEditAPI.DocumentResult[]) {

    // Show results modal if any results 
    if (results.filter(x => x.statusCode != 200).length > 0) {
      this.showResult(results);
    }

    this.onDocumentChanges.emit(results);
  }

  showResult(results: PDFWebEditAPI.DocumentResult[]) {

    const modalRef = this.modalService.open(BatchResultComponent);
    modalRef.componentInstance.results = results;
  }
}

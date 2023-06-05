import { Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { BehaviorSubject, combineLatest, Observable, Subject, Subscription } from 'rxjs';
import { PDFWebEditAPI } from '../../../../../api/PDFWebEditAPI';
import { SessionService } from '../../../../services/session/session.service';
import { Doc } from '../../../models/doc';
import { Page } from '../../../models/page';
import { ToolbarButton } from '../../../models/toolbar-button';
import { DirectoryPickerComponent } from '../../modals/directory-picker/directory-picker.component';
import { InputBoxComponent } from '../../modals/input-box/input-box.component';
import { MergeDocmentComponent } from '../../modals/merge-docment/merge-docment.component';
import { MessageBoxComponent } from '../../modals/message-box/message-box.component';
import { MaxPreviewSizeChangedEvent, SelectionChangedEvent } from '../pages/pages.component';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'document',
  templateUrl: './document.component.html',
  styleUrls: ['./document.component.scss']
})
export class DocumentComponent implements OnInit {

  @Input() document!: Doc;
  @Input() size!: Observable<string>;
  @Input() config!: PDFWebEditAPI.Config;
  @Input() directory!: PDFWebEditAPI.TargetDirectory;
  @Input() directoryStructure: PDFWebEditAPI.Folder[] = [];
  @Input() documents: Doc[] = [];

  @Output() onNewDocument: EventEmitter<PDFWebEditAPI.Document> = new EventEmitter();
  @Output() onReplaceDocument: EventEmitter<ReplaceDocument> = new EventEmitter();
  @Output() onRemoveDocument: EventEmitter<string> = new EventEmitter();

  @ViewChild('scrollWrapper', { static: false }) scrollWrapper!: ElementRef;

  targetDirectories = PDFWebEditAPI.TargetDirectory;

  inboxButtons: Array<ToolbarButton> = [];
  outboxButtons: Array<ToolbarButton> = [];
  archiveButtons: Array<ToolbarButton> = [];
  passwordProtectedButtons: Array<ToolbarButton> = [];
  corruptButtons: Array<ToolbarButton> = [];

  pageHeight = 400;
  pageWidth = 282;

  previewMarginBottom = 20;

  maxPreviewHeight!: number;
  resetDocumentPreview: Subject<void> = new Subject<void>();

  subscriptions: Subscription[] = [];

  // Colour modes
  colourMode!: string;

  constructor(private api: PDFWebEditAPI.DocumentClient, private sessionService: SessionService, private modalService: NgbModal, private translateService: TranslateService) { }

  ngOnInit(): void {

    let sizeSubscription = this.size.subscribe(result => {
      this.setSize(result);
      this.loadDocumentPages();
    });

    // Colour mode
    this.sessionService.colourMode.subscribe((colourMode) => this.colourMode = colourMode);

    this.subscriptions.push(sizeSubscription);

    this.generateToolbars();
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(subscription => subscription.unsubscribe());
  }

  generateToolbars() {

    // Inbox
    this.inboxButtons.push({
      label: "documents.buttons.selectAll",
      icon: "bi bi-check2-square",
      separator: false,
      if: this.config.inboxConfig.showSelectAll,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.selectAll.bind(this)
    }, {
      label: "documents.buttons.deselectAll",
      icon: "bi bi-square",
      separator: true,
      if: this.config.inboxConfig.showUnselect,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.unselect.bind(this)
    }, {
      label: "documents.buttons.rotateClockwise",
      icon: "bi bi-arrow-clockwise",
      separator: false,
      if: this.config.inboxConfig.showRotateAntiClockwise,
      enabled: this.document.hasSelectedPages,
      function: this.rotateClockwise.bind(this)
    }, {
      label: "documents.buttons.rotateAntiClockwise",
      icon: "bi bi-arrow-counterclockwise",
      separator: false,
      if: this.config.inboxConfig.showRotateAntiClockwise,
      enabled: this.document.hasSelectedPages,
      function: this.rotateAntiClockwise.bind(this)
    }, {
      label: "documents.buttons.remove",
      icon: "bi bi-x-lg",
      separator: false,
      if: this.config.inboxConfig.showRemove,
      enabled: this.document.hasSelectedPages,
      function: this.remove.bind(this)
    }, {
      label: "documents.buttons.split",
      icon: "bi bi-subtract",
      separator: false,
      if: this.config.inboxConfig.showSplit,
      enabled: this.document.hasSelectedPages,
      function: this.split.bind(this)
    }, {
      label: "documents.buttons.merge",
      icon: "bi bi-union",
      separator: true,
      if: this.config.inboxConfig.showMerge,
      enabled: this.document.hasSelectedPages,
      function: this.merge.bind(this)
    }, {
      label: "documents.buttons.rename",
      icon: "bi bi-pencil",
      separator: false,
      if: this.config.inboxConfig.showRename,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.rename.bind(this)
    }, {
      label: "documents.buttons.archive",
      icon: "bi bi-archive-fill",
      separator: false,
      if: this.config.inboxConfig.showArchive,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.archive.bind(this)
    }, {
      label: "documents.buttons.download",
      icon: "bi bi-download",
      separator: true,
      if: this.config.inboxConfig.showDownload,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.download.bind(this)
    }, {
      label: "documents.buttons.revertChanges",
      icon: "bi bi-repeat",
      separator: false,
      if: this.config.inboxConfig.showRevert,
      enabled: this.document.canRevertChanges,
      function: this.revert.bind(this)
    }, {
      label: "documents.buttons.saveTo",
      icon: "bi bi-check2-all",
      separator: false,
      if: this.config.inboxConfig.showSaveTo,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.saveTo.bind(this)
    }, {
      label: "documents.buttons.save",
      icon: "bi bi-check2",
      separator: this.config.generalConfig.debugMode,
      if: this.config.inboxConfig.showSave,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.save.bind(this)
    }, {
      label: "documents.buttons.reloadPages",
      icon: "bi bi-repeat",
      separator: false,
      if: this.config.generalConfig.debugMode,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.loadDocumentPages.bind(this)
    });

    // Outbox
    this.outboxButtons.push({
      label: "documents.buttons.download",
      icon: "bi bi-download",
      separator: true,
      if: this.config.outboxConfig.showDownload,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.download.bind(this)
    }, {
      label: "documents.buttons.moveToInbox",
      icon: "bi bi-folder-symlink",
      separator: false,
      if: this.config.outboxConfig.showRestore,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.restore.bind(this)
    });

    // Archive
    this.archiveButtons.push({
      label: "documents.buttons.permanentlyDelete",
      icon: "bi bi-trash3",
      separator: false,
      if: this.config.archiveConfig.showDelete,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.permenentlyDeleteFromArchive.bind(this)
    }, {
      label: "documents.buttons.download",
      icon: "bi bi-download",
      separator: true,
      if: this.config.archiveConfig.showDownload,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.download.bind(this)
    }, {
      label: "documents.buttons.moveToInbox",
      icon: "bi bi-folder-symlink",
      separator: false,
      if: this.config.archiveConfig.showRestore,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.restore.bind(this)
    });

    // Password protected
    this.passwordProtectedButtons.push({
      label: "documents.buttons.archive",
      icon: "bi bi-archive-fill",
      separator: false,
      if: true,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.archive.bind(this)
    }, {
      label: "documents.buttons.download",
      icon: "bi bi-download",
      separator: true,
      if: true,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.download.bind(this)
    }, {
      label: "documents.buttons.unlock",
      icon: "bi bi-unlock-fill",
      separator: false,
      if: true,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.unlock.bind(this)
    })

    // Corrupt
    this.corruptButtons.push({
      label: "documents.buttons.archive",
      icon: "bi bi-archive-fill",
      separator: false,
      if: true,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.archive.bind(this)
    }, {
      label: "documents.buttons.download",
      icon: "bi bi-download",
      separator: true,
      if: true,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.download.bind(this)
    }, {
      label: "documents.buttons.revertChanges",
      icon: "bi bi-repeat",
      separator: false,
      if: true,
      enabled: this.document.canRevertChanges,
      function: this.revert.bind(this)
    }, {
      label: "documents.buttons.moveToInbox",
      icon: "bi bi-folder-symlink",
      separator: false,
      if: this.directory != PDFWebEditAPI.TargetDirectory.Inbox,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.restore.bind(this)
    });
  }

  setSize(size: string) {

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
  }

  //
  // Preview
  // 

  updatePreviewSize($event: MaxPreviewSizeChangedEvent) {
    this.maxPreviewHeight = $event.height + this.previewMarginBottom + this.getScrollWrapperScrollbarHeight();
  }

  getScrollWrapperScrollbarHeight(): number {

    let div = (this.scrollWrapper.nativeElement as HTMLDivElement);
    let scrollbarHeight = div.offsetHeight - div.clientHeight;

    return scrollbarHeight;
  }

  // 
  // Selection
  // 

  selectionChanged($event: SelectionChangedEvent) {
    this.document.hasSelectedPages.next($event.selectedPages.length > 0);
  }

  selectAll() {
    this.document.pages.forEach(page => {
      page.active = true;
      this.document.hasSelectedPages.next(true);
    });
  }

  unselect() {
    this.document.pages.forEach(page => {
      page.active = false;
    });

    this.document.hasSelectedPages.next(false);
  }

  //
  // Manipulation
  // 

  rotateClockwise() {

    let pagesToRotate = this.getSelectedPageNumbers();

    if (pagesToRotate.length > 0) {

      this.setPagesUnloaded(pagesToRotate);

      this.api.rotatePagesClockwise(this.directory, this.document.name, pagesToRotate, this.document.directory).subscribe(() => {

        this.loadDocumentPages(pagesToRotate);
        this.setDocumentModifiedState(true);

      }, error => {
        this.loadDocumentPages();
        this.showMessageBox(error);
      });

    } else {
      this.showMessageBox(this.translateService.instant('errors.noPagesSelectedForRotate'));
    }
  }

  rotateAntiClockwise() {

    let pagesToRotate = this.getSelectedPageNumbers();

    if (pagesToRotate.length > 0) {

      this.setPagesUnloaded(pagesToRotate);

      this.api.rotatePagesAntiClockwise(this.directory, this.document.name, pagesToRotate, this.document.directory).subscribe(() => {

        this.loadDocumentPages( pagesToRotate);
        this.setDocumentModifiedState(true);

      }, error => {
        this.loadDocumentPages();
        this.showMessageBox(error);
      });

    } else {
      this.showMessageBox(this.translateService.instant('errors.noPagesSelectedForRotate'));
    }
  }

  remove() {

    let pagesToRemove = this.getSelectedPageNumbers();

    if (pagesToRemove.length > 0) {

      this.setPagesUnloaded(pagesToRemove);

      this.api.deletePages(this.directory, this.document.name, pagesToRemove, this.document.directory).subscribe(() => {

        this.loadDocumentPages();
        this.setDocumentModifiedState(true);
        this.setHasSelectedPages();

      }, error => {
        this.loadDocumentPages();
        this.showMessageBox(error);
      });

    } else {
      this.showMessageBox(this.translateService.instant('errors.noPagesSelectedForRemove'));
    }
  }

  reorder($event: Page[]) {

    let newPageOrder: number[] = [];

    $event.forEach(page => {
      newPageOrder.push(page.number);
    });

    this.setPagesUnloaded( newPageOrder);

    this.api.reorderPages(this.directory, this.document.name, newPageOrder, this.document.directory).subscribe(() => {

      this.loadDocumentPages();
      this.setDocumentModifiedState(true);

    }, error => {
      this.loadDocumentPages();
      this.showMessageBox(error);
    });
  }

  split() {

    let pages: number[] = [];

    this.document.pages.forEach(page => {
      if (page.active) {
        pages.push(page.number);
      }
    });

    this.api.splitPages(this.directory, this.document.name, pages, this.document.directory).subscribe((newDocument) => {

      if (newDocument != null) {

        this.onNewDocument.emit(newDocument);

      } else {
        this.showMessageBox(this.translateService.instant('errors.splitUnableToLoadNewDocument'));
      }

    }, error => {
      this.loadDocumentPages();
      this.showMessageBox(error);
    });
  }

  merge() {

    const modalRef = this.modalService.open(MergeDocmentComponent);
    modalRef.componentInstance.documents = this.documents;

    modalRef.result.then((result: Doc) => {
      this.api.merge(this.directory, this.document.name, result.name, this.document.directory, result.directory).subscribe((updatedDocument) => {

        if (updatedDocument != null) {
          this.onReplaceDocument.emit({ originalDoc: this.document, newDocument: updatedDocument });
        } else {
          this.showMessageBox(this.translateService.instant('errors.mergeUnableToLoadUpdatedDocument'));
        }

      }, error => this.showMessageBox(error));
    }, () => {

      // Dismissed
      return;
    });
  }

  revert() {
    this.api.revertChanges(this.directory, this.document.name, this.document.directory).subscribe(() => {
      this.api.getDocument(this.directory, this.document.name, this.document.directory).subscribe((updatedDocument) => {

        // Make sure the document isn't null
        if (updatedDocument != null) {
          this.onReplaceDocument.emit({ originalDoc: this.document, newDocument: updatedDocument });
        } else {
          this.showMessageBox(this.translateService.instant('errors.fileDoesNotExist'));
        }
      });
    }, error => this.showMessageBox(error));
  }

  archive() {
    this.api.archive(this.directory, this.document.name, this.document.directory).subscribe(() => {
      this.onRemoveDocument.emit(this.document.name);
    }, error => this.showMessageBox(error));
  }

  permenentlyDeleteFromArchive() {
    this.api.deleteFromArchive(this.document.name).subscribe(() => {
      this.onRemoveDocument.emit(this.document.name);
    }, error => this.showMessageBox(error));
  }

  download() {
    var a = document.createElement("a");
    a.href = this.getDownloadUrl(this.document.name, this.document.directory);
    a.target = '_blank';
    // Don't set download attribute
    // a.download = "Example.pdf";
    a.click();
  }

  saveTo() {

    const modalRef = this.modalService.open(DirectoryPickerComponent);
    modalRef.componentInstance.folders = this.directoryStructure;

    modalRef.result.then(result => {
      this.api.saveTo(this.document.name, this.document.directory, result).subscribe(() => {
        this.onRemoveDocument.emit(this.document.name);
      }, error => this.showMessageBox(error));
    }, () => {

      // Dismissed
      return;
    });
  }

  save() {
    this.api.save(this.document.name, this.document.directory).subscribe(() => {
      this.onRemoveDocument.emit(this.document.name);
    }, error => this.showMessageBox(error));
  }

  restore() {
    this.api.restore(this.directory, this.document.name, this.document.directory).subscribe(() => {
      this.onRemoveDocument.emit(this.document.name);
    }, error => this.showMessageBox(error));
  }

  rename() {

    this.showInputBox(this.translateService.instant('renameDocument.message'),
      this.translateService.instant('renameDocument.title'),
      this.translateService.instant('renameDocument.buttons.rename'),
      this.translateService.instant('renameDocument.buttons.close'),
      false, this.document.name).then(result => {
      if (result) {
        this.api.rename(this.directory, this.document.name, result, this.document.directory).subscribe(() => {
          this.api.getDocument(this.directory, result, this.document.directory).subscribe((updatedDocument) => {

            // Make sure the document isn't null
            if (updatedDocument != null) {
              this.onReplaceDocument.emit({ originalDoc: this.document, newDocument: updatedDocument });
            } else {
              this.showMessageBox(this.translateService.instant('errors.fileDoesNotExist'));
            }
          });
        }, error => this.showMessageBox(error.detail));
      }
    }, () => {

      // Dismissed
      return;
    });
  }

  unlock() {

    this.showInputBox(this.translateService.instant('unlockDocument.message'),
      this.translateService.instant('unlockDocument.title'),
      this.translateService.instant('unlockDocument.buttons.unlock'),
      this.translateService.instant('unlockDocument.buttons.close'), true).then(result => {
      if (result) {
        this.api.unlock(this.directory, this.document.name, result, this.document.directory).subscribe(() => {
          this.api.getDocument(this.directory, this.document.name, this.document.directory).subscribe((updatedDocument) => {

            // Make sure the document isn't null
            if (updatedDocument != null) {
              this.onReplaceDocument.emit({ originalDoc: this.document, newDocument: updatedDocument });
            } else {
              this.showMessageBox(this.translateService.instant('errors.fileDoesNotExist'));
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

  loadDocumentPages(pages?: number[]) {

    combineLatest(this.document.corrupt, this.document.passwordProtected,
      (corrupt, passwordProtected) => ({ corrupt, passwordProtected })).subscribe(res => {

        if ((!res.corrupt) && (!res.passwordProtected)) {

          if (pages == undefined) {

            // Load all pages
            this.document.pages = [];

            this.maxPreviewHeight = 0;
            this.resetDocumentPreview.next();

            this.api.getPageCount(this.directory, this.document.name, this.document.directory).subscribe((pageCount) => {

              // Get page previews
              for (let i = 1; i <= pageCount; i++) {

                let page: Page = {
                  number: i,
                  active: false,
                  url: this.getPagePreviewUrl(i, this.pageWidth, this.pageHeight),
                  loaded: false,
                  show: true
                };

                this.document.pages.push(page);
              }
            }, error => this.showMessageBox(error));

          } else {

            // Load defined pages
            pages.forEach(page => {

              this.document.pages[page - 1].url = this.getPagePreviewUrl(page, this.pageWidth, this.pageHeight);
            })
          }
        }

      });
  }

  getPagePreviewUrl(page: number, width: number, height: number) {
    return '/api/documents/' + this.directory + '/' + this.document.name + '/preview/' + page + '?subdirectory=' + encodeURIComponent(this.document.directory || '') + '&width=' + width + '&height=' + height + '&t=' + new Date().getTime();
  }

  getDownloadUrl(name: string, directory: string) {
    return '/api/documents/' + this.directory + '/' + name + '/download?subdirectory=' + encodeURIComponent(directory || '');
  }

  getSelectedPageNumbers(): number[] {
    let selectedPagesNumbers: number[] = [];

    this.document.pages.forEach(page => {
      if (page.active) {
        selectedPagesNumbers.push(page.number);
      }
    });

    return selectedPagesNumbers;
  }

  setHasSelectedPages() {

    let hasSelectedPages = false;

    this.document.pages.forEach(p => {
      if (p.active) {
        hasSelectedPages = true;
      }
    });

    this.document.hasSelectedPages.next(hasSelectedPages);
  }

  setPagesUnloaded(pages: number[]) {

    this.document.pages.forEach(page => {
      if (pages.indexOf(page.number) > -1) {
        page.loaded = false;
      }
    })
  }

  setDocumentModifiedState(state: boolean) {
    this.document.canRevertChanges.next(state);
  }

  showMessageBox(message: any, title = '') {

    let error: string | undefined | null;

    if (!title) {
      title = this.translateService.instant('errors.anErrorOccurred');
    }

    if (typeof message === 'string') {
      error = message;
    }

    if (typeof message === 'object') {

      if (message instanceof PDFWebEditAPI.ProblemDetails) {
        error = message.detail;
      } else {
        error = this.translateService.instant('errors.unhandledException');
      }
    }

    const modalRef = this.modalService.open(MessageBoxComponent);
    modalRef.componentInstance.title = title;
    modalRef.componentInstance.message = error;
  }

  showInputBox(message: string, title: string, okButton: string, closeButton: string, password: boolean, defaultValue?: string): Promise<string> {
    const modalRef = this.modalService.open(InputBoxComponent);
    modalRef.componentInstance.title = title;
    modalRef.componentInstance.message = message;
    modalRef.componentInstance.okButton = okButton;
    modalRef.componentInstance.closeButton = closeButton;
    modalRef.componentInstance.password = password;
    modalRef.componentInstance.value = defaultValue;

    return modalRef.result;
  }
}

export interface ReplaceDocument {
  originalDoc: Doc;
  newDocument: PDFWebEditAPI.Document
}

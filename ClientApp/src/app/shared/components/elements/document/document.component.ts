import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
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
import { MaxPreviewSizeChangedEvent, PageOrderChangedEvent, SelectionChangedEvent } from '../pages/pages.component';
import { TranslateService } from '@ngx-translate/core';
import { NgScrollbar, NgScrollbarState } from 'ngx-scrollbar';
import { UiService } from '../../../../services/ui/ui.service';

@Component({
  selector: 'document',
  templateUrl: './document.component.html',
  styleUrls: ['./document.component.scss']
})
export class DocumentComponent implements OnInit, AfterViewInit {

  @Input() document!: Doc;
  @Input() size!: Observable<string>;
  @Input() config!: PDFWebEditAPI.Config;
  @Input() directory!: PDFWebEditAPI.TargetDirectory;
  @Input() directoryStructure: PDFWebEditAPI.Folder[] = [];
  @Input() documents: Doc[] = [];

  @Output() onNewDocument: EventEmitter<PDFWebEditAPI.Document> = new EventEmitter();
  @Output() onReplaceDocument: EventEmitter<ReplaceDocument> = new EventEmitter();
  @Output() onRemoveDocument: EventEmitter<string> = new EventEmitter();

  @ViewChild('toolbarScrollbar', { static: false }) toolbarScrollbar!: NgScrollbar;
  @ViewChild('documentScrollbar', { static: false }) documentScrollbar!: NgScrollbar;
  @ViewChild('toolbarScrollWrapper', { static: false }) toolbarScrollWrapper!: ElementRef;
  @ViewChild('documentScrollWrapper', { static: false }) documentScrollWrapper!: ElementRef;

  targetDirectories = PDFWebEditAPI.TargetDirectory;

  inboxButtons: Array<ToolbarButton> = [];
  outboxButtons: Array<ToolbarButton> = [];
  archiveButtons: Array<ToolbarButton> = [];
  passwordProtectedButtons: Array<ToolbarButton> = [];
  corruptButtons: Array<ToolbarButton> = [];

  pageHeight = 400;
  pageWidth = 282;

  maxPreviewHeight!: number;
  resetDocumentPreview: Subject<void> = new Subject<void>();

  subscriptions: Subscription[] = [];

  // Colour modes
  colourMode!: string;

  toolbarScrollable = false;
  documentScrollable = false;

  constructor(private api: PDFWebEditAPI.DocumentClient, private sessionService: SessionService, private uiService: UiService,
    private modalService: NgbModal, private translateService: TranslateService, private cdRef: ChangeDetectorRef) { }

  ngOnInit(): void {

    let sizeSubscription = this.size.subscribe((result: string) => {
      this.setSize(result);
      this.loadDocumentPages();
    });

    // Colour mode
    this.sessionService.colourMode.subscribe((colourMode: string) => this.colourMode = colourMode);

    this.subscriptions.push(sizeSubscription);

    this.generateToolbars();
  }

  ngAfterViewInit(): void {
    this.toolbarScrollable = (this.toolbarScrollbar.state.isHorizontallyScrollable!);
    this.documentScrollable = (this.documentScrollbar.state.isHorizontallyScrollable!);
    this.cdRef.detectChanges();
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
      label: "documents.buttons.reversePageOrder",
      icon: "bi bi-arrow-left-right",
      separator: false,
      if: this.config.inboxConfig.showReversePageOrder,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.reverseOrder.bind(this)
    }, {
      label: "documents.buttons.remove",
      icon: "bi bi-x-lg",
      separator: true,
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
      enabled: new BehaviorSubject<boolean>(true),
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
      label: "documents.buttons.saveAs",
      icon: "bi bi-check2-all",
      separator: false,
      if: this.config.inboxConfig.showSaveAs,
      enabled: new BehaviorSubject<boolean>(true),
      function: this.saveAs.bind(this)
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

  onToolbarScrollbarUpdate(state: NgScrollbarState) {
    this.toolbarScrollable = state.isHorizontallyScrollable!;
  }

  onDocumentScrollbarUpdate(state: NgScrollbarState) {
    this.documentScrollable = state.isHorizontallyScrollable!;
  }

  //
  // Preview
  // 

  updatePreviewSize($event: MaxPreviewSizeChangedEvent) {
    this.maxPreviewHeight = $event.height + this.getDocumentScrollWrapperScrollbarHeight();
  }

  getDocumentScrollWrapperScrollbarHeight(): number {

    let div = (this.documentScrollWrapper.nativeElement as HTMLDivElement);
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
        this.uiService.showMessageBox(error);
      });

    } else {
      this.uiService.showMessageBox(this.translateService.instant('errors.noPagesSelectedForRotate'));
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
        this.uiService.showMessageBox(error);
      });

    } else {
      this.uiService.showMessageBox(this.translateService.instant('errors.noPagesSelectedForRotate'));
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
        this.uiService.showMessageBox(error);
      });

    } else {
      this.uiService.showMessageBox(this.translateService.instant('errors.noPagesSelectedForRemove'));
    }
  }

  reverseOrder() {

    let allPageNumbers = this.getAllPageNumbers();
    this.setPagesUnloaded(allPageNumbers);

    this.api.reversePagesOrder(this.directory, this.document.name, this.document.directory).subscribe(() => {

      this.loadDocumentPages();
      this.setDocumentModifiedState(true);

    }, error => {
      this.loadDocumentPages();
      this.uiService.showMessageBox(error);
    });
  }

  reorder($event: PageOrderChangedEvent) {

    this.setPagesUnloaded($event.newPageOrder);

    this.api.reorderPages(this.directory, this.document.name, $event.newPageOrder, this.document.directory).subscribe(() => {

      this.loadDocumentPages();
      this.setDocumentModifiedState(true);

    }, error => {
      this.loadDocumentPages();
      this.uiService.showMessageBox(error);
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
        this.uiService.showMessageBox(this.translateService.instant('errors.splitUnableToLoadNewDocument'));
      }

    }, error => {
      this.loadDocumentPages();
      this.uiService.showMessageBox(error);
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
          this.uiService.showMessageBox(this.translateService.instant('errors.mergeUnableToLoadUpdatedDocument'));
        }

      }, error => this.uiService.showMessageBox(error));
    }, () => {

      // Dismissed
      return;
    });
  }

  revert() {
    this.api.revertChanges(this.directory, this.document.name, this.document.directory).subscribe(() => {
      this.api.getDocument(this.directory, this.document.name, this.document.directory).subscribe((updatedDocument: PDFWebEditAPI.Document | null) => {

        // Make sure the document isn't null
        if (updatedDocument != null) {
          this.onReplaceDocument.emit({ originalDoc: this.document, newDocument: updatedDocument });
        } else {
          this.uiService.showMessageBox(this.translateService.instant('errors.fileDoesNotExist'));
        }
      });
    }, error => this.uiService.showMessageBox(error));
  }

  archive() {
    this.api.archive(this.directory, this.document.name, this.document.directory).subscribe(() => {
      this.onRemoveDocument.emit(this.document.name);
    }, error => this.uiService.showMessageBox(error));
  }

  permenentlyDeleteFromArchive() {
    this.api.deleteFromArchive(this.document.name, this.document.directory).subscribe(() => {
      this.onRemoveDocument.emit(this.document.name);
    }, error => this.uiService.showMessageBox(error));
  }

  download() {
    var a = document.createElement("a");
    a.href = this.getDownloadUrl(this.document.name, this.document.directory);
    a.target = '_blank';
    // Don't set download attribute
    // a.download = "Example.pdf";
    a.click();
  }

  saveAs() {

    const modalRef = this.modalService.open(DirectoryPickerComponent, {
      size: 'lg'
    });

    modalRef.componentInstance.folders = this.directoryStructure;
    modalRef.componentInstance.name = this.document.name;
    modalRef.componentInstance.showNameEditor = true;

    modalRef.result.then(result => {

      let path = result.path;
      let name = result.name;

      this.api.saveAs(this.document.name, this.document.directory, path, name).subscribe(() => {
        this.onRemoveDocument.emit(this.document.name);
      }, error => this.uiService.showMessageBox(error));
    }, () => {

      // Dismissed
      return;
    });
  }

  save() {
    this.api.save(this.document.name, this.document.directory).subscribe(() => {
      this.onRemoveDocument.emit(this.document.name);
    }, error => this.uiService.showMessageBox(error));
  }

  restore() {
    this.api.restore(this.directory, this.document.name, this.document.directory).subscribe(() => {
      this.onRemoveDocument.emit(this.document.name);
    }, error => this.uiService.showMessageBox(error));
  }

  rename() {

    this.uiService.showInputBox(this.translateService.instant('renameDocument.message'),
      this.translateService.instant('renameDocument.title'),
      this.translateService.instant('renameDocument.buttons.rename'),
      this.translateService.instant('renameDocument.buttons.close'),
      false, this.document.name).then(result => {
      if (result) {
        this.api.rename(this.directory, this.document.name, result, this.document.directory).subscribe(() => {
          this.api.getDocument(this.directory, result, this.document.directory).subscribe((updatedDocument: PDFWebEditAPI.Document | null) => {

            // Make sure the document isn't null
            if (updatedDocument != null) {
              this.onReplaceDocument.emit({ originalDoc: this.document, newDocument: updatedDocument });
            } else {
              this.uiService.showMessageBox(this.translateService.instant('errors.fileDoesNotExist'));
            }
          });
        }, error => this.uiService.showMessageBox(error.detail));
      }
    }, () => {

      // Dismissed
      return;
    });
  }

  unlock() {

    this.uiService.showInputBox(this.translateService.instant('unlockDocument.message'),
      this.translateService.instant('unlockDocument.title'),
      this.translateService.instant('unlockDocument.buttons.unlock'),
      this.translateService.instant('unlockDocument.buttons.close'), true).then(result => {
      if (result) {
        this.api.unlock(this.directory, this.document.name, result, this.document.directory).subscribe(() => {
          this.api.getDocument(this.directory, this.document.name, this.document.directory).subscribe((updatedDocument: PDFWebEditAPI.Document | null) => {

            // Make sure the document isn't null
            if (updatedDocument != null) {
              this.onReplaceDocument.emit({ originalDoc: this.document, newDocument: updatedDocument });
            } else {
              this.uiService.showMessageBox(this.translateService.instant('errors.fileDoesNotExist'));
            }
          });
        }, error => this.uiService.showMessageBox(error.detail));
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
      (corrupt, passwordProtected) => ({ corrupt, passwordProtected })).subscribe((res: { corrupt: boolean; passwordProtected: boolean; }) => {

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
            }, error => this.uiService.showMessageBox(error));

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
    return '/api/documents/preview/' + this.directory + '/' + this.document.name + '/' + page + '?subdirectory=' + encodeURIComponent(this.document.directory || '') + '&width=' + width + '&height=' + height + '&t=' + new Date().getTime();
  }

  getDownloadUrl(name: string, directory: string) {
    return '/api/documents/download/' + this.directory + '/' + name + '?subdirectory=' + encodeURIComponent(directory || '');
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

  getAllPageNumbers(): number[] {
    let allPagesNumbers: number[] = [];

    this.document.pages.forEach(page => {
      allPagesNumbers.push(page.number);
    });

    return allPagesNumbers;
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
}

export interface ReplaceDocument {
  originalDoc: Doc;
  newDocument: PDFWebEditAPI.Document
}

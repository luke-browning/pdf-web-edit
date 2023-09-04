import { Component, Input, OnInit } from '@angular/core';
import { NgbActiveModal, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateService } from '@ngx-translate/core';
import { PDFWebEditAPI } from '../../../../../api/PDFWebEditAPI';
import { SessionService } from '../../../../services/session/session.service';
import { UiService } from '../../../../services/ui/ui.service';
import { Doc } from '../../../models/doc';
import { PickerMode } from '../../../models/picker-mode';

@Component({
  selector: 'app-directory-picker',
  templateUrl: './directory-picker.component.html',
  styleUrls: ['./directory-picker.component.scss']
})
export class DirectoryPickerComponent implements OnInit {

  @Input() name: string | undefined;
  @Input() showNameEditor!: boolean;

  rootFolder!: PDFWebEditAPI.Folder;
  targetDirectory = PDFWebEditAPI.TargetDirectory.Outbox;

  parentFolder?: PDFWebEditAPI.Folder;
  currentPath: PDFWebEditAPI.Folder[] = [];
  currentDirectoryFolders: PDFWebEditAPI.Folder[] = [];

  folderFiles: PDFWebEditAPI.Document[] = [];

  pickerModes = PickerMode;
  pickerMode: PickerMode = PickerMode.Outbox;

  favourites: string[] = [];
  history: string[] = [];

  showFiles = false;
  loading = false;

  directoryCacheKey = 'save_as_directory_listing';

  constructor(private activeModal: NgbActiveModal, private api: PDFWebEditAPI.DocumentClient, private uiService: UiService,
    private sessionService: SessionService, private modalService: NgbModal, private translateService: TranslateService) { }

  ngOnInit(): void {

    this.loading = true;

    // Try and load cached copy of directory structure
    let rootFolder = this.sessionService.getCache(this.directoryCacheKey);

    if (rootFolder) {
      this.directoriesUpdated(rootFolder as PDFWebEditAPI.Folder);
    }

    // Load directories
    this.api.getDirectories().subscribe(result => {
      this.directoriesUpdated(result!);
    });

    // Load history and favourites
    this.favourites = (localStorage.getItem('save_favourites') || '').split(',').filter(n => n);
    this.history = (localStorage.getItem('save_history') || '').split(',').filter(n => n);

    // Show files
    this.sessionService.showFilesOnSaveAs.subscribe(x => this.showFiles = x);
  }

  directoriesUpdated(rootFolder: PDFWebEditAPI.Folder) {

    // Cache the latest directory listing
    this.sessionService.setCache(this.directoryCacheKey, rootFolder);

    // Get the current path before we replace the folder structure
    let currentPath = this.getCurrentPath();

    // Replace the folder structure
    this.rootFolder = rootFolder;

    // Set the current directory back to what it was
    this.setFolderByPath(currentPath);

    // Finished load
    this.loading = false;
  }

  up() {
    this.currentPath.pop();

    if (this.currentPath.length == 0) {
      this.currentDirectoryFolders = this.rootFolder.subFolders;
      this.folderFiles = this.rootFolder.documents;
    } else {
      this.currentDirectoryFolders = this.currentPath[this.currentPath.length - 1].subFolders;
      this.folderFiles = this.currentPath[this.currentPath.length - 1].documents;
    }
  }

  setFolder(folder: PDFWebEditAPI.Folder) {
    this.parentFolder = folder;
    this.currentPath.push(folder);
    this.currentDirectoryFolders = folder.subFolders;
    this.folderFiles = folder.documents;
  }

  getCurrentSubDirectory() {
    return this.currentPath.map(x => x.name).join('/');
  }

  createDirectory() {

    this.uiService.showInputBox(this.translateService.instant('directoryPicker.createDirectory.message'), this.translateService.instant('directoryPicker.createDirectory.title'),
      this.translateService.instant('directoryPicker.createDirectory.buttons.create'), this.translateService.instant('directoryPicker.createDirectory.buttons.close'),
      false).then(result => {
        if (result) {

          var currentDirectory = this.getCurrentSubDirectory();

          this.api.createDirectory(this.targetDirectory, result, currentDirectory).subscribe(folder => {
            this.currentDirectoryFolders.push(folder!);
          }, error => this.uiService.showMessageBox(error.detail));
        }
      }, () => {

        // Dismissed
        return;
      });
  }

  setFolderByPath(path: string) {

    // Get path segments
    let segments = path.split('/').filter(n => n);

    // Reset current path
    this.currentPath = [];

    // Get folders in root directory
    let currentFolder = this.rootFolder.subFolders;
    let currentDocuments = this.rootFolder.documents;

    // Go through each segment and find the corresponding folder in the tree
    for (var i = 0; i < segments.length; i++) {

      let currentSegment = segments[i];

      for (var j = 0; j < currentFolder.length; j++) {

        let folder = currentFolder[j];

        // Is this the folder we're looking for?
        if (folder.name == currentSegment) {

          // Add it to the array
          this.parentFolder = folder;
          this.currentPath.push(folder);

          // And set the next set of folders and documents to be this folders
          // sub folders and documents
          currentFolder = folder.subFolders;
          currentDocuments = folder.documents;

          break;
        }
      }
    }

    // The last subfolders and documents are the current directory
    this.currentDirectoryFolders = currentFolder;
    this.folderFiles = currentDocuments;

    // Switch to the outbox view listing directories
    this.setPickerMode(PickerMode.Outbox);
  }

  setPickerMode(mode: PickerMode) {
    this.pickerMode = mode;
  }

  getCurrentPath(folder?: string): string {
    let path = '/';

    this.currentPath.forEach(folder => {
      path += folder.name + '/';
    });

    if (folder) {
      path += folder + '/';
    }

    return path;
  }

  addToFavourites(folder: PDFWebEditAPI.Folder) {
    let fullPath = this.getCurrentPath(folder.name);
    this.addToFavouritesByPath(fullPath);
  }

  addToFavouritesByPath(path: string) {

    // Don't add the favourite twice
    if (this.favourites.indexOf(path) < 0) {

      // Update the array
      this.favourites.push(path);

      // Save to local storage
      localStorage.setItem('save_favourites', this.favourites.join(','));
    }
  }

  removeFromFavourites(folder: PDFWebEditAPI.Folder) {
    let fullPath = this.getCurrentPath(folder.name);
    this.removeFromFavouritesByPath(fullPath);
  }

  removeFromFavouritesByPath(path: string) {

    let index = this.favourites.indexOf(path);

    // Check the favourite exists
    if (index > -1) {

      // Remove from the array
      this.favourites.splice(index, 1);

      // Save to local storage
      localStorage.setItem('save_favourites', this.favourites.join(','));
    }
  }

  isFavourite(folder: PDFWebEditAPI.Folder) {
    let fullPath = this.getCurrentPath(folder.name);
    return this.favourites.indexOf(fullPath) > -1;
  }

  getFavouriteName(path: string): string {

    let name = 'INVALID NAME';
    let segments = path.split('/').filter(n => n);

    if (segments.length > 0) {
      name = segments[segments.length - 1];
    }

    return name;
  }

  getFavouriteContainingFolder(path: string): string {

    let containingFolder = '';
    let segments = path.split('/').filter(n => n);

    if (segments.length > 0) {

      for (var i = 0; i < segments.length - 1; i++) {
        containingFolder += segments[i] + '/';
      }
    }

    return containingFolder;
  }

  addToHistory(path: string) {

    let index = this.history.indexOf(path);

    // Don't add the history twice
    if (index < 0) {

      // Update the array
      this.history.push(path);

    } else {

      // Reorder history
      let reorderedHistory = this.history.filter(item => item !== path);
      reorderedHistory.push(path);

      this.history = reorderedHistory;
    }

    // Only store the latest 10 records
    this.history = this.history.slice(-10);

    // Save to local storage
    localStorage.setItem('save_history', this.history.join(','));
  }

  removeFromHistory(path: string) {

    let index = this.history.indexOf(path);

    // Check the favourite exists
    if (index > -1) {

      // Remove from the array
      this.history.splice(index, 1);

      // Save to local storage
      localStorage.setItem('save_history', this.history.join(','));
    }
  }

  sortBy(arr: Array<any>, prop: string) {
    return arr.sort((a, b) => a[prop] > b[prop] ? 1 : a[prop] === b[prop] ? 0 : -1);
  }

  sortFavourites(arr: Array<string>) {
    return arr.sort((a, b) => {
      a = a.split('/')
        .filter(n => n)
        .pop()!;
      b = b.split('/')
        .filter(n => n)
        .pop()!;

      return a > b ? 1 : a === b ? 0 : -1;
    });
  }

  updateShowFilesDefault($event: any) {
    this.sessionService.setShowFilesOnSaveAs(this.showFiles);
  }

  download(doc: Doc) {
    var a = document.createElement("a");
    a.href = this.uiService.getDownloadUrl(PDFWebEditAPI.TargetDirectory.Outbox, doc);
    a.target = '_blank';
    // Don't set download attribute
    // a.download = "Example.pdf";
    a.click();
  }

  ok() {

    // Get the selected path
    let path = this.getCurrentPath();

    // Update the history 
    this.addToHistory(path);

    // Close the dialog
    this.activeModal.close({
      path: path,
      name: this.showNameEditor ? this.name : null,
    });
  }

  close() {
    this.activeModal.dismiss();
  }
}

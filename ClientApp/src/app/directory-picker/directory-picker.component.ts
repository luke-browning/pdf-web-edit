import { Component, Input, OnInit } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { PDFWebEditAPI } from '../../api/PDFWebEditAPI';

@Component({
  selector: 'app-directory-picker',
  templateUrl: './directory-picker.component.html',
  styleUrls: ['./directory-picker.component.css']
})
export class DirectoryPickerComponent implements OnInit {

  @Input() folders: PDFWebEditAPI.Folder[] = [];

  parentFolder?: PDFWebEditAPI.Folder;
  currentPath: PDFWebEditAPI.Folder[] = [];
  currentDirectoryFolders: PDFWebEditAPI.Folder[] = [];

  constructor(private activeModal: NgbActiveModal) { }

  ngOnInit(): void {

    this.currentPath = [];
    this.currentDirectoryFolders = this.folders;
  }

  up() {
    this.currentPath.pop();

    if (this.currentPath.length == 0) {
      this.currentDirectoryFolders = this.folders;
    } else {
      this.currentDirectoryFolders = this.currentPath[this.currentPath.length - 1].subFolders;
    }
  }

  setFolder(folder: PDFWebEditAPI.Folder) {
    this.parentFolder = folder;
    this.currentPath.push(folder);
    this.currentDirectoryFolders = folder.subFolders;
  }

  ok() {
    let path = '/';

    this.currentPath.forEach(folder => {
      path += folder.name + '/';
    });

    this.activeModal.close(path);
  }

  close() {
    this.activeModal.dismiss();
  }
}

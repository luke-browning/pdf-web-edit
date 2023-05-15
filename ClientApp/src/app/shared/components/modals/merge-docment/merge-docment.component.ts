import { Component, Input, OnInit } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { Doc } from '../../../models/doc';

@Component({
  selector: 'app-merge-docment',
  templateUrl: './merge-docment.component.html',
  styleUrls: ['./merge-docment.component.scss']
})
export class MergeDocmentComponent implements OnInit {

  @Input() documents: Doc[] = [];

  selected: Doc | undefined;
  search = '';

  constructor(private activeModal: NgbActiveModal) { }

  ngOnInit(): void {
  }

  select(document: Doc) {

    if (this.selected === document) {
      this.selected = undefined;
    } else {
      this.selected = document;
    }
  }

  ok() {
    this.activeModal.close(this.selected);
  }

  close() {
    this.activeModal.dismiss();
  }
}

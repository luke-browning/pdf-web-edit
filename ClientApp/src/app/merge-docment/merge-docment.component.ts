import { Component, Input, OnInit } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { Doc } from '../../models/Doc';

@Component({
  selector: 'app-merge-docment',
  templateUrl: './merge-docment.component.html',
  styleUrls: ['./merge-docment.component.css']
})
export class MergeDocmentComponent implements OnInit {

  @Input() documents: Doc[] = [];

  selected: Doc | undefined;

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

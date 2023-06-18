import { Component, Input, OnInit } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-input-box',
  templateUrl: './input-box.component.html',
  styleUrls: ['./input-box.component.scss']
})
export class InputBoxComponent implements OnInit {

  @Input() title!: string;
  @Input() message!: string;
  @Input() okButton!: string;
  @Input() closeButton!: string;
  @Input() password!: boolean;
  @Input() value!: string;

  constructor(private activeModal: NgbActiveModal) { }

  ngOnInit(): void {

  }

  ok() {
    this.activeModal.close(this.value);
  }

  close() {
    this.activeModal.dismiss();
  }

}

import { Component, Input, OnInit } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-input-box',
  templateUrl: './input-box.component.html',
  styleUrls: ['./input-box.component.css']
})
export class InputBoxComponent implements OnInit {

  @Input() title!: string;
  @Input() message!: string;
  @Input() password!: boolean;

  value = '';

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

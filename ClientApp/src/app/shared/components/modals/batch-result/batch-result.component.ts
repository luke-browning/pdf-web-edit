import { Component, Input } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { PDFWebEditAPI } from '../../../../../api/PDFWebEditAPI';

@Component({
  selector: 'app-batch-result',
  templateUrl: './batch-result.component.html',
  styleUrls: ['./batch-result.component.scss']
})
export class BatchResultComponent {

  @Input() results: PDFWebEditAPI.DocumentResult[] = [];

  constructor(private activeModal: NgbActiveModal) { }

  ngOnInit(): void {

  }

  close() {
    this.activeModal.dismiss();
  }
}

import { Injectable } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateService } from '@ngx-translate/core';
import { PDFWebEditAPI } from '../../../api/PDFWebEditAPI';
import { InputBoxComponent } from '../../shared/components/modals/input-box/input-box.component';
import { MessageBoxComponent } from '../../shared/components/modals/message-box/message-box.component';

@Injectable({
  providedIn: 'root'
})
export class UiService {

  constructor(private modalService: NgbModal, private translateService: TranslateService) { }

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

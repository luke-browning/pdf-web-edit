import { Component } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { SettingsComponent } from '../../modals/settings/settings.component';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  isExpanded = false;

  constructor(private modalService: NgbModal) {

  }

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }

  editConfig() {
    const modalRef = this.modalService.open(SettingsComponent,
      {
        size: 'lg'
      });

    modalRef.result.then(result => {
      
    }, () => {

      // Dismissed
      return;
    });
  }
}

import { Component, OnInit } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { PDFWebEditAPI } from '../../../../../api/PDFWebEditAPI';
import { ConfigService } from '../../../../services/config/config.service';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.scss']
})
export class SettingsComponent implements OnInit {

  // Config
  config!: PDFWebEditAPI.Config;

  // Options
  folders = [
    { label: "Inbox", value: "Inbox" },
    { label: "Outbox", value: "Outbox" },
    { label: "Archive", value: "Archive" },
  ];

  sortColumns = [
    { label: "Name", value: "Name" },
    { label: "Created", value: "Created" },
    { label: "Last Modified", value: "Last Modified" },
  ];

  sortDirections = [
    { label: "Ascending", value: "Asc" },
    { label: "Descending", value: "Desc" },
  ];

  colourModes = [
    { label: "Auto", value: "auto" },
    { label: "Light", value: "light" },
    { label: "Dark", value: "dark" },
  ];

  // Preview Options
  sizes = [
    { label: "Small", value: "small" },
    { label: "Medium", value: "medium" },
    { label: "Large", value: "large" },
  ];

  // Tab
  active = 'general';
  
  constructor(private activeModal: NgbActiveModal, private configService: ConfigService) {

    // Load the app config
    configService.getConfig().subscribe(config => {

      // Create a copy of the config object so we are not manipulating the 
      // object that is in use throughout the project
      this.config = JSON.parse(JSON.stringify(config!));
    });
  }

  ngOnInit(): void {
  }

  reloadConfiguration() {
    this.configService.reloadConfig().then(config => {
      this.config = config!;
    });
  }

  ok() {
    this.configService.saveConfig(this.config).then(config => {
      this.config = config!;
      this.activeModal.close(this.config);
    });
  }

  close() {
    this.activeModal.dismiss();
  }
}

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
  languages = [
    { label: "shared.languages.languages.en", value: "en" },
    { label: "shared.languages.languages.fr", value: "fr" },
  ];

  folders = [
    { label: "shared.folders.inbox", value: "Inbox" },
    { label: "shared.folders.outbox", value: "Outbox" },
    { label: "shared.folders.archive", value: "Archive" },
  ];

  sortColumns = [
    { label: "shared.sort.by.name", value: "Name" },
    { label: "shared.sort.by.created", value: "Created" },
    { label: "shared.sort.by.lastModified", value: "Last Modified" },
  ];

  sortDirections = [
    { label: "shared.sort.direction.asc", value: "Asc" },
    { label: "shared.sort.direction.desc", value: "Desc" },
  ];

  colourModes = [
    { label: "shared.colourMode.modes.auto", value: "auto" },
    { label: "shared.colourMode.modes.light", value: "light" },
    { label: "shared.colourMode.modes.dark", value: "dark" },
  ];

  // Preview Options
  sizes = [
    { label: "shared.previewSize.sizes.small", value: "small" },
    { label: "shared.previewSize.sizes.medium", value: "medium" },
    { label: "shared.previewSize.sizes.large", value: "large" },
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

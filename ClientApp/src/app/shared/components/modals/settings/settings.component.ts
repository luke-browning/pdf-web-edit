import { Component, OnInit } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { PDFWebEditAPI } from '../../../../../api/PDFWebEditAPI';
import { ConfigService } from '../../../../services/config/config.service';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.css']
})
export class SettingsComponent implements OnInit {

  // Config
  config!: PDFWebEditAPI.Config;

  // Options
  folders = [
    { label: "Input", value: "Input" },
    { label: "Output", value: "Output" },
    { label: "Trash", value: "Trash" },
  ];

  sortColumns = [
    { label: "Name", value: "Name" },
    { label: "Created", value: "Created" },
    { label: "Last Modified", value: "Last Modified" },
  ];

  sortDirection = [
    { label: "Ascending", value: "Asc" },
    { label: "Descending", value: "Desc" },
  ];

  // Preview Options
  sizes = [
    { label: "Small", value: "small" },
    { label: "Medium", value: "medium" },
    { label: "Large", value: "large" },
  ];
  
  constructor(private activeModal: NgbActiveModal, private configService: ConfigService) {

    // Load the app config
    configService.getConfig().subscribe(config => {
      this.config = config!;
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

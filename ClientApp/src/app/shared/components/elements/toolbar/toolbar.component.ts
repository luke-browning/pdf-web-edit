import { Component, Input, OnInit } from '@angular/core';
import { PDFWebEditAPI } from '../../../../../api/PDFWebEditAPI';
import { ToolbarButton } from '../../../models/toolbar-button';

@Component({
  selector: 'toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.scss']
})
export class ToolbarComponent implements OnInit {

  @Input() buttons!: Array<ToolbarButton>;
  @Input() config!: PDFWebEditAPI.Config;

  showLabels = true;
  showIcons = true;

  constructor() { }

  ngOnInit(): void {

    this.showIcons = this.config?.generalConfig?.showIcons!;
    this.showLabels = this.config?.generalConfig?.showLabels!;
  }

}

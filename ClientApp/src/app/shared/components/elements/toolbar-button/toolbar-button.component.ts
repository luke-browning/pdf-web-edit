import { Component, Input, OnInit } from '@angular/core';
import { ToolbarButton } from '../../../models/toolbar-button';

@Component({
  selector: 'toolbar-button',
  templateUrl: './toolbar-button.component.html',
  styleUrls: ['./toolbar-button.component.scss']
})
export class ToolbarButtonComponent implements OnInit {

  @Input() button!: ToolbarButton;
  @Input() showIcon!: boolean;
  @Input() showLabel!: boolean;

  constructor() { }

  ngOnInit(): void {

  }

  run() {

    // Run the function if its defined
    if (this.button.function) {
      this.button.function();
    }
  }
}

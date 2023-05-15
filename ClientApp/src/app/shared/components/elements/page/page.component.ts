import { Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { Observable } from 'rxjs';
import { PDFWebEditAPI } from '../../../../../api/PDFWebEditAPI';
import { Page } from '../../../models/page';

@Component({
  selector: 'page',
  templateUrl: './page.component.html',
  styleUrls: ['./page.component.scss']
})
export class PageComponent implements OnInit {

  @Input() page!: Page;
  @Input() size!: Observable<string>;
  @Input() config!: PDFWebEditAPI.Config;

  @Output() onSelect: EventEmitter<SelectEvent> = new EventEmitter();
  @Output() onPagePreviewLoad: EventEmitter<PagePreviewLoadEvent> = new EventEmitter();

  @ViewChild('img', { static: false }) img!: ElementRef;

  previewWidth!: number;
  previewHeight!: number;

  borderWidthPx = 3;

  constructor() { }

  ngOnInit(): void {
    
  }

  setActive($event: MouseEvent) {

    // Make sure the page is loaded before we allow selection
    if (this.page.loaded) {

      let activatePage = true;

      // If the page is already active, deactivate it
      if (this.page.active) {
        activatePage = false;
      }

      // Set the state of the selected page
      this.page.active = activatePage;

      // Emit selected event
      this.onSelect.emit({ pageNumber: this.page.number, multiSelect: $event.ctrlKey, active: this.page.active });
    }
  }

  pageLoaded(pageNumber: number) {
    this.page.loaded = true;

    let imgWidth = (this.img.nativeElement as HTMLImageElement).naturalWidth;
    let imgHeight = (this.img.nativeElement as HTMLImageElement).naturalHeight;

    // Add the borders to the image size
    this.previewWidth = imgWidth + (this.borderWidthPx * 2);
    this.previewHeight = imgHeight + (this.borderWidthPx * 2);

    // Emit the page preview load event
    this.onPagePreviewLoad.emit({ pageNumber: pageNumber, width: this.previewWidth, height: this.previewHeight });
  }

  pageNotLoaded($event: Event) {
    this.page.show = false;
    this.page.loaded = true;
  }
}

export interface SelectEvent {
  pageNumber: number;
  multiSelect: boolean;
  active: boolean;
}

export interface PagePreviewLoadEvent {
  pageNumber: number;
  height: number;
  width: number;
}

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { max, Observable } from 'rxjs';
import { PDFWebEditAPI } from '../../../../../api/PDFWebEditAPI';
import { Page } from '../../../models/page';
import { PagePreviewLoadEvent, SelectEvent } from '../page/page.component';
import { Doc } from '../../../models/doc';
import { DndDropEvent } from 'ngx-drag-drop';

@Component({
  selector: 'pages',
  templateUrl: './pages.component.html',
  styleUrls: ['./pages.component.scss']
})
export class PagesComponent implements OnInit {

  @Input() pages!: Page[];
  @Input() document!: Doc;
  @Input() size!: Observable<string>;
  @Input() config!: PDFWebEditAPI.Config;
  @Input() resetDocumentPreview!: Observable<void>;

  @Output() onSelectionChange: EventEmitter<SelectionChangedEvent> = new EventEmitter();
  @Output() onMaxPreviewSizeChange: EventEmitter<MaxPreviewSizeChangedEvent> = new EventEmitter();
  @Output() onPageOrderChange: EventEmitter<PageOrderChangedEvent> = new EventEmitter();

  maxPageWidth = 0;
  maxPageHeight = 0;

  pageSizes: any = {};

  constructor() { }

  ngOnInit(): void {

    this.resetDocumentPreview.subscribe(() => {
      this.maxPageWidth = 0;
      this.maxPageHeight = 0;
    });

    // Set default page sizes
    for (var i = 0; i < this.pages.length; i++) {
      this.pageSizes[this.pages[i].number] = {
        height: 0,
        width: 0
      } as PageSize
    }
  }

  onDrop(event: DndDropEvent) {

    let orderOfPages: number[] = [];

    this.pages.forEach(page => {
      orderOfPages.push(page.number);
    });

    let movedElementIndex = event.data.number - 1;
    let newElementIndex: number;

    if (event.index! < event.data.number) {

      // Moved left
      newElementIndex = event.index!;

    } else {

      // Moved right
      newElementIndex = event.index! - 1;
    }

    // Only fire the event if the element moves
    if (movedElementIndex != newElementIndex) {

      // Reorder the list
      var movedElement = orderOfPages[movedElementIndex];
      orderOfPages.splice(movedElementIndex, 1);
      orderOfPages.splice(newElementIndex, 0, movedElement);

      // Emit the change event
      this.onPageOrderChange.emit({ newPageOrder: orderOfPages });
    }
  }

  pageSelected($event: SelectEvent) {

    // Deactivate all pages in the doc (unless the ctrl key is pressed - multiselect)
    if (!$event.multiSelect && !$event.rangeSelect) {
      this.pages
        .filter(pages => pages.number != $event.pageNumber)
        .forEach(p => {
          p.active = false;
        });
    } else if ($event.rangeSelect) {

      let firstSelectedPageNumber = -1;
      let lastSelectedPageNumber = -1;

      // Get first and last page number
      this.pages
        .sort(function (a, b) {
          return a.number - b.number;
        })
        .forEach(p => {
          if (p.active) {
            if (firstSelectedPageNumber < 0) {
              firstSelectedPageNumber = p.number;
            } else {
              lastSelectedPageNumber = p.number;
            }
          }
        });

      // Select every page inbetween
      if ((firstSelectedPageNumber > 0) && (lastSelectedPageNumber > 0)) {
        this.pages
          .forEach(p => {
            p.active = ((p.number >= firstSelectedPageNumber) && (p.number <= lastSelectedPageNumber));
          });
      }
    }

    this.onSelectionChange.emit({ selectedPages: this.pages.filter(page => page.active).map(page => page.number) });
  }

  registerPageSize($event: PagePreviewLoadEvent) {

    let currentPageSize = this.pageSizes[$event.pageNumber] as PageSize;

    if ((!currentPageSize) || (currentPageSize.height != $event.height) || (currentPageSize.width != $event.width)) {

      // Set the new page size
      this.pageSizes[$event.pageNumber] = {
        height: $event.height,
        width: $event.width
      } as PageSize;

      // Get the largest page sizes in the document
      this.maxPageHeight = this.getMaxPageHeight();
      this.maxPageWidth = this.getMaxPageWidth();

      // Register the largest page size
      this.onMaxPreviewSizeChange.emit({ width: this.maxPageWidth, height: this.maxPageHeight });
    }
  }

  getMaxPageHeight(): number {

    let maxHeight = 0;

    let pages = Object.keys(this.pageSizes);

    for (var i = 0; i < pages.length; i++) {

      let page = this.pageSizes[pages[i]] as PageSize;

      if (page.height > maxHeight) {
        maxHeight = page.height;
      }
    }

    return maxHeight;
  }

  getMaxPageWidth(): number {

    let maxWidth = 0;

    let pages = Object.keys(this.pageSizes);

    for (var i = 0; i < pages.length; i++) {

      let page = this.pageSizes[pages[i]] as PageSize;

      if (page.width > maxWidth) {
        maxWidth = page.width;
      }
    }

    return maxWidth;
  }
}

export interface SelectionChangedEvent {
  selectedPages: number[];
}

export interface MaxPreviewSizeChangedEvent extends PageSize {

}

export interface PageSize {
  height: number;
  width: number;
}

export interface PageOrderChangedEvent {
  newPageOrder: number[];
}

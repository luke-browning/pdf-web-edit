import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Observable } from 'rxjs';
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

  constructor() { }

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

  ngOnInit(): void {

    this.resetDocumentPreview.subscribe(() => {
      this.maxPageWidth = 0;
      this.maxPageHeight = 0;
    });
  }

  pageSelected($event: SelectEvent) {

    // Deactivate all pages in the doc (unless the ctrl key is pressed - multiselect)
    if (!$event.multiSelect) {
      this.pages
        .filter(pages => pages.number != $event.pageNumber)
        .forEach(p => {
        p.active = false;
      });
    }

    this.onSelectionChange.emit({ selectedPages: this.pages.filter(page => page.active).map(page => page.number) });
  }

  registerPageSize($event: PagePreviewLoadEvent) {

    let changed = false;

    // Keep track of the largest page dimensions to size the scrollable area
    if ($event.height > this.maxPageHeight) {
      this.maxPageHeight = $event.height;
      changed = true;
    }

    if ($event.width > this.maxPageWidth) {
      this.maxPageWidth = $event.width;
      changed = true;
    }

    if (changed) {
      this.onMaxPreviewSizeChange.emit({ width: this.maxPageWidth, height: this.maxPageHeight });
    }
  }
}

export interface SelectionChangedEvent {
  selectedPages: number[];
}

export interface MaxPreviewSizeChangedEvent {
  height: number;
  width: number;
}

export interface PageOrderChangedEvent {
  newPageOrder: number[];
}

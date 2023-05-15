import { Component, ElementRef, Input, ViewChild } from '@angular/core';
import { NavigationEnd, Router, RouterEvent } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { fromEvent } from 'rxjs';
import { debounceTime, distinctUntilChanged, filter, tap } from 'rxjs/operators';
import { PDFWebEditAPI } from '../../../../../api/PDFWebEditAPI';
import { ConfigService } from '../../../../services/config/config.service';
import { SessionService } from '../../../../services/session/session.service';
import { SettingsComponent } from '../../modals/settings/settings.component';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss']
})
export class NavMenuComponent {

  config!: PDFWebEditAPI.Config;

  // Search
  @ViewChild('search') search!: ElementRef;
  searchFocused = false;

  // Colour modes
  colourMode!: string;
  selectedColourMode!: string;

  // Sorting
  sortBy!: string;
  sortDirection!: string;

  // Previews
  previewSize!: string;

  currentDirectory = 'Loading...';

  targetDirectory = PDFWebEditAPI.TargetDirectory;

  isExpanded = false;

  constructor(private router: Router, private modalService: NgbModal, private configService: ConfigService, private sessionService: SessionService) {

    configService.getConfig().subscribe(config => {
      this.config = config!;
    });

    // Watch url for changes
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {

      switch (event.urlAfterRedirects.toLowerCase()) {
        case '/inbox':
          this.currentDirectory = 'Inbox';
          break;

        case '/outbox':
          this.currentDirectory = 'Outbox';
          break;

        case '/archive':
          this.currentDirectory = 'Archive';
          break;
      }
    });
    
    // Colour mode
    this.sessionService.colourMode.subscribe((colourMode) => this.colourMode = colourMode);
    this.sessionService.selectedColourMode.subscribe((selectedColourMode) => this.selectedColourMode = selectedColourMode);

    // Sorting
    this.sessionService.sortBy.subscribe((sortBy) => this.sortBy = sortBy);
    this.sessionService.sortDirection.subscribe((sortDirection) => this.sortDirection = sortDirection);

    // Page size
    this.sessionService.previewSize.subscribe((previewSize) => this.previewSize = previewSize);
  }

  ngOnInit() {

  }

  ngAfterViewInit() {

    // Search
    fromEvent(this.search.nativeElement, 'keyup')
      .pipe(
        filter(Boolean),
        debounceTime(0),
        distinctUntilChanged(),
        tap((text) => {
          this.sessionService.setSearch(this.search.nativeElement.value)
        })
      )
      .subscribe();
  }

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }

  editConfig() {
    const modalRef = this.modalService.open(SettingsComponent, {
      size: 'lg'
    });

    modalRef.result.then(result => {
      
    }, () => {

      // Dismissed
      return;
    });
  }

  setColourMode(colourMode: string) {
    this.sessionService.setColourMode(colourMode);
  }

  setSortBy(sortBy: string) {
    this.sessionService.setSortBy(sortBy);
  }

  setSortDirection(sortDirection: string) {
    this.sessionService.setSortDirection(sortDirection);
  }

  setPreviewSize(size: string) {
    this.sessionService.setPreviewSize(size);
  }

  clearSearch() {
    this.search.nativeElement.value = '';
    this.sessionService.setSearch('');
  }
}

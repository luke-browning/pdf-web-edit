import { Component, ElementRef, Input, ViewChild } from '@angular/core';
import { NavigationEnd, Router, RouterEvent } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { fromEvent, Observable } from 'rxjs';
import { debounceTime, distinctUntilChanged, filter, tap } from 'rxjs/operators';
import { PDFWebEditAPI } from '../../../../../api/PDFWebEditAPI';
import { ConfigService } from '../../../../services/config/config.service';
import { SessionService } from '../../../../services/session/session.service';
import { SettingsComponent } from '../../modals/settings/settings.component';
import { TranslateService } from '@ngx-translate/core';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss']
})
export class NavMenuComponent {

  @Input() config!: PDFWebEditAPI.Config | null | undefined;

  // Search
  @ViewChild('search') search!: ElementRef<HTMLInputElement>;
  searchFocused = false;

  // Colour modes
  colourMode!: string;
  selectedColourMode!: string;

  // Sorting
  sortBy!: string;
  sortByTranslated!: string;
  sortDirection!: string;
  sortDirectionTranslated!: string;

  // Previews
  previewSize!: string;

  // Language
  language!: string;

  currentDirectory = '';
  currentDirectoryTranslated = '';

  targetDirectory = PDFWebEditAPI.TargetDirectory;

  isExpanded = false;

  constructor(private router: Router, private modalService: NgbModal, private configService: ConfigService,
    private sessionService: SessionService, private translateService: TranslateService, private titleService: Title) {

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

      this.translateVariables();
    });
    
    // Colour mode
    this.sessionService.colourMode.subscribe((colourMode: string) => this.colourMode = colourMode);
    this.sessionService.selectedColourMode.subscribe((selectedColourMode: string) => this.selectedColourMode = selectedColourMode);

    // Sorting
    this.sessionService.sortBy.subscribe((sortBy: string) => this.sortBy = sortBy);
    this.sessionService.sortDirection.subscribe((sortDirection: string) => this.sortDirection = sortDirection);

    // Page size
    this.sessionService.previewSize.subscribe((previewSize: string) => this.previewSize = previewSize);

    // Translations
    this.sessionService.language.subscribe((language: string) => this.language = language);

    // When the language changes, update variables
    translateService.onLangChange.subscribe((lang) => {
      this.translateVariables();
    });
  }

  translateVariables() {

    this.translateCurrentDirectory();
    this.translateSortOptions();
  }

  translateCurrentDirectory() {

    switch (this.currentDirectory) {
      case 'Inbox':
        this.currentDirectoryTranslated = this.translateService.instant('shared.folders.inbox');
        break;

      case 'Outbox':
        this.currentDirectoryTranslated = this.translateService.instant('shared.folders.outbox');
        break;

      case 'Archive':
        this.currentDirectoryTranslated = this.translateService.instant('shared.folders.archive');
        break;
    }

    // Update the page title
    this.titleService.setTitle(this.currentDirectoryTranslated + ' - ' + this.translateService.instant('app.name'));
  }

  translateSortOptions() {

    switch (this.sortBy) {
      case 'Name':
        this.sortByTranslated = this.translateService.instant('shared.sort.by.name');
        break;

      case 'Created':
        this.sortByTranslated = this.translateService.instant('shared.sort.by.created');
        break;

      case 'Last Modified':
        this.sortByTranslated = this.translateService.instant('shared.sort.by.lastModified');
        break;
    }

    switch (this.sortDirection) {
      case 'Asc':
        this.sortDirectionTranslated = this.translateService.instant('shared.sort.direction.asc');
        break;

      case 'Desc':
        this.sortDirectionTranslated = this.translateService.instant('shared.sort.direction.desc');
        break;
    }
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
        tap(() => {
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
    this.translateSortOptions();
  }

  setSortDirection(sortDirection: string) {
    this.sessionService.setSortDirection(sortDirection);
    this.translateSortOptions();
  }

  setPreviewSize(size: string) {
    this.sessionService.setPreviewSize(size);
  }

  setLanguage(language: string) {
    this.sessionService.setLanguage(language);
  }

  clearSearch() {
    this.search.nativeElement.value = '';
    this.sessionService.setSearch('');
  }
}

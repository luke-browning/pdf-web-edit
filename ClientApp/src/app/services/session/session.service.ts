import { ApplicationRef, Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { ConfigService } from '../config/config.service';
import { TranslateService } from '@ngx-translate/core';
import { PDFWebEditAPI } from '../../../api/PDFWebEditAPI';

@Injectable({
  providedIn: 'root'
})
export class SessionService {

  // Browser dark mode
  browserDarkModeOn = false;

  // Colour mode
  private colourMode$ = new BehaviorSubject<string>('');
  colourMode = this.colourMode$.asObservable();

  // Selected colour mode - powers colorMode$
  private selectedColourMode$ = new BehaviorSubject<string>('auto');
  selectedColourMode = this.selectedColourMode$.asObservable();

  // Preview size
  private previewSize$ = new BehaviorSubject<string>('');
  previewSize = this.previewSize$.asObservable();

  // Sort by
  private sortBy$ = new BehaviorSubject<string>('');
  sortBy = this.sortBy$.asObservable();

  // Sort direction
  private sortDirection$ = new BehaviorSubject<string>('');
  sortDirection = this.sortDirection$.asObservable();

  // Search
  private search$ = new BehaviorSubject<string>('');
  search = this.search$.asObservable();

  // DocumentListRefresh
  private refreshDocuments$ = new BehaviorSubject('');
  refreshDocuments = this.refreshDocuments$.asObservable();

  // Language
  private language$ = new BehaviorSubject<string>('');
  language = this.language$.asObservable();

  // Show Files on Save As
  private showFilesOnSaveAs$ = new BehaviorSubject<boolean>(false);
  showFilesOnSaveAs = this.showFilesOnSaveAs$.asObservable();

  // Cache
  private cache = new Map<string, object>();

  constructor(private ref: ApplicationRef, private configService: ConfigService, private translate: TranslateService) {

    this.readBrowserColourMode();

    configService.getConfig().subscribe((config: PDFWebEditAPI.Config | null | undefined) => {

      // Work out what colour mode we should start with
      let defaultColourMode = config?.generalConfig.defaultColourMode;
      let localStorageColourMode = localStorage.getItem('colour_mode');

      this.setColourMode(localStorageColourMode || defaultColourMode || 'auto');

      // Work out what preview size to start with
      let defaultPreviewSize = config?.previewConfig.defaultSize;
      let localStoragePreviewSize = localStorage.getItem('preview_size');

      this.setPreviewSize(localStoragePreviewSize || defaultPreviewSize || 'medium');

      // Work out what to sort by to start with
      let defaultSortBy = config?.generalConfig.defaultSortColumn;
      let localStorageSortBy = localStorage.getItem('sort_by');

      this.setSortBy(localStorageSortBy || defaultSortBy || 'Name');

      // Work out what sort direction to start with
      let defaultSortDirection = config?.generalConfig.defaultSortDirection;
      let localStorageSortDirection = localStorage.getItem('sort_direction');

      this.setSortDirection(localStorageSortDirection || defaultSortDirection || 'Asc');

      // Get the language
      let defaultLanguage = config?.generalConfig.defaultLanguage;
      let localStorageLanguage = localStorage.getItem('language');

      this.setLanguage(localStorageLanguage || defaultLanguage || 'en');

      // Work out if we should show files to start with
      let defaultShowFilesOnSaveAs = config?.generalConfig.defaultShowFilesOnSaveAs;
      let localStorageShowFilesOnSaveAs = localStorage.getItem('show_files') == 'true';

      this.setShowFilesOnSaveAs(localStorageShowFilesOnSaveAs || defaultShowFilesOnSaveAs || false);
    });
  }

  setLanguage(language: string) {

    // This language will be used as a fallback when a translation isn't found in the current language
    this.translate.setDefaultLang('en');

    // Save the currently selected language
    this.language$.next(language);
    localStorage.setItem('language', language);

    // The lang to use, if the lang isn't available, it will use the current loader to get them
    this.translate.use(language);
  }

  setColourMode(mode: string) {

    // Save the currently selected colour mode
    this.selectedColourMode$.next(mode);
    localStorage.setItem('colour_mode', mode);

    // Update the current value
    switch (mode) {
      case 'light':
        this.colourMode$.next('light');
        break;

      case 'dark':
        this.colourMode$.next('dark');
        break;

      case 'auto':
      default:
        this.colourMode$.next(this.browserDarkModeOn ? 'dark' : 'light');
        break;
    }
  }

  setPreviewSize(size: string) {

    // Save the currently selected preview size
    this.previewSize$.next(size);
    localStorage.setItem('preview_size', size);
  }

  setSortBy(sortBy: string) {

    // Save the currently selected sort by
    this.sortBy$.next(sortBy);
    localStorage.setItem('sort_by', sortBy);
  }

  setSortDirection(sortDirection: string) {

    // Save the currently selected sort direction
    this.sortDirection$.next(sortDirection);
    localStorage.setItem('sort_direction', sortDirection);
  }

  setShowFilesOnSaveAs(showFilesOnSaveAs: boolean) {

    // Save the currently selected sort direction
    this.showFilesOnSaveAs$.next(showFilesOnSaveAs);
    localStorage.setItem('show_files', showFilesOnSaveAs ? 'true' : 'false');
  }

  setSearch(search: string) {

    // Save the currently selected sort direction
    this.search$.next(search);
  }


  refreshDocumentsNow() {
    this.refreshDocuments$.next(new Date().toISOString());
  }

  setCache(key: string, value: object) {

    // Save the key-value pair
    this.cache.set(key, value);
  }

  getCache(key: string): object | undefined {

    // Get the value for the key
    return this.cache.get(key);
  }

  /// <summary>
  /// Reads browser colour mode.
  /// </summary>
  /// <returns>
  /// The browser colour mode.
  /// </returns>
  private readBrowserColourMode() {

    // Read browser colour mode
    this.browserDarkModeOn = window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches;

    // Watch for changes to the colour scheme
    window.matchMedia("(prefers-color-scheme: dark)").addListener(e => {

      this.browserDarkModeOn = e.matches;

      // Update the colour mode if set to auto
      if (this.selectedColourMode$.value == 'auto') {
        this.setColourMode('auto');
      }

      this.ref.tick();
    });
  }
}

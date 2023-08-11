import { DOCUMENT } from '@angular/common';
import { Component, Inject, Renderer2 } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { TranslateService } from '@ngx-translate/core';
import { TourService } from 'ngx-ui-tour-ng-bootstrap';
import { PDFWebEditAPI } from '../api/PDFWebEditAPI';
import { ConfigService } from './services/config/config.service';
import { SessionService } from './services/session/session.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {

  config!: PDFWebEditAPI.Config | null | undefined;

  colourMode!: string;

  stickyHeader = false;

  constructor(private tourService: TourService, private configService: ConfigService, private sessionService: SessionService,
    private renderer: Renderer2, private translateService: TranslateService, private titleService: Title) {

    configService.getConfig().subscribe(config => {

      this.config = config;

      // Is the tour enabled?
      if (config?.generalConfig.enableTour) {

        // When the language changes, update variables
        translateService.onDefaultLangChange.subscribe((lang) => {

          // Check if the tour has been completed already
          let hasCompletedTour = (localStorage.getItem('completed_tour') || 'false') === 'true';

          if (!hasCompletedTour) {

            this.tourService.initialize([
              {
                anchorId: 'folder',
                content: this.translateService.instant('tour.folderStep.content'),
                title: this.translateService.instant('tour.folderStep.title'),
                enableBackdrop: true,
                placement: 'right-top'
              }, {
                anchorId: 'size',
                content: this.translateService.instant('tour.previewStep.content'),
                title: this.translateService.instant('tour.previewStep.title'),
                enableBackdrop: true,
                placement: 'bottom'
              }, {
                anchorId: 'order',
                content: this.translateService.instant('tour.sortStep.content'),
                title: this.translateService.instant('tour.sortStep.title'),
                enableBackdrop: true,
                placement: 'left-top'
              }
            ]);

            // Start the tour
            tourService.start();

            // Mark the tour has completed once ended
            tourService.end$.subscribe(() => {

              // Tour ended?
              localStorage.setItem('completed_tour', 'true');
            });
          }
        });
      }

      // Should we keep the header stuck to the top of the page
      this.stickyHeader = config?.headerConfig.stickyHeader || false;

      // Colour mode
      this.sessionService.colourMode.subscribe((colourMode: string) => {

        this.renderer.removeClass(document.body, 'light');
        this.renderer.removeClass(document.body, 'dark');

        this.renderer.addClass(document.body, colourMode);
      });
    });
  }
}

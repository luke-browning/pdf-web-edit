import { Component } from '@angular/core';
import { TourService } from 'ngx-ui-tour-ng-bootstrap';
import { ConfigService } from './services/config/config.service';
import { SessionService } from './services/session/session.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {

  title = 'PDF Web Edit';

  colourMode!: string;

  stickyHeader = false;

  constructor(private tourService: TourService, private configService: ConfigService, private sessionService: SessionService) {

    configService.getConfig().subscribe(config => {

      // Is the tour enabled?
      if (config?.generalConfig.enableTour) {

        // Check if the tour has been completed already
        let hasCompletedTour = (localStorage.getItem('completed_tour') || 'false') === 'true';

        if (!hasCompletedTour) {

          this.tourService.initialize([
            {
              anchorId: 'folder',
              content: 'Pick the directory you want to view all it\'s documents.',
              title: 'Current Directory',
              enableBackdrop: true,
              placement: 'right-top'
            }, {
              anchorId: 'size',
              content: 'Choose the size of your previews.',
              title: 'Preview Size',
              enableBackdrop: true,
              placement: 'bottom'
            }, {
              anchorId: 'order',
              content: 'Order your documents by name, creation or last modified date.',
              title: 'Order Documents',
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
      }

      // Should we keep the header stuck to the top of the page
      this.stickyHeader = config?.generalConfig.stickyHeader || false;
    });
  }
}

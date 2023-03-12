import { BrowserModule } from '@angular/platform-browser';
import { APP_INITIALIZER, NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { DragulaModule } from 'ng2-dragula';
import { MessageBoxComponent } from './message-box/message-box.component';
import { InputBoxComponent } from './input-box/input-box.component';
import { AutofocusDirective } from './shared/autofocus.directive';
import { DirectoryPickerComponent } from './directory-picker/directory-picker.component';
import { MergeDocmentComponent } from './merge-docment/merge-docment.component';
import { SearchPipe } from './shared/filter.pipe';
import { AppConfigService } from './services/app-config.service';
import { SettingsComponent } from './settings/settings.component';
import { TourNgBootstrapModule } from 'ngx-ui-tour-ng-bootstrap';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    MessageBoxComponent,
    InputBoxComponent,
    AutofocusDirective,
    DirectoryPickerComponent,
    MergeDocmentComponent,
    SearchPipe,
    SettingsComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'input', component: HomeComponent, pathMatch: 'full' },
      { path: 'output', component: HomeComponent, pathMatch: 'full' },
      { path: 'trash', component: HomeComponent, pathMatch: 'full' },
    ]),
    NgbModule,
    DragulaModule.forRoot(),
    TourNgBootstrapModule.forRoot(),
  ],
  providers: [
    {
      provide: APP_INITIALIZER,
      multi: true,
      deps: [AppConfigService],
      useFactory: (appConfigService: AppConfigService) => () => appConfigService.loadAppConfig()
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

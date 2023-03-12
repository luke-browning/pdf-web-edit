import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { PDFWebEditAPI } from '../../api/PDFWebEditAPI';

@Injectable({
  providedIn: 'root'
})
export class AppConfigService {

  private appConfig$ = new BehaviorSubject<PDFWebEditAPI.Config | null | undefined>(null);

  constructor(private api: PDFWebEditAPI.ConfigurationClient) { }

  loadAppConfig() {
    return this.api.getConfiguration()
      .toPromise()
      .then(config => {
        this.appConfig$.next(config);
      });
  }

  saveConfig(config: PDFWebEditAPI.Config): Promise<PDFWebEditAPI.Config | null> {
    return this.api.saveConfiguration(config).pipe(
      tap(newConfig => {
        this.appConfig$.next(newConfig);
      })
    ).toPromise();
  }

  reloadConfig(): Promise<PDFWebEditAPI.Config | null> {
    return this.api.reloadConfiguration().pipe(
      tap(config => {
        this.appConfig$.next(config);
      })
    ).toPromise();
  }

  getConfig(): Observable<PDFWebEditAPI.Config | null | undefined> {
    return this.appConfig$.asObservable();
  }
}

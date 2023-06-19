import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { PDFWebEditAPI } from '../../../api/PDFWebEditAPI';

@Injectable({
  providedIn: 'root'
})
export class ConfigService {

  private appConfig$ = new BehaviorSubject<PDFWebEditAPI.Config | null | undefined>(null);

  constructor(private api: PDFWebEditAPI.ConfigurationClient) { }

  loadAppConfig() {
    return this.api.getConfiguration().pipe(
      tap((config: PDFWebEditAPI.Config | null) => {
        this.appConfig$.next(config);
      }));
  }

  saveConfig(config: PDFWebEditAPI.Config): Observable<PDFWebEditAPI.Config | null> {
    return this.api.saveConfiguration(config).pipe(
      tap((newConfig: PDFWebEditAPI.Config | null) => {
        this.appConfig$.next(newConfig);
      })
    );
  }

  reloadConfig(): Observable<PDFWebEditAPI.Config | null> {
    return this.api.reloadConfiguration().pipe(
      tap((config: PDFWebEditAPI.Config | null) => {
        this.appConfig$.next(config);
      })
    );
  }

  getConfig(): Observable<PDFWebEditAPI.Config | null | undefined> {
    return this.appConfig$.asObservable();
  }
}

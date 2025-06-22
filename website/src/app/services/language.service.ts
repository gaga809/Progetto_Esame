import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';

@Injectable({
  providedIn: 'root',
})
export class LanguageService {
  private currentLanguage = new BehaviorSubject<string>('en');
  language$ = this.currentLanguage.asObservable();

  constructor(private translate: TranslateService) {
    const browserLang = localStorage.getItem('lang') || translate.getBrowserLang() || 'en';
    this.setLanguage(browserLang);
  }

  setLanguage(lang: string) {
    this.translate.use(lang);
    this.currentLanguage.next(lang);
    localStorage.setItem('lang', lang);
  }

  getCurrentLanguage(): string {
    return this.currentLanguage.value;
  }
}

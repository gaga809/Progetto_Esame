import { Component, OnInit, ElementRef, HostListener } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { UpperCasePipe } from '@angular/common';
import { LanguageService } from '../../services/language.service';

@Component({
  selector: 'custom-header',
  standalone: true,
  imports: [TranslateModule, UpperCasePipe],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css'],
})
export class HeaderComponent implements OnInit {
  selectedLang: string = 'en';
  showDropdown = false;

  languages = [
    { code: 'en', label: 'English' },
    { code: 'it', label: 'Italiano' },
    { code: 'ch', label: '中国人' },
    { code: 'fr', label: 'Français' },
  ];

  constructor(private languageService: LanguageService, private el: ElementRef) { }

  ngOnInit(): void {
    this.languageService.language$.subscribe((lang) => {
      this.selectedLang = lang;
    });
  }

  get selectedLanguageLabel(): string {
    const language = this.languages.find((l) => l.code === this.selectedLang);
    return language ? language.label : '';
  }

  toggleDropdown() {
    this.showDropdown = !this.showDropdown;
  }

  selectLanguage(lang: string) {
    this.languageService.setLanguage(lang);
    this.showDropdown = false;
  }

  @HostListener('document:click', ['$event'])
  onClick(event: MouseEvent) {
    const dropdownElement = this.el.nativeElement.querySelector('.custom-dropdown');
    if (!dropdownElement.contains(event.target as Node)) {
      this.showDropdown = false;
    }
  }

  play() {
    window.open('https://github.com/gaga809/Progetto_Esame', '_blank');
  }
}

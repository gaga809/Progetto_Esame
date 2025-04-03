import { Component } from '@angular/core';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { UpperCasePipe } from '@angular/common';

@Component({
  selector: 'custom-header',
  standalone: true,
  imports: [TranslateModule, UpperCasePipe],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent {
  selectedLanguage: string = 'English'; 
  selectedLanguageFlag: string = 'https://upload.wikimedia.org/wikipedia/commons/a/a4/Flag_of_the_United_States.svg'; 
  constructor(private translateService: TranslateService) {}
  dropdownVisible: boolean = false; 

  toggleDropdown() {
    this.dropdownVisible = !this.dropdownVisible;
  }

  onLanguageChange(language: string, flagUrl: string, nationality: string) {
    this.selectedLanguage = nationality;
    this.selectedLanguageFlag = flagUrl;

    this.dropdownVisible = false;

    this.translateService.use(language);
  }
}

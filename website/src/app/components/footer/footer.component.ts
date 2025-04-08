import { Component } from '@angular/core';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { UpperCasePipe } from '@angular/common';

@Component({
  selector: 'custom-footer',
  imports: [TranslateModule, UpperCasePipe],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.css'
})
export class FooterComponent {

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

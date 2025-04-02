import { Component } from '@angular/core';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'custom-header',
  standalone: true,
  imports: [TranslateModule],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent {
  constructor(private translateService: TranslateService) {}

  onLanguageChange(event: Event) {
    const selectedLanguage = (event.target as HTMLSelectElement).value;
    this.translateService.use(selectedLanguage);
  }
}

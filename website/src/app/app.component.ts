import { Component } from '@angular/core';
import { ScrollComponent } from './components/scroll/scroll.component';

@Component({
  selector: 'app-root',
  imports: [ScrollComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'website';
}

import { Component } from '@angular/core';
import { ScrollComponent } from './components/scroll/scroll.component';
import { HeaderComponent } from "./components/header/header.component";
import { VideosComponent } from "./components/videos/videos.component";

@Component({
  selector: 'app-root',
  imports: [ScrollComponent, HeaderComponent, VideosComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'website';
}

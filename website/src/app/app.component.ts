import { Component } from '@angular/core';
import { ScrollComponent } from './components/scroll/scroll.component';
import { HeaderComponent } from "./components/header/header.component";
import { VideosComponent } from "./components/videos/videos.component";
import { NewsComponent } from "./components/news/news.component";
import { FooterComponent } from "./components/footer/footer.component";

@Component({
  selector: 'app-root',
  imports: [ScrollComponent, HeaderComponent, VideosComponent, NewsComponent, FooterComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'website';
}

import { Component } from '@angular/core';
import { ScrollComponent } from "../../components/scroll/scroll.component";
import { VideosComponent } from "../../components/videos/videos.component";
import { NewsComponent } from "../../components/news/news.component";
import { FooterComponent } from "../../components/footer/footer.component";
import { HeaderComponent } from "../../components/header/header.component";

@Component({
  selector: 'app-home',
  imports: [ScrollComponent, VideosComponent, NewsComponent, FooterComponent, HeaderComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {

}

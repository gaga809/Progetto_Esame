import { UpperCasePipe, DatePipe } from '@angular/common';
import {
  AfterViewInit,
  Component,
  ElementRef,
  Renderer2,
  ViewChild,
  OnDestroy,
  OnInit
} from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { gsap } from 'gsap';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'scores',
  standalone: true,
  imports: [UpperCasePipe, TranslateModule, DatePipe],
  templateUrl: './scores.component.html',
  styleUrls: ['./scores.component.css']
})
export class ScoresComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('section', { static: false }) section!: ElementRef;
  leaderboardData: any[] = [];

  private squares: HTMLElement[] = [];

  constructor(
    private renderer: Renderer2,
    private userService: UserService
  ) { }
  showScoreDropdown = false;
  selectedScore = 1;
  selectedScoreLabel = '1';

  toggleScoreDropdown() {
    this.showScoreDropdown = !this.showScoreDropdown;
  }

  selectScore(score: number) {
    this.selectedScore = score;
    this.selectedScoreLabel = score.toString();
    this.showScoreDropdown = false;

    this.userService.getLeaderboard(undefined, undefined, this.selectedScore).subscribe({
       next: (res) => {
        this.leaderboardData = res.leaderboard;

        setTimeout(() => {
          if (this.section) {
            this.addRandomBorderRadiusToTableCells(this.section);
          }
        }, 10);
      },
      error: (err) => console.error('Errore nel recupero leaderboard', err)
    });
  }


  ngOnInit(): void {
    this.userService.getLeaderboard().subscribe({
      next: (res) => {
        this.leaderboardData = res.leaderboard;

        setTimeout(() => {
          if (this.section) {
            this.addRandomBorderRadiusToTableCells(this.section);
          }
        }, 10);
      },
      error: (err) => console.error('Errore nel recupero leaderboard', err)
    });
  }

  ngAfterViewInit(): void {
    this.addRandomSquares(this.section);
  }

  ngOnDestroy(): void {
    this.squares.forEach(square => {
      this.renderer.removeChild(document.body, square);
    });
    this.squares = [];
  }

  addRandomSquares(section: ElementRef) {
    const container = section.nativeElement.querySelector('.random-boxes');
    if (!container) return;

    const containerRect = container.getBoundingClientRect();
    const numSquares = Math.floor(Math.random() * 2) + 3;
    const colors = ['red', 'green', 'blue'];

    for (let i = 0; i < numSquares; i++) {
      const paddingX = 0.1 * containerRect.width;
      const paddingY = 0.1 * containerRect.height;
      const x = paddingX + Math.random() * (containerRect.width - 2 * paddingX);
      const y = paddingY + Math.random() * (containerRect.height - 2 * paddingY);

      const square = this.renderer.createElement('div');
      this.renderer.addClass(square, 'square');
      this.renderer.addClass(square, colors[Math.floor(Math.random() * colors.length)]);

      Object.assign(square.style, {
        position: 'absolute',
        left: `${x}px`,
        top: `${y}px`,
        width: '70px',
        height: '70px',
        zIndex: '2',
      });

      this.renderer.appendChild(container, square);
      this.squares.push(square);

      gsap.to(square, {
        x: (Math.random() * 100) - 50,
        y: (Math.random() * 100) - 50,
        duration: Math.floor(Math.random() * 10) + 6,
        repeat: -1,
        yoyo: true,
        ease: "power1.inOut",
      });
    }
  }

  addRandomBorderRadiusToTableCells(section: ElementRef) {
    const rows: HTMLElement[] = Array.from(
      section.nativeElement.querySelectorAll('table tbody tr')
    );

    const medalColors = ['#FFD700', '#C0C0C0', '#CD7F32'];
    const bgColors = ['#ff9100', '#ff0808', '#ff089c'];

    rows.forEach((row, index) => {
      const cells = Array.from(row.querySelectorAll('td, th')) as HTMLElement[];
      const bgColor = bgColors[index % bgColors.length];

      cells.forEach(cell => {
        const tl = Math.floor(Math.random() * 25) + 5;
        const tr = Math.floor(Math.random() * 25) + 5;
        const br = Math.floor(Math.random() * 25) + 5;
        const bl = Math.floor(Math.random() * 25) + 5;
        cell.style.borderRadius = `${tl}px ${tr}px ${br}px ${bl}px`;

        if (index < 3) {
          cell.style.border = `4px solid ${medalColors[index]}`;
        } else {
          cell.style.border = '';
        }

        cell.style.backgroundColor = bgColor;
      });
    });
  }
}

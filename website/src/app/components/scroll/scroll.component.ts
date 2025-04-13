import { Component, ElementRef, ViewChild, AfterViewInit, Renderer2 } from '@angular/core';
import { UpperCasePipe } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import gsap from 'gsap';
import ScrollTrigger from 'gsap/ScrollTrigger';

gsap.registerPlugin(ScrollTrigger);

@Component({
  selector: 'scroll',
  templateUrl: './scroll.component.html',
  styleUrls: ['./scroll.component.css'],
  imports: [UpperCasePipe, TranslateModule]
})
export class ScrollComponent implements AfterViewInit {
  @ViewChild('section1') section1!: ElementRef;
  @ViewChild('section2') section2!: ElementRef;
  @ViewChild('section3') section3!: ElementRef;

  constructor(private renderer: Renderer2) {}

  ngAfterViewInit() {
    [this.section1, this.section2, this.section3].forEach((section) => {
      this.addScrollTrigger(section);
      this.addRandomSquares(section); 
    });
  }

  addScrollTrigger(section: ElementRef) {
    gsap.to(section.nativeElement, {
      scrollTrigger: {
        trigger: section.nativeElement,
        start: 'top top',
        end: 'bottom top',
        pin: true, 
        scrub: 1,
        snap: 1,
        toggleActions: 'play none none reverse', 
      },
    });
  }

  addRandomSquares(section: ElementRef) {
    const container = section.nativeElement.querySelector('.random-boxes');
    if (!container) return;

    const numSquares = Math.floor(Math.random() * 2) + 3; 

    const positions: { x: number; y: number }[] = [];

    const pickEdge = () => {
      const low = 10 + Math.random() * 5;
      const high = 70 + Math.random() * 5;
      return Math.random() < 0.5 ? low : high;
    };

    const isTooClose = (x: number, y: number) => {
      return positions.some((pos) => {
        const dx = Math.abs(pos.x - x);
        const dy = Math.abs(pos.y - y);
        return dx < 5 && dy < 5;
      });
    };

    for (let i = 0; i < numSquares; i++) {
      let attempts = 0;
      let x = 0;
      let y = 0;

      do {
        x = pickEdge();
        y = pickEdge();
        attempts++;
      } while (isTooClose(x, y) && attempts < 10);

      positions.push({ x, y });

      const square = this.renderer.createElement('div');
      const colors = ['red', 'green', 'blue'];
      const randomColor = colors[Math.floor(Math.random() * colors.length)];

      this.renderer.addClass(square, 'square');
      this.renderer.addClass(square, randomColor);

      square.style.left = `${x}%`;
      square.style.top = `${y}%`;
      square.style.transform = 'translate(-50%, -50%)';

      this.renderer.appendChild(container, square);

      const randomDuration = Math.floor(Math.random() * 10) + 3;

      gsap.to(square, {
        x: `${Math.random() * 100}%`,
        y: `${Math.random() * 100}%`,
        duration: randomDuration,
        repeat: -1,
        yoyo: true,  
        ease: "power1.inOut",
      });
    }
  }
}

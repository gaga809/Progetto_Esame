import { Component, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import gsap from 'gsap';
import ScrollTrigger from 'gsap/ScrollTrigger';

gsap.registerPlugin(ScrollTrigger);

@Component({
  selector: 'scroll',
  templateUrl: './scroll.component.html',
  styleUrls: ['./scroll.component.css'],
})
export class ScrollComponent implements AfterViewInit {
  @ViewChild('section1') section1!: ElementRef;
  @ViewChild('section2') section2!: ElementRef;
  @ViewChild('section3') section3!: ElementRef;

  ngAfterViewInit() {
    this.addScrollTrigger(this.section1);
    this.addScrollTrigger(this.section2);
    this.addScrollTrigger(this.section3);
  }

  addScrollTrigger(section: ElementRef) {
    gsap.to(section.nativeElement, {
      scrollTrigger: {
        trigger: section.nativeElement,
        start: 'top top',
        end: 'bottom top',
        pin: true, 
        scrub: 2,
        snap: 1,
        toggleActions: 'play none none reverse', 
      },
    });
  }
}

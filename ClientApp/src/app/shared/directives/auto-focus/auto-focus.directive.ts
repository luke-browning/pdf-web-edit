import { AfterViewInit, Directive, ElementRef } from '@angular/core';

@Directive({
  selector: '[auto-focus]'
})
export class AutofocusDirective implements AfterViewInit {

  constructor(private el: ElementRef) {
  }

  ngAfterViewInit() {
    this.el.nativeElement.focus();
  }
}

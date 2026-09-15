import { Directive, ElementRef, inject } from '@angular/core';

/**
 * The seed data points at public placeholder services (picsum.photos,
 * pravatar.cc). Offline — or behind a restrictive network — those requests
 * fail and leave broken-image icons everywhere, so swap in a deterministic
 * gradient instead: same URL always yields the same colour.
 */
@Directive({
  selector: 'img[appFallbackImage]',
  host: {
    '(error)': 'onError()',
  },
})
export class FallbackImageDirective {
  private readonly el = inject<ElementRef<HTMLImageElement>>(ElementRef);

  onError(): void {
    const img = this.el.nativeElement;

    // Guard against a loop if the replacement itself somehow errors.
    if (img.dataset['fallbackApplied'] === 'true') {
      return;
    }
    img.dataset['fallbackApplied'] = 'true';

    const hue = this.hueFrom(img.getAttribute('src') ?? img.alt ?? '');

    // Point at a 1x1 transparent GIF rather than clearing src: a broken image
    // paints its alt text over the gradient, whereas a loaded-but-invisible
    // one leaves the gradient clean and keeps alt for assistive tech.
    img.src =
      'data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7';
    img.style.background =
      `linear-gradient(135deg, hsl(${hue} 65% 72%), hsl(${(hue + 48) % 360} 70% 55%))`;
  }

  private hueFrom(seed: string): number {
    let hash = 0;
    for (let i = 0; i < seed.length; i++) {
      hash = (hash * 31 + seed.charCodeAt(i)) % 360;
    }
    return hash;
  }
}

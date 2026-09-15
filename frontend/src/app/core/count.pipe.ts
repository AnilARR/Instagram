import { Pipe, PipeTransform } from '@angular/core';

/** Compacts large counts: 1200 -> "1.2K", 3_400_000 -> "3.4M". */
@Pipe({ name: 'compactCount' })
export class CompactCountPipe implements PipeTransform {
  transform(value: number | null | undefined): string {
    const n = value ?? 0;

    if (n < 1000) {
      return `${n}`;
    }
    if (n < 1_000_000) {
      return `${this.trim(n / 1000)}K`;
    }
    return `${this.trim(n / 1_000_000)}M`;
  }

  /** One decimal, but drop a trailing ".0" so 2.0K reads as 2K. */
  private trim(n: number): string {
    return n.toFixed(1).replace(/\.0$/, '');
  }
}

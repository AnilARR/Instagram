import { Pipe, PipeTransform } from '@angular/core';

/**
 * Renders a timestamp the way Instagram does: "4h", "3d", "2w", then a date
 * once it is older than a year.
 */
@Pipe({ name: 'timeAgo' })
export class TimeAgoPipe implements PipeTransform {
  transform(value: string | Date | null | undefined): string {
    if (!value) {
      return '';
    }

    const then = value instanceof Date ? value : new Date(value);
    if (Number.isNaN(then.getTime())) {
      return '';
    }

    const seconds = Math.floor((Date.now() - then.getTime()) / 1000);

    // Clock skew between client and server can make a fresh post look future-dated.
    if (seconds < 60) {
      return 'just now';
    }
    if (seconds < 3600) {
      return `${Math.floor(seconds / 60)}m`;
    }
    if (seconds < 86_400) {
      return `${Math.floor(seconds / 3600)}h`;
    }
    if (seconds < 604_800) {
      return `${Math.floor(seconds / 86_400)}d`;
    }
    if (seconds < 31_536_000) {
      return `${Math.floor(seconds / 604_800)}w`;
    }

    return then.toLocaleDateString(undefined, { day: 'numeric', month: 'short', year: 'numeric' });
  }
}

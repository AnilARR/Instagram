import { ChangeDetectionStrategy, Component, input } from '@angular/core';

export type IconName =
  | 'home'
  | 'home-filled'
  | 'search'
  | 'explore'
  | 'messages'
  | 'heart'
  | 'heart-filled'
  | 'create'
  | 'comment'
  | 'share'
  | 'bookmark'
  | 'bookmark-filled'
  | 'more'
  | 'verified'
  | 'grid'
  | 'close'
  | 'chevron-left'
  | 'chevron-right'
  | 'camera';

/**
 * Inline SVG icons. Kept as a switch of literal `<svg>` bodies rather than an
 * innerHTML map so the templates stay sanitiser-friendly and tree-shakeable.
 */
@Component({
  selector: 'app-icon',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '[style.line-height]': '0', '[style.display]': '"inline-flex"' },
  template: `
    <svg
      [attr.width]="size()"
      [attr.height]="size()"
      viewBox="0 0 24 24"
      [attr.fill]="filled() ? 'currentColor' : 'none'"
      stroke="currentColor"
      [attr.stroke-width]="strokeWidth()"
      stroke-linecap="round"
      stroke-linejoin="round"
      aria-hidden="true"
      focusable="false"
    >
      @switch (name()) {
        @case ('home') {
          <path d="M3 10.5 12 3l9 7.5V20a1 1 0 0 1-1 1h-5v-6H9v6H4a1 1 0 0 1-1-1z" />
        }
        @case ('home-filled') {
          <path d="M3 10.5 12 3l9 7.5V20a1 1 0 0 1-1 1h-5v-6H9v6H4a1 1 0 0 1-1-1z" fill="currentColor" />
        }
        @case ('search') {
          <circle cx="11" cy="11" r="7" />
          <path d="m20 20-3.5-3.5" />
        }
        @case ('explore') {
          <circle cx="12" cy="12" r="9.5" />
          <path d="m15.5 8.5-2 5.5-5.5 2 2-5.5z" fill="currentColor" stroke="none" />
        }
        @case ('messages') {
          <path d="M21 11.5a8.5 8.5 0 0 1-12.4 7.6L3.5 21l1.9-5.1A8.5 8.5 0 1 1 21 11.5z" />
        }
        @case ('heart') {
          <path
            d="M12 20.5s-7.5-4.6-7.5-9.4A4.6 4.6 0 0 1 12 8.2a4.6 4.6 0 0 1 7.5 2.9c0 4.8-7.5 9.4-7.5 9.4z"
          />
        }
        @case ('heart-filled') {
          <path
            d="M12 20.5s-7.5-4.6-7.5-9.4A4.6 4.6 0 0 1 12 8.2a4.6 4.6 0 0 1 7.5 2.9c0 4.8-7.5 9.4-7.5 9.4z"
            fill="currentColor"
          />
        }
        @case ('create') {
          <rect x="3" y="3" width="18" height="18" rx="4" />
          <path d="M12 8v8M8 12h8" />
        }
        @case ('comment') {
          <path d="M21 11.5a8.5 8.5 0 0 1-12.4 7.6L3.5 21l1.9-5.1A8.5 8.5 0 1 1 21 11.5z" />
        }
        @case ('share') {
          <path d="M22 3 11 14" />
          <path d="M22 3 15 21l-4-7-7-4z" />
        }
        @case ('bookmark') {
          <path d="M6 3h12a1 1 0 0 1 1 1v17l-7-5-7 5V4a1 1 0 0 1 1-1z" />
        }
        @case ('bookmark-filled') {
          <path d="M6 3h12a1 1 0 0 1 1 1v17l-7-5-7 5V4a1 1 0 0 1 1-1z" fill="currentColor" />
        }
        @case ('more') {
          <circle cx="5" cy="12" r="1.4" fill="currentColor" stroke="none" />
          <circle cx="12" cy="12" r="1.4" fill="currentColor" stroke="none" />
          <circle cx="19" cy="12" r="1.4" fill="currentColor" stroke="none" />
        }
        @case ('verified') {
          <path
            d="m12 2.5 2.2 1.6 2.7-.3 1 2.5 2.3 1.4-.7 2.6.7 2.6-2.3 1.4-1 2.5-2.7-.3L12 21.5l-2.2-1.6-2.7.3-1-2.5L3.8 16.3l.7-2.6-.7-2.6 2.3-1.4 1-2.5 2.7.3z"
            fill="currentColor"
            stroke="none"
          />
          <path d="m8.8 12.2 2.1 2.1 4.3-4.4" stroke="var(--surface, #fff)" stroke-width="2" />
        }
        @case ('grid') {
          <rect x="3" y="3" width="7" height="7" />
          <rect x="14" y="3" width="7" height="7" />
          <rect x="3" y="14" width="7" height="7" />
          <rect x="14" y="14" width="7" height="7" />
        }
        @case ('close') {
          <path d="M6 6l12 12M18 6 6 18" />
        }
        @case ('chevron-left') {
          <path d="M15 5l-7 7 7 7" />
        }
        @case ('chevron-right') {
          <path d="M9 5l7 7-7 7" />
        }
        @case ('camera') {
          <path d="M4 8h3l1.5-2h7L17 8h3a1 1 0 0 1 1 1v9a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1V9a1 1 0 0 1 1-1z" />
          <circle cx="12" cy="13.5" r="3.5" />
        }
      }
    </svg>
  `,
})
export class Icon {
  readonly name = input.required<IconName>();
  readonly size = input(24);
  readonly strokeWidth = input(1.8);

  /** Filled variants draw their own fill, so only the plain ones stay hollow. */
  readonly filled = input(false);
}

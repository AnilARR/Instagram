import {
  ChangeDetectionStrategy,
  Component,
  OnDestroy,
  computed,
  effect,
  input,
  linkedSignal,
  output,
  untracked,
} from '@angular/core';
import { FallbackImageDirective } from '../../core/fallback-image.directive';
import { Story } from '../../core/models';
import { TimeAgoPipe } from '../../core/time-ago.pipe';
import { Icon } from '../../shared/icon';

/** How long each story stays on screen before advancing. */
const STORY_DURATION_MS = 5000;

/**
 * Full-screen story overlay with the usual auto-advancing progress bars.
 * Rendered only while `stories` is non-empty; the host closes it via `closed`.
 */
@Component({
  selector: 'app-story-viewer',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Icon, TimeAgoPipe, FallbackImageDirective],
  host: {
    '(document:keydown.escape)': 'closed.emit()',
    '(document:keydown.arrowRight)': 'next()',
    '(document:keydown.arrowLeft)': 'previous()',
  },
  templateUrl: './story-viewer.html',
  styleUrl: './story-viewer.scss',
})
export class StoryViewer implements OnDestroy {
  readonly stories = input.required<Story[]>();
  readonly startIndex = input(0);

  readonly closed = output<void>();
  readonly seen = output<Story>();

  /**
   * Starts on whichever story was clicked, and resets only when that input
   * changes — a plain effect would also fire when `stories` is replaced (which
   * happens as soon as a story is marked seen) and snap back to the start.
   */
  protected readonly index = linkedSignal(() => Math.max(this.startIndex(), 0));

  protected readonly current = computed(() => this.stories()[this.index()] ?? null);

  /**
   * Keyed on the id rather than the object: marking a story seen hands back a
   * new object with the same id, and restarting the countdown for that is wrong.
   */
  private readonly currentId = computed(() => this.current()?.id ?? null);

  private timer: ReturnType<typeof setTimeout> | null = null;

  constructor() {
    effect(() => {
      if (!this.currentId()) {
        return;
      }

      // Read the story itself without subscribing to it, so only an id change
      // re-runs this.
      const story = untracked(() => this.current());
      if (story) {
        this.seen.emit(story);
      }

      this.restartTimer();
    });
  }

  next(): void {
    if (this.index() >= this.stories().length - 1) {
      this.closed.emit();
      return;
    }
    this.index.update((i) => i + 1);
  }

  previous(): void {
    this.index.update((i) => Math.max(0, i - 1));
  }

  ngOnDestroy(): void {
    this.clearTimer();
  }

  private restartTimer(): void {
    this.clearTimer();
    this.timer = setTimeout(() => this.next(), STORY_DURATION_MS);
  }

  private clearTimer(): void {
    if (this.timer !== null) {
      clearTimeout(this.timer);
      this.timer = null;
    }
  }
}

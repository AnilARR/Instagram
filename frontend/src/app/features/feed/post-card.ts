import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { CompactCountPipe } from '../../core/count.pipe';
import { FallbackImageDirective } from '../../core/fallback-image.directive';
import { Post } from '../../core/models';
import { SessionStore } from '../../core/session.store';
import { TimeAgoPipe } from '../../core/time-ago.pipe';
import { Icon } from '../../shared/icon';

@Component({
  selector: 'app-post-card',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, FormsModule, Icon, TimeAgoPipe, CompactCountPipe, FallbackImageDirective],
  templateUrl: './post-card.html',
  styleUrl: './post-card.scss',
})
export class PostCard {
  private readonly api = inject(ApiService);
  private readonly session = inject(SessionStore);

  readonly post = input.required<Post>();

  /** Local copy of the like state, so a tap feels instant. */
  private readonly likeOverride = signal<{ count: number; liked: boolean } | null>(null);

  /** Comments added in this session, appended after whatever the API sent. */
  private readonly addedComments = signal<Post['comments']>([]);

  private readonly expanded = signal(false);
  protected readonly captionExpanded = signal(false);
  protected readonly draft = signal('');
  protected readonly submitting = signal(false);
  protected readonly saved = signal(false);

  protected readonly currentUser = this.session.currentUser;

  protected readonly liked = computed(
    () => this.likeOverride()?.liked ?? this.post().likedByCurrentUser,
  );

  protected readonly likeCount = computed(
    () => this.likeOverride()?.count ?? this.post().likeCount,
  );

  protected readonly comments = computed(() => [...this.post().comments, ...this.addedComments()]);

  protected readonly commentCount = computed(
    () => this.post().commentCount + this.addedComments().length,
  );

  /**
   * The feed only ships the last couple of comments. "View all" pulls the rest
   * and swaps them in.
   */
  protected readonly hasHiddenComments = computed(
    () => !this.expanded() && this.commentCount() > this.comments().length,
  );

  protected readonly captionIsLong = computed(() => this.post().caption.length > 120);

  toggleLike(): void {
    const optimistic = {
      liked: !this.liked(),
      count: this.likeCount() + (this.liked() ? -1 : 1),
    };
    this.likeOverride.set(optimistic);

    this.api.toggleLike(this.post().id).subscribe({
      // Trust the server's count over the guess.
      next: (result) =>
        this.likeOverride.set({ count: result.likeCount, liked: result.likedByCurrentUser }),
      error: () => this.likeOverride.set(null),
    });
  }

  /** Double-tapping the photo only ever adds a like, never removes one. */
  likeFromImage(): void {
    if (!this.liked()) {
      this.toggleLike();
    }
  }

  expandComments(): void {
    this.api.getComments(this.post().id).subscribe((all) => {
      // Replace both lists with the authoritative set to avoid duplicates.
      const known = new Set(this.post().comments.map((c) => c.id));
      this.addedComments.set(all.filter((c) => !known.has(c.id)));
      this.expanded.set(true);
    });
  }

  submitComment(): void {
    const text = this.draft().trim();
    if (!text || this.submitting()) {
      return;
    }

    this.submitting.set(true);
    this.api.addComment(this.post().id, text).subscribe({
      next: (comment) => {
        this.addedComments.update((list) => [...list, comment]);
        this.draft.set('');
        this.submitting.set(false);
      },
      error: () => this.submitting.set(false),
    });
  }

  /** Bookmarking is presentation-only — there is no saved-posts endpoint. */
  toggleSaved(): void {
    this.saved.update((v) => !v);
  }
}

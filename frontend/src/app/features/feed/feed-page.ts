import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ApiService } from '../../core/api.service';
import { Post, Story } from '../../core/models';
import { PostCard } from './post-card';
import { StoriesBar } from './stories-bar';
import { StoryViewer } from './story-viewer';
import { Suggestions } from './suggestions';

const PAGE_SIZE = 4;

@Component({
  selector: 'app-feed-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [PostCard, StoriesBar, StoryViewer, Suggestions],
  templateUrl: './feed-page.html',
  styleUrl: './feed-page.scss',
})
export class FeedPage {
  private readonly api = inject(ApiService);

  protected readonly posts = signal<Post[]>([]);
  protected readonly stories = signal<Story[]>([]);
  protected readonly loading = signal(true);
  protected readonly loadingMore = signal(false);
  protected readonly hasMore = signal(false);
  protected readonly failed = signal(false);

  private page = 0;

  /** Index of the story the viewer opened on; null means the viewer is closed. */
  protected readonly viewerIndex = signal<number | null>(null);

  constructor() {
    this.loadNextPage();
    this.api.getStories().subscribe({
      next: (stories) => this.stories.set(stories),
      // Stories are decorative; a failure here should not blank the feed.
      error: () => this.stories.set([]),
    });
  }

  loadNextPage(): void {
    if (this.loadingMore()) {
      return;
    }

    const next = this.page + 1;
    this.loadingMore.set(true);

    this.api.getFeed(next, PAGE_SIZE).subscribe({
      next: (result) => {
        this.posts.update((existing) => [...existing, ...result.items]);
        this.hasMore.set(result.hasMore);
        this.page = next;
        this.loading.set(false);
        this.loadingMore.set(false);
        this.failed.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.loadingMore.set(false);
        this.failed.set(true);
      },
    });
  }

  openStory(story: Story): void {
    const index = this.stories().findIndex((s) => s.id === story.id);
    this.viewerIndex.set(index < 0 ? 0 : index);
  }

  markSeen(story: Story): void {
    if (story.seen) {
      return;
    }

    this.api.markStorySeen(story.id).subscribe(() =>
      this.stories.update((list) =>
        list.map((s) => (s.id === story.id ? { ...s, seen: true } : s)),
      ),
    );
  }

  retry(): void {
    this.failed.set(false);
    this.loading.set(true);
    this.page = 0;
    this.posts.set([]);
    this.loadNextPage();
  }
}

import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { Post } from '../../core/models';
import { Icon } from '../../shared/icon';
import { PostGrid } from '../../shared/post-grid';

@Component({
  selector: 'app-explore-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [PostGrid, Icon, FormsModule],
  templateUrl: './explore-page.html',
  styleUrl: './explore-page.scss',
})
export class ExplorePage {
  private readonly api = inject(ApiService);

  protected readonly posts = signal<Post[]>([]);
  protected readonly loading = signal(true);
  protected readonly query = signal('');

  /** Client-side filter over captions, usernames and locations. */
  protected readonly filtered = computed(() => {
    const q = this.query().trim().toLowerCase();
    if (!q) {
      return this.posts();
    }

    return this.posts().filter((post) =>
      [post.caption, post.author.username, post.author.fullName, post.location]
        .join(' ')
        .toLowerCase()
        .includes(q),
    );
  });

  constructor() {
    this.api.getExplore().subscribe({
      next: (posts) => {
        this.posts.set(posts);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}

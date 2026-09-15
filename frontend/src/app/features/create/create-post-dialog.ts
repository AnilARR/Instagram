import { ChangeDetectionStrategy, Component, inject, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { FallbackImageDirective } from '../../core/fallback-image.directive';
import { Post } from '../../core/models';
import { Icon } from '../../shared/icon';

/**
 * Composer modal. There is no file upload behind the API, so a post is created
 * from an image URL; a "surprise me" button fills in a random placeholder.
 */
@Component({
  selector: 'app-create-post-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, Icon, FallbackImageDirective],
  host: { '(document:keydown.escape)': 'closed.emit()' },
  templateUrl: './create-post-dialog.html',
  styleUrl: './create-post-dialog.scss',
})
export class CreatePostDialog {
  private readonly api = inject(ApiService);

  readonly closed = output<void>();
  readonly created = output<Post>();

  protected readonly imageUrl = signal('');
  protected readonly caption = signal('');
  protected readonly location = signal('');
  protected readonly submitting = signal(false);
  protected readonly error = signal('');

  constructor() {
    this.randomImage();
  }

  randomImage(): void {
    const seed = Math.random().toString(36).slice(2, 9);
    this.imageUrl.set(`https://picsum.photos/seed/${seed}/900/900`);
  }

  submit(): void {
    const url = this.imageUrl().trim();
    if (!url || this.submitting()) {
      return;
    }

    this.submitting.set(true);
    this.error.set('');

    this.api.createPost(url, this.caption().trim(), this.location().trim()).subscribe({
      next: (post) => {
        this.submitting.set(false);
        this.created.emit(post);
        this.closed.emit();
      },
      error: () => {
        this.submitting.set(false);
        this.error.set('Could not create the post. Is the API running?');
      },
    });
  }
}

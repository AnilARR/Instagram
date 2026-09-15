import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { CompactCountPipe } from '../core/count.pipe';
import { FallbackImageDirective } from '../core/fallback-image.directive';
import { Post } from '../core/models';
import { Icon } from './icon';

/**
 * The three-across thumbnail grid used by both the profile and explore pages,
 * with the like/comment counts revealed on hover.
 */
@Component({
  selector: 'app-post-grid',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Icon, CompactCountPipe, FallbackImageDirective],
  template: `
    <div class="grid">
      @for (post of posts(); track post.id) {
        <figure class="cell">
          <img
            appFallbackImage
            [src]="post.imageUrl"
            [alt]="post.caption || 'Post by ' + post.author.username"
            loading="lazy"
          />
          <figcaption class="overlay">
            <span><app-icon name="heart-filled" [size]="18" /> {{ post.likeCount | compactCount }}</span>
            <span><app-icon name="comment" [size]="18" /> {{ post.commentCount | compactCount }}</span>
          </figcaption>
        </figure>
      }
    </div>
  `,
  styles: `
    .grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 4px;
    }

    @media (min-width: 720px) {
      .grid {
        gap: 28px;
      }
    }

    .cell {
      position: relative;
      margin: 0;
      aspect-ratio: 1;
      background: var(--hover);
      overflow: hidden;
    }

    .cell img {
      width: 100%;
      height: 100%;
      object-fit: cover;
      display: block;
    }

    .overlay {
      position: absolute;
      inset: 0;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 22px;
      color: #fff;
      font-weight: 700;
      background: rgba(0, 0, 0, 0.35);
      opacity: 0;
      transition: opacity 0.15s ease;
    }

    .overlay span {
      display: inline-flex;
      align-items: center;
      gap: 6px;
    }

    .cell:hover .overlay,
    .cell:focus-within .overlay {
      opacity: 1;
    }

    /* Hover reveals nothing on touch, so show the counts outright. */
    @media (hover: none) {
      .overlay {
        opacity: 1;
        background: linear-gradient(to top, rgba(0, 0, 0, 0.55), transparent 55%);
        align-items: flex-end;
        justify-content: flex-start;
        padding: 8px;
        gap: 12px;
        font-size: 12px;
      }
    }
  `,
})
export class PostGrid {
  readonly posts = input.required<Post[]>();
}

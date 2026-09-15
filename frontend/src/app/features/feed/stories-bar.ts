import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { FallbackImageDirective } from '../../core/fallback-image.directive';
import { Story } from '../../core/models';

@Component({
  selector: 'app-stories-bar',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FallbackImageDirective],
  template: `
    @if (stories().length) {
      <div class="tray" role="list">
        @for (story of stories(); track story.id) {
          <button type="button" role="listitem" class="item" (click)="open.emit(story)">
            <span class="story-ring" [class.is-seen]="story.seen">
              <img
                class="avatar"
                appFallbackImage
                [src]="story.author.avatarUrl"
                [alt]=""
                width="56"
                height="56"
              />
            </span>
            <span class="name">{{ story.author.username }}</span>
          </button>
        }
      </div>
    }
  `,
  styles: `
    .tray {
      display: flex;
      gap: 14px;
      overflow-x: auto;
      padding: 14px 12px;
      margin-bottom: 20px;
      background: var(--surface);
      border: 1px solid var(--border);
      border-radius: var(--radius);
      scrollbar-width: none;
    }

    .tray::-webkit-scrollbar {
      display: none;
    }

    .item {
      background: none;
      border: none;
      padding: 0;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 6px;
      flex: none;
      width: 68px;
    }

    .avatar {
      width: 56px;
      height: 56px;
    }

    .name {
      font-size: 12px;
      max-width: 68px;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }
  `,
})
export class StoriesBar {
  readonly stories = input.required<Story[]>();

  readonly open = output<Story>();
}

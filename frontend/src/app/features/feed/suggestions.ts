import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { FallbackImageDirective } from '../../core/fallback-image.directive';
import { UserSummary } from '../../core/models';
import { SessionStore } from '../../core/session.store';
import { Icon } from '../../shared/icon';

/** The "Suggested for you" rail beside the feed on wide screens. */
@Component({
  selector: 'app-suggestions',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, Icon, FallbackImageDirective],
  templateUrl: './suggestions.html',
  styleUrl: './suggestions.scss',
})
export class Suggestions {
  private readonly api = inject(ApiService);
  private readonly session = inject(SessionStore);

  protected readonly currentUser = this.session.currentUser;
  protected readonly people = signal<UserSummary[]>([]);

  /** Usernames followed from this panel, so the button can flip to "Following". */
  protected readonly followed = signal<ReadonlySet<string>>(new Set());

  constructor() {
    this.api.getSuggestions(5).subscribe((people) => this.people.set(people));
  }

  follow(user: UserSummary): void {
    this.api.toggleFollow(user.username).subscribe((result) => {
      this.followed.update((set) => {
        const next = new Set(set);
        if (result.isFollowedByCurrentUser) {
          next.add(user.username);
        } else {
          next.delete(user.username);
        }
        return next;
      });
    });
  }
}

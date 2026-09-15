import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { switchMap, tap } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { Post, UserProfile } from '../../core/models';
import { SessionStore } from '../../core/session.store';
import { PostGrid } from '../../shared/post-grid';
import { Icon } from '../../shared/icon';
import { CompactCountPipe } from '../../core/count.pipe';
import { FallbackImageDirective } from '../../core/fallback-image.directive';

@Component({
  selector: 'app-profile-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [PostGrid, Icon, CompactCountPipe, FallbackImageDirective],
  templateUrl: './profile-page.html',
  styleUrl: './profile-page.scss',
})
export class ProfilePage {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly session = inject(SessionStore);

  protected readonly profile = signal<UserProfile | null>(null);
  protected readonly posts = signal<Post[]>([]);
  protected readonly loading = signal(true);
  protected readonly notFound = signal(false);

  constructor() {
    // Re-fetch when the username in the URL changes, not just on first load.
    this.route.paramMap
      .pipe(
        tap(() => {
          this.loading.set(true);
          this.notFound.set(false);
        }),
        switchMap((params) => {
          const username = params.get('username') ?? '';
          return this.api.getProfile(username).pipe(
            tap((profile) => this.profile.set(profile)),
            switchMap(() => this.api.getUserPosts(username)),
          );
        }),
      )
      .subscribe({
        next: (posts) => {
          this.posts.set(posts);
          this.loading.set(false);
        },
        error: () => {
          this.notFound.set(true);
          this.loading.set(false);
        },
      });
  }

  toggleFollow(): void {
    const profile = this.profile();
    if (!profile || profile.isCurrentUser) {
      return;
    }

    this.api.toggleFollow(profile.username).subscribe((result) => {
      this.profile.set({
        ...profile,
        followerCount: result.followerCount,
        isFollowedByCurrentUser: result.isFollowedByCurrentUser,
      });

      // The current user's own following count moved too.
      const me = this.session.currentUser();
      if (me) {
        this.session.currentUser.set({
          ...me,
          followingCount: me.followingCount + (result.isFollowedByCurrentUser ? 1 : -1),
        });
      }
    });
  }
}

import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { SessionStore } from './core/session.store';
import { CreatePostDialog } from './features/create/create-post-dialog';
import { NavBar } from './layout/nav-bar';

@Component({
  selector: 'app-root',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterOutlet, NavBar, CreatePostDialog],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private readonly session = inject(SessionStore);
  private readonly router = inject(Router);

  protected readonly composerOpen = signal(false);

  constructor() {
    this.session.load();
  }

  /** After sharing, land on the author's profile so the new post is visible. */
  onCreated(): void {
    const me = this.session.currentUser();
    if (me) {
      this.router.navigate(['/u', me.username]);
    }
  }
}

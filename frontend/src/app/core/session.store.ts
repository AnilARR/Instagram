import { Injectable, inject, signal } from '@angular/core';
import { ApiService } from './api.service';
import { UserProfile } from './models';

/**
 * Holds the signed-in user. There is no real authentication in this demo — the
 * API decides who "me" is — so this just caches that answer for the whole app.
 */
@Injectable({ providedIn: 'root' })
export class SessionStore {
  private readonly api = inject(ApiService);

  readonly currentUser = signal<UserProfile | null>(null);
  readonly loadFailed = signal(false);

  load(): void {
    this.api.getCurrentUser().subscribe({
      next: (user) => {
        this.currentUser.set(user);
        this.loadFailed.set(false);
      },
      error: () => this.loadFailed.set(true),
    });
  }
}

import { ChangeDetectionStrategy, Component, inject, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { FallbackImageDirective } from '../core/fallback-image.directive';
import { SessionStore } from '../core/session.store';
import { Icon } from '../shared/icon';

/**
 * The persistent left rail on desktop, which collapses to a top bar plus a
 * bottom tab bar on narrow screens — the same split Instagram uses.
 */
@Component({
  selector: 'app-nav-bar',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, RouterLinkActive, Icon, FallbackImageDirective],
  templateUrl: './nav-bar.html',
  styleUrl: './nav-bar.scss',
})
export class NavBar {
  private readonly session = inject(SessionStore);

  /** Raised when "Create" is picked, so the shell can open the composer. */
  readonly create = output<void>();

  readonly currentUser = this.session.currentUser;
}

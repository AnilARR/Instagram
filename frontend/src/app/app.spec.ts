import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';
import { routes } from './app.routes';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter(routes), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should load the current user on start-up', () => {
    TestBed.createComponent(App);
    const http = TestBed.inject(HttpTestingController);

    // The shell asks who "me" is so the nav can show an avatar.
    const request = http.expectOne('/api/users/me');
    expect(request.request.method).toBe('GET');
  });

  it('should render the brand in the navigation', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();

    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('Instagram');
  });
});

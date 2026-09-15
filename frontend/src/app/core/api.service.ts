import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  Comment,
  FollowResult,
  LikeResult,
  PagedResult,
  Post,
  Story,
  UserProfile,
  UserSummary,
} from './models';

/**
 * Thin typed wrapper over the API. Requests go to `/api`, which the Angular
 * dev server proxies to the .NET backend (see proxy.conf.json), so the browser
 * never sees a cross-origin call in development.
 */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly base = '/api';

  // ----------------------------------------------------------------- posts

  getFeed(page = 1, pageSize = 6): Observable<PagedResult<Post>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<Post>>(`${this.base}/posts`, { params });
  }

  getExplore(): Observable<Post[]> {
    return this.http.get<Post[]>(`${this.base}/posts/explore`);
  }

  getPost(id: string): Observable<Post> {
    return this.http.get<Post>(`${this.base}/posts/${id}`);
  }

  toggleLike(postId: string): Observable<LikeResult> {
    return this.http.post<LikeResult>(`${this.base}/posts/${postId}/like`, {});
  }

  getComments(postId: string): Observable<Comment[]> {
    return this.http.get<Comment[]>(`${this.base}/posts/${postId}/comments`);
  }

  addComment(postId: string, text: string): Observable<Comment> {
    return this.http.post<Comment>(`${this.base}/posts/${postId}/comments`, { text });
  }

  createPost(imageUrl: string, caption: string, location: string): Observable<Post> {
    return this.http.post<Post>(`${this.base}/posts`, { imageUrl, caption, location });
  }

  // ----------------------------------------------------------------- users

  getCurrentUser(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.base}/users/me`);
  }

  getProfile(username: string): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.base}/users/${username}`);
  }

  getUserPosts(username: string): Observable<Post[]> {
    return this.http.get<Post[]>(`${this.base}/users/${username}/posts`);
  }

  getSuggestions(limit = 5): Observable<UserSummary[]> {
    const params = new HttpParams().set('limit', limit);
    return this.http.get<UserSummary[]>(`${this.base}/users/suggestions`, { params });
  }

  toggleFollow(username: string): Observable<FollowResult> {
    return this.http.post<FollowResult>(`${this.base}/users/${username}/follow`, {});
  }

  // --------------------------------------------------------------- stories

  getStories(): Observable<Story[]> {
    return this.http.get<Story[]>(`${this.base}/stories`);
  }

  markStorySeen(id: string): Observable<Story> {
    return this.http.post<Story>(`${this.base}/stories/${id}/seen`, {});
  }
}

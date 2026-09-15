/** Mirrors the DTOs returned by Instagram.Api. */

export interface UserSummary {
  id: string;
  username: string;
  fullName: string;
  avatarUrl: string;
  isVerified: boolean;
}

export interface UserProfile extends UserSummary {
  bio: string;
  website: string;
  postCount: number;
  followerCount: number;
  followingCount: number;
  isFollowedByCurrentUser: boolean;
  isCurrentUser: boolean;
}

export interface Comment {
  id: string;
  postId: string;
  author: UserSummary;
  text: string;
  createdAt: string;
  likeCount: number;
  likedByCurrentUser: boolean;
}

export interface Post {
  id: string;
  author: UserSummary;
  imageUrl: string;
  caption: string;
  location: string;
  createdAt: string;
  likeCount: number;
  likedByCurrentUser: boolean;
  commentCount: number;
  comments: Comment[];
}

export interface Story {
  id: string;
  author: UserSummary;
  imageUrl: string;
  createdAt: string;
  seen: boolean;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  hasMore: boolean;
}

export interface LikeResult {
  postId: string;
  likeCount: number;
  likedByCurrentUser: boolean;
}

export interface FollowResult {
  userId: string;
  followerCount: number;
  isFollowedByCurrentUser: boolean;
}

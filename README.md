# Instagram Dummy

An Instagram-style demo app: an **ASP.NET Core 8 Web API** serving seeded
in-memory data, and an **Angular 21** frontend that consumes it.

There is no database, no authentication and no file upload — everything lives in
a singleton store that is re-seeded every time the API starts. It exists to be
read, run and pulled apart, not deployed.

## What's in it

**Feed** — paged post cards with optimistic likes, inline comments, a
"view all comments" expansion, double-click-to-like, caption clamping, and a
suggestions rail.

**Stories** — a scrollable tray with gradient rings for unseen stories, and a
full-screen viewer with auto-advancing progress bars and keyboard navigation.

**Profiles** — avatar, bio, post/follower/following counts, a working follow
toggle, and a hover-reveal post grid.

**Explore** — every post ranked by likes, with client-side search over captions,
names and locations.

**Composer** — create a post from an image URL (with a "surprise me" button),
caption and location.

Also: responsive down to phone width, a dark theme that follows
`prefers-color-scheme`, loading skeletons, empty and error states, and a
deterministic gradient placeholder for any image that fails to load.

## Requirements

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) `^20.19.0 || ^22.12.0 || >=24.0.0` (what Angular 21 requires)

## Running it

```bash
git clone https://github.com/AnilARR/Instagram.git
cd Instagram
```

Install the frontend dependencies once:

```bash
cd frontend && npm install && cd ..
```

Then start both halves. On macOS or Linux:

```bash
./scripts/dev.sh
```

Or run them yourself, in two terminals:

```bash
# Terminal 1 — API on http://localhost:5065
dotnet run --project backend/Instagram.Api

# Terminal 2 — Angular on http://localhost:4200
cd frontend && npm start
```

Open **http://localhost:4200**. Swagger lives at
**http://localhost:5065/swagger**.

The dev server proxies `/api` to the backend (see `frontend/proxy.conf.json`),
so the browser makes no cross-origin calls. The API also sets a CORS policy for
`localhost:4200` for when you'd rather skip the proxy.

> If `npm install` fails with `Cannot read properties of null (reading 'edgesOut')`,
> that's an npm 10.9.x bug rather than a problem with this project — retry with
> `npm install --legacy-peer-deps`, or update npm.

## Tests

```bash
dotnet test                      # 32 API tests
cd frontend && npm test           # Angular component tests
```

## Layout

```
backend/
  Instagram.Api/
    Controllers/         PostsController, UsersController, StoriesController
    Data/                DummyDataStore — the seeded in-memory "database"
    Models/              User, Post, Comment, Story
    Dtos/                what the API actually returns
    Services/            DtoMapper — models to DTOs, resolving "liked/followed by me"
    Program.cs           DI, CORS, Swagger
  Instagram.Api.Tests/   xUnit tests over the store and controllers
frontend/
  src/app/
    core/                API client, session store, pipes, image fallback
    layout/              nav rail + mobile top/bottom bars
    features/
      feed/              feed page, post card, stories bar, story viewer, suggestions
      profile/           profile page
      explore/           explore page
      create/            composer dialog
    shared/              icon set, post grid
scripts/dev.sh           runs both halves together
```

## API

All routes are under `/api`. Because there is no login, the API answers as a
fixed current user (`you`), which is what `likedByCurrentUser` and
`isFollowedByCurrentUser` are relative to.

| Method   | Route                                      | Purpose                                  |
| -------- | ------------------------------------------ | ---------------------------------------- |
| `GET`    | `/api/health`                              | Liveness probe                           |
| `GET`    | `/api/posts?page=&pageSize=`               | Paged home feed (followed users + own)   |
| `GET`    | `/api/posts/explore`                       | All posts, most-liked first              |
| `GET`    | `/api/posts/{id}`                          | One post, with every comment             |
| `POST`   | `/api/posts`                               | Create a post                            |
| `POST`   | `/api/posts/{id}/like`                     | Toggle like                              |
| `GET`    | `/api/posts/{id}/comments`                 | All comments on a post                   |
| `POST`   | `/api/posts/{id}/comments`                 | Add a comment                            |
| `DELETE` | `/api/posts/{postId}/comments/{commentId}` | Delete your own comment                  |
| `GET`    | `/api/users`                               | All users                                |
| `GET`    | `/api/users/me`                            | The current user                         |
| `GET`    | `/api/users/suggestions?limit=`            | People you don't follow yet              |
| `GET`    | `/api/users/{username}`                    | A profile with counts                    |
| `GET`    | `/api/users/{username}/posts`              | That user's posts                        |
| `POST`   | `/api/users/{username}/follow`             | Toggle follow                            |
| `GET`    | `/api/users/{username}/followers`          | Who follows them                         |
| `GET`    | `/api/users/{username}/following`          | Who they follow                          |
| `GET`    | `/api/stories`                             | Live stories, unseen first               |
| `POST`   | `/api/stories/{id}/seen`                   | Mark a story watched                     |

Likes, comments, follows and new posts are written to the in-memory store, so
they persist while the API runs and reset when it restarts.

## Notes on the dummy data

Seed images come from [picsum.photos](https://picsum.photos) and
[pravatar.cc](https://pravatar.cc). Offline, or behind a restrictive network,
those requests fail — so every `<img>` falls back to a gradient derived from its
URL, which keeps the layout intact instead of showing broken-image icons.

Seeding uses a fixed random seed, so like counts and comments are identical on
every run. The people, captions and comments are invented.

## Things you might add next

Real persistence (EF Core against SQLite or SQL Server), authentication,
actual file uploads, a post-detail route, saved posts, notifications, and
direct messages.

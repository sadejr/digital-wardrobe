# Digital Wardrobe

A free, open-source web app for keeping your clothes in a digital wardrobe. Each user has a private wardrobe where they can add photos of their clothes, organise them with categories and tags, and search and filter them.

## Status

The first version is still in development.

**Working now:**
- User accounts with registration, login and logout
- Adding, viewing, editing and deleting clothing items
- Photo upload, automatically resized and converted to WebP
- Each user can only see and change their own items and photos

**Next:** the first deployment, then tags, filtering, sorting and search.

## Tech stack

- C# and ASP.NET Core MVC (.NET 10)
- Entity Framework Core with PostgreSQL
- SkiaSharp for image processing
- Docker and Docker Compose
- GitHub Actions for automatic build and tests

## Documentation

- [Requirements](docs/requirements.md)
- [Architecture](docs/architecture.md)
- [Decisions](docs/decisions.md)
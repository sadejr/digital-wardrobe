# Architecture – Digital Wardrobe (first version)

This document describes the architecture of the first version of the Digital Wardrobe application, and the responsibilities of each part of the code. The requirements are described separately in `requirements.md`.

## 1. Overview

The application is a single ASP.NET Core MVC web application with server-rendered pages, using Bootstrap and a small amount of JavaScript for the user interface. Data is stored in PostgreSQL through Entity Framework Core, users are managed with ASP.NET Core Identity, and uploaded photos are stored as files outside the database. The application and the database run as Docker containers in production.

The code is divided into three projects with a fixed direction of dependencies, so that the rules of the application are kept separate from the database code and the web layer.

## 2. Solution structure

```
digital-wardrobe/
  docs/
    requirements.md
    architecture.md
    decisions.md
  src/
    DigitalWardrobe.Core
      Entities/       ClothingItem, Tag
      Enums/          Category, Season
      Interfaces/     IClothingItemRepository, ITagRepository, IFileStorage,
                      IImageProcessor
      Services/       ClothingItemService, TagService
      Models/         ClothingItemFilter, paged result classes
    DigitalWardrobe.Data
      AppDbContext, entity configurations, migrations
      Repositories/   ClothingItemRepository, TagRepository
      Storage/        LocalFileStorage
      Images/         image processing implementation
    DigitalWardrobe.Web
      Controllers/, Views/, ViewModels/, Program.cs
  tests/
    DigitalWardrobe.Tests
```

### Project references

```
Web  ──► Core
Web  ──► Data   (only for registering implementations in Program.cs)
Data ──► Core
Core ──► (nothing)
```

`Core` does not reference any other project and has no dependency on Entity Framework Core or ASP.NET Core. `Data` implements the interfaces defined in `Core`. `Web` uses only the interfaces from `Core` in its controllers, and references `Data` only so that the implementations can be registered in the dependency injection container. Because references between projects can only go in one direction, the compiler prevents code in `Core` from using the database directly.

## 3. Responsibilities

A request passes through the following parts:

```
Browser → Controller → Service → Repository → DbContext → PostgreSQL
```

- **Controllers** (`Web`) receive the web request, read form data, call a service and decide which page or response to return. They do not contain application rules.
- **Services** (`Core`) contain the rules of the application, such as usage limits, normalisation of colour values, ownership checks of submitted tag IDs and the order of steps when photos are saved, replaced or deleted. They use repositories and file storage only through interfaces.
- **Repositories** (`Data`) load and save entities using Entity Framework Core. They answer questions about the data but do not decide what is allowed.
- **Entities** (`Core`) are plain C# classes representing database rows. The instructions for how Entity Framework Core stores them, such as constraints, indexes and array columns, are written in `Data`.

## 4. Repository design rules

- Each entity type has its own repository with methods written for this application. A generic repository (`IRepository<T>`) is not used.
- Every repository method that reads or changes user data takes the user ID as a required parameter and includes it in the database query.
- Repository methods return finished results, such as a single entity, a list or a paged result, and never return `IQueryable`.
- Filtering, searching, sorting and paging of clothing items are handled by one repository method that accepts a `ClothingItemFilter` object defined in `Core`. The repository translates this object into an Entity Framework Core query, including the PostgreSQL array operators used for colours and seasons.

## 5. Data isolation

Data isolation between users is enforced in the following ways:

- Ownership filtering is part of every repository query (see section 4), so a method that loads user data cannot be called without stating whose data is requested.
- Every ID received from the client, including tag IDs submitted with an item form, is checked for ownership in the service before it is used.
- Requests for data that does not exist and requests for data owned by another user both return "not found", so that the application does not confirm that another user's data exists.
- Photos are not served from a public folder. They are returned only through a controller action that loads the item through the repository with the current user's ID.

## 6. File storage

Photos are stored through the `IFileStorage` interface defined in `Core`. The first version uses `LocalFileStorage`, which saves files in a folder whose path is set in the configuration. In production this folder is a Docker volume. The file name in storage is generated by the server, and only this generated key is saved in the `ImageKey` column of `ClothingItem`. Before a photo is stored, the service passes it to the `IImageProcessor` interface defined in `Core`, which resizes it to fit within 1000 × 1500 pixels, converts it to WebP and removes hidden information such as GPS location. The implementation in `Data` uses an image processing library, so `Core` stays free of outside packages. When a photo is replaced or an item is deleted, the service deletes the old file from storage. Because storage is accessed through an interface, the local implementation can later be replaced with object storage without changes to the services or controllers.

## 7. Configuration and secrets

Non-secret settings, such as the usage limits and the photo storage path, are stored in `appsettings.json`. Secrets, such as the database connection string with its password, are never committed to the repository. In development they are stored with the .NET user secrets tool, and in Docker, GitHub Actions and production they are provided as environment variables.

## 8. Environments

**Development.** Only PostgreSQL runs in Docker. The application runs directly from the IDE or with the `dotnet` command, which allows normal debugging and fast restarts. Uploaded photos are saved to a local folder that is excluded from Git.

**Production.** Docker Compose starts two containers, the application and PostgreSQL. The application container starts only after the database has passed a healthcheck, with two named volumes: one for the database files and one for the uploaded photos, so that no data is lost when a container is replaced. The same Docker Compose setup can be started locally to test the production build (NFR-18).

## 9. Testing approach

The services in `Core` are tested with unit tests that use simple fake implementations of the repository and file storage interfaces, so that application rules can be tested without a database. The repositories in `Data` are tested with integration tests against PostgreSQL. Tests that check data isolation between users are required by NFR-20. The tests run in GitHub Actions on every push and pull request.

The reasons behind the main design choices in this document are recorded in `decisions.md`.
# FoodBlog

A Blazor WebAssembly application for food blogging.

This repository contains the basic boilerplate for a Blazor WebAssembly project.

## Structure
- `FoodBlog.csproj` - project file
- `Program.cs` - application entry point
- `App.razor` - root component
- `Pages/` - razor pages
- `Shared/` - shared components and layout
- `wwwroot/` - static assets

## Requirements
- [.NET SDK 9.0 or newer](https://dotnet.microsoft.com/download)

## Running
After installing the .NET SDK, restore dependencies and launch the development server:

```bash
dotnet restore
dotnet watch run
```

The application will be available locally and reload on file changes.

## Authentication

The sample now includes a very simple login system stored in the browser's local storage. Three roles are supported:

- **Administrator** – full access to all pages
- **Editor** – can create and edit articles
- **Reader** – can only view articles and does not see the navigation menu

Use the **Login** page to choose a username and role. Logging out clears the saved credentials.

### User accounts

A default administrator account is created automatically:

- **Username:** `Admin`
- **Password:** `Admin1`

Administrators can create editor accounts from the **Create Editor** page. Regular
readers register through the **Register** page. A simple math question prevents
bot signups and a verification code must be entered on the **Verify** page
before a reader can log in.

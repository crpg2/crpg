# cRPG Project

cRPG is a mod for Mount & Blade II: Bannerlord that adds persistence (XP, gold, items, stats) to the multiplayer experience.

## Project Structure

### Main Projects

#### src/Application

The core application layer containing business logic, commands, queries, and domain models.

#### src/Common

Shared utilities and helpers used across the solution.

#### src/Domain

Domain entities and business logic.

#### src/Module.Client and src/Module.Server

Respectively, the client-side and server-side mod implementation for the mod cRPG. The client references many files
from the server.

#### src/Persistence

Database access layer with Entity Framework Core configurations and migrations.

#### src/Sdk

Common abstractions and utilities.

#### src/WebApi

REST API backend built with ASP.NET Core.

#### src/WebUI

Vue.js/Nuxt.js frontend application.

## Testing

Unit tests are located in the test/ directory with separate projects for each main part.

## Bannerlord Source Code

Decompiled source code of Bannerlord can be found using the environment variable `MB_SOURCES_PATH`. This code can be
helpful to understand the internals of the game, the API, and the breaking changes between version. Read the AGENTS.md
there for more info.

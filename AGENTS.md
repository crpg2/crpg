# cRPG Project

cRPG is a mod for Mount & Blade II: Bannerlord that adds persistence (XP, gold, items, stats) to the multiplayer experience.

Mount & Blade II: Bannerlord (aka Bannerlord) is a multiplayer, team-based, large-scale melee combat simulator.

## Project Structure

### Main Projects

#### src/Application

The core application layer for the WebAPI, containing business logic, commands (read/write), queries (read-only).
Entity Framework is used to query the PostgreSQL database.

#### src/Common

Shared utilities and helpers used across the WebAPI.

#### src/Domain

Domain entities for the WebAPI.

#### src/Persistence

Database access layer for the WebAPI with Entity Framework Core configurations and migrations.

#### src/Sdk

Common abstractions and utilities for the WebAPI.

#### src/WebApi

REST API backend built with ASP.NET Core. Controller actions usually send a Mediator message handled in the Application
project by a handler that deals with the business logic.

#### src/WebUI

Vue.js/Nuxt.js frontend application which uses the WebAPI as the backend.

#### src/Module.Client and src/Module.Server

Respectively, the client-side and server-side mod implementation for the mod cRPG for Bannerlord. The client references
many files from the server.

There are several game modes available in the cRPG mod:
- Battle: two teams fight on a battlefield with one life per player per round
- Conquest: a siege mode with one attacker team against one defender team
- Defend the Viscount (DTV): fight as a team against waves of bots
- Duel: 1v1 duel against other players
- Team Deathmatch: two teams fight with unlimited amount of lives but limited time

## Testing

Unit tests are located in the test/ directory with separate projects for each main part.

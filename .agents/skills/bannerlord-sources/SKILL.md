---
name: bannerlord-sources
description: |
    "Read Mount & Blade II: Bannerlord (aka Bannerlord) sources to understand the API, the internals, and the changes
    between versions. Use this skill if the user asks anything about Bannerlord of if you are working on the mod and
    need more data about how Bannerlord works."
---

# Bannerlord Source Code

Decompiled source code of Bannerlord can be found using the environment variable `MB_SOURCES_PATH`. This code can be
helpful to understand the internals of the game, the API, and the breaking changes between versions.

Note that code is decompiled from dlls so it doesn't look like idiomatic code and only the logic should be used as an
example but not the style.

In the git history there, you'll find one commit per version. When the user tries to understand a breaking or behavioral
change between two versions, you can use that git history to understand it. Diff between two commits can be extremely
large, so avoid commands outputting entire commits.

If the environment variable is not set, and you would like to read Bannerlord source code, notify the user.

## Classes of Interest

- `MultiplayerMissions`: starting point to start the native mods
- `MBGameMode<T>`: hooks into the game calculations

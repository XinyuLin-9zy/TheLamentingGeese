# Project Guidelines

<!-- Fill in your project's coding standards and conventions below -->
<!-- Delete sections that don't apply, add your own -->

## Naming Conventions

- Classes: PascalCase
- Methods: PascalCase
- Private fields: _camelCase with underscore prefix
- Constants: UPPER_SNAKE_CASE or PascalCase
- Scene objects: PascalCase with descriptive names

## Code Style

- Use explicit access modifiers (private, public, etc.)
- Prefer [SerializeField] private over public fields
- Add XML documentation comments on public APIs
- Keep methods under 30 lines where possible

## Project Structure

- `Assets/Scripts/` — All gameplay scripts
- `Assets/Prefabs/` — Prefab assets
- `Assets/Scenes/` — Scene files
- `Assets/Art/` — Visual assets
- `Assets/Audio/` — Sound effects and music

## Git Workflow

- Feature branches from `develop`
- PR required for merge to `main`
- Commit messages: `type: description` (feat, fix, refactor, etc.)

## Unity Version

- Target Unity version: <!-- e.g., 2022.3 LTS -->
- Render pipeline: <!-- URP / HDRP / Built-in -->

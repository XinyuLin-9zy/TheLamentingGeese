# AB Unity MCP — Project Context

This folder contains project-specific context files that are automatically
provided to AI agents when they connect via the AB Unity MCP plugin.

## How It Works

- Place `.md` files in this folder with project documentation
- Agents receive this context automatically when they connect
- Use standard filenames for recognized categories, or add custom files

## Standard Categories

| File | Purpose |
|------|---------|
| `ProjectGuidelines.md` | Coding standards, naming conventions, workflow rules |
| `Architecture.md` | System architecture, module map, tech stack |
| `GameDesign.md` | Core gameplay, mechanics, progression, balancing |
| `NetworkingGuidelines.md` | Networking architecture, sync strategy, protocols |
| `NetworkingCSP.md` | Content Security Policy, allowed endpoints, security rules |

## Custom Context

Add any additional `.md` files to the `Custom/` subfolder, or directly in this folder.
All `.md` files will be discovered and served to agents.

## Tips

- Keep files concise — agents work best with focused, relevant context
- Use clear headings and bullet points for easy parsing
- Update files as your project evolves
- This folder is designed to be version-controlled with your project

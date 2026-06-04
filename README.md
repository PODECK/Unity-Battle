# PODECK_3D

Unity project for the PODECK 3D prototype.

## Project Structure

- `Assets/_Project` contains project-owned scripts, scenes, prefabs, materials, UI, audio, and supporting assets.
- `Assets/ThirdParty` contains imported free, sample, or license-check-required assets used by the project.
- `Assets/PaidAssets` contains locally installed paid or license-restricted assets that are not committed.
- `Packages` and `ProjectSettings` are required Unity project configuration folders.
- `Library`, `Logs`, `UserSettings`, `GeneratedAssets`, `Backups`, Codex work records, and Unity AI temporary files are local-only and ignored by Git.

## Required External Assets

The following assets are not committed when they are paid, license-restricted, generated, or local-only. Install them locally before opening scenes that reference them.

| Asset                              | Expected path                                 | Status                     | Notes                                                                         |
| ---------------------------------- | --------------------------------------------- | -------------------------- | ----------------------------------------------------------------------------- |
| ToonScapes Spring Isles            | `Assets/PaidAssets/ToonScapes`                | Paid or license-restricted | Not included in Git. Import from the licensed Unity Asset Store package.      |
| Unity AI Toolkit temporary outputs | `Assets/AI Toolkit` and `GeneratedAssets`     | Local-only                 | Not required for normal collaboration. Regenerate locally if needed.          |
| Loading Effect                     | `Assets/ThirdParty/Loading Effect`            | Verify license             | Currently used by loading UI. Confirm redistribution rights before public PR. |
| Footsteps Mini Sound Pack          | `Assets/ThirdParty/Footsteps Mini Sound Pack` | Verify license             | Currently used by SFX. Confirm redistribution rights before public PR.        |
| Animated PBR Chest Demo            | `Assets/ThirdParty/Animated PBR Chest Demo`   | Verify license             | Confirm redistribution rights before public PR.                               |
| Starter Assets                     | `Assets/ThirdParty/StarterAssets`             | Unity package sample       | Used by the player controller.                                                |
| TextMesh Pro essential resources   | `Assets/ThirdParty/TextMesh Pro`              | Unity package resources    | Used by TMP UI text assets.                                                   |

## Setup

### Required Unity Settings

Before managing this Unity project with Git, confirm these settings.

- `Edit > Project Settings > Editor`
- `Version Control / Mode` -> `Visible Meta Files`
- `Asset Serialization / Mode` -> `Force Text`

These settings keep `.meta` files and Unity asset changes stable in version control.

### Git LFS

This repository uses Git LFS for large binary assets.

```bash
git lfs install
```

The binary file patterns are defined in `.gitattributes`.

### Repository Rules

- Do not commit generated folders such as `Library/`, `Temp/`, `Build/`, `Builds/`, `Logs/`, or `UserSettings/`.
- Always commit `.meta` files together with their Unity assets.
- Manage large images, models, audio, fonts, and archives through Git LFS.
- Keep the Unity version aligned with `ProjectSettings/ProjectVersion.txt`.

### WebGL Build

This project targets WebGL deployment.

- `File > Build Settings`
- Select `WebGL`
- Run `Switch Platform`

Build outputs should be managed as deployment artifacts, not committed directly to the repository.

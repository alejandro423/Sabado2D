# Platformer Game — Feature Plan

Separate 2D platformer scene built through the Unity MCP editor pipeline, delivered step by step with branches, commits, pull requests and merges on `main`.
Unity 6000.6.2f1 (URP 2D, Input System package active). Verification: editor compile with 0 console errors, structural assertions through the MCP `RunCommand` script, scene captures.

## Specs

- **S1** The game lives in its own scene `Assets/Scenes/Platformer.unity`, separate from `Assets/Scenes/SampleScene.unity`, because the user asked for "una escena aparte". It is enabled in Build Settings.
- **S2** A 2D platformer player moves left/right and jumps, using Physics2D (`Rigidbody2D`, `BoxCollider2D`) and the active Input System package.
- **S3** The level has ground and platforms with static colliders; an orthographic camera follows the player.
- **S4** Collectible coins increase a score shown on the HUD.
- **S5** A goal area shows a win message and `R` restarts the level.
- **S6** Work follows a git/GitHub flow: one branch per step, conventional commits, a pull request per step, merge into `main`; no direct commits to `main` for feature work.
- **S7** Every sprite is generated inside the project (PNG + Unity `.meta`), no external assets.

## Tasks

| ID | Specs | Route | Status |
| --- | --- | --- | --- |
| T1 | S6 | inline | done (PR #1) |
| T2 | S1, S3, S7 | inline | done (PR #2 sprites, #3 camara, #4 escena) |
| T3 | S2 | inline | done (PR #6) |
| T4 | S4 | inline | done (PR #7) |
| T5 | S5 | inline | done (PR #8) |

## Log

### L1 — user request (verbatim)

> creemos una escena con el mcp de unity, una escena aparte, pero vamos a hacer paso a paso con git y github osea tenemos que seguir los pasos para trabajar paso a paso con git y github con commits y pull request con merges, vamos a hacer un pequeño juego plataformero

### Delivery rule (user preference)

- One pull request per small group of related changes, even when it is a few lines.
- Branch names: `mijael_` + the group in Spanish (`mijael_arte`, `mijael_camara`, `mijael_jugador`, ...). The agent picks the group name.
- Commit messages: basic and in Spanish ("agregue monedas y puntaje").
- Merge with a merge commit, then delete the branch.

### L2 — T1 project sync

- PR #1 `chore(unity): sync package resolution and project settings` merged with a merge commit.

### L3 — T2 scene, camera and art

- `odd/scripts/platformer-art.cs` generates `Assets/Art/{player,ground,wall,coin,goal}.png` with sprite import settings.
- `odd/scripts/platformer-scene.cs` builds `Assets/Scenes/Platformer.unity` and verifies it (4 roots, 7 box colliders, build settings enabled).
- Defect found and fixed: `EditorBuildSettings.scenes` only updated memory, so the scene never reached `ProjectSettings/EditorBuildSettings.asset`; now written through `SerializedObject`.
- Defect found and fixed: walls reused the grass-capped ground sprite and tiled into a ladder; walls now use a dedicated brick sprite.

### Baseline

- Repo `alejandro423/Sabado2D`, branch `main`, unprotected, GitHub API reachable with the cached git credential; `gh` is not installed so the REST API is used.
- Unity console baseline: 0 errors (one unrelated `Unity.AI.Toolkit` account warning).
- Working tree had uncommitted package/project-settings changes; T1 commits them so later PRs stay scoped.

### L4 — T3 player controller (PR #6)

- `PlayerController` owns physics and `KeyboardInput` reads the keyboard, so the controller can be driven from tests.
- Play Mode test: moved 1.97 units right, jumped 0.46 units.

### L5 — T4 coins and score (PR #7)

- `Coin` triggers on the player and asks `GameManager` to add score; HUD uses a legacy `Text` with `LegacyRuntime.ttf`.
- Play Mode test: score 0 -> 1, coins 3 -> 2, HUD "Monedas: 1".

### L6 — T5 goal and restart (PR #8)

- `Goal` trigger wins the run; `GameManager` shows the win text and reloads the scene on `R`.
- Play Mode test: win text shown, restart returns to score 0 without win.

### L7 — Tooling notes

- Unity can be driven without the MCP RunCommand tool through `unity command script run --file <file> --entry <Type.Method>`; scripts live in `odd/scripts/` so they stay outside `Assets/` and trigger no domain reload.
- `unity command editor set_auto_tick --enabled true` is required to keep the editor ticking while unfocused.
- `script eval --file` runs inside Play Mode, which is how the gameplay checks were executed.

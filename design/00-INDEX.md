# CHRONOBLADE — Technical Designer Tooling Specs

Hack & slash roguelite where **time is the single unified resource**: HP, currency, and the
clock you're racing, all at once. See the concept brief for the full design (Vibe, Loop,
Camera/Controls, RPG layer, Enemy design, Tension curve) — these specs assume that context.

This is a **technical design portfolio track**: each spec below is a self-contained tool/system
a technical designer would build so that *designers* (not just programmers) can iterate on
combat, economy, encounters, and progression without touching gameplay code. Hand each spec
file to a separate agent/session — they're written to be actionable without the rest of this
conversation.

## Build order (dependency graph)

```
01-core-data-layer  (foundation — build first, everything else depends on it)
        │
        ├── 02-time-economy-balancer      (reads/simulates the data layer)
        ├── 03-combat-combo-editor        (authors CombatMoveDefinition assets)
        ├── 04-encounter-designer         (composes EnemyDefinition + room data)
        └── 05-progression-tree-editor    (authors UpgradeNodeDefinition assets)

06-runtime-debug-hud   (depends on 01 + whichever systems are live: economy, combos, encounters)
```

Specs `02`–`05` can be built **in parallel** once `01` exists — hand them to four different
agents simultaneously. `06` should go last (or to whoever finishes first, since it's mostly
read-only instrumentation).

## Files

| Spec | Tool | Portfolio angle |
|---|---|---|
| [01-core-data-layer.md](01-core-data-layer.md) | ScriptableObject data architecture | Data-driven design foundations |
| [02-time-economy-balancer.md](02-time-economy-balancer.md) | In-editor economy simulator/grapher | Systems/economy design, custom EditorWindow |
| [03-combat-combo-editor.md](03-combat-combo-editor.md) | Combo + hitbox authoring tool | Custom PropertyDrawers, timeline UI |
| [04-encounter-designer.md](04-encounter-designer.md) | Wave/room composition tool | Procedural/encounter design tooling |
| [05-progression-tree-editor.md](05-progression-tree-editor.md) | Node-based skill tree editor | GraphView, node-graph UI (flashy portfolio piece) |
| [06-runtime-debug-hud.md](06-runtime-debug-hud.md) | Live in-game debug overlay | Runtime instrumentation, telemetry |

## How to hand off to another agent

Paste the spec file's contents (or point the agent at the path) plus this line:

> "Read `00-INDEX.md` for project context (CHRONOBLADE: hack & slash roguelite, time = HP +
> currency). Implement the attached spec inside the Unity project at `<project path>`. Depend
> only on `01-core-data-layer.md`'s public API — ask before changing its contracts."

# Spec 04 — Encounter / Wave Designer

**Depends on:** `01-core-data-layer.md` (composes `EnemyDefinition` assets into encounters).
**Owner role:** Technical designer / encounter designer.

## Goal

A tool to compose and preview **encounters** — which enemies spawn, in what order, at what
pacing, per room — as data, so encounter pacing (the "fodder → tank → elite" rhythm from the
design brief) can be authored and previewed without hand-placing GameObjects per room.

## Why this matters for a portfolio

Encounter/wave design is what makes a roguelite's moment-to-moment loop actually feel
hand-crafted instead of randomly dumped. A tool that lets you preview "this room's difficulty
and expected time cost" before playtesting is a strong systems-design portfolio piece, and it
plugs directly into spec 02's balancer (an encounter here becomes a graphable "room" there).

## Scope

1. **`EncounterDefinition` (ScriptableObject)**:
   - `encounterId`, ordered `SpawnWave[]` list, where each wave has:
     - `enemies[]` (references to `EnemyDefinition` + count)
     - `spawnDelay` (time after previous wave clears, or a fixed timer)
     - `spawnPattern` enum (`AllAtOnce`, `Staggered`, `Reinforcements` — triggered at % room HP
       remaining)
   - `expectedDifficultyTier` (Easy/Medium/Hard/Elite-room) — a designer-set label, not derived,
     used for filtering/sorting in the tool.

2. **Editor window** (`Window > CHRONOBLADE > Encounter Designer`):
   - List/grid of all `EncounterDefinition` assets, filterable by difficulty tier.
   - Per-encounter summary: total enemy count, sum of `timeRewardOnKill` (best case) vs. sum of
     estimated `damageOnHit * expected hits` (worst case) — a quick "time swing range" readout
     that reuses `01`'s data without needing to open spec 02's full simulator.
   - A simple 2D top-down layout preview (since the design brief specifies a top-down/isometric
     camera): drag enemy spawn points onto a grid representing the arena, so spatial pacing
     (enemies boxing you in vs. spread out) is visible, not just a list.

3. **Send-to-balancer button** — export the selected sequence of encounters as the ordered list
   spec 02's `TimeEconomySimulator` expects, so pacing changes here can be immediately checked
   against the economy curve.

## Technical approach

- UI Toolkit `EditorWindow` with a `GridBackground`-style manipulator for the spawn-point
  layout (Unity's Shader Graph / GraphView samples are a good reference for the drag/grid
  interaction, even though this isn't a node graph).
- Keep `EncounterDefinition` → `TimeEconomySimulator` export as a plain data transform function
  so it has no dependency on spec 02's UI code, only its data types.

## Acceptance criteria

- [ ] Can author an encounter with 3 waves of mixed enemy types and staggered timing, entirely
      through the window.
- [ ] Spawn-point layout preview accurately reflects authored positions and is editable by drag.
- [ ] Difficulty tier filter works across at least 5 sample encounters.
- [ ] "Send to balancer" produces a valid input for spec 02 without manual reformatting.

## Explicitly out of scope

- Actual runtime spawning logic beyond a minimal `EncounterRunner` needed to demo the data
  (spawns from a list, respects `spawnDelay`) — full AI/navigation is separate scope.

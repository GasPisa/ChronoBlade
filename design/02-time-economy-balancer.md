# Spec 02 — Time Economy Balancing Tool

**Depends on:** `01-core-data-layer.md` (reads `TimeEconomyConfig`, `EnemyDefinition`).
**Owner role:** Technical designer.

## Goal

A custom Unity `EditorWindow` that **simulates the time economy without pressing Play** —
answers "if the player fights room composition X with these enemy definitions, do they run out
of time, and when?" This is the tool that turns "hack & slash where time is life" from a vibe
into a tuned, playable curve.

## Why this matters for a portfolio

This is the single most distinctive piece of tooling in the project — most hack & slash
portfolios show combat; almost none show the *balancing instrument* behind an unusual economy.
A graph of "time remaining over encounter count, three difficulty tunings overlaid" is a strong,
unusual portfolio image.

## Scope

1. **EditorWindow** (`Window > CHRONOBLADE > Time Economy Balancer`):
   - Select a `TimeEconomyConfig` and an ordered list of `EnemyDefinition`s (representing one
     "run" — a sequence of rooms/encounters).
   - Simulate: starting time → passive drain over an assumed room duration → time lost to
     average expected hits (configurable "player skill" hit-avoidance %) → time gained per kill
     → net time after each room.
   - Render as an inline line graph (time remaining on Y, room/encounter index on X).
2. **Scenario comparison** — run 2–3 named scenarios side by side (e.g. "cautious player,
   80% dodge rate" vs "aggressive player, 50% dodge rate") to sanity-check the curve doesn't
   trivially win or unavoidably lose either playstyle.
3. **Export** — a "Copy as CSV" button so the numbers can go into a spreadsheet or a design doc
   image without re-deriving them.
4. **Threshold flags** — visually flag on the graph where time crosses `lowTimeWarningThreshold`
   or hits zero, so a designer instantly sees "run dies at room 7 on the aggressive scenario."

## Technical approach

- `EditorWindow` + `IMGUI` or `UI Toolkit` (`UI Toolkit` preferred — reuses skills from the
  `ui-uitk` Unity skill and reads better in a portfolio screenshot).
- Simulation logic must NOT live inside the EditorWindow class — put it in a plain C# class
  (`TimeEconomySimulator`) with no Unity Editor dependency, so it's unit-testable and reusable
  if a runtime "predicted time remaining" HUD element is wanted later.
- Graph rendering: simplest is `Handles.DrawAAPolyLine` inside `OnGUI`, or a UI Toolkit
  `IMGUIContainer` doing custom draw. Don't pull in a charting package for this.

## Acceptance criteria

- [ ] Opens from the menu, no errors with zero data selected (graceful empty state).
- [ ] Simulating a 10-room sequence with 3 sample `EnemyDefinition`s produces a plausible,
      non-flat curve.
- [ ] Changing any `TimeEconomyConfig` value updates the graph on the next simulate/refresh
      without restarting the window.
- [ ] CSV export opens cleanly in a spreadsheet with correct headers.
- [ ] `TimeEconomySimulator` has at least one EditMode test proving drain math is correct.

## Explicitly out of scope

- Actually running the simulation inside Play mode against real player input — this is a
  pre-production/offline balancing tool, not a replay system.

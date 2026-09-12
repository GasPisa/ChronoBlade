# Spec 06 — Runtime Debug HUD (Time Economy Overlay)

**Depends on:** `01-core-data-layer.md` (`TimeEconomyController` events), and benefits from
whichever of specs 02–05 are live (shows their data in context, live).
**Owner role:** Technical designer / gameplay programmer.
**Build order:** Last — it's instrumentation over systems that need to exist first.

## Goal

An in-game (Play mode / dev build) debug overlay that makes the time economy's live state
legible while playtesting: current time, drain rate breakdown (passive vs. recent hits vs.
recent kills), and a rolling ledger of every `SpendTime`/reward event from
`TimeEconomyController`.

## Why this matters for a portfolio

Instrumentation/telemetry tooling is what separates "I balanced this by feel" from "I balanced
this by watching real numbers while playing" — a strong, concrete signal in a technical design
portfolio, and cheap to build once spec 01's event API exists.

## Scope

1. **Overlay UI** (UI Toolkit, toggled with a debug key, e.g. F1):
   - Big readable time-remaining readout (the actual player-facing clock element can reuse this
     same binding later — build it clean enough to double as production UI, not throwaway).
   - A small rolling log (last ~10 entries) of economy events: `"+3.2s (Fodder kill)"`,
     `"-1.5s (hit by Tank)"`, `"-8.0s (Altar: Vigor II)"` — sourced from `TimeEconomyController`'s
     `SpendTime` ledger (spec 01 requires `reason` strings for exactly this).
   - A live mini-graph of time-remaining over the last N seconds (reuse the polyline drawing
     approach from spec 02 if that's already built, for visual/code consistency).

2. **Encounter context line** (if spec 04 is live): show the current `EncounterDefinition`'s id
   and difficulty tier, so a playtester can correlate "I almost died" with "that was a Hard-tier
   room."

3. **Toggle/config** — overlay must be a no-op / fully compiled out (or at minimum invisible and
   input-inert) in a non-dev build; never ship it active by default.

## Technical approach

- UI Toolkit `UIDocument` overlay, driven entirely by subscribing to `TimeEconomyController`
  events from spec 01 — no polling, no duplicate state.
- Gate visibility behind a `#if DEVELOPMENT_BUILD || UNITY_EDITOR` compile check plus a runtime
  toggle key, so it's safe to leave in the project.

## Acceptance criteria

- [ ] Overlay toggles on/off with a key press in Play mode, no errors if toggled before any
      economy events have fired yet.
- [ ] Ledger correctly shows the last 10 events with correct sign and reason string, in order.
- [ ] Mini-graph updates live and matches the actual `TimeEconomyController` value (spot-check
      against the big readout).
- [ ] Confirmed absent/inert in a non-development build.

## Explicitly out of scope

- Persisting telemetry across sessions or sending it anywhere (analytics backend, file export)
  — this is a live playtesting aid, not a data pipeline.

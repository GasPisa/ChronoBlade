# Spec 07 — Closing the Core Loop (prototype pass)

**Status:** Implemented directly (not delegated) — small enough to finish in one pass rather
than write a handoff spec for another agent.

## Gap found after first playtest

The first playable build was an infinite survival timer: enemies spawn forever, kill = +time,
hit = -time, until the clock hits zero. That's the *combat moment*, but not the loop from the
original design brief:

```
enter room → combat → clear → altar (spend time to level up) → push on (harder) or bank & end
                                                                         → time hits 0 → dead
```

Nothing enforced discrete rooms, nothing let the player spend time deliberately at a choice
point, and there was no way to end a run on your own terms (bank a good result) instead of only
dying. That's "no está el loop."

## What closes it (minimum, not the full spec-05 tree editor)

1. **Waves instead of continuous spawning** — `WaveManager` spawns a fixed, escalating batch
   per wave (`3 + waveNumber` enemies, tougher mix at higher waves) and reports back when the
   batch is cleared, instead of a timer that never stops.
2. **Altar choice point** — `RunManager` pauses combat on wave-clear, offers 3 random
   `UpgradeNodeDefinition` picks (reusing spec 01's data types directly — no new asset type).
   Picking one pays its `timeCost` and applies its `statModifiers` via `PlayerRuntimeStats`.
3. **Push on vs. bank** — after picking an upgrade, the player chooses `C` (continue to a
   harder wave) or `B` (bank the run and end it on a high note) — the actual risk/reward
   decision the design brief describes, not just an inevitable death spiral.
4. **Stats actually wired to gameplay** — `PlayerRuntimeStats` existed since spec 01 but wasn't
   read anywhere. Now: Vigor extends `TimeEconomyController`'s capacity, Siega multiplies
   `RegisterKill` rewards, Premura multiplies player move speed.
5. **A real end state** — death (time hits 0) and a banked run both reach `RunPhase.Complete`
   with a summary (waves survived, kills, banked or not) and `R` to restart.

## Explicitly still out of scope (belongs to the specs already written)

- Real encounter authoring / spatial layout → **spec 04** (encounter designer).
- Balancing the actual numbers against a real curve → **spec 02** (economy balancer).
- A visual tree editor for authoring upgrade nodes → **spec 05** — this pass hand-authors 3
  sample nodes (Vigor/Siega/Premura, one each) as placeholders for that tool to manage later.
- Combo/hitbox depth for the attack itself → **spec 03**.

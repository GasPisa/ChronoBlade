# Spec 01 — Core Data Layer (ScriptableObject Architecture)

**Status:** Build first — every other spec depends on this.
**Owner role:** Technical designer / gameplay systems programmer.

## Goal

Define the data contracts for CHRONOBLADE's central mechanic: time as HP + currency. Everything
downstream (combat, encounters, progression, balancing tools) reads/writes through these types.
Get this right and every other tool in this project becomes "fill in a ScriptableObject," not
"write code."

## Why this matters for a portfolio

Data-driven architecture is the single most-asked-about skill in technical design interviews:
"show me a system where a designer changed a number and never touched code." This is that piece.

## Scope

1. **`TimeEconomyConfig` (ScriptableObject)** — the tunable knobs for the whole game:
   - `startingTimeCapacity` (float, seconds)
   - `passiveDrainPerSecond` (float — the "even standing still" drain)
   - `hitTimeCost` (curve or per-enemy-type override, see `EnemyDefinition`)
   - `lowTimeWarningThreshold` (float, for UI/audio stingers)

2. **`EnemyDefinition` (ScriptableObject)**:
   - `enemyId`, `displayName`, `archetype` enum (`Fodder`, `Tank`, `Fast`, `Elite`)
   - `maxHealth`, `moveSpeed`, `damageOnHit` (→ time cost to player)
   - `timeRewardOnKill` (float) — the risk/reward number the balancer (spec 02) will graph
   - `lootTable` (reference to Time Fragment drop data, elites only)

3. **`StatBlock` / `PlayerRuntimeStats`**:
   - Base + modified values for **Vigor** (max time capacity), **Siega** (time-per-kill
     multiplier), **Premura** (move/attack speed multiplier)
   - A clean modifier-stacking API (`AddModifier(source, stat, value, type: Flat|Percent)`,
     `RemoveModifiersFromSource(source)`) so upgrades (spec 05) and buffs can layer without
     fighting each other.

4. **`UpgradeNodeDefinition`** (consumed by spec 05, defined here for the shared contract):
   - `nodeId`, `displayName`, `timeCost`, `statModifiers[]`, `prerequisiteNodeIds[]`

5. **A single `TimeEconomyController` MonoBehaviour** (thin — logic only, no tuning values
   hardcoded) that owns the live time value at runtime, exposes:
   - `event Action<float> OnTimeChanged`
   - `event Action OnTimeDepleted`
   - `void SpendTime(float amount, string reason)` — funnel everything through here so the
     debug HUD (spec 06) can log a full ledger.

## Technical approach

- Pure C# + `ScriptableObject`, no third-party packages required.
- Use Unity's `CreateAssetMenu` attributes so designers can right-click → Create → CHRONOBLADE →
  `EnemyDefinition` etc.
- Keep `TimeEconomyController` decoupled from Unity's `Update()` tick rate assumptions — expose
  drain as a rate, not a per-frame decrement, so the balancer tool (spec 02) can simulate it
  outside Play mode.

## Acceptance criteria

- [ ] All four ScriptableObject types exist with `CreateAssetMenu`, serialize cleanly, and
      round-trip through the Inspector without data loss.
- [ ] `TimeEconomyController` fires `OnTimeChanged`/`OnTimeDepleted` correctly under unit test
      (no Play mode required) for: passive drain, hit cost, kill reward, altar spend.
- [ ] At least 3 sample `EnemyDefinition` assets exist (one per non-Elite archetype) so spec 02
      and 04 have real data to point at immediately.
- [ ] No other spec's tool needs to touch these files to add new content — only new asset
      instances.

## Explicitly out of scope

- Actual combat animation/hitbox execution (spec 03).
- Any Editor tooling UI (specs 02, 04, 05 build the UI on top of this).

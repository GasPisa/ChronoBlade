# Spec 03 — Combat Combo & Hitbox Authoring Tool

**Depends on:** `01-core-data-layer.md` (uses `StatBlock` modifiers for Premura/attack speed).
**Owner role:** Technical designer / combat designer.

## Goal

A data-driven way to author melee combos (light/heavy chains, cancel windows, hitbox
active-frames) as ScriptableObject assets with a custom Inspector — so combat feel can be tuned
by adjusting numbers/curves in the Editor, not by editing animation event code.

## Why this matters for a portfolio

Combat feel tuning is the highest-value, most-scrutinized skill in a hack & slash portfolio.
Showing the *authoring tool*, not just the final combat, demonstrates you can build the pipeline
other designers would use — a step above "I scripted one combo."

## Scope

1. **`CombatMoveDefinition` (ScriptableObject)**:
   - `moveId`, `inputType` (Light/Heavy), `animationClip` reference
   - `startupFrames`, `activeFrames`, `recoveryFrames` (or time-based equivalents)
   - `hitboxShape` (capsule/box, offset, size) — authored per move
   - `cancelWindow` (frame range during which the next input buffers into the following combo
     step instead of being dropped)
   - `damage`, `nextMoveInChain` (reference to the next `CombatMoveDefinition`, or null to end
     the combo and return to idle)

2. **Custom Inspector / Editor window** with a **visual timeline** for one move: a horizontal
   bar showing startup/active/recovery/cancel-window segments, draggable to adjust frame ranges,
   with the hitbox shape drawn as a gizmo overlay in the Scene view at the authored offset.

3. **Combo chain visualizer** — a simple node/list view showing the full chain
   (Light → Light → Heavy → …) so a designer can see the whole combo tree for one weapon at a
   glance, not just one move at a time.

## Technical approach

- `CustomEditor` + `CustomPropertyDrawer` for the timeline widget (IMGUI is fine here — this is
  a classic use case for `Handles`/`GUI.Box` frame-bar drawing).
- Hitbox gizmo: `OnSceneGUI` in the CustomEditor, drawing at the object's transform + authored
  offset so the shape is visible without entering Play mode.
- Keep move **data** (this spec) fully separate from move **execution** (a runtime
  `ComboExecutor` MonoBehaviour that reads a `CombatMoveDefinition` and drives animation/hitbox
  activation) — the execution half is normal gameplay code, not part of this tooling spec, but
  needs to exist for the tool to be demoable. Build a minimal executor if one doesn't exist yet.

## Acceptance criteria

- [ ] Can author a 3-hit combo (Light-Light-Heavy) as three `CombatMoveDefinition` assets
      chained via `nextMoveInChain`, entirely through the Inspector.
- [ ] Timeline widget accurately reflects and lets you edit startup/active/recovery/cancel
      values, with clear visual boundaries between segments.
- [ ] Hitbox gizmo appears at the correct authored position in the Scene view for the selected
      move.
- [ ] A minimal runtime executor can play the authored combo against a dummy enemy and register
      hits only during the active-frame window.

## Explicitly out of scope

- Full animation blending/IK — assume pre-made animation clips exist or use capsule
  placeholders; this tool is about combo/hitbox data, not animation authoring.

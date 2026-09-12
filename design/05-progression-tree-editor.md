# Spec 05 — Progression Tree (Altar Upgrade) Editor

**Depends on:** `01-core-data-layer.md` (`UpgradeNodeDefinition`, `StatBlock` modifier API).
**Owner role:** Technical designer.

## Goal

A node-graph editor (Unity `GraphView`) for authoring the Vigor/Siega/Premura upgrade tree that
altars offer — visually laying out nodes, prerequisites, and time costs instead of hand-wiring
`UpgradeNodeDefinition` references in a flat list.

## Why this matters for a portfolio

`GraphView`-based tooling (the same API behind Shader Graph and Unity's own visual scripting) is
one of the more advanced, resume-visible skills in Unity technical design — a node-graph
screenshot reads immediately as "serious tooling" to anyone reviewing a portfolio.

## Scope

1. **`GraphView`-based EditorWindow** (`Window > CHRONOBLADE > Progression Tree Editor`):
   - Nodes represent `UpgradeNodeDefinition` assets (one node per upgrade: name, time cost,
     stat modifier, icon placeholder).
   - Edges represent prerequisites — dragging an edge from node A to node B sets B's
     `prerequisiteNodeIds` to include A.
   - Three visually grouped lanes/columns, one per stat branch (Vigor / Siega / Premura), so the
     tree's shape communicates build identity at a glance.
   - Node inspector panel (side panel or popup) to edit `timeCost` and `statModifiers` without
     leaving the graph.

2. **Validation pass** — a button that walks the graph and flags:
   - Cycles in prerequisites (invalid, must error clearly).
   - Unreachable nodes (no path from any zero-prerequisite root).
   - Nodes with no `displayName` or zero `timeCost` (likely authoring mistakes).

3. **Save/load** — graph layout (node positions) persisted separately from the actual
   `UpgradeNodeDefinition` data (e.g. a small companion asset or `EditorPrefs`/JSON keyed by
   node id), so re-opening the tool restores the visual layout instead of auto-arranging nodes
   every time.

## Technical approach

- `UnityEditor.Experimental.GraphView` (or non-experimental namespace depending on Editor
  version — check current API name via the `unity-cli` / Editor docs at build time).
- Each `Node` subclass wraps one `UpgradeNodeDefinition`; edges are purely a graph-editing
  affordance that write back into `prerequisiteNodeIds` on save — don't invent a second source
  of truth for the dependency graph.
- Reuse Shader Graph's public samples as a structural reference for node/edge/port setup; don't
  copy proprietary code, just the general GraphView patterns.

## Acceptance criteria

- [ ] Can create, connect, and reposition at least 6 nodes across the 3 stat lanes, save, close
      the window, and reopen with the same layout and connections intact.
- [ ] Cycle detection correctly flags a deliberately-introduced circular prerequisite.
- [ ] Unreachable-node detection correctly flags a node with a prerequisite that doesn't exist
      in the graph.
- [ ] Editing `timeCost`/`statModifiers` in the node inspector panel writes back to the
      underlying `UpgradeNodeDefinition` asset (visible in the normal Unity Inspector too).

## Explicitly out of scope

- In-game altar UI/UX for the player to actually purchase upgrades — this tool is for
  *authoring* the tree, not the runtime shop screen.

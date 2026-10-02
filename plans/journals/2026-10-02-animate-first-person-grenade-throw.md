---
title: Animate first-person grenade throw
date: 2026-10-02
summary: Added a procedural left-hand throw animation that releases the physics grenade at the correct point in the motion.
---

# Animate first-person grenade throw

## What changed

Added `GrenadeThrowAnimation` to build and animate a small first-person armored left hand. Pressing `G` now moves the hand from off-screen into a windup pose, swings toward a release pose, spawns the physical grenade from the held model's world position, and then recovers out of frame.

`GrenadeThrower.TryThrow` now starts the animation rather than spawning the projectile immediately. The animation prevents overlapping throws. `GrenadeVisualFactory` centralizes the grenade model so the held grenade and thrown projectile share the same appearance.

## Decision

The project has no humanoid character rig or Animator controller, so the animation is procedural and camera-relative. This keeps the feature self-contained and compatible with stale scene backups because `GrenadeThrower` creates the animation component at runtime when needed.

## Verification

Unity `6000.0.58f2` validated the scene without compiler warnings or errors. A Play Mode smoke test observed the hand become visible, move between poses, release a Rigidbody grenade after a delay, recover out of frame, and preserve explosion VFX, audio, and target damage. It returned `RUNTIME_GRENADE_HAND_ANIMATION_TEST_PASSED`, including through Unity's backup-scene restore path.

> Historical work record — not durable authority. Prefer docs/specs/ADRs for current decisions.

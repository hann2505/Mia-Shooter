---
title: Add throwable grenade and impact explosion
date: 2026-10-02
summary: "Added a physics grenade with impact-triggered VFX, 3D audio, blast force, target damage, and Unity Play Mode verification."
---

# Add throwable grenade and impact explosion

## What happened

The demo needed a throwable grenade that visibly falls through the 3D scene and explodes on impact. The existing runtime VFX helpers, target damage API, camera shake, and explosion audio provided the integration points.

## Implementation

Added `GrenadeThrower` with the `G` input, runtime scene-backup installation, a visible procedural grenade, Rigidbody launch velocity, continuous collision detection, and a short throw cooldown. Added `GrenadeProjectile` to explode on the first collision, play spatial audio, apply blast force, damage each nearby target once, shake the camera, and destroy the projectile.

Extended `VfxUtility` with layered fire, sparks, smoke, and a timed explosion light. Updated the scene builder, validator, HUD, scene, audio resources, and Vietnamese demo guide.

## Verification

Unity `6000.0.58f2` rebuilt the demo scene successfully. The runtime smoke test verified the grenade component and audio resource, created a Rigidbody projectile, forced a collision near a target, observed explosion VFX and 3D audio, confirmed projectile destruction, and confirmed target destruction. It returned `RUNTIME_GRENADE_TEST_PASSED`. The scene validator also completed successfully.

## Notes

The first test harness failed to compile because a coroutine yielded inside `try/catch`; the harness was corrected without changing gameplay code. A later damage assertion initially failed because the test teleported an interpolated Rigidbody through its Transform instead of `Rigidbody.position`; instrumentation confirmed the grenade was still exploding at its original location. Correcting the test placement proved the blast damage behavior.

> Historical work record — not durable authority. Prefer docs/specs/ADRs for current decisions.

---
title: Use dedicated MP3 audio for frag and smoke grenades
date: 2026-10-02
summary: Mapped bomb-explosion.mp3 to fragmentation impacts and smoke.mp3 to smoke grenade impacts with 3D playback and runtime fallback.
---

# Use dedicated MP3 audio for frag and smoke grenades

## What changed

The user supplied `Assets/Audio/bomb-explosion.mp3` and `Assets/Audio/smoke.mp3`. Valid Unity AudioImporter metadata was added because both source MP3 files lacked `.meta` files.

`GrenadeThrower` now stores separate fragmentation and smoke clips and selects the correct one when creating the projectile. The current scene and scene builder reference the original files directly. Identical copies under `Assets/Resources/Audio` provide runtime fallback for stale backup scenes or auto-installed grenade components.

`GrenadeProjectile` continues to create a spatial 3D AudioSource at the impact position, but now plays the clip selected for its grenade type.

## Verification

FFprobe confirmed both inputs are MP3: smoke is 5.112 seconds stereo at 48 kHz, while bomb explosion is 5.042 seconds mono at 44.1 kHz. Unity `6000.0.58f2` validated the scene. The Play Mode test verified the configured clip paths and the impact AudioSources for both grenade types, then returned `RUNTIME_GRENADE_EQUIP_FRAG_SHOCKWAVE_SMOKE_TEST_PASSED` without compiler warnings, runtime exceptions, or invalid GUIDs.

> Historical work record — not durable authority. Prefer docs/specs/ADRs for current decisions.

---
title: Unity sound and VFX demo
date: 2026-10-01
summary: Built and validated a self-contained Unity 6 shooting gallery demonstrating 3D audio and runtime visual effects.
---

# Unity sound and VFX demo

## What happened

The Unity 6 project began with no Assets. A complete shooting-gallery scene was generated at `Assets/Scenes/SoundVfxDemo.unity`, with FPS controls, six targets, scoring, spatial gun/impact/explosion/footstep audio, ambience, muzzle flash, tracers, impact sparks, camera shake, target explosions, fragments, lighting, and fog.

Five original WAV clips were synthesized locally with FFmpeg. `SoundVfxDemoBuilder` can rebuild and validate the scene, and the Vietnamese usage guide lives at `docs/huong-dan-demo-am-thanh-vfx.md`.

## Verification

Unity `6000.0.58f2` compiled the project in a temporary isolated copy, built the scene, and ran `MiaShooterEditor.SoundVfxDemoBuilder.ValidateDemoScene`. Validation confirmed the player systems, six targets, required audio sources, and all five imported audio clips. A scoped simplification pass was followed by a second successful Unity compile and validation.

## Decision

The demo uses only built-in Unity modules and generated audio so it remains self-contained and requires no Asset Store downloads.

## Next steps

Open `Assets/Scenes/SoundVfxDemo.unity` and press Play. Rebuild or validate it from the `Tools > Mia Shooter` menu when needed.

> Historical work record — not durable authority. Prefer docs/specs/ADRs for current decisions.

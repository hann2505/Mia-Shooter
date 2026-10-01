---
title: Use MP3 gameplay audio and add landing sound
date: 2026-10-01
summary: "Switched gunshot, reload, and footsteps to MP3 and added fall-aware landing audio with runtime verification."
---

# Use MP3 gameplay audio and add landing sound

## What changed

The scene builder now loads `gunshot.mp3`, `reload.mp3`, and `footstep.mp3`, and assigns the new `land.mp3` clip to `FirstPersonController`. Landing detection records the fastest downward velocity while airborne and only plays the landing one-shot after crossing a minimum fall speed, avoiding noise on small steps.

## Asset repair

The four supplied MP3 files initially had empty `.meta` files. Unity ignored them and reported invalid GUIDs. Valid Unity 6 AudioImporter metadata and unique GUIDs were added before rebuilding the scene.

## Verification

FFprobe confirmed all four files use the MP3 codec. Unity `6000.0.58f2` rebuilt and validated the scene. A Play Mode smoke test observed the player become airborne, land, and play the movement AudioSource; it returned `RUNTIME_LANDING_AUDIO_TEST_PASSED` with no exception or invalid-GUID warning.

## Runtime audio follow-up

The open Editor later kept restoring `Temp/__Backupscenes/0.backup`. That backup retained audio references from before the repaired MP3 metadata, so changing only the scene asset did not change the sound heard in the running game. The four gameplay MP3 files now also live under `Assets/Resources/Audio`, and both controllers reload them during `Awake`; serialized scene references remain a fallback.

A fresh Unity 6 isolated Play Mode test deliberately passed through the same backup-scene restore path and verified the runtime asset path for gunshot, reload, footsteps, and landing. It returned `RUNTIME_MP3_AUDIO_TEST_PASSED`.

> Historical work record — not durable authority. Prefer docs/specs/ADRs for current decisions.

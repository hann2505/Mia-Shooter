---
title: Unity jump reload and blaster upgrade
date: 2026-10-01
summary: "Added grounded jumping, a 12-round animated reload system, and a detailed energy blaster model to the Unity demo."
---

# Unity jump reload and blaster upgrade

## What happened

The demo now supports grounded jumping through the existing Input Manager `Jump` action. The weapon uses a 12-round magazine, manual reload with `R`, automatic reload when empty, HUD ammo state, recoil, a magazine removal/insertion animation, an energy pulse, and a locally generated mechanical reload sound.

The original block gun was replaced by a multi-part blaster with metal receiver, side armor, rail, sights, grip, trigger, barrel, muzzle shroud, glowing energy core, side cells, and animated magazine.

## Verification

Unity `6000.0.58f2` compiled the updated project, rebuilt `Assets/Scenes/SoundVfxDemo.unity`, and passed `ValidateDemoScene`. Validation confirmed six targets, gameplay systems, required audio assets including `reload.wav`, and the detailed blaster renderer count. Serialized scene references for weapon root, magazine, reload clip, and magazine size were also checked.

## Next steps

Open the scene and press Play. Use `Space` to jump, `R` to reload, or fire all 12 rounds to trigger automatic reload.

> Historical work record — not durable authority. Prefer docs/specs/ADRs for current decisions.

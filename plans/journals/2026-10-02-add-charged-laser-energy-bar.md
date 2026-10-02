---
title: Add charged laser energy bar
date: 2026-10-02
summary: Added a gun-mounted laser charge meter that unlocks after five target destructions and drains while firing.
---

# Add charged laser energy bar

## What happened
Implemented the charged laser feature for the Unity sound/VFX demo. Destroyed targets now emit a score/target event, and the weapon listens for that event to charge a visible energy bar on the gun by 20% per target.

## Changes
- Added target-destroyed notification in `Assets/Scripts/DemoGameManager.cs`.
- Added charged laser state, Fire2 input, beam rendering, damage pulses, and runtime-created gun energy bar in `Assets/Scripts/WeaponController.cs`.
- Updated the HUD to show laser controls, charge percentage, ready state, and firing state.
- Updated `docs/huong-dan-demo-am-thanh-vfx.md` with the new 5-target laser demo flow.

## Validation
Validated the copied Unity project with `MiaShooterEditor.SoundVfxDemoBuilder.ValidateDemoScene`. Ran a temporary Play Mode probe in `/tmp/mia-shooter-grenade-test.ESUqOJ` that confirmed the laser stays locked before 5 target destructions, unlocks at full charge, fires continuously, shrinks the gun bar, drains energy, and locks again when empty.

## Next steps
Manually preview in the Unity editor if visual placement of the bar needs art tuning on a specific display/camera FOV.

> Historical work record — not durable authority. Prefer docs/specs/ADRs for current decisions.

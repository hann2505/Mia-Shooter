---
title: Fix Unity reload and redesign ion blaster
date: 2026-10-01
summary: "Diagnosed stale scene references and a HUD hot-reload exception, repaired reload defensively, and replaced the block gun with a verified multi-part ion blaster."
---

# Fix Unity reload and redesign ion blaster

## Root cause

Unity was playing `Temp/__Backupscenes/0.backup`, which still contained the original `Energy Cell` gun. That scene lacked the newly serialized weapon root, magazine, and reload clip, so the previous reload guard returned immediately. Separately, `DemoHud.EnsureStyles` returned when only the old styles survived a domain reload, leaving `ammoStyle` null and producing repeated exceptions at `DemoHud.cs:41`.

## Repair

`WeaponController` now resolves camera, weapon root, muzzle, magazine, audio, and camera shake at runtime. Reload no longer depends on a model reference, and pressing `R` deliberately runs a tactical reload even at 12/12. The HUD initialization guard now requires every style.

The weapon scene builder now creates a scaled, angled ion blaster with ceramic shell, swept armor, holo sight, spherical ion core, three coils, side vents, illuminated magazine, and a four-prong muzzle. A rendered preview was inspected and the viewmodel placement was adjusted to keep the silhouette visible.

## Verification

Unity `6000.0.58f2` rebuilt and validated the scene. A Play Mode smoke test called tactical reload, observed `IsReloading`, waited for the animation, verified `12/12`, and failed on runtime exceptions; it returned `RUNTIME_RELOAD_TEST_PASSED`. Every MonoBehaviour GUID in the final scene resolves to a script meta file.

> Historical work record — not durable authority. Prefer docs/specs/ADRs for current decisions.

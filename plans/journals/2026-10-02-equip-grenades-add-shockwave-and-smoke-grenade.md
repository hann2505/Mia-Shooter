---
title: "Equip grenades, add shockwave and smoke grenade"
date: 2026-10-02
summary: "Changed grenades into equip-then-click weapons, enlarged the frag blast with a shockwave, and added a persistent smoke grenade."
---

# Equip grenades, add shockwave and smoke grenade

## What changed

`G` now equips or stows a fragmentation grenade and `H` equips or stows a smoke grenade. Equipping hides the blaster and leaves the matching grenade visibly held in the first-person hand. Left click starts the throw animation; weapon fire stays blocked until recovery completes, then the blaster returns.

The fragmentation grenade blast radius increased to 7.5 units and blast force to 900. Its visual effect now has more fire, sparks and smoke, a larger light, and a `LineRenderer` shockwave expanding to a nine-unit radius.

The new smoke grenade has a distinct blue body and indicator. On impact it creates a noise-driven particle cloud with five-to-seven-second particles, plays a quieter spatial detonation sound, and does not apply radial target damage.

## Implementation notes

Added `GrenadeType` and updated the visual factory, throw animation, thrower, projectile, weapon input guard, HUD, and Vietnamese guide. Runtime installation remains compatible with Unity backup scenes.

## Verification

Unity `6000.0.58f2` compiled and validated the scene. The Play Mode smoke test proved that throwing before equipping is rejected, equipping hides the gun and displays the held grenade, clicking releases each grenade type, the fragmentation grenade creates a large blast plus shockwave and target damage, the smoke grenade creates a persistent cloud, and the gun returns after each throw. The final post-review run returned `RUNTIME_GRENADE_EQUIP_FRAG_SHOCKWAVE_SMOKE_TEST_PASSED`.

> Historical work record — not durable authority. Prefer docs/specs/ADRs for current decisions.

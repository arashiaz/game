# Delivery roadmap

## 1 — Foundations (source implemented; Unity validation pending)
- Unity 6 project manifest, runtime URP bootstrap, Android ARM64 build helper.
- Placeholder forest/meadow and bounded movement area.
- First-person touch + desktop controls, rifle/shotgun, hit detection.
- Placeholder deer/gazelle/boar; wandering, fleeing, health and harvest counter.
- Manual quality/FPS, saved preferences and conservative auto render-scale fallback.

## 2 — Validate vertical slice (next)
- Import and compile in Unity; fix editor/device findings before adding scope.
- Build development APK; test on the user's exact Xiaomi/POCO model.
- Validate touch ownership, UI safe area, 20-minute thermal behavior and frame pacing.
- Replace steering with obstacle-aware navigation; add collision/terrain grounding.
- Per-weapon inventory, reserves, mission persistence, restart flow.
- Automated EditMode/PlayMode tests and Android build CI after validated import.

## 3 — Art and game feel
- Source licensed realistic vegetation, terrain textures, animals and weapon assets.
- Keep an asset/license register; no unlicensed imports.
- LODs, instancing, baked/static lighting, animation and audio.
- Track footprints, binoculars, crouch/hold breath and mission progression.
- Persian localization, adjustable sensitivity/control layout, accessibility.

## 4 — Device optimization and release
- Profile CPU/GPU, draw calls, memory, load time and battery/heat on real devices.
- Confirm Android 16 compatibility and exact device chipset; do not infer from marketing RAM.
- Tune performance tiers from measured results; optional adaptive-performance provider.
- Signed release AAB/APK, privacy/store requirements, install/update tests.

## Scope decisions
Offline first. No multiplayer, open-world streaming, vehicles, paid economy or gore in the first slice. Initial goal: stable 30 FPS; 60 FPS optional after measurement. High visual fidelity is a later asset/optimization milestone, not a property of this primitive prototype.

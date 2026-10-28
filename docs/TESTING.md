# Validation checklist

No Unity executable is installed in the authoring sandbox. These checks are **pending**, not passing test claims.

## Editor
- [ ] Clean package import on Unity 6000.0.62f1; zero compile errors.
- [ ] Prepare project saves scene and sets ARM64/IL2CPP/landscape.
- [ ] Play empty hunting scene: exactly one game, camera, event system and HUD.
- [ ] Materials render via URP without pink shaders.
- [ ] Movement, ground collision and all four world boundaries work.
- [ ] Rifle and shotgun hit wildlife; dead animals count once.
- [ ] Wildlife flee after shots/proximity; harvest objective completes.
- [ ] Reload/switch/fire cooldown work; pause freezes reload timer.
- [ ] Opening settings blocks firing and movement; resume works.
- [ ] Manual graphics/cap survive restart; Auto reduces scale only after sustained low FPS.

## Android device
- [ ] Record exact model, SoC, GPU, Android and display refresh rate.
- [ ] APK installs on Android 16; cold start and resume have no crash.
- [ ] Concurrent move/look/fire touches work; releasing one finger does not cancel another.
- [ ] Notch/safe-area and landscape layout readable.
- [ ] No stuck movement after focus loss, notification, pause or interrupted touch.
- [ ] 30/60 FPS cap and Low/Medium/High measured using profiler.
- [ ] At least 20 minutes sustained play: note average FPS, slow frames, heat and memory.
- [ ] No visible quality oscillation in Auto; manual mode does not adapt.
- [ ] No credentials, signing keys or build products included in repository.

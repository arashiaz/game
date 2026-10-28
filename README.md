# Wildlands Hunt

Unity Android hunting **prototype**. Procedural placeholder meadow/forest, first-person movement, touch controls, rifle/shotgun hitscan, three placeholder species, flee behavior, harvest objective and persistent graphics settings.

**Status:** source scaffold; not yet compiled or play-tested in Unity. No APK is included. The current procedural primitives are NOT the planned realistic art. Unity Editor is unavailable in the coding environment.

## Open and run

1. Install **Unity 6000.0.62f1** with Android Build Support, SDK/NDK and OpenJDK through Unity Hub.
2. Add this repository as a project and let Unity resolve packages (URP 17.0.3, UGUI 2.0.0).
3. Run **Wildlands → Prepare Android project**. Allow any requested input-backend restart. Open `Assets/_Game/Scenes/HuntingGround.unity`.
4. Press Play. Runtime bootstrap creates the world, URP pipeline, player and HUD.
5. Desktop: WASD/arrows to move, hold right mouse to look, Space to fire, R to reload, Q to switch. On a phone use the left pad, drag the right half to look and use the labeled buttons.
6. **Wildlands → Build development APK** builds `Builds/Wildlands-dev.apk`. Switch to Android in Build Profiles first if prompted. APK outputs and signing keys must stay out of Git.

A headless build on a machine with the editor installed can run:

```sh
Unity -batchmode -quit -projectPath "$PWD" -buildTarget Android \
  -executeMethod Wildlands.Editor.ProjectTools.BuildAndroid -logFile build.log
```

## Graphics behavior

Manual Low / Medium / High and 30 / 60 FPS targets are saved locally. Auto uses a conservative memory/GPU heuristic, waits eight seconds, then measures four-second FPS windows. After three consecutive slow windows it reduces URP render scale by 0.1 down to 0.6. This is **not** a thermal API, a verified device benchmark or a guarantee of 60 FPS. No automatic upscale yet. 45 FPS is intentionally omitted pending refresh-rate/pacing tests. RAM expansion is not treated as real RAM.

## Deliberate prototype limitations

- Primitive animals, trees and weapons; no realistic assets, audio or reload animation yet.
- Animal steering is direct and can pass through props; obstacle-aware navigation is next.
- Weapons have independent firing characteristics but switching refills the magazine; reserve ammo and persistent per-weapon inventory are next.
- Flat bounded 200 × 200 m map, no terrain streaming, water, tracking or weather.
- Mission progress resets per session. Only graphics preferences are persisted.
- UI is English temporarily; Persian needs proper RTL shaping/localization and licensed font assets.
- Touch safe area is applied at startup; dynamic orientation changes are not supported.

See [roadmap](docs/ROADMAP.md) and [test checklist](docs/TESTING.md). No third-party art or audio is bundled.

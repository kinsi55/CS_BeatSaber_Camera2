# Spout2 output

Spout2 integration by **TheDoctorTTV**, built on Camera2 by Kinsi55 and the
KlakSpout/Spout dependencies credited below.

Select a camera in Camera2, open **Spout2**, and turn on **Enable Spout2**.
In OBS's Spout2 capture source, select **Camera2 - <camera name>**.
The toggle is saved immediately to that camera's existing JSON configuration.

Output follows the camera's effects, transparency, FPS limit and normal Camera2
scene activation. An excluded camera has no feed, except while temporarily
selected for editing as in normal Camera2. This applies in Menu, gameplay, FPFC
and custom scenes, regardless of desktop hiding. No extra scene camera is created.

**Hide on Beat Saber window** only hides the desktop preview, including when
selected for editing. It never keeps a scene-excluded camera or its Spout sender
active. Other cameras do not move or resize.

**Output resolution** offers Match camera, 720p, 1080p, 1440p and 2160p. Match
camera preserves the old desktop-size/render-scale behaviour. Fixed sizes are
independent of the desktop rectangle and window size: General's Render Scale
multiplies the fixed size for internal rendering, then the result is scaled to
the chosen Spout output size. For example, 1920x1080 at 1.5 render scale renders
2880x1620 internally and sends 1920x1080. With Spout disabled, rendering uses the
normal desktop-based dimensions again. Existing configs default to Match camera
and do not automatically change dimensions.

Renaming a camera or changing its output dimensions recreates its sender. A
render-scale change leaves a fixed-size sender intact. Turning Spout off releases
the sender. Spout does not include the desktop layout or settings selection outline.

## Requirements
- On Windows, use the OBS plugin [Spout2](https://github.com/Off-World-Live/obs-spout2-plugin).
- On Linux use [Spout2PW](https://github.com/hoshinolina/spout2pw), [Spout2PW fork for newer protons](https://github.com/tasokait/spout2pw) and the OBS plugin [OBS-PWVideo](https://github.com/hoshinolina/obs-pwvideo).

## Native dependency

`Native/Camera2Spout.dll` is the unmodified x64 KlakSpout binary, renamed to avoid
collisions with other mods. Source and binary provenance:

- https://github.com/keijiro/KlakSpout/tree/e1e492fb147614a166941be1d65c68a717127f3d
- Original path: `Packages/jp.keijiro.klak.spout/Plugin/x64/KlakSpout.dll`
- Git blob SHA-1: `5d91b47a9dc5731b26eac1bdf8a5148107f9a68a`
- Licenses: `Native/LICENSE.KlakSpout.md` and `Native/LICENSE.Spout.txt`
  (also embedded in Camera2.dll).

The native plugin is embedded in Camera2.dll and extracted to
`Beat Saber_Data/Plugins/x86_64/Camera2Spout.dll` during IPA initialization, before
Unity initializes native graphics plugins. It is loaded for the game's lifetime.
GPU sender resources are created only for cameras with Spout enabled.

## Build checks

This branch is based on Camera2 commit `4494b4d` for Beat Saber 1.40.x.
Build against that game's own assemblies, with `DisableCopyToPlugins=True` to
leave the installed mod untouched. The native DLL is embedded, so replacing
Camera2.dll is sufficient for installation; it must be in place before game startup.

Run `dotnet run --project Tests/SpoutSenderSmoke.csproj -c Release` for the
sender lifecycle checks. These compile the real sender against test doubles;
they do not exercise Unity, the native DLL or OBS.

## Manual acceptance checks

1. Enable output on one camera. Verify its named feed appears in OBS, with the
   correct orientation, colour, dimensions and transparency.
2. Enable a second camera and verify that both feeds update independently.
3. Enter FPFC with a third-person camera excluded from the FPFC scene and Spout
   enabled. With desktop hiding off OR on, the feed must stop after leaving settings.
   Repeat for a camera excluded from Menu and for custom scene switching.
4. Toggle desktop hiding, including on the selected camera. Verify a scene-enabled
   camera keeps sending, and hiding the last desktop preview clears its previous
   image. Disable Spout on an excluded hidden camera and verify its feed stops.
5. Choose a fixed resolution and vary Render Scale and desktop window/rectangle
   size. OBS dimensions must stay fixed; internal rendering follows Render Scale.
6. Set an existing camera FPS limit and verify output follows rendered frames.
7. Toggle output off/on, rename/resize the camera, and switch menu/song scenes.
   Verify senders disappear/reconnect without duplicates or continuing VRAM growth.
8. Restart the game and confirm each camera's settings are restored.
9. Verify normal desktop output and camera controls still work with Spout off,
   and that unsupported graphics backends log one failure without breaking rendering.

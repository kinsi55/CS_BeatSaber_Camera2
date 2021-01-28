# Camera2

[Camera Plus](https://github.com/Snow1226/CameraPlus) rewritten from scratch with focus on a cleaner, more optimized code base

### New features / improvements (Unless said otherwise, everything configurable per camera)

#### General
- **Support for Replays and FPFC / FPFC toggle**
- Decent performance improvements, especially on wall maps
- FPS cap
- More options for toggling the visibility of things
	- Walls can be hidden entirely, not just be made transparent (Useful for Side cams)
	- **Walls can automagically be made visible if you happen to play a modded map**
	- Hiding Floor(s)
	- Hiding Notes (Currently incompatible with custom notes)
- Every third person camera can follow or ignore the 360 world rotation, its not a special camera type any more

#### First person cameras
- Position smoothing ignores the rotation applied by mod maps so that [fast map rotations are actually watchable](https://www.youtube.com/watch?v=yjbFchHnZ74)
- "Force Upright" respects the map rotation, so if a map rotates you upside down your view will be upright in respect to the world rotation

#### Third person cameras
- Can not just be always visible or hidden but also hidden only while playing
- Ingame preview size customizable

### Changes

#### General
- The implementation of profiles has changed

Where as before you had profiles, and cameras associated to them, you now have scenes and can enable or disable any camera on a per-scene basis. Scenes are the menu, when in game, when watching a replay, etc. More details on Scenes can be found [HERE](#TODO_im_dumb)

#### Third person cameras
- When moving third person cameras the preview gets bigger to give you an easier time positioning correctly.
- Movement scripts
	- The format of movement scripts was changed to more correctly make it represent what it should be - a list of keyframes instead of a list of movements. Camera Plus movement scripts can still be used and will automatically be converted when you try to first use them
	- Map rotations (360 / Mod maps) are added onto the movement script to result in the combined movement
	- You can assign multiple movement scripts to a camera, Camera2 will then always pick a different one at random

### Omitted features (For now™)
- Multiplayer related features
- **Configuration UI**

You can only move around and resize camera views on the desktop window - thats it. I would've loved to add configuration using a desktop UI like Camera Plus has but quite honestly implementing desktop UI's in this Engine using just code is an even bigger pain than failing a 5 minute map 5 seconds before the end when you're on full combo otherwise. **For now**™ the only way to configure Camera 2 is using the config files, **eventually** I'm hoping to add ingame configuration.
# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.9.3.1] - 2023-XX-XX

### Updates
- Performance improvements: changed most background objects to be static. Optimized sound compression.
- Reduced player inactionable frames when swinging slightly.
- Changed the sound of the jump smash to be a little more "impactful".
- Reduced the size of the hitboxes slightly.
- Added walls and made court more visible in training mode.
- Added translucent background behind score and information.
- Made the net narrower, reducing chance of the shuttle "rolling" on the top.
- Increased base XP gain by 50%.
- Added some more detailed explanations in the tutorial page.
- Shorted the shuttle trail slightly.
- Removed "online" buttons.
- Added some variability (randomized) to shots that are further away from the player. Variability is intensified along the left-right axis.
- Shots are now weaker when the shuttle is behind the player (-3%).
- Smashes are now less steep in singles games.
- Bots no longer jump when below 33% energy.
- In doubles, input buffer time has been halved compared to singles.
- Cleaned up the main menu a bit.
- Added a real pause menu that actually pauses the game.
- Improved visibility of shuttle by darkening the courts and the background.
- Reduced bot reaction speed very slightly.
- Added a frame rate option.
- Singles camera has been improved - it now moves with the player and gives a better FOV.
- Singles energy consumption per hit has increased from 12 -> 14 for smashes and 6 -> 7 for other shots.
- Doubles energy consumption per hit has decreased from 12 -> 10 for smashes and 6 -> 5 for other shots.
- MOBILE: joystick now has no snap to direction.
- MOBILE: improved touch control ergonomics.

### Fixes
- Fixed typos in the "tutorial".
- Fixed the shuttle trail teleporting everywhere.
- Fixed a bug where getting over enough exp to level twice was only leveling once.
- Fixed a bug where yellow in doubles was not moving to the correct locations.
- Fixed an issue where having the camera as side view and two players on PC mode was playing as split screen.
- Fixed the camera in 4P.
- 2P doubles no longer splitscreens on PC.


- need to fix energy in 90-120 fps.
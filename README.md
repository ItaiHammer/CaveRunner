# Cave Runner Game

## Goal
This game is a platformer/speedrun, where the goal is to move and jump around collecting pickups before exiting out the door. Each designed level is timed, and consequently ramp up in difficulty as you progress. 

### Technical Criteria

Audio: Implementation of background music, spatial 3D sound effects, and UI feedback sounds.
* Custom Audio SFX and soundtrack recorded, processed, and written by Itai, with audio points originating from character, exit door, or background

VFX (Visual Effects): Use of particle systems (e.g., explosions, dust, magic effects) to enhance gameplay.
* Particle effects displayed from pick-ups to exude "magical" effect

UI (User Interface): A cohesive menu system, HUD (heads-up display), and "Game Over/Win" screens.
* Full level select menu + appropriate win/loss screens 

Animations: Create keyframed animations or import externally created animations to your game.
* Player character fully keyframe animated via custom-made pixel sprites

Shaders & Materials: Custom or advanced use of materials to create a specific aesthetic or environmental effect.
* All materials were made use the URP Sprite-Lit shader to support 2D lighting. Separated elements using sorting layers and applied targeted lights to selectively control brightness, ensuring important objects (like platforms) remain visible without breaking the overall dark scene aesthetic. Key elements such as the FragmentCore and lava use brighter emissive-style lighting to create a subtle glow/bloom effect, helping them stand out and visually guide the player. 

Lighting: Effective use of real-time or baked lighting to set the mood and guide the player's eye.
* Player character and Pick-ups present point lights in the mostly dark level, giving a sense of intimacy/closeness
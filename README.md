# Custom Animation Platform Fighter 

Welcome to the repository for this unique platform fighting game. Built on a combat premise similar to *Brawlhalla*, this project introduces a fundamental twist: **players animate their own character's attacks and movements**.

## Overview
This project is a multiplayer fighting game that gives players complete creative control over their character's combat animations. Players utilize a custom in-game editor to pose character models, define keyframes, and save these sequences. In multiplayer matches, these custom animations are serialized, transmitted to other clients, and reconstructed in real-time so opponents see exactly what was authored.

## Features
*   **In-Game Animation Editor:** A robust timeline and transform tools allow players to modify positional and rotational data to author custom keyframed animations.
*   **Authoritative Multiplayer:** Built with the Mirror networking framework, ensuring synchronized combat, health state management, and reliable delivery of custom animation data.
*   **Dynamic Combat System:** Movement features like multi-jumping, dashing, and directional attacks map directly to player-authored animations.
*   **JSON Serialization Pipeline:** Custom keyframes are translated to JSON strings, allowing complex animation data to be easily saved locally and shared across the network.

## Architecture: The Animation Pipeline
Building an animation engine from scratch within Unity presented significant engineering challenges, particularly regarding data serialization and playback synchronization.

### 1. The Animation Editor (Development Struggles)
Creating a runtime animation editor required building custom timeline logic and manipulation tools from the ground up:
*   Players interact with individual character joints using custom `Tool` scripts. These scripts track mouse coordinates on the screen and translate them into local position and rotation shifts for `AnimatableObjects`.
*   The `TimeLine` script manages an array of `KeyFrameSlot` objects. A major struggle during development was managing coroutine execution for accurate playback.The TimeLine required a delicate loop that measures the `durationBetweenFrames` before triggering the character model to visually update.

### 2. JSON Serialization & Networking
Because standard Unity `AnimationClip` objects cannot be easily created and networked at runtime, the project uses a custom data structure to share animations.
*   A custom `KeyFrame` class stores `Vector2` arrays for model positions and `float` arrays for rotations.
*   When an animation is saved, `CharacterCharacteristic` serializes the data. Because Unity's native `JsonUtility` struggles with multi-dimensional arrays, the system converts individual animations into strings, wraps them in brackets, and separates multiple animations using a `^` delimiter.
*   This compacted JSON string is saved locally to a `MyPlayerCharacteristics.txt` file via the `MyPlayerInitializer`.
*   When joining a Mirror multiplayer session, this text payload is sent to the server to be distributed to other connected clients.

### 3. Reconstruction & Playback
Once the networked clients receive the JSON payload, the reverse process occurs so the animation can be played:
*   The string is split by the `^` delimiter, and `JsonUtility.FromJson` reconstructs the arrays into playable `KeyFrame` data.
*   The `PlayerAnimation` script listens for custom inputs (e.g., Q, E, F) and temporarily disables the standard Unity Animator.
*   It then uses a coroutine to manually step through the deserialized `KeyFrame` arrays, applying the exact transform data authored by the opposing player frame-by-frame and calculating the exact wait times between each keyframe.

## Multiplayer Integration
The multiplayer architecture relies heavily on Mirror for state synchronization:
*   **Damage & Health:** Health changes are managed by the server using `[SyncVar]` hooks. When a player takes damage, a `[ClientRpc]` triggers the physics calculation on the specific local player's `Rigidbody` to apply knockback.
*   **Matchmaking & Menus:** A custom `MainMenu` script interfaces with Mirror's `NetworkManagerHUD` to handle server hosting, client connection statuses, and asynchronous scene loading.

---
*Developed by Sleem Alaa.*

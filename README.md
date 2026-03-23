<div align="center">

# VR Interaction Showcase
### Physics-Driven Puzzle Experience · Meta Quest

<p>
  <img src="https://img.shields.io/badge/Unity-2022.3 LTS-000000?style=flat-square&logo=unity&logoColor=white">
  <img src="https://img.shields.io/badge/XRI-2.6.5-0467DF?style=flat-square&logo=meta&logoColor=white">
  <img src="https://img.shields.io/badge/OpenXR-1.9-6B4FBB?style=flat-square">
  <img src="https://img.shields.io/badge/Platform-Meta Quest 2 / 3-1C1C1E?style=flat-square&logo=meta&logoColor=white">
  <img src="https://img.shields.io/badge/Language-C%23-239120?style=flat-square&logo=c-sharp&logoColor=white">
</p>

**[Portfolio](https://tonygamedev.carrd.co/) · [GitHub Repo](https://github.com/Legato12/VR-Showcase-Project-Advan-ed?tab=readme-ov-file)**

</div>

---

## Overview

A technical VR showcase built from scratch in Unity, demonstrating physics-driven interactions, custom grab mechanics, and polished game feel on standalone headsets. Every system was designed to go beyond typical XRI defaults — prioritizing tactile feedback, event-driven architecture, and clean, maintainable code.

> **Focus:** Physics-accurate interactions · Modular event-driven design · Quest optimization

---

## Demo

<!-- ============================================================
     YOUTUBE THUMBNAIL — replace the two URLs below:
     1. IMAGE_URL  → a direct link to your thumbnail image
        (e.g. https://img.youtube.com/vi/YOUR_VIDEO_ID/maxresdefault.jpg)
     2. VIDEO_URL  → your YouTube video link
        (e.g. https://www.youtube.com/watch?v=YOUR_VIDEO_ID)
     ============================================================ -->
[![Watch the demo](https://img.youtube.com/vi/gTZA1NBkTvE/maxresdefault.jpg)](https://youtu.be/gTZA1NBkTvE)

---

## Screenshots

<!-- Replace the src paths with your actual screenshot URLs or local paths -->
| | | |
|:---:|:---:|:---:|
| ![Socket Puzzle](ADD_SCREENSHOT_1) | ![Lever System](ADD_SCREENSHOT_2) | ![Physical Buttons](ADD_SCREENSHOT_3) |
| Color Socket Puzzle | Physics Lever | Physical Buttons |

---

## Core Systems

| System | Description |
| :--- | :--- |
| **HL:Alyx Grab** | Three-phase grab: distant objects are held in place, a sharp wrist flick launches them to hand, transitioning into a standard near-grip with full throw support. |
| **Physics Lever** | `HingeJoint`-based lever driven by `AddTorque` — no `XRGrabInteractable` conflicts. Includes angle-threshold event system with hysteresis to prevent double-triggers. |
| **Color Socket Puzzle** | Three XR socket interactors validate object color via tag before accepting. All three correct → reward object activates. Wrong-color objects are accepted but produce no feedback. |
| **Physical Buttons** | Each button supports ray-hover activation *and* physical collision press. A runtime trigger zone detects downward velocity ≥ 0.05 m/s from any Rigidbody for true physical interaction. |
| **Hover Glow** | Per-instance material emission boost on ray hover. Avoids shared-asset mutation; reverts cleanly on exit. |
| **Target Hit Plate** | Collision-reactive shooting target with cooldown, material state change, and optional camera flash. |
| **Pause Menu** | WorldSpace canvas that smoothly follows the camera in `LateUpdate`. Supports Resume, Restart, and Quit. |
| **Main Menu** | Standalone VR menu scene with volume and brightness sliders persisted via `PlayerPrefs` and applied automatically on game scene load. |

---

## VR Controls

| Input | Action |
| :--- | :--- |
| **Left Stick** | Continuous locomotion (DynamicMoveProvider) |
| **Right Stick** | Snap turn — 45° per step |
| **Right Trigger (distant object)** | Far-hold grab — object stays in place, locked at current distance |
| **Right Stick Y (while holding)** | Adjust hold distance (0.5 m – 8 m) |
| **Wrist flick toward head** | Flick gesture — launches held object to hand |
| **Right Trigger (near object)** | Standard near grab — throw / hand-off supported |
| **Ray + Trigger** | Ray-based UI interaction (menus, buttons, sockets) |
| **Menu Button** | Open / close pause menu |

---

## Tech Stack

| | |
| :--- | :--- |
| **Engine** | Unity 2022.3 LTS (URP) |
| **VR Framework** | XR Interaction Toolkit 2.6.5 · OpenXR |
| **Rendering** | Universal Render Pipeline · Baked lighting |
| **Physics** | PhysX — Rigidbody, HingeJoint, ConfigurableJoint |
| **Platform** | Android (Meta Quest 2 / 3) |
| **Language** | C# 9 |
| **Architecture** | Event-driven (Observer pattern via UnityEvents + C# delegates) |

---

## Project Structure

```
Assets/
├── _Project/
│   ├── Scripts/
│   │   ├── Interactions/   # Lever, Button, Socket, Grab systems
│   │   ├── Systems/        # Game Manager, Settings, Rewards
│   │   └── UI/             # Spatial UI, Pause Menu, Main Menu
│   ├── Prefabs/            # Reusable VR components
│   └── ScriptableObjects/  # Data configuration
└── XR/                     # XR Rig and input settings
```

---

## Getting Started

```bash
git clone https://github.com/Legato12/VR-Showcase-Project-Advan-ed.git
```

1. Open in **Unity 2022.3 LTS**
2. Install dependencies via Package Manager: `XR Plugin Management`, `Meta XR Plugin`, `XR Interaction Toolkit 2.6.5`
3. `Build Settings` → Switch to **Android** → select your Quest device → **Build and Run**

---

<div align="center">

Built by **[Tony](https://tonygamedev.carrd.co/)** — Unity Developer & VR Specialist · Open to freelance and full-time opportunities.

</div>

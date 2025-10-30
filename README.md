# Procedural Robotic Arm (2D)

This project implements a **procedural 2D robotic arm** with **Inverse Kinematics (IK)** in **Godot 4**. The arm follows the mouse cursor with smooth motion and respects joint length constraints for realistic movement.

The project includes two script versions:
- **GDScript** (`arm.gd`) – original Godot script version.
- **C#** (`arm.cs`) – C# version for Godot 4, with automatic node assignment in `_Ready()`.

---

## Features

- 2-segment procedural arm controlled in 2D.
- Smooth movement with joint constraints.
- Flipping support for arm direction.
- Hand automatically points toward the mouse cursor.
- Safe resting position when the cursor is too close to the base.

---

## Installation

1. Clone or download the repository.
2. Open the project in **Godot 4**.
3. If using the **C# version**, make sure **Mono / C# support** is configured in Godot.
4. Open the scene `Arm.tscn`, which has the following hierarchy:

Arm (Node2D)
joint1 (Node2D)
joint2 (Node2D)
hand (Node2D / Sprite)

5. Attach the script to the `Arm` node:
   - `arm.gd` for GDScript
   - `arm.cs` for C#

6. Run the scene and move the mouse to see the procedural arm in action.

---

## Usage

- Adjust parameters in the Inspector:
  - **Flipped** – toggles arm direction.
  - **RotationSmoothing** – controls how smoothly the arm follows the mouse.
- Segment lengths are automatically calculated from the positions of `joint2` and `hand`.

---

## Assets

- All 2D images for the robotic arm are in the `assets` folder.
- You can replace them with your own sprites if desired.


# Sprite Slicer Editor

Sprite Slicer is an Editor tool developed for Unity. It automatically slices one or more sprites at a time to a specific width and height.

                Editor path = Tools -> SpriteSlicer

---

🚀 Features

- Drag-and-drop multiple sprites
- Predefined and custom slice sizes
- Pivot adjustment
- Pixels Per Unit (PPU) support
- Sprite naming schemes
- Filter mode selection
- One-click slicing and clearing

---

⚙️ Usage

1. From the top Unity menu, select `Tools > SpriteSlicer`.
2. Drag and drop your sprites into the list in the opened window.
3. In the "Slice Settings" section, choose the slice dimensions.
4. In the "Sprite Settings" section, adjust PPU, pivot, filter mode, etc.
5. Click the "Slice" button to begin slicing.

---

⚖️ Settings

**Slice Settings**

- **Slice Width / Height**: Defines the width and height of each slice (e.g., 32x32).

- **Enable Predefined Sizes**: Lets you choose from common sizes (8, 16, 32, ...). Disable it to enter custom values.

**Sprite Settings**

- **Pivot**: Sets the pivot point for each sliced sprite (0,0 = bottom-left; 0.5,0.5 = center).
  
- **Pixels Per Unit (PPU)**: Determines how much space a sprite takes in the scene. 100 PPU = 1 Unity unit.
  
- **Filter Mode**: Controls how the sprite appears when scaled.
  
- **Naming Scheme**:
  - `PrefixNumber`: `sprite_1`, `sprite_2`, ...
  - `PrefixParenthesesNumber`: `sprite_(1)`, ...
  - `PrefixNumberWithLeadingZeros`: `sprite_001`, `sprite_002`, ...
    
- **Sprite Prefix**: A prefix added to the name of each sliced sprite (e.g., `sprite_`).
  
- **Leading Zeros**: Number of digits used in zero-padding (e.g., 3 = `sprite_001`).
  
- **Add '(Sliced)' to Parent Sprite Name**: Appends `(Sliced)` to the original texture's name.


![2024-08-09-21-48-12](https://github.com/user-attachments/assets/15d4db75-89cc-4d80-9920-0ad512f685ff)

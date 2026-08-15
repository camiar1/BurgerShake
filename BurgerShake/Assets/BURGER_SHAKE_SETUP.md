# Burger Shake foundation setup

This branch contains only the reusable presentation pieces from the old Food Truck project plus new Burger Shake gameplay foundations. It deliberately does not import the old player, cooking, recipe, order, serving, customer-patience, or station logic.

## 1. Two-view food truck setup

Create two world-space roots in the scene:

- `AssemblyView`
- `CustomerWindowView`

Place them side-by-side in world space. Put an empty `CameraTarget` under each root at the position where the camera should sit for that view.

On the Main Camera add:

- `ViewController`
- `ViewKeyboardInput`

Assign the Assembly and Customer Window camera targets in `ViewController`.

Controls:

- A / Left Arrow: turn toward Assembly
- D / Right Arrow: turn toward Customer Window

The cooking station has intentionally been removed.

## 2. Blender physics setup

Under `AssemblyView`, create a `Blender` root with static `BoxCollider2D` objects for the left wall, right wall, and bottom. Do not add Rigidbody2D to the blender walls.

Create an empty `IngredientDropper` object above the blender. Add `IngredientDropper.cs` and set `minX`, `maxX`, and `dropY` to match the inside of the blender.

The dropper only responds while the player is facing the Assembly view.

## 3. Ingredient prefab setup

Each physics ingredient prefab should contain:

- `SpriteRenderer`
- `Rigidbody2D` (Dynamic)
- an appropriate `Collider2D`
- `Ingredient.cs`

Recommended Rigidbody2D starting values:

- Gravity Scale: 1
- Collision Detection: Continuous
- Interpolate: Interpolate
- Rotation: not frozen

Create ingredient data with `Create > Burger Shake > Ingredient`. Assign its sprite and prefab, then configure base points, points per touch, base Mult, Mult per touch, tags, and draft weight.

## 4. Three-choice draft setup

Create three UI Buttons. Add `IngredientChoiceButton.cs` to each and wire its icon, name text, scoring text, and Button reference.

Create a `GameManager` object with `IngredientDraftManager.cs`. Assign:

- the ingredient pool
- the three choice buttons
- the IngredientDropper

The manager rolls three distinct weighted choices, disables the choices after one is selected, and refreshes them after the chosen ingredient is dropped.

## 5. Scoring

Add `ScoreManager.cs` to the GameManager.

Current prototype formula:

`Total Score = total Points x total Mult`

Each ingredient currently contributes:

- base points
- points per physical ingredient touching it
- base Mult
- Mult per physical ingredient touching it

`Ingredient.cs` also exposes ingredient tags and `CountTouchingWithTag`, which is the intended hook for more advanced synergies such as Meat-only or Vegetable-only touch bonuses.

## 6. Reusable UI polish

The following generic scripts were ported from the Food Truck project:

- `UIButtonAnimator.cs`
- `UIPanelAnimator.cs`

They have no dependency on the old cooking gameplay and can be used on Burger Shake choice buttons, shop cards, score panels, and future overlays.

## Intentionally not migrated

- Player / Chef movement
- GameModeManager
- Cooking station and cooking scripts
- BurgerBuilder and assembly-order logic
- Recipes
- Customer orders and patience
- Serving baskets / service window mechanics
- Day ratings and stars
- Old shop/progression logic
- Old ingredient prefabs with cooking/drag components

Reuse old artwork selectively, but build new Burger Shake prefabs around Rigidbody2D physics instead of copying old gameplay prefabs.

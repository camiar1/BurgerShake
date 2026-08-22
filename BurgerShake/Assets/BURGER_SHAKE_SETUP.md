# Burger Shake architecture and Unity setup

This project is structured around the current Burger Shake GDD: Suika-style ingredient dropping, touch-based scoring synergies, one customer challenge per day, customer restrictions/wants, run loss on a failed goal, and a shop between successful days.

The important architectural rule is **content lives in ScriptableObjects; gameplay flow lives in managers**. New ingredients, customers, scoring rules, restrictions, preferences, and upgrades should normally be addable without editing a manager.

## Scene hierarchy

Recommended gameplay scene:

```text
Main Camera
EventSystem
GameManager
  RunProgress
  RunManager
  GameplayModifiers
  UpgradeManager
  CustomerChallengeController
  IngredientDraftManager
  ScoreManager
  ShopManager
Canvas (World Space)
  Viewport
    ViewController
    ViewKeyboardInput
    StationStrip
      AssemblyView
        CanvasGroup
        Background
        PhysicsRoot
          BlenderRoot
            BlenderColliders
              LeftWall
              RightWall
              BottomWall
          IngredientContainer
          IngredientDropper
        ForegroundCanvas
          BlenderFront
          ScorePanel
          IngredientChoices
            Choice1
            Choice2
            Choice3
      CustomerWindowView
        CanvasGroup
        Background
        Customer
        WindowUI
```

The Canvas/StationStrip setup keeps the original Food Truck turning presentation. The Assembly and Customer Window are complete panels; the strip slides rather than showing/hiding HUD elements.

## View setup

Add `ViewController` and `ViewKeyboardInput` to `Viewport`.

Assign `ViewController`:

- Viewport -> `Viewport`
- Station Strip -> `StationStrip`
- Assembly View -> `AssemblyView`
- Customer Window View -> `CustomerWindowView`
- Assembly Canvas Group -> AssemblyView CanvasGroup
- Customer Window Canvas Group -> CustomerWindowView CanvasGroup
- Assembly Physics Root -> `PhysicsRoot`

Controls:

- A / Left Arrow -> Assembly
- D / Right Arrow -> Customer Window

## Blender physics

Under `PhysicsRoot`, create `BlenderRoot` and three static `BoxCollider2D` children for the left wall, right wall and bottom. Do not add Rigidbody2D components to the walls.

Create `IngredientContainer` under `PhysicsRoot`. All spawned ingredients are parented here so they stay with the Assembly panel.

Create `IngredientDropper` above the blender and add `IngredientDropper.cs`.

Assign:

- Gameplay Camera -> Main Camera
- View Controller -> Viewport's ViewController
- Gameplay Modifiers -> GameManager's GameplayModifiers
- Ingredient Container -> IngredientContainer
- Min X / Max X / Drop Y -> match the blender opening

## Ingredient prefabs

Each ingredient prefab should contain:

- SpriteRenderer
- Rigidbody2D (Dynamic)
- an appropriate Collider2D
- Ingredient.cs

Recommended Rigidbody2D starting values:

- Gravity Scale = 1
- Collision Detection = Continuous
- Interpolate = Interpolate
- Rotation not frozen

Ingredient prefabs do not need a Definition assigned permanently. `IngredientDropper` initializes the spawned instance with the selected IngredientDefinition.

## Ingredient data and scoring

Create ingredient data with:

`Create > Burger Shake > Ingredient`

Each IngredientDefinition contains:

- name / description
- sprite / prefab
- draft weight
- tags
- a list of reusable IngredientScoringRule assets

Create scoring rules with:

`Create > Burger Shake > Scoring Rule`

A scoring rule has a target and reward. Current targets:

- Self
- Touching Any
- Touching Tag
- Touching Ingredient

Current rewards:

- Points
- Mult

Examples:

- Pickle `+2 points`: target Self, reward Points, amount 2
- Onion `+3 points for every Tomato touching it`: target TouchingIngredient, requiredIngredient Tomato, reward Points, amount 3
- Pickle `+0.5 Mult for every Lettuce touching it`: target TouchingIngredient, requiredIngredient Lettuce, reward Mult, amount 0.5

Because rules are separate assets, the same rule can be reused and new rule types can be added without turning IngredientDefinition into a giant switch statement.

## Ingredient draft

Add `IngredientDraftManager` to GameManager.

Assign:

- Choice Buttons -> Choice1, Choice2, Choice3 (and additional buttons later if desired)
- Dropper -> IngredientDropper
- Gameplay Modifiers -> GameplayModifiers

The active ingredient pool is supplied by RunProgress through RunManager. The draft manager supports changing choice count through restrictions or upgrades.

Each choice button needs `IngredientChoiceButton` and references for icon, name, scoring text and Button.

## Score

Add `ScoreManager` to GameManager.

Current formula:

`Total Score = Points x Mult`

Mult starts at 1 plus any run-upgrade bonus. Each ingredient evaluates its list of scoring-rule assets based on its current physical contacts.

## Run setup

Create:

`Create > Burger Shake > Run Definition`

Configure:

- Starting Ingredient Count (2-3 matches the GDD starting intent)
- Starting Ingredients
- Customer list in day order
- Goal Multiplier By Day curve
- Starting Coins

Add `RunProgress` and `RunManager` to GameManager.

Assign RunManager:

- Run Definition -> your run asset
- Progress -> RunProgress
- Draft Manager -> IngredientDraftManager
- Challenge Controller -> CustomerChallengeController

Call `RunManager.StartRun()` from your future Start Run button / menu.

Run states are:

`Setup -> Customer -> Shop -> Customer ... -> Won/Lost`

Failing a customer's target sends the run to Lost. Passing awards coins and moves to Shop unless it was the final customer.

## Customer data

Create customers with:

`Create > Burger Shake > Customer`

Each customer contains:

- identity / portrait
- base goal score
- base coin reward
- restrictions
- optional preferences/wants

Create restrictions with:

`Create > Burger Shake > Customer Restriction`

Current restriction types:

- Blender Scale
- Ingredient Scale
- Draft Choice Count
- Drop Limit

Create wants with:

`Create > Burger Shake > Customer Preference`

Current preference types:

- Ingredient Count
- Tag Count
- Score Over Goal

Each satisfied preference awards its configured bonus coins.

Add `GameplayModifiers` and `CustomerChallengeController` to GameManager.

Assign CustomerChallengeController:

- Score Manager -> ScoreManager
- Gameplay Modifiers -> GameplayModifiers
- Upgrade Manager -> UpgradeManager
- Ingredient Dropper -> IngredientDropper
- Blender Root -> the object whose scale controls blender size

## Shop and run upgrades

Add `ShopManager` and `UpgradeManager` to GameManager.

ShopManager supports:

- ingredient crates that roll ingredients not already owned
- choosing one rolled ingredient and adding it to the run pool
- purchasing persistent run upgrades

Create upgrades with:

`Create > Burger Shake > Run Upgrade`

Current scalable upgrade effects:

- Draft Choice Bonus
- Starting Mult Bonus
- Ingredient Scale Multiplier
- Bonus Coins Per Win

Assign ShopManager:

- Progress -> RunProgress
- Draft Manager -> IngredientDraftManager
- Upgrade Manager -> UpgradeManager
- All Ingredients -> all ingredients that can appear in crates
- Available Upgrades -> the upgrade catalog

Assign UpgradeManager:

- Progress -> RunProgress
- Gameplay Modifiers -> GameplayModifiers
- Score Manager -> ScoreManager

## Adding content later

### New ingredient

1. Make sprite and physics prefab.
2. Create IngredientDefinition.
3. Assign tags and scoring rules.
4. Add it to ShopManager's ingredient catalog or starting run pool.

No draft/scoring manager changes should be required.

### New customer

1. Create CustomerDefinition.
2. Create/reuse restriction and preference assets.
3. Add the customer to RunDefinition.

No RunManager changes should be required.

### New scoring idea

If it fits an existing target/reward combination, create a new ScoringRule asset only. If it needs a genuinely new spatial condition (above, below, exact touch count, floor contact, etc.), add one new rule target/evaluator instead of adding ingredient-specific code.

### New restriction or powerup

Add a new enum effect plus one application case in GameplayModifiers / UpgradeManager. Customer and run data remain unchanged.

## Deliberately excluded old Food Truck mechanics

- walking Chef/player movement
- Cooking station
- cooking / patty preparation
- burger recipe assembly
- service baskets
- customer patience
- star/day-rating system
- old order evaluator

The new architecture keeps the Food Truck presentation but does not depend on those mechanics.

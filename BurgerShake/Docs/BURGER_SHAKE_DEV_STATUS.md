# Burger Shake — Development Status

This file tracks what is currently known, what changed recently, what still needs Unity validation, and what should be worked on next. Update it whenever a milestone materially changes.

## Current branch

- Repository: camiar1/BurgerShake
- Working branch: agent/burger-shake-foundation
- User's latest baseline update: `Agent Update` (`e71a99e49e783e8216305cda5b8a9756e10bf173`)
- Development resumed after that update with explicit permission to modify gameplay systems and placeholder UI.

## Confirmed current foundation

### Run flow
- `RunManager` includes `StartingDraft`, customer intro, assembly, score reveal, customer outro, shop, won, and lost states.
- Auto-start enters the Starting Ingredient Draft rather than a fixed Starting Pantry.
- `RegularRun` currently contains Chad, Glorb, and Gregg, making it the active 3-customer vertical-slice run.
- Passing the final customer skips the shop and transitions through CustomerOutro into `RunState.Won`.

### Starting Ingredient Draft
- `StartingIngredientDraftController` exists and is scene-wired.
- `StartingIngredientDraftUI` exists and is scene-wired.
- Scene configuration currently uses 2 rounds, 3 choices per round, 3 copies per selected ingredient, and 1 shared opening-draft reroll.
- Reroll behavior is per individual choice slot.
- Round 2 supports curated synergy partners.
- `RegularRun.asset` currently includes the 14 implemented ingredients and curated synergy lists.

### Ingredients / scoring
- Current fully playable ingredient data still contains 14 implemented ingredients:
  - Fruit: Apple, Blueberry, Orange, Pineapple
  - Protein: Bacon, Burger Patty, Egg, Sausage
  - Vegetable: Lettuce, Mushroom, Onion, Pickle, Red Chili, Tomato
- The final first-pass roster is now formally designed at **30 ingredients**, split into 10 Protein / 10 Vegetable / 10 Fruit entries.
- The 16 planned additions are:
  - Protein: Cheese, Chicken Nugget, Ham, Meatball, Shrimp, Tofu
  - Vegetable: Avocado, Carrot, Cucumber, Corn
  - Fruit: Banana, Strawberry, Grape, Watermelon, Cherry, Lemon
- Full target base values, abilities, build families, and physical-shape intentions live in `Docs/BURGER_SHAKE_INGREDIENT_ROSTER.md`.
- These 16 planned ingredients are not yet active ScriptableObject/prefab content; they will be added in small batches after their required generic rules compile-test.

### Scoring-rule expansion in this phase
- `ContactCountScoringRule` now has an optional tag filter. Existing assets preserve their old behavior unless the filter is enabled.
- `UniqueNeighborScoringRule` now supports ignoring an initial number of unique neighbors when calculating per-neighbor reward, allowing “starting with the Nth unique neighbor” designs without ingredient-specific code.
- New `OrientationScoringRule` supports Horizontal / Vertical / Diagonal placement, angle tolerance, optional min/max contact count, and optional per-contact reward.
- Bacon now uses the new tag-filtered Contact Count behavior: **+0.5 Mult when touching at least 2 Proteins**, instead of counting arbitrary contacts.
- Apple now uses thresholded diversity scaling: starting with its 3rd unique neighboring ingredient type, **+0.1 Mult per scoring unique type**.
- These changes intentionally keep the scoring framework generic and reusable for the 30-ingredient roster.

### Customers
- There are 10 `CustomerDefinition` assets matching the 10 existing portraits: Chad, Chloe, Cole, Eve, Glorb, Gregg, Ms. Pam, Old Man Joe, Ronda, and Tommy.
- The active vertical slice remains Chad → Glorb → Gregg.
- The remaining 7 customer definitions are content-ready but are not yet in `RegularRun`.

### Customer restrictions
- Six reusable restriction assets exist: Compact Blender, Tight Blender, Chunky Ingredients, Big Ingredients, Two Choice Toss, and Four Drop Order.
- Blender-width restrictions squeeze only the X axis of both physics BlenderRoot and the visible BlenderFront artwork.
- The user confirmed the visible blender now correctly narrows with the restriction.

### Receipt
- `RoundReceiptUI` displays customer description, base reward, optional special requests/preferences and bonus coin value, restrictions, and score goal.
- Visual spacing/layout still needs further Unity inspection for long receipts.

### Win screen
- A runtime-generated placeholder `WinScreenController` exists for the vertical slice.
- It shows customers served, final shake score, coins remaining, final pantry, Helpers, Play Again, and Main Menu.
- Final end-to-end Win-screen behavior still needs explicit local validation.

### Shop / Helpers
- Existing shop architecture and UI are present.
- Current helper assets: HeadStart and TipJar.
- Final target remains 20+ Helpers with cross-system interactions.

## Product targets locked so far

- 30+ ingredients.
- 20+ Helpers.
- 20+ customers.
- No active-run save/resume.
- Restrictions should alter the puzzle without routinely invalidating a build.
- Receipt is the central readable summary of customer conditions.
- Website-style shop remains the intended final shop presentation.
- Temporary UI creation/movement is authorized during implementation.
- Use only legal/free external resources unless explicit approval is given for something paid.

## Needs Unity validation

Because repository editing cannot run the Unity Editor, these should be checked locally after pulling the latest branch:

1. Project compiles after the Contact Count / Unique Neighbor / Orientation scoring-rule additions.
2. Existing scoring assets deserialize without missing-field or script-reference errors.
3. Bacon only receives its +0.5 Mult bonus when at least 2 touching ingredients have the Protein tag.
4. Apple gives no special Mult at 0–2 unique neighboring ingredient types, +0.1 at 3, +0.2 at 4, etc.
5. Starting Draft still appears and individual reroll still works.
6. Chad plays with normal blender/ingredient settings.
7. Glorb still narrows the visible and physical blender together.
8. Gregg still enlarges dropped ingredients and restrictions reset between customers.
9. Receipt details remain readable with restriction text.
10. Three-customer flow reaches the final Won state without stale ingredients or blocked UI.
11. Win screen appears after Gregg leaves and both Play Again / Main Menu work.

## Immediate next work

Priority order:

1. Local compile/play validation of the new reusable ingredient-rule layer.
2. Add the first planned ingredient batch only after that validation, so new content does not hide a framework compile problem.
3. Build the formal 20-Helper interaction matrix and extend Helper architecture around the now-stable ingredient vocabulary.
4. Expand the customer/restriction library toward 20 while keeping restrictions moderate.
5. Return to shop presentation and receipt polish after the new content systems are stable.

## Current milestone

`M4 — Ingredient interaction framework` is now in progress.

The 30-ingredient design matrix is complete, and the generic scoring library has been expanded to express its new threshold/tag/orientation patterns. The next safe content step is a small playable batch of new ingredients after Unity compile validation.

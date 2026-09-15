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
- Current `RegularRun` has 3 customers, making it appropriate for the first vertical-slice target.

### Starting Ingredient Draft
- `StartingIngredientDraftController` exists and is scene-wired.
- `StartingIngredientDraftUI` exists and is scene-wired.
- Scene configuration currently uses:
  - 2 rounds
  - 3 choices per round
  - 3 copies per selected ingredient
  - 1 reroll for the opening draft
- Reroll behavior is per individual choice slot.
- Round 2 supports curated synergy partners.
- `RegularRun.asset` currently includes 14 starting-draft ingredients and curated synergy lists.

### Ingredients / scoring
- Current playable ingredient data contains 14 ingredients:
  - Fruit: Apple, Blueberry, Orange, Pineapple
  - Protein: Bacon, Burger Patty, Egg, Sausage
  - Vegetable: Lettuce, Mushroom, Onion, Pickle, Red Chili, Tomato
- The Agent Update added dedicated special scoring-rule assets for the current roster.
- The Agent Update also added reusable +1 / +2 / +3 base-point scoring assets and assigned base scoring to ingredients.
- Exact balance values still require playtesting and a formal ingredient matrix.

### Customers
- Current `CustomerDefinition` assets: Chad, Glorb, Gregg.
- Current customer system supports:
  - goal score
  - coin reward
  - restrictions
  - optional preferences
- Current restriction types include BlenderScale, IngredientScale, DraftChoiceCount, and DropLimit.
- Current preference types include IngredientCount, TagCount, and ScoreOverGoal.

### Receipt
- `RoundReceiptUI` already existed and is wired in SampleScene.
- The receipt has now been extended to display:
  - customer description
  - base reward
  - optional special requests/preferences and their bonus coin value
  - customer restrictions
  - score goal
- The existing DescriptionText object in the scene was inactive in the serialized scene, so the receipt script now activates it when populating customer details.
- Visual spacing/layout still needs Unity inspection because longer receipts can exceed the current placeholder text area.

### Shop / Helpers
- Existing shop architecture and UI are present.
- Current helper assets: HeadStart, TipJar.
- Final target remains 20+ Helpers with cross-system interactions.

## Product targets locked so far

- 30+ ingredients.
- 20+ Helpers.
- 20+ customers.
- Challenging true-run structure with no active-run save/resume.
- Restrictions should alter the puzzle without routinely invalidating a build.
- Receipt is the central readable summary of customer conditions.
- Website-style shop remains the intended final shop presentation.
- Temporary UI creation/movement is authorized during implementation.
- Use only legal/free external resources unless explicit approval is given for something paid.

## Needs Unity validation

Because repository editing cannot run the Unity Editor, these should be checked locally after pulling the latest branch:

1. Project compiles after the latest receipt changes.
2. Starting Draft appears at run start.
3. Individual reroll replaces only one card and consumes the shared reroll.
4. First pick transitions into the second round and second pick starts Customer 1.
5. Runtime pantry contains exactly 3 copies of each selected ingredient.
6. Customer receipt visibly shows its DescriptionText after the new activation behavior.
7. Receipt still fits visually when a customer has multiple preferences/restrictions.
8. All 14 current ingredients apply both their base +1/+2/+3 score and their special rule.
9. Three-customer flow reaches shop/customer transitions and the Won state without stale ingredients or blocked UI.

## Immediate next work

Priority order:

1. Validate and harden the 3-customer vertical slice.
2. Build a formal 30-ingredient interaction matrix before adding large quantities of content.
3. Improve receipt layout after visual Unity feedback.
4. Audit current Helper architecture and expand it only after the vertical slice is stable.
5. Add a proper Win screen once the 3-customer flow is verified.

## Current milestone

`M3 — Three-customer vertical slice` is the main near-term target, while M1/M2 receive fixes discovered during testing.

A successful near-term build should play:

Main Menu → Starting Draft → Customer 1 → Assembly → Score → Shop → Customer 2 → Shop → Customer 3 → Win

without requiring Inspector changes during the run.

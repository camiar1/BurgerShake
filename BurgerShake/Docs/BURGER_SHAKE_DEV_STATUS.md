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
- There are now 10 `CustomerDefinition` assets matching the 10 existing customer portraits:
  - Chad
  - Chloe
  - Cole
  - Eve
  - Glorb
  - Gregg
  - Ms. Pam
  - Old Man Joe
  - Ronda
  - Tommy
- All newly created definitions point at their corresponding sliced portrait sprite and use the current `CustomerDefinition` visual fields.
- The 7 newly created customers are content-ready definitions but are not yet added to `RegularRun`; the vertical slice intentionally remains Chad → Glorb → Gregg.
- Base goal/reward values for the newly created customers are currently neutral placeholders matching the existing customer defaults and will be tuned when the full customer pool is designed.

### Customer restrictions
- Six reusable restriction assets now exist:
  - Compact Blender — blender is 8% smaller.
  - Tight Blender — blender is 14% smaller.
  - Chunky Ingredients — ingredients are 8% larger.
  - Big Ingredients — ingredients are 15% larger.
  - Two Choice Toss — cat toss offers 2 choices instead of 3.
  - Four Drop Order — at most 4 ingredients may be dropped.
- Current staged customer assignments:
  - Chad — none, intentionally starter-friendly.
  - Glorb — Compact Blender.
  - Gregg — Chunky Ingredients.
  - Chloe — Two Choice Toss.
  - Cole — Four Drop Order.
  - Eve — Compact Blender.
  - Ms. Pam — Chunky Ingredients.
  - Old Man Joe — Tight Blender.
  - Ronda — Big Ingredients.
  - Tommy — Two Choice Toss.
- These restrictions use the existing generic `CustomerRestriction` / `GameplayModifiers` architecture; no customer-specific gameplay branches were added.

### Receipt
- `RoundReceiptUI` is wired in SampleScene.
- The receipt has been extended to display:
  - customer description
  - base reward
  - optional special requests/preferences and their bonus coin value
  - customer restrictions
  - score goal
- The existing DescriptionText object in the scene was inactive in the serialized scene, so the receipt script activates it when populating customer details.
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

1. Project imports/compiles with the new customer and restriction assets.
2. Starting Draft appears at run start.
3. Individual reroll replaces only one card and consumes the shared reroll.
4. First pick transitions into the second round and second pick starts Customer 1.
5. Runtime pantry contains exactly 3 copies of each selected ingredient.
6. Chad plays with normal blender/ingredient settings.
7. Glorb's receipt shows `Blender is 8% smaller.` and the blender actually scales down for his challenge.
8. Gregg's receipt shows `Ingredients are 8% larger.` and dropped ingredients actually scale up for his challenge.
9. Restrictions reset correctly when moving from one customer to the next.
10. Receipt details remain readable with the new restriction section.
11. All 14 current ingredients apply both their base +1/+2/+3 score and their special rule.
12. Three-customer flow reaches shop/customer transitions and the Won state without stale ingredients or blocked UI.
13. Each new customer definition previews the correct portrait in the Inspector.

## Immediate next work

Priority order:

1. Validate and harden the 3-customer vertical slice, especially customer-to-customer modifier reset and the final Won transition.
2. Add/finish a proper Win presentation for the 3-customer slice.
3. Build a formal 30-ingredient interaction matrix before adding large quantities of ingredient content.
4. Improve receipt layout after visual Unity feedback.
5. Audit current Helper architecture and expand it only after the vertical slice is stable.

## Current milestone

`M3 — Three-customer vertical slice` is the main near-term target, while M1/M2 receive fixes discovered during testing.

A successful near-term build should play:

Main Menu → Starting Draft → Customer 1 → Assembly → Score → Shop → Customer 2 → Shop → Customer 3 → Win

without requiring Inspector changes during the run.

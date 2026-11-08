# Burger Shake — AI Development Handoff

Last updated: 2026-11-07
Repository: camiar1/BurgerShake
Active development branch: agent/full-run-integration
Branch head before this handoff file: 0516d1cc908ce08fbb7c3ea703eecaac73fbd338 — "Standardize Mult rewards to round values"
Default branch: main
Unity version last verified on the development machine: 6000.3.8f1

## 0. Read this first

This file is intended to let a new AI development agent continue Burger Shake without reconstructing months of context from chat history.

The active development branch is agent/full-run-integration. Do not assume main contains the latest gameplay work.

There are also known LOCAL-ONLY art edits on the user's development machine that were not committed when the machine was last audited. Do not reset, checkout over, clean, or otherwise discard local changes without inspecting them first. See the "Local-only work" section below.

The user strongly prefers direct implementation over repeated clarification questions. Make a reasonable best effort, preserve existing work, test changes, and report exactly what was changed.

## 1. High-level game concept

Burger Shake is a comedic/satirical physics-based ingredient-dropping game built in Unity.

The physical assembly gameplay is Suika-like in the sense that ingredients are dropped into a blender/container and settle with physics, but there is NO merge mechanic.

The player builds a "burger shake" from a run-based pantry. Scoring depends on:
- base Points from ingredients,
- special placement/contact/tag rules,
- Mult,
- ingredient-to-ingredient synergies,
- customer requests,
- run upgrades/helpers,
- shop/crate decisions.

Core score formula:

Total Score = Points × Mult

The intended design identity is:
- funny and satirical rather than serious/cute-only,
- expressive ingredient personalities,
- readable rules,
- lots of overlapping synergies,
- freedom to build rather than rigid recipes,
- physical placement should matter,
- most useful interactions should pay off immediately instead of requiring awkward minimum thresholds.

The user explicitly referenced the Omelet You Cook wiki as a FUTURE DESIGN/BALANCE REFERENCE:
https://omeletyoucook.miraheze.org/wiki/Main_Page

Important constraint:
Use it for inspiration around pacing, category identity, shop/build design, and synergy philosophy. DO NOT copy specific mechanics, ingredient rules, helpers, art, or progression one-to-one.

## 2. Important user design preferences already decided

Do not re-litigate these unless playtesting proves a real problem:

1. A scoring rule should reward EITHER Points OR Mult, not both.
2. The user dislikes many "minimum touch" gates because dead placements feel unfun.
3. Prefer immediate payoff:
   - good: "+1 Point per touching Vegetable"
   - weaker direction: "If touching at least 3 Vegetables, gain +4"
4. Rare harder mechanics are okay, but most of the roster should be useful from the first successful interaction.
5. Ingredient systems should synergize with each other:
   tags, placement, Points, Mult, customers, shop choices, crates, and helpers should reinforce the same builds.
6. The old Pickle bridge mechanic was disliked and intentionally removed.
7. Centerline scoring currently belongs only to Carrot and Cucumber.
8. The centerline should project across the whole blender, not stop at the ingredient's ends.
9. Centerline visual guide should appear only on hover.
10. Mult values should use satisfying round values such as +0.10, +0.15, +0.20, not +0.04/+0.06/+0.07/etc.
11. Art direction should lean funny/satirical and expressive.
12. Do not over-constrain the player into recipes.

## 3. Current run/game architecture

Primary state machine:
RunState:
- Setup
- StartingDraft
- CustomerIntro
- Assembly
- ScoreReveal
- CustomerOutro
- Shop
- Won
- Lost

Important systems/classes under Assets/Scripts:

Run:
- RunManager
- RunDefinition
- RunProgress
- CustomerChallengeController
- CustomerDefinition
- CustomerPreference
- CustomerRestriction
- GameplayModifiers
- StartingIngredientDraftController
- StartingPantryDefinition
- RunIngredientEntry

Draft:
- CatTossDraftController
- DraftChoiceVisual

Dropping:
- IngredientDraftManager
- IngredientDropper

Ingredients:
- Ingredient
- IngredientDefinition
- IngredientTag
- IngredientHighlight
- IngredientHoverController
- CenterlineHoverGuide
- IngredientPlaceholderSpriteFactory
- PolygonColliderScaler

Scoring:
- IngredientScoringRule
- ContactCountScoringRule
- OrientationScoringRule
- CrossTagScoringRule
- UniqueNeighborScoringRule
  - NOTE: filename historically has typo: UniqueNeightborScoringRule.cs
- BridgeScoringRule
- VerticalRelationshipScoringRule
- TagDiversityRule
- ClusterScoringRule
- CenterlineScoringRule
- ScoreManager
- ScoreRevealController
- ScoreUI
- ScoreValue
- IngredientScoreStep

Shop:
- IngredientCrateDefinition
- RunUpgradeDefinition
- ShopManager
- UpgradeManager

UI:
- IngredientTooltipUI
- IngredientTooltipTrigger
- IngredientChoiceButton
- StartingIngredientDraftUI
- ShopUI
- ShopOfferButton
- ShopIngredientOfferButton
- ShopUpgradeOfferButton
- ShopBuildSummaryUI
- RunStatusHUDController
- RoundReceiptUI
- CustomerIntroUI
- CustomerSPeechbubbleUI
- FinishShakeButtonVisibility
- WinScreenController
- LoseScreenController
- MainMenuController
- PauseMenuController
- settings/animation helpers

View:
- view controllers / transitions live under Assets/Scripts/View.

## 4. Scoring-rule semantics

IngredientScoringRule:
- targets: Self, TouchingAny, TouchingTag, TouchingIngredient
- rewards: Points or Mult
- default reward amount is generally amount × triggerCount

ContactCountScoringRule:
- counts touching ingredients, optionally filtered by tag
- minimum/maximum contact constraints are supported
- rewardPerContact false = one-time reward
- rewardPerContact true = amount × count

OrientationScoringRule:
- Horizontal / Vertical / Diagonal
- angle tolerance
- optional contact constraints

CrossTagScoringRule:
- requires neighbors from two specified tags/categories

UniqueNeighborScoringRule:
- distinct neighboring ingredient types
- can reward per unique neighbor
- can ignore first N neighbors for reward calculations

BridgeScoringRule:
- legacy mechanism for pairs of touching neighbors that do not touch each other
- still exists in code, but Pickle no longer uses it

VerticalRelationshipScoringRule:
- above / below / both / nothing above / nothing below relationships

TagDiversityScoringRule:
- counts how many configured tags/categories are represented among neighbors

CenterlineScoringRule:
- projected line/band through the ingredient
- currently used by Carrot/Cucumber
- details below

## 5. Current 30-ingredient roster

There are 30 IngredientDefinition assets:
- 10 Fruit
- 10 Protein
- 10 Vegetable

### Fruit

| Ingredient | Base | Special rule | Tags |
|---|---:|---|---|
| Apple | +2 Points | +0.10 Mult per unique neighboring ingredient type | Fruit, Round |
| Banana | +3 Points | +0.10 Mult per touching ingredient | Fruit, Long |
| Blueberry | +2 Points | +1 Point per touching ingredient | Fruit, Small, Round |
| Cherry | +2 Points | +0.10 Mult per touching Fruit | Fruit, Small, Round |
| Grape | +2 Points | +1 Point per touching Small ingredient | Fruit, Small, Round |
| Lemon | +2 Points | +0.10 Mult per touching Protein | Fruit, Small, Round |
| Orange | +2 Points | +0.10 Mult per touching Round ingredient | Fruit, Round |
| Pineapple | +3 Points | +2 Points per touching Vegetable | Fruit, Large |
| Strawberry | +2 Points | +2 Points per touching Fruit | Fruit, Small |
| Watermelon | +3 Points | +0.10 Mult per touching ingredient | Fruit, Large, Round |

### Protein

| Ingredient | Base | Special rule | Tags |
|---|---:|---|---|
| Bacon | +3 Points | +0.10 Mult per touching Long ingredient | Protein, Flat, Long, Salty, Greasy |
| Burger Patty | +3 Points | +2 Points per touching Protein | Protein, Round, Large |
| Cheese | +2 Points | +0.10 Mult per touching Protein | Protein, Dairy, Flat |
| Chicken Nugget | +2 Points | +2 Points per touching Fruit | Protein, Small |
| Egg | +2 Points | +0.15 Mult per ingredient touching below it | Protein, Round, Small |
| Ham | +2 Points | +0.10 Mult per touching Flat ingredient | Protein, Flat, Large |
| Meatball | +2 Points | +0.10 Mult per touching Small ingredient | Protein, Small, Round |
| Sausage | +2 Points | +2 Points per touching ingredient | Protein, Long, Salty |
| Shrimp | +2 Points | +0.15 Mult per touching Fruit | Protein, Small, Long |
| Tofu | +2 Points | +2 Points per touching Vegetable | Protein, Small, Flat |

### Vegetable

| Ingredient | Base | Special rule | Tags |
|---|---:|---|---|
| Avocado | +2 Points | +0.10 Mult per touching Vegetable | Vegetable, Round |
| Carrot | +3 Points | +1 Point per unique ingredient crossing its projected centerline anywhere in the blender | Vegetable, Long |
| Corn | +2 Points | +1 Point per touching Long ingredient | Vegetable, Long |
| Cucumber | +2 Points | +0.15 Mult per unique ingredient crossing its projected centerline anywhere in the blender | Vegetable, Long |
| Lettuce | +2 Points | +2 Points per ingredient touching above it | Vegetable, Flat, Large |
| Mushroom | +2 Points | +1 Point per touching Round ingredient | Vegetable, Small, Round |
| Onion | +2 Points | +1 Point per unique neighboring ingredient type | Vegetable, Round |
| Pickle | +2 Points | +1 Point per major ingredient category touching it | Vegetable, Small, Long |
| Red Chili | +2 Points | +0.15 Mult per major ingredient category touching it | Vegetable, Spicy, Small, Long |
| Tomato | +2 Points | +2 Points per touching Vegetable | Vegetable, Round |

## 6. Synergy-web philosophy

A major recent redesign intentionally paired Point builders and Mult builders around the same board states.

Examples:

Fruit clusters:
- Strawberry builds Points from Fruit neighbors
- Cherry builds Mult from Fruit neighbors

Protein clusters:
- Burger Patty builds Points from Protein neighbors
- Cheese builds Mult from Protein neighbors

Vegetable clusters:
- Tomato/Tofu build Points from Vegetable neighbors
- Avocado builds Mult from Vegetable neighbors

Small:
- Grape builds Points from Small
- Meatball builds Mult from Small

Long:
- Corn builds Points from Long
- Bacon builds Mult from Long

Variety:
- Onion builds Points from unique neighbors
- Apple builds Mult from unique neighbors

Major category diversity:
- Pickle builds Points from major categories
- Red Chili builds Mult from major categories

Vertical:
- Lettuce rewards things above it with Points
- Egg rewards things below it with Mult

General dense contact:
- Blueberry/Sausage are Point-oriented
- Banana/Watermelon are Mult-oriented

The goal is that one board state can benefit several ingredients and several systems, rather than every ingredient behaving like a disconnected puzzle.

## 7. Centerline mechanic

Current centerline ingredients:
- Carrot: +1 Point per unique ingredient crossing projected centerline
- Cucumber: +0.15 Mult per unique ingredient crossing projected centerline

Important behavior:
- the line is based on the ingredient's local axis and rotates with the ingredient
- scoring uses a narrow forgiving band rather than a mathematically zero-width line
- the projected line extends across effectively the entire play area
- serialized lineLength is 100 world units, intentionally much larger than the blender
- the crossed ingredient does NOT need to physically touch Carrot/Cucumber
- each unique ingredient counts at most once
- scoring detection does not alter physics colliders

Visual guide:
- visible only while hovering a centerline ingredient
- generated by CenterlineHoverGuide
- bright cyan/teal for visibility
- approximately 95% opacity
- dot spacing approximately 0.16 world units
- dot size approximately 0.09
- max dot pool approximately 256
- rendered above ingredient art
- visual line spans the visible play area/viewport diagonal and follows rotation
- visual guide is separate from scoring logic

Do not revert it to a short line tied to the physical ingredient length.

## 8. Starting draft

Current starter-eligible pool is 15 ingredients:

Fruit:
- Blueberry
- Strawberry
- Grape
- Watermelon
- Cherry

Protein:
- Burger Patty
- Cheese
- Tofu
- Chicken Nugget
- Ham

Vegetable:
- Tomato
- Corn
- Mushroom
- Avocado
- Carrot

The second starting choice is intentionally influenced by curated synergy partners after the first pick.

Known synergy map:

- Blueberry -> Watermelon, Grape, Mushroom
- Strawberry -> Cherry, Chicken Nugget, Watermelon
- Grape -> Watermelon, Mushroom, Blueberry
- Watermelon -> Blueberry, Strawberry, Grape
- Cherry -> Strawberry, Chicken Nugget, Grape
- Burger Patty -> Cheese, Ham, Chicken Nugget
- Cheese -> Burger Patty, Ham, Chicken Nugget
- Tofu -> Avocado, Tomato, Ham
- Chicken Nugget -> Strawberry, Cherry, Grape
- Ham -> Cheese, Tofu, Burger Patty
- Tomato -> Avocado, Tofu, Corn
- Corn -> Carrot, Tomato, Avocado
- Mushroom -> Grape, Blueberry, Burger Patty
- Avocado -> Tomato, Tofu, Corn
- Carrot -> Corn, Tomato, Avocado

This was designed so the first two ingredients begin a build instead of feeling like two disconnected random picks.

## 9. Regular run progression

RegularRun.asset current values last audited:

startingCoins: 0
allowCustomerRetry: false

Goal score by customer:
1. 17
2. 25
3. 34
4. 46
5. 59
6. 74
7. 91
8. 110
9. 131
10. 154

Dispenses:
- startingDispenses: 5
- customersPerDispenseIncrease: 2
- maxBaseDispenses: 9

So base dispenses increase approximately:
- customers 1–2: 5
- customers 3–4: 6
- customers 5–6: 7
- customers 7–8: 8
- customers 9–10: 9

Important:
The score curve was set before the latest Mult buff. Re-test this curve before assuming it is still correct.

## 10. Customers

Regular run customer order:

1. Gregg
2. Chad
3. Glorb
4. Chloe
5. Cole
6. Eve
7. Ms Pam
8. Old Man Joe
9. Ronda
10. Tommy

Base customer reward was last known as 5 coins each before request bonuses.

### Customer restrictions

Known restriction assets:
- BigIngredients
- ChunkyIngredients
- CompactBlender
- FourDropOrder
- TightBlender
- TwoChoiceToss

Last known assignments:
- Gregg -> Chunky
- Chad -> none
- Glorb -> Compact
- Chloe -> TwoChoice
- Cole -> none (FourDrop intentionally removed)
- Eve -> Compact
- MsPam -> Chunky
- OldManJoe -> Tight
- Ronda -> Big
- Tommy -> TwoChoice

### Customer preference/request assets

Current request pool last audited:

- Balanced Pair — use at least 2 different major ingredient types — +2 bonus coins
- Crowded Cup — create at least 7 ingredient-to-ingredient contacts — +3
- Egg Please — include at least 1 Egg — +3
- Fruit Fix — use at least 2 Fruit ingredients — +2
- High Standards — beat required score by at least 25 — +3
- Long Order — use at least 2 Long ingredients — +2
- Packed Tight — create at least 5 ingredient-to-ingredient contacts — +2
- Protein Punch — use at least 2 Protein ingredients — +2
- Push the Score — beat required score by at least 12 — +2
- Round Table — use at least 2 Round ingredients — +2
- Small Bites — use at least 2 Small ingredients — +2
- Three-Way Mix — use Fruit, Protein, and Vegetable in the same shake — +3

Recent shape-based assignments:
- Chad uses Long Order
- Chloe uses Small Bites
- Tommy uses Round Table

The point of these requests is to reward the same tag/build systems the player is already engaging with, not to create disconnected side quests.

## 11. Shop and crates

ShopManager was recently changed to make ingredient offers pantry-aware.

Crate ingredient candidates are still constrained by the crate's required tag, but choices are weighted toward ingredients whose scoring rules interact with the player's current pantry.

This weighting looks at things such as:
- tag-based contact rules
- TagDiversity
- centerline rules
- UniqueNeighbor
- VerticalRelationship
- TouchingAny
- TouchingTag

The intention:
- preserve randomness and discovery
- reduce "three completely irrelevant choices"
- allow a build to develop across the run

Monitor for over-snowballing.

### Current crate set

There are 8 crate types:

| Crate | Required tag | Last audited eligible pool |
|---|---|---:|
| Fruit Crate | Fruit | 10 |
| Protein Crate | Protein | 10 |
| Vegetable Crate | Vegetable | 10 |
| Small Bites Crate | Small | 13 |
| Roundabout Crate | Round | 14 |
| Long Haul Crate | Long | 9 |
| Flat Pack Crate | Flat | 5 |
| Big Batch Crate | Large | 5 |

Current crate behavior:
- 3 ingredient choices
- new ingredient generally grants 2 copies
- already-owned ingredient generally grants 1 copy
- current cost was 5 coins

Intentionally NOT added yet:
- Dairy crate
- Spicy crate
- Salty crate
- other tiny tag pools

Reason:
Those tags currently contain too few ingredients, making the crate overly deterministic.

## 12. Helpers / run upgrades

Known Helpers:

Counter Extension
- cost 6
- blender 6% wider
- intended to support stacking and centerline layouts

Extra Scoop
- cost 7
- permanently +1 dispense per customer
- gives all synergies another trigger opportunity

Head Start
- cost 7
- start each customer with +0.15 Mult
- helps Point-heavy builds

Prep Board
- cost 5
- start each customer with +3 Points
- helps Mult-heavy builds

Tiny Tongs
- cost 5
- ingredients 8% smaller
- helps dense contact and centerline setups

Tip Jar
- cost 3
- +2 bonus coins when satisfying a customer
- converts customer success into shop power

Recent description changes intentionally explain how Helpers connect to the broader build system.

## 13. UI/tooltips

Ingredient tooltips are implemented across:
- placed ingredients
- cat toss
- starting draft
- legacy selection
- shop ingredient offers

IngredientTooltipUI:
- ExecuteAlways
- code-built panel
- shows ingredient name
- primary type
- touching count when applicable
- description
- scoring rules
- tags
- follows cursor and clamps to screen

The current tooltip visual is a plain dark brown runtime-generated rectangle. It is functional but should be considered placeholder art.

Other recently generated/code-built UI that the user wants to replace with authored visuals:
- Ingredient Tooltip Frame
- Run Status HUD Panel
- Shop Build Summary Panel
- Shop Reroll Button
- Win Screen Main Panel
- Win screen primary/secondary buttons
- Win screen divider
- optional centerline dot sprite if desired

Existing authored UI PNGs already in Assets/Art/UI and should NOT be assumed placeholders:
- BlenderV1.png
- Capsule.png
- ReadyButton.png
- Reciept.png
- SpeechBubble.png
- Square.png
- TV.png
- WindowCustomerUI.png

The user wants a more coherent/funny polished UI art pass but does not want functionality lost.

## 14. Ingredient art ownership and art direction

The user originally created 14 ingredients:

- Apple
- Bacon
- Blueberry
- Burger Patty
- Egg
- Lettuce
- Mushroom
- Onion
- Orange
- Pickle
- Pineapple
- Red Chili
- Sausage
- Tomato

16 ingredients were introduced during assistant-driven roster expansion:

- Banana
- Strawberry
- Avocado
- Carrot
- Cucumber
- Cheese
- Tofu
- Chicken Nugget
- Ham
- Meatball
- Shrimp
- Corn
- Grape
- Watermelon
- Cherry
- Lemon

The user has asked to improve/replace assistant-generated ingredient sprites with higher-quality art that fits the game's funny/satirical vibe.

Art goals:
- readable silhouette at gameplay scale
- expressive anthropomorphic face
- amusing/satirical personality
- polished enough for Steam/storefront use
- transparent background for ingredient sprite assets
- physical shape should remain suitable for collider/placement identity
- do not make every ingredient's expression identical
- avoid generic AI-looking rendering
- preserve the ingredient's recognizable food form

Important:
Generated sprite/contact sheets from prior AI iterations are NOT authoritative. Some image generations accidentally substituted wrong ingredients (for example broccoli/potato) and should be treated only as visual brainstorming, not a roster definition.

## 15. LOCAL-ONLY ART WORK — DO NOT CLOBBER

This section is critical.

On the last direct development-machine audit (2026-10-27), the working tree had uncommitted art/prefab changes beyond GitHub branch head.

Known local-only/new files included:
- Assets/Art/Ingredients/BananaV1.png
- Assets/Art/Ingredients/CheeseV2.png
- Assets/Art/Ingredients/CherryV1.png
- Assets/Art/Ingredients/GrapesV1.png
- Assets/Art/Ingredients/LemonV1.png
- Assets/Art/Ingredients/StrawberryV1.png
- Assets/Art/Ingredients/WatermelonV1.png

Known deleted/replaced older files included:
- Assets/Art/Ingredients/Cheese.png
- Assets/Art/Ingredients/Strawberry.png

Known modified prefabs included:
- Fruit/Banana.prefab
- Fruit/Cherry.prefab
- Fruit/Grape.prefab
- Fruit/Lemon.prefab
- Fruit/Strawberry.prefab
- Fruit/Watermelon.prefab

Known modified Fruit IngredientDefinition assets included:
- Banana
- Cherry
- Grape
- Lemon
- Strawberry
- Watermelon

Important issue discovered:
At that audit, the prefab SpriteRenderers were pointed at several new sprites, but IngredientDefinition.sprite was NULL for:
- Banana
- Cherry
- Grape
- Lemon
- Watermelon

This matters because draft/shop UI reads IngredientDefinition.sprite directly. The placed prefab can look correct while the draft/shop icon appears blank.

Strawberry did not show the same confirmed null-reference issue in that audit.

There was also an unrelated local modification to:
Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset

Before making art changes:
1. inspect git status
2. inspect these local files
3. do NOT git reset/clean
4. do NOT overwrite user-created V1/V2 replacement art blindly
5. fix IngredientDefinition.sprite references once final replacement art is chosen
6. preserve .meta/GUIDs when replacing art in-place where practical

As of 2026-11-07, Desktop Commander was offline, so these local changes could not be re-verified. Treat this section as "last known local state" until the machine is available again.

## 16. Recent important commits

Recent gameplay sequence on agent/full-run-integration:

0516d1c — Standardize Mult rewards to round values
- moved odd Mult values to clean +0.10 / +0.15 tiers

4b6844d — Add tag-based ingredient crates
- Small / Round / Long / Flat / Large crates added

cdf27d3 — Improve centerline guide visibility
- bright cyan, denser dots, stronger visibility

3a21687 — Show dotted centerline on hover
- centerline guide only appears on hover

600c02e — Extend centerlines across blender
- projected centerlines extend through the full play area

e0d4ba0 — Connect ingredient systems into synergy web
- large scoring/tag/customer/shop/helper integration pass
- Pickle bridge removed
- centerline rule introduced
- starting synergy map refreshed
- pantry-aware shop weighting added

0f7ac06 — Complete 30 ingredient prefab roster
- structural 30-prefab roster completed

Earlier relevant commits:
374f223 — Add dedicated prefabs for generated ingredients
18c971c — Add generated art for new ingredients
fd97985 — Strengthen ingredient synergies and reduce self stacking
fa400bc — Expand to balanced 30 ingredient roster
c433737 — Refine starter draft and soften score curve
a4b1642 — Improve tooltip positioning and shop build feedback
8bb948a — Expand customer requests helpers and run HUD
4e7e8c1 — Show ingredient tooltips during selection
a1d1479 — Make win and tooltip UI editor-visible and fix hover
96cda6b — Rebalance run difficulty and add ingredient tooltips
09ece13 — Integrate full customer run flow and retry cadence

## 17. Known balance risks / things to playtest

Do NOT immediately "fix" these without playtesting, but watch them closely.

### Cucumber
Current rule:
+0.15 Mult per unique projected centerline crossing.

Because the line spans the whole blender and does not require physical touching, Cucumber may scale extremely hard.
Example:
5 crossings = +0.75 Mult from one ingredient.

This may be fun/high-skill, but it could become an automatic pick.

### Banana / Watermelon
Both are broad:
+0.10 Mult per touching ingredient.

Dense piles can easily create several contacts, so these may be highly reliable Mult engines.

### Sausage vs Blueberry
Sausage:
- base +2
- +2 Points per touching ingredient

Blueberry:
- base +2
- +1 Point per touching ingredient

Sausage may be broadly stronger unless physical size/tag/build identity creates enough tradeoff.

### Score curve
The current 17–154 customer goals were not re-derived after raising Mult rewards.
Later customers may now be easier than intended.

### Pantry-aware shop weighting
This is supposed to make a build coherent.
Risk:
once a build starts, the shop could feed it so aggressively that choices stop being interesting.

### Flat Pack / Big Batch crates
Only about 5 eligible ingredients each at last audit.
They are much more predictable than Small/Round crates.

### Tag identity
Shape tags are now meaningful, but future design should make a "Long build", "Small build", "Round build", etc. feel like a playstyle rather than just a filter label.

## 18. Current development priorities

Recommended order:

1. Reconnect to the development machine and inspect LOCAL uncommitted art.
2. Preserve/finish the replacement ingredient sprites already in progress.
3. Fix IngredientDefinition.sprite references for the local replacement fruit art.
4. Continue the art-quality pass for remaining assistant-created ingredients.
5. Replace recent code-generated UI panels with authored Burger Shake UI frames while preserving logic.
6. Playtest the +0.10/+0.15 Mult economy.
7. Record real achieved scores across all 10 customers.
8. Retune customer score targets from data.
9. Evaluate Cucumber/Banana/Watermelon/Sausage strength.
10. Evaluate pantry-aware shop weighting for over-snowballing.
11. Expand tiny tag pools before adding Dairy/Spicy/Salty crates.
12. Only then add major new systems/content.

Avoid adding lots of new systems before the current run loop is properly balanced and polished.

## 19. Technical workflow / safety rules for future agents

If Remote Desktop Commander is available, use it for the live project.

Last known live project location on Windows:
C:\Users\camia\OneDrive\Desktop\Unity Projects\BurgerShake\BurgerShake\BurgerShake

Last known git repo root:
C:\Users\camia\OneDrive\Desktop\Unity Projects\BurgerShake\BurgerShake

Do NOT use an older/duplicate project at:
C:\Users\camia\BurgerShake

Unity editor path last verified:
C:\Program Files\Unity\Hub\Editor\6000.3.8f1\Editor\Unity.exe

Before edits:
- git status --short
- git branch --show-current
- git log -1 --oneline

Preserve unexpected user changes.

Do not kill Unity.

Prefer filesystem/code edits over GUI automation where possible.

When Unity is running:
- refresh assets/scripts
- inspect Editor.log for:
  - error CS
  - Compilation failed
  - Scripts have compiler errors
  - deserialization/YAML failures
  - runtime exceptions

Before commit:
- inspect git diff
- git diff --check
- stage only coherent intended changes
- do not accidentally include unrelated TMP/font/editor changes
- commit with a clear message
- push agent/full-run-integration

Be truthful about what was actually changed and tested.

## 20. Important gameplay behavior already implemented

- Score = Points × Mult
- ingredient draft / pantry system
- weighted pantry bag/discard behavior
- customer progression
- customer restrictions
- customer preferences / bonus coins
- score reveal
- shop
- ingredient crates
- Helpers/upgrades
- persistent RunProgress
- win/lose flow
- ingredient tooltips
- hover highlighting
- centerline hover guide
- collision-aware dropping
- rotation
- drop/dispense limits
- finish-shake visibility gating
- cat toss draft flow
- starting draft
- pantry-aware crate choice weighting

Do not replace working systems with a simpler mock version just to make a new feature easier.

## 21. Design review summary

Burger Shake recently transitioned from:

"each ingredient has its own isolated scoring puzzle"

toward:

ingredient
-> tags
-> neighboring ingredients
-> physical placement
-> Points / Mult
-> customer request
-> coins
-> shop
-> crate
-> new ingredient
-> Helper
-> stronger build

That interconnected loop is intentional and should be protected.

The next phase should be polish + balance, not another wholesale redesign.

## 22. When continuing from this handoff

First actions for a new AI agent:

1. Read this file fully.
2. Check the current branch and commit.
3. Check for local uncommitted changes before doing ANY reset/pull/checkout.
4. If Desktop Commander is available, inspect the local art status described above.
5. Run Unity compile/play-mode sanity check before assuming the project is healthy.
6. Confirm whether the user wants to continue art polish, UI polish, or balance testing.
7. Preserve the design constraints in Section 2.

If anything in this file conflicts with the live project, the live project wins, but investigate why before overwriting or reverting anything.


## 23. Collider authoring update — 2026-11-07

Commit 5970d86 replaces PolygonColliderScaler's automatic percentage-scaling behavior with an explicit outline authoring tool. Class name, script GUID, saved originalPaths and legacy scale fields are retained. No ingredient prefabs or sprite assets were regenerated.

- No ExecuteAlways, OnEnable regeneration, or OnValidate collider writes. Existing saved collider paths survive reloads and spawning; manual Edit Collider changes persist.
- Inspector buttons: Generate From Sprite + Inset; Apply Inset From Captured Outline; Capture Manual Edits (Reset Inset to 0); Restore Captured Outline.
- Generation reads Sprite.GetPhysicsShape, transforms renderer coordinates into collider-local space, accounts for flipX/flipY and Collider2D.offset, and captures the clean source outline.
- Inset Distance is an even edge offset in collider-local units, NOT a percentage. Default 0.02. Simplification Tolerance defaults to 0.005; use 0 to keep all source vertices.
- Apply Inset always starts from the captured source, avoiding cumulative shrinking. Capture Manual Edits saves current points unchanged and resets inset/simplification to 0.
- Degenerate/self-intersecting, overlapping or nested outlines are rejected before mutation. Narrow features cause the tool to retry smaller insets and report the effective amount.
- This tool supports simple solid outlines and separate islands. It does not interpret nested loops as holes. Sprite Editor > Custom Physics Shape is the source for silhouette detail; a missing physics shape produces a warning rather than a rectangle fallback.
- Undo, prefab instance override recording and scene dirty marking are included. Authoring actions are blocked in Play Mode. Saved collider paths are used at runtime.
- Legacy ApplyColliderScale remains as an explicit context-menu action only.

Suggested workflow: open ingredient prefab outside Play Mode, start with Inset Distance 0.02 and Simplification Tolerance 0.005, click Generate From Sprite + Inset, then manually touch up vertices and save. No Bake button is needed because regeneration is never automatic. Click Capture Manual Edits only when those edits should become the baseline for subsequent insets.

Verification: whitespace/diff checks and Python-ported geometry checks passed for both windings, convex/concave shapes, narrow stems, excessive inset, invalid/multiple paths, simplification error bounds and the actual committed Watermelon collider (66 to 52 vertices, effective inset 0.02). Source checks verified no automatic path writes. These checks are not a Unity compilation or Play Mode test. Both Desktop Commander devices were offline, so local application, Unity compilation, Inspector interaction, Undo and prefab reload checks remain to be verified on the user's machine.

Earlier handoff details superseded by recent committed work: the Fruit Art overhaul is now on GitHub; the previously local-only replacement art was committed. Five fruit IngredientDefinition.sprite references remain empty (Banana, Cherry, Grape, Lemon, Watermelon), and RegularRun now has 10 startingDraftIngredients rather than the older documented 15.


## 24. Strong inset correction — 2026-11-07

The screenshot exposed a limitation in section 23's first implementation: requested inset 0.53 was repeatedly halved to 0.03313 to preserve the apple stem. That global reduction has been removed.

PolygonColliderScaler now uses Clipper2 polygon erosion with the exact requested distance (round joins, precision 5, arc tolerance 0.002). Narrow features may disappear and narrow connections may split into separate collider paths. If erosion removes the entire outline, or Unity-coordinate conversion yields invalid paths, the saved collider remains unchanged and a warning is shown. No fallback silently reduces the inset. Inspector status reports the applied requested distance.

The pinned Clipper2 C# dependency is included under Assets/ThirdParty/Clipper2 with a separate auto-referenced assembly, licenses, provenance and Unity metadata. Pull the complete commit, including that directory. Do not copy only PolygonColliderScaler.cs.

Manual-edit protection, snapshot capture, Undo, renderer coordinate conversion, and no automatic runtime/reload regeneration are preserved. Existing prefabs are not regenerated. With an existing captured outline, set the desired Inset Distance and click Apply Inset From Captured Outline. Use Generate From Sprite + Inset to start from sprite physics geometry again.

Verification: native Clipper2 from the same pinned release executed 10 regression cases covering a full 0.53 inset, reversed winding, narrow stems disappearing, a narrow waist splitting, separate islands, complete erosion, zero inset, and the actual committed Apple and Watermelon source outlines. Apple area changed from 8.1440 to 3.4625 at 0.53 (about 57% reduction). Source checks confirm removal of global halving and preservation of no automatic collider rewriting and Undo. These are native geometry and source checks, not a Unity/C# compilation or editor-interaction test; Unity verification remains outstanding.

End of handoff.

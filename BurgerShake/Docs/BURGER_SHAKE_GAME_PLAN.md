# Burger Shake — Game Plan

This document is the current source of truth for the intended finished game. Systems should stay data-driven and reusable so content can grow without ingredient-, helper-, or customer-specific manager code.

## Core pitch

Burger Shake is a physics-based food roguelike. The player builds a pantry, drafts ingredients from a cat toss, physically drops and rotates them into a blender, exploits spatial scoring interactions, serves increasingly strange customers, and upgrades the run through a deliberately weird online food-supplier shop.

The game should create the feeling of: "I know what I could do better next run." Difficulty should come from build decisions, physical execution, customer adaptation, and drafting/shop variance rather than arbitrary hard counters.

## Final content target

- At least 30 ingredients.
- At least 20 Helpers.
- At least 20 customers.
- 3 ingredient crate categories: Protein, Vegetable, Fruit.
- Exactly 4 shop offers: 2 distinct crate categories + 2 Helpers.
- A normal run is currently targeted at 7 customers, subject to playtesting.
- No save/resume for an active run. Quitting abandons the run.
- Persistent settings and optional future unlock/stat tracking are allowed, but they must not preserve an active run.

## Run flow

1. Start a new run.
2. Complete the Starting Ingredient Draft.
3. Meet the current customer at the truck window.
4. Read the receipt: score goal, reward, preference(s), and restriction(s).
5. Enter Assembly.
6. Repeatedly choose 1 of 3 cat-toss ingredients and physically drop it into the blender.
7. Finish the shake and reveal Points × Mult.
8. If the goal is missed, the run ends.
9. If the goal is met, collect coins and optional preference bonuses.
10. Visit the website-style shop between customers.
11. Continue until the final customer is beaten or the run is lost.

## Starting Ingredient Draft

The opening draft replaces fixed starting pantries.

- 2 selection rounds.
- 3 distinct ingredient choices per round.
- Pick exactly 1 ingredient each round.
- Each selected ingredient contributes 3 pantry copies.
- The two selected ingredients must be different.
- Starting pantry size: 6 copies total.
- Ingredients already shown are normally excluded from later starting-draft offers.
- The first selected ingredient is always excluded from round 2.
- Round 2 tries to include 1 curated compatible partner plus 2 random unseen ingredients.
- 1 reroll is available for the whole starting draft.
- Rerolls are per-slot: rerolling one ingredient replaces only that ingredient.
- A rerolled-away ingredient counts as seen during the starting draft.
- Rejected/rerolled ingredients are NOT banned from the run; they may appear in shops/crates later.
- Starting-draft selection ignores normal gameplay draft weights.

## Pantry and gameplay drafting

The pantry acts like the run's deck.

- Each ingredient definition has a runtime copy count.
- More copies make that ingredient more represented in normal customer drafts.
- Cat toss shows 3 choices by default.
- All shown choices consume a physical copy from the current draw bag and enter discard, including unchosen options.
- Drawing is without replacement from the copy bag.
- If the draw bag empties during a hand, discard is recycled immediately.
- A new draft cycle begins for each customer.
- Duplicate visible choices are allowed when the pantry composition produces them, but avoid a triple-identical hand when another definition is available.

## Ingredient design

Each ingredient has two scoring layers:

1. A base drop value of +1, +2, or +3 Points so every ingredient contributes even when its special condition fails.
2. A unique spatial/scoring rule that creates build decisions.

The 30+ ingredient roster should form an interaction network rather than 30 isolated gimmicks. Reusable concepts include:

- touching / contact count
- same-category and cross-category contact
- same-ingredient and mixed-ingredient neighbors
- above / below / between relationships
- bridges
- clusters
- isolated placement
- crowded placement
- edge/center placement
- orientation/rotation
- ingredient size
- drop order
- first/last drop
- points-focused vs Mult-focused effects

A good ingredient should be usable on its own but become meaningfully stronger through synergy. Exact named-ingredient dependencies should be uncommon; broader tags and spatial relationships are preferred so many combinations can emerge.

## Scoring

Final score is Points × Mult.

- Starting Mult is 1.
- Base drop scores mainly provide a reliable Points floor.
- Special rules can add Points or Mult.
- High-Mult strategies should generally require more positional/build commitment than basic Points strategies.
- Score reveal should eventually show abilities triggering instead of jumping immediately to one total.

## Customers

Target: 20+ customers with different personalities and gameplay conditions.

Each customer can define:

- portrait and presentation
- dialogue
- base score goal
- base coin reward
- optional preferences for bonus coins
- one or more restrictions

### Restriction philosophy

Restrictions should change the puzzle without deleting the player's build. Favor:

- blender size changes
- ingredient size changes
- drop-count changes
- draft-choice-count changes
- narrower placement zones
- rotation/placement twists
- mild physics changes
- reward/risk tradeoffs

Avoid frequent hard counters such as "Protein does not score" or "your best Helper is disabled." Those may exist only as rare challenge-mode ideas, not the normal run experience.

### Receipt

The receipt is the clean central summary of the customer's order. It should show, in a compact readable layout:

- customer name
- goal score
- base reward
- preference(s) and bonus reward
- restriction(s)

Restrictions should read like customer/order instructions rather than videogame debuffs.

## Shop

Final presentation direction: a strange, hand-drawn website storefront used by the food truck after each successful customer.

- Exactly 4 offers.
- 2 distinct crate categories selected from Protein / Vegetable / Fruit.
- 2 Helpers.
- Purchased offers become visibly sold/unavailable.
- Website can contain deliberately bad ads, fake reviews, support-cat widgets, error messages, and other flavor as long as strategic information remains immediately readable.

### Crates

Buying a crate opens an ingredient draft.

- Player chooses from a small set of ingredients in the crate category.
- New ingredient: add it to the pantry with multiple starter copies (current target: +2).
- Already-owned ingredient: reinforce it with fewer copies (current target: +1).

## Helpers

Target: 20+ Helpers.

Helpers are run-defining rule modifiers, not primarily flat stat sticks. Good Helpers should change what the player wants to draft, buy, or physically do. Useful design spaces include:

- reroll manipulation
- draft size/choice manipulation
- pantry copy manipulation
- category bonuses
- first/last drop effects
- orientation effects
- contact/neighbor modifiers
- scoring-rule amplification
- shop/economy changes
- placement control
- risk/reward conditions

Helpers should deliberately overlap with multiple ingredient mechanics so unexpected combinations emerge.

## Difficulty and replayability

The desired loop is challenging but fair enough to create "one more run" pressure.

Difficulty should rise through:

- higher score goals
- stronger need for synergy
- more consequential shop/pantry decisions
- progressively more interesting customer restrictions
- physical placement pressure

Randomness should create different problems, not decide wins by itself. Starting drafts, normal drafts, and shops should have anti-frustration protections where needed without removing uncertainty.

## Losing and winning

Failing a required customer goal ends the run. There is no mid-run save/resume safety net.

End screens should eventually summarize useful run information such as:

- customers cleared / day reached
- final pantry
- Helpers
- best shake / best score
- coins earned

The intended reaction to a loss is "I want to try that build again differently," not "I lost progress to an arbitrary rule."

## Presentation direction

Keep the existing handmade Burger Shake identity: rough, warm, hand-drawn, weird, food-truck-ish, and slightly suspicious. Do not polish the game into generic clean mobile UI.

Temporary UI is allowed. During development, functional placeholder panels, cards, buttons, text, icons, anchors, and layout changes may be created or moved without waiting for finished art. Existing finished character/ingredient/background art should be preserved unless a change is necessary or explicitly requested.

## Development constraint

Use legal/free resources only unless the owner explicitly approves a paid resource later. Prefer original implementation, Unity/package functionality already in the project, free/open-source tooling with compatible licenses, and user-provided assets.

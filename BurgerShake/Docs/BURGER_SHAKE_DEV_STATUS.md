# Burger Shake — Development Status

This file tracks what is currently known, what changed recently, what still needs Unity validation, and what should be worked on next.

## Current branch

- Repository: camiar1/BurgerShake
- Working branch: agent/ingredient-batch-followup (based on agent/burger-shake-foundation)
- User baseline: `Agent Update`, followed by local Unity fixes in `updated from agent errors`.
- Development has explicit permission to modify gameplay systems, create/move placeholder UI, and use legal/free resources.

## Run / vertical slice

- `RunManager` supports StartingDraft, CustomerIntro, Assembly, ScoreReveal, CustomerOutro, Shop, Won, and Lost.
- Active slice remains Chad → Glorb → Gregg.
- Passing the final customer goes to `RunState.Won`.
- Runtime placeholder Win screen shows customers served, final score, coins, pantry, Helpers, Play Again, and Main Menu.

## Starting Ingredient Draft

- 2 rounds, 3 choices each, 3 copies per selected ingredient, 1 shared per-slot reroll.
- Round 2 supports curated synergy partners.
- `RegularRun` starter eligibility has expanded from 14 to **21 ingredients** with the first placeholder content batch.

## Playable ingredient content

Existing final-art ingredients:
- Fruit: Apple, Blueberry, Orange, Pineapple
- Protein: Bacon, Burger Patty, Egg, Sausage
- Vegetable: Lettuce, Mushroom, Onion, Pickle, Red Chili, Tomato

New temporary-playable ingredients:
- **Cheese** — +1 base; +0.15 Mult per touching Protein.
- **Tofu** — +1 base; +2 Points per touching Vegetable.
- **Avocado** — +2 base; +0.3 Mult when touching both a Protein and a Vegetable.
- **Strawberry** — +2 base; +2 Points per touching Fruit.

- **Banana** — +3 base; +0.3 Mult within 15 degrees of horizontal.
- **Carrot** — +3 base; +0.25 Mult within 15 degrees of vertical with at least one ingredient contact.
- **Cucumber** — +2 base; +4 Points within 15 degrees of horizontal with at least two ingredient contacts.

The orientation batch uses the shared placeholder prefab and colored, elongated procedural sprites. All three have a horizontal local long axis, including Carrot, so the existing rotation-based rule measures their physical orientation correctly. Their temporary box colliders match the 1.8 by 0.84 visible silhouette bounds; curved/tapered silhouettes remain future art work.

The scene shop pool now includes all 21 ingredients. The previous four additions were missing from that pool even though they were in the opening draft. New ingredients also have curated second-round draft partners.

The final first-pass roster remains 30 ingredients. The remaining planned additions are Chicken Nugget, Ham, Meatball, Shrimp, Corn, Grape, Watermelon, Cherry, and Lemon.

## Temporary ingredient art / physics framework

- `IngredientDefinition` now contains data-driven placeholder visual fields: shape, color, and temporary collider size.
- `IngredientPlaceholderSpriteFactory` procedurally creates a rough colored ingredient sprite with a dark hand-drawn-style outline when final art is missing.
- Existing ingredients with real sprites are unaffected because their placeholder shape defaults to `None`.
- A shared `PlaceholderIngredient.prefab` supplies temporary Rigidbody2D / SpriteRenderer / Ingredient / BoxCollider2D components.
- `Ingredient.Initialize` assigns the generated sprite and sizes the placeholder collider from the ingredient definition.
- This lets future ingredients become playable before final PNG art/prefabs exist, without hardcoding ingredient names in gameplay managers.

## Scoring framework

- Contact Count supports optional tag filtering.
- Unique Neighbor supports thresholded reward counts.
- Orientation scoring supports horizontal, vertical, and diagonal conditions.
- Bacon uses 2+ touching Proteins for its Mult condition.
- Apple begins special Mult scaling at its 3rd unique neighbor.

## Customers / restrictions

- 10 current CustomerDefinition assets match existing portraits.
- Reusable restrictions include Compact Blender, Tight Blender, Chunky Ingredients, Big Ingredients, Two Choice Toss, and Four Drop Order.
- Blender-width restrictions squeeze both BlenderRoot physics and BlenderFront art on X only; user confirmed this works.
- Receipt displays description, reward, preferences/bonuses, restrictions, and score goal.

## Shop / Helpers

- Existing shop architecture remains present.
- Current Helper assets: HeadStart and TipJar.
- Final target: 20+ Helpers with cross-system interactions.

## Locked product targets

- 30+ ingredients.
- 20+ Helpers.
- 20+ customers.
- No active-run save/resume.
- Restrictions should change the puzzle without routinely invalidating a build.
- Website-style shop remains the final presentation direction.

## Needs Unity validation

After pulling this batch, check:

1. Project imports/compiles without errors.
2. Cheese, Tofu, Avocado, and Strawberry can appear in the opening draft.
3. Each new ingredient displays a colored temporary sprite rather than an empty card.
4. Selecting a new ingredient produces a visible drop preview.
5. Dropping it creates a physical ingredient in the blender with collision.
6. Cheese and Tofu receive +1 base; Avocado and Strawberry receive +2 base.
7. Cheese gains Mult from Protein neighbors; Tofu gains Points from Vegetable neighbors; Avocado requires both Protein and Vegetable neighbors; Strawberry gains Points from Fruit neighbors.
8. Existing 14 ingredients still render and score normally.
9. Chad → Glorb → Gregg restrictions and the Win screen still work.

## Immediate next work

1. Fix any Unity import/serialization issues from the new placeholder-content framework.
2. Add the next placeholder ingredient batch using the same generic system.
3. Begin the formal 20-Helper interaction matrix once the first ingredient batch is locally confirmed.
4. Expand customers/restrictions toward 20 after Helper vocabulary stabilizes.

## Current milestone

`M4 — Ingredient content expansion` is active. The project now has 21 starter-eligible ingredients, including seven additions.

## Orientation batch verification

Static asset validation checks all new GUID references, rule types and values, base scores, category tags, valid physics prefab components, draft eligibility/partners, and shop inclusion. Unity is unavailable in the development environment; import, rendered UI, actual collisions, and a complete run are not yet playtested.

In Unity, check the three new pieces in draft cards and drop previews, then rotate/drop them. Banana scores 3 Points plus 0.3 Mult horizontally and just 3 Points vertically. Carrot scores 3 Points plus 0.25 Mult vertically only with an ingredient neighbor. Cucumber scores 6 Points horizontally with two or more ingredient neighbors, otherwise 2 Points. Blender walls do not count as ingredient neighbors. Test flipped orientations and both sides of the 15-degree cutoff. Check the seven additions in their matching shop crate, then finish the Chad → Glorb → Gregg run.

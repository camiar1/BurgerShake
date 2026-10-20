# Burger Shake — 30 Ingredient Roster

This is the first content-complete ingredient design pass for the final game target. It defines the intended interaction network before the remaining art/prefabs/ScriptableObjects are created.

**Important:** the 14 ingredients already in the project remain the only fully implemented ingredient content right now. The 16 new entries below are design-locked targets, not yet added to `RegularRun` or crates. Their exact numbers are initial tuning values and can change during playtesting.

## Design rules

- Every ingredient always contributes a base drop value of **+1, +2, or +3 Points**.
- +1 pieces are generally flexible, compact, or synergy-heavy.
- +2 pieces are general-purpose.
- +3 pieces are generally awkward, large, rolling, or have a lower/reliability tradeoff.
- Every ingredient also has a special spatial/scoring identity.
- Most synergies use broad tags, geometry, or neighbor patterns instead of requiring one exact named partner.
- Same-ingredient dependencies are reserved for a small number of intentional cluster pieces.
- A player should be able to discover several viable build directions from almost any two starter ingredients.

## Protein — 10

| Ingredient | Status | Base | Target special ability | Main role |
|---|---|---:|---|---|
| Burger Patty | Existing | +3 | **+2 Points per touching Protein.** | Reliable Protein engine |
| Bacon | Existing | +3 | **+0.5 Mult if touching at least 2 Proteins.** | High-value Protein payoff |
| Egg | Existing | +2 | **+0.5 Mult if nothing is touching above it.** | Top-of-stack precision |
| Sausage | Existing | +3 | **+1 Point per touching ingredient.** | Crowding / contact payoff |
| Cheese | Planned | +1 | **+0.15 Mult per touching Protein.** | Flexible Protein multiplier |
| Chicken Nugget | Planned | +2 | If connected to at least 1 other Nugget, **+1 Point per member of its connected Nugget cluster.** | Same-type cluster build |
| Ham | Planned | +2 | **+0.3 Mult if something touches it above and below.** | Sandwich / vertical payoff |
| Meatball | Planned | +3 | **+5 Points if touching exactly 2 ingredients.** | Precision contact count |
| Shrimp | Planned | +2 | **+0.15 Mult per touching Fruit.** | Protein/Fruit crossover |
| Tofu | Planned | +1 | **+2 Points per touching Vegetable.** | Protein/Vegetable connector |

## Vegetable — 10

| Ingredient | Status | Base | Target special ability | Main role |
|---|---|---:|---|---|
| Lettuce | Existing | +1 | **+4 Points if something touches it above and below.** | Sandwich geometry |
| Mushroom | Existing | +2 | Looking only at Protein / Vegetable / Fruit neighbors, if at least 2 categories are represented, **+2 Points per represented category.** | Category diversity |
| Onion | Existing | +2 | **+1 Point per unique neighboring ingredient type.** | Variety / connector |
| Pickle | Existing | +2 | **+5 Points when it bridges two neighbors that are not touching each other.** | Bridge geometry |
| Red Chili | Existing | +2 | **+0.15 Mult per touching Protein.** | Spicy Protein multiplier |
| Tomato | Existing | +1 | **+2 Points per touching Vegetable.** | Vegetable engine |
| Avocado | Planned | +2 | **+0.3 Mult if touching both a Protein and a Vegetable.** | Mixed-category payoff |
| Carrot | Planned | +3 | **+0.25 Mult when nearly vertical and touching at least 1 ingredient.** | Orientation / precision |
| Cucumber | Planned | +2 | **+4 Points when nearly horizontal and touching at least 2 ingredients.** | Orientation / bridge-like placement |
| Corn | Planned | +3 | **+2 Points per unique neighboring ingredient type after the first.** | Diversity payoff with a floor |

## Fruit — 10

| Ingredient | Status | Base | Target special ability | Main role |
|---|---|---:|---|---|
| Apple | Existing | +2 | Starting with its 3rd unique neighboring ingredient type, **+0.1 Mult per unique type.** | High-diversity multiplier |
| Blueberry | Existing | +1 | **+2 Points per touching ingredient.** | Tiny gap-filler / dense scoring |
| Orange | Existing | +2 | **+4 Points if touching exactly 1 ingredient.** | Precise low-contact placement |
| Pineapple | Existing | +3 | **+5 Points if touching both a Protein and a Vegetable.** | Cross-category anchor |
| Banana | Planned | +3 | **+0.3 Mult when nearly horizontal.** | Awkward orientation payoff |
| Strawberry | Planned | +2 | **+2 Points per touching Fruit.** | Fruit engine |
| Grape | Planned | +1 | In a connected Grape cluster of at least 3, **+0.05 Mult per cluster member.** | Same-type cluster multiplier |
| Watermelon | Planned | +3 | **+0.5 Mult if touching at least 3 ingredients.** | Large crowded centerpiece |
| Cherry | Planned | +1 | **+0.4 Mult if touching no other ingredient.** | Risky isolation build |
| Lemon | Planned | +2 | **+4 Points if touching both a Protein and a Fruit.** | Fruit/Protein crossover |

## Base-point distribution

The first pass intentionally does not distribute base values evenly. It uses the number as a physical/reliability balancing lever.

- **+1:** Cheese, Tofu, Lettuce, Tomato, Blueberry, Grape, Cherry — 7 ingredients.
- **+2:** Egg, Chicken Nugget, Ham, Shrimp, Mushroom, Onion, Pickle, Red Chili, Avocado, Cucumber, Apple, Orange, Strawberry, Lemon — 14 ingredients.
- **+3:** Burger Patty, Bacon, Sausage, Meatball, Carrot, Corn, Pineapple, Banana, Watermelon — 9 ingredients.

## Intended build families

These are not rigid classes. They are overlapping paths that should naturally emerge from drafts and shop decisions.

### Protein pile

Burger Patty + Cheese + Bacon + Red Chili reward creating Protein contact networks. Tofu and Avocado let that network branch into Vegetables instead of forcing a pure-Protein build.

### Produce web

Tomato + Tofu + Mushroom + Onion + Avocado reward Vegetable density, tag diversity, and mixed neighborhoods. Corn can become a payoff piece once the pantry has several different ingredient types.

### Fruit crossover

Strawberry gives Fruit density a basic engine while Shrimp and Lemon let Fruit builds connect to Protein. Pineapple provides a Protein/Vegetable bridge, so crossover builds can chain through all three crate categories.

### Dense blender

Sausage + Blueberry + Watermelon want many contacts. Meatball wants exactly two contacts, creating an interesting tension inside the same crowded build rather than every ingredient wanting identical placement.

### Precision / low-contact

Egg + Orange + Cherry reward deliberately leaving space. Lettuce and Ham reward controlled vertical sandwiches. These pieces give players reasons not to simply pack the blender as densely as possible.

### Geometry / orientation

Pickle + Lettuce + Ham + Banana + Carrot + Cucumber make physical placement itself part of the build. This is important because Burger Shake should not feel like a card game with physics added decoratively.

### Diversity

Apple + Onion + Corn + Mushroom reward having different neighbors/categories. These naturally make shops and crate choices matter because increasing pantry variety can improve scoring instead of only reducing consistency.

### Same-type clusters

Chicken Nugget and Grape are deliberately the main same-ingredient cluster pieces. Keeping this mechanic rare makes duplicate-copy builds special instead of turning every ingredient into a matching puzzle.

## Reusable scoring support

The 30-ingredient plan should be implemented through generic rule types rather than named-ingredient code.

Already supported:

- base/self reward
- touching any ingredient
- touching a tag
- touching a specific ingredient
- exact/min/max contact count
- same-ingredient cluster
- cross-tag requirements
- tag diversity
- bridge relationships
- unique-neighbor count
- above/below/sandwiched relationships

Added for this roster phase:

- Contact-count rules can optionally count **only a specified tag**. This supports rules such as Bacon's “2+ Proteins” without Bacon-specific code.
- Unique-neighbor rules can ignore a configurable number of early unique neighbors when calculating the reward. This supports Apple/Corn-style “after the first N” scoring.
- `OrientationScoringRule` supports horizontal, vertical, and diagonal placement with configurable angle tolerance and optional min/max contact requirements.

## Art / physics targets for planned ingredients

The physical silhouette should support the scoring identity when possible:

- Cheese — thin/flat, easy contact maker.
- Chicken Nugget — small rounded irregular piece, good for clustering.
- Ham — wide flat slice, naturally sandwichable.
- Meatball — round and roll-prone.
- Shrimp — small curved asymmetric piece.
- Tofu — compact square/rectangular block.
- Avocado — oval/pear shape, medium stability.
- Carrot — long narrow piece, difficult to keep vertical.
- Cucumber — long piece that naturally lies horizontal.
- Corn — long and relatively bulky.
- Banana — curved, awkward, naturally orientation-sensitive.
- Strawberry — compact triangular/rounded piece.
- Grape — very small round piece.
- Watermelon — large round/oval piece.
- Cherry — tiny round piece, difficult to keep isolated once the blender fills.
- Lemon — medium oval piece.

## Implementation order

1. Keep the 14 existing ingredients playable while the generic rule additions compile-test.
2. Migrate current Apple/Bacon-style rules to the richer generic options only after local Unity validation.
3. Create placeholder or final art/prefabs for the 16 planned ingredients in small batches.
4. Add ScriptableObject definitions/scoring assets for one batch at a time.
5. Add new ingredients to starter-draft eligibility and the appropriate crate only after they have a valid prefab, collider, sprite, description, base score, and special rule.
6. Balance by build family instead of comparing ingredients only one-by-one.

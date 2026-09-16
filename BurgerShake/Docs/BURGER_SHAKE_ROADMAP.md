# Burger Shake — Development Roadmap

This roadmap is ordered by dependency. The rule is simple: each milestone should leave the game more playable than before.

## M0 — Current project audit

Status: IN PROGRESS

Goals:
- Audit the latest Agent Update commit and current scene wiring.
- Preserve working systems and current art/UI unless changes are necessary.
- Identify compile-risk and scene-reference risks before broad changes.
- Keep development data-driven for 30+ ingredients, 20+ Helpers, and 20+ customers.

Exit criteria:
- Current branch state is understood.
- Game plan and dev-status documents exist in the repo.

## M1 — Run opening and Starting Draft

Goals:
- Confirm 2-round starting ingredient draft works end-to-end.
- 3 choices per round.
- 1 per-card reroll shared across the whole opening draft.
- 3 copies per selected ingredient.
- Duplicate/seen prevention and round-2 synergy option work correctly.
- Draft transitions cleanly into Customer 1.
- Add temporary presentation polish where useful.

Exit criteria:
- Main Menu → New Run → Starting Draft → Customer 1 works without Inspector intervention.

## M2 — Ingredient scoring foundation

Goals:
- Every ingredient has a +1/+2/+3 base drop score.
- Existing special scoring rules are validated and clearly described.
- Reusable scoring-rule primitives cover the intended future interaction spaces.
- Avoid ingredient-specific manager code.
- Create a balance/content matrix for all ingredients.

Exit criteria:
- Current ingredient roster has a reliable base score plus a meaningful special effect.
- Adding future ingredients is mostly asset/content work.

## M3 — Three-customer vertical slice

Goals:
- Customer 1 → Assembly → Score → Shop → Customer 2 → Shop → Customer 3 → Win.
- Clean lose flow at any failed customer.
- Correct cleanup between customers.
- Receipt shows goal, reward, preferences, and restrictions.
- Customer intro/outro presentation is stable.
- No run-breaking state transitions.

Exit criteria:
- A complete 3-customer run can be played from launch to win/lose without touching the Inspector.

## M4 — Website shop presentation

Goals:
- Keep existing shop logic, replace/extend presentation into the strange online supplier website direction.
- Exactly 4 offers: 2 distinct crate categories + 2 Helpers.
- Clear prices, sold states, coin count, crate-selection flow, and Continue.
- Placeholder UI is acceptable until final art is supplied.

Exit criteria:
- Shop is readable, functional, thematically distinct, and works across a full run.

## M5 — Helper framework and first Helper set

Goals:
- Strengthen reusable Helper effect architecture.
- Build an initial representative set that touches drafting, placement, pantry composition, scoring, and economy.
- Make Helpers interact with multiple ingredients rather than one exact item whenever possible.

Exit criteria:
- At least 8 varied Helpers prove the framework can scale without one-off manager branches.

## M6 — Full run structure

Goals:
- Expand from vertical slice to the target normal run length, currently 7 customers.
- Create a starter-friendly first-customer pool and progressively more complex later customer pools.
- Tune goal scaling and shop frequency.
- Add proper final run victory flow.

Exit criteria:
- Full run loop is playable and has a clear difficulty arc.

## M7 — Content expansion: 30+ ingredients

Goals:
- Reach at least 30 ingredients.
- Each ingredient has unique identity, clear tooltip wording, base +1/+2/+3 Points, and a special interaction.
- Roster intentionally supports clusters, bridges, contact builds, isolation, vertical relations, category builds, orientation, order, and other reusable spaces.
- Maintain a healthy Points/Mult mix.

Exit criteria:
- 30+ ingredients exist and combinations regularly create non-obvious build decisions.

## M8 — Content expansion: 20+ Helpers

Goals:
- Reach at least 20 Helpers.
- Avoid a roster dominated by flat score bonuses.
- Ensure Helpers can redirect a run and create new ingredient priorities.

Exit criteria:
- 20+ Helpers with meaningful cross-system interactions.

## M9 — Content expansion: 20+ customers

Goals:
- Reach at least 20 customers.
- Build varied personalities, dialogue, goals, preferences, and restrictions.
- Restrictions should reshape the puzzle without routinely hard-countering a build.
- Receipt clearly communicates all special conditions.

Exit criteria:
- 20+ customers and enough selection variety that repeated runs do not feel scripted.

## M10 — End-of-run experience

Goals:
- Proper Win screen.
- Proper Lose screen.
- Run summary: customers cleared, final pantry, Helpers, score highlights, coin totals where useful.
- Play Again and Main Menu flows.
- No active-run save/resume.

Exit criteria:
- Run ending feels intentional rather than a debug stop state.

## M11 — Juice, UX, and tutorial

Goals:
- Improve draft transitions, card selection, reroll feedback, ingredient drops, score reveal, customer reactions, receipt motion, and shop purchases.
- Add lightweight first-run onboarding instead of a large tutorial modal.
- Improve keyboard/controller-friendly navigation where practical.

Exit criteria:
- Core actions give clear feedback and first-time players can learn without external instructions.

## M12 — Audio and settings

Goals:
- Add music/SFX hooks and free/legal audio resources or user-provided assets.
- Master/Music/SFX controls.
- Display/settings polish.
- Screen shake/accessibility toggle if used.

Exit criteria:
- Full game has functional audio feedback and appropriate settings.

## M13 — Balance and one-more-run tuning

Goals:
- Tune base drop values, special rules, Helpers, customer goals, economy, crate values, rerolls, and run pacing.
- Reduce unwinnable-feeling randomness while preserving uncertainty.
- Ensure failed runs usually reveal a decision or execution improvement the player can try next time.

Exit criteria:
- Multiple build archetypes can succeed.
- No single ingredient/Helper strategy dominates ordinary play.
- Difficulty feels challenging but learnable.

## M14 — Release cleanup

Goals:
- Remove debug UI and obsolete assets/systems.
- Resolve warnings and stale references.
- Validate all scenes and ScriptableObjects.
- Finalize build settings and release candidate.

Exit criteria:
- Stable release build candidate ready for external playtesting/distribution.

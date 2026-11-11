# 01: Generalize combat with a Monster base class

**What to build:** A pure refactor with no change to gameplay. Add a `Monster` base class holding what any enemy needs: name, Strength, Agility, Health Points, weapon, the die, and the attack resolution currently in `Dragon.Attack` (hit, block, damage, armor absorption, HP floor). `Dragon` becomes a subclass that supplies its own stats and flavour text keys. `Combat` takes `(Player, Monster, Messages)` instead of a `Dragon`.

**Blocked by:** None (can start immediately)

**Status:** not started

- [ ] `Monster` base class with the shared attack resolution
- [ ] `Dragon` inherits from `Monster`. Its stats, claws, taunts and damage replies are unchanged
- [ ] `Combat` accepts any `Monster`
- [ ] No new or changed player-visible text
- [ ] Tests: every existing dragon, combat and game test passes with its assertions unchanged

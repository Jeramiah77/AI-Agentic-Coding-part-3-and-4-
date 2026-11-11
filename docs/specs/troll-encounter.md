# Spec: Troll Encounter on the West Path

_Status: planned._

## Problem Statement

The crossroads offers only two real choices: south to the Merchant, which is a one-time visit, and north to the dragon, which always ends the game. Once the Merchant has gone, the only thing left to do is fight the dragon. Gold is only useful at the Merchant, and nothing in the game lets a Player take a risk in exchange for a better chance against the dragon. Combat is also built around the dragon alone: `Combat` takes a `Dragon`, so adding any other enemy means copying the combat loop.

## Solution

A new **West path** leads to a bridge guarded by a **Troll** named **Grumblegut**. At the **Troll Encounter** the Player chooses one of three options:

- **Attack**: fight the Troll using the same combat rules as the dragon. Winning earns the Troll's **Hoard** (Gold) and **Troll Hide** (armor).
- **Riddle**: answer one of three multiple-choice riddles. A right answer **Outwits** the Troll, which gives up a smaller amount of Gold and no hide. A wrong answer starts the fight immediately.
- **Flee**: go back to the crossroads unharmed. The Troll is still waiting.

The Troll is a **one-time encounter**. Once it is **Slain** or **Outwitted**, later trips west show a single aftermath line and send the Player back to the crossroads. Fleeing, before or during the fight, does not resolve it. Losing the fight is a real game over. Damage taken from the Troll carries over to the dragon fight, so the detour is a gamble.

To support a second enemy, combat is generalized: `Dragon` and `Troll` share a **Monster** base, and `Combat` fights any Monster. All new text appears in English, Spanish, French and Italian.

## User Stories

1. As a Player, I want a third path at the crossroads, so that there is more to explore than the Merchant and the dragon.
2. As a Player, I want to type `w` or `west` to take it, so that it works like the other paths.
3. As a Player, I want the crossroads prompt to describe three paths, so that I know West exists.
4. As a Player, I want to meet a Troll guarding a bridge the first time I go west, so that the path has a purpose.
5. As a Player, I want the Troll introduced with a name, red text, ASCII art and its stats, so that it feels like a proper encounter, like the dragon.
6. As a Player, I want to choose between attacking, answering a riddle, or fleeing, so that I have more than one way to deal with the Troll.
7. As a Player who mistypes a choice, I want to be asked again, so that a typo never starts a fight.
8. As a Player who flees before fighting, I want to return to the crossroads unharmed, so that a bad matchup isn't fatal.
9. As a Player who attacks, I want the fight to use the same hit, block, damage and armor rules as the dragon fight, so that I already know how it works.
10. As a Player, I want the Troll to be weaker than the dragon, so that it is a mid-tier fight and not a second boss.
11. As a Player fighting the Troll, I want to be able to flee mid-fight back to the crossroads, keeping the damage I took, so that I can cut my losses.
12. As a Player who beats the Troll, I want Gold and Troll Hide, so that the risk pays off.
13. As a Player with no armor, I want Troll Hide to give me armor with Protection 3, so that it helps against the dragon.
14. As a Player who already bought the Merchant's armor, I want Troll Hide to add +2 Protection to it, so that the reward is still useful whatever order I visit things in.
15. As a Player killed by the Troll, I want Troll-specific death text followed by the game-over ending, so that the death matches what killed me.
16. As a Player, I want to answer a riddle instead of fighting, so that I can avoid the danger.
17. As a Player, I want the riddle chosen at random from a small pool, so that it isn't always the same one.
18. As a Player, I want to answer by picking `a`, `b` or `c`, so that I'm never marked wrong for a spelling slip.
19. As a Player who answers correctly, I want a small amount of Gold, so that being clever is rewarded, though less than fighting.
20. As a Player who answers wrongly, I want the fight to start straight away, so that a wrong answer has consequences.
21. As a Player who fled and comes back, I want to be able to try a riddle again, possibly a different one, so that fleeing doesn't lock me out of the clever option.
22. As a Player, I want fighting to never pay less Gold than the riddle, so that the bigger risk always has the bigger reward.
23. As a Player, I want my Troll damage to carry over to the dragon fight, so that the detour is a real gamble.
24. As a Player who slew the Troll, I want to see its body by the bridge when I go west again, then return to the crossroads, so that the world remembers what I did.
25. As a Player who outwitted the Troll, I want it to wave me across when I go west again, then return to the crossroads, so that the outcome is remembered.
26. As a Spanish, French or Italian speaker, I want all Troll, riddle and West path text in my language, so that the game never falls back to English or shows a `[missing key]`.
27. As a developer, I want `Combat` to fight any Monster, so that new enemies don't need a copy of the combat loop.
28. As a developer, I want the dragon to behave exactly as before after the refactor, so that existing play and tests are unaffected.
29. As a developer, I want the riddle pick and the Gold rolls to use the injected die, so that tests can fix the outcome.
30. As a developer, I want a test that fails if any language is missing a Troll or riddle message, so that translation gaps are caught before release.

## Implementation Decisions

- **Monster** is a new base class holding what `Dragon` and `Troll` share: name, Strength, Agility, Health Points, weapon, the die, and the attack resolution now in `Dragon.Attack` (hit on d20 ≤ Strength, block on d20 ≤ the Player's Agility, damage = d(weapon MaxDamage), reduced by armor Protection to a minimum of 0, Player HP floored at 0). Each subclass supplies its own stats and flavour text keys (taunts, damage replies, intro).
- **Combat** takes `(Player, Monster, Messages)` instead of a `Dragon`. The loop and retreat prompt are unchanged. The dragon refactor comes first and must leave every existing dragon and combat test passing without changes to their assertions.
- **Troll stats:** name **Grumblegut**. Strength, Agility and Health Points are each a **d12**. Weapon is a **club** with MaxDamage **8**. For comparison, the dragon uses d20 stats and claws with MaxDamage 12.
- **Crossroads:** `PromptForPath` and `IsValidPathChoice` accept `w`/`west`. The path prompt text lists three paths and "Enter 'w' for West". `e` stays as exit. The test at `GameTests.cs:168` that asserts "west" is invalid is changed on purpose to assert it is valid.
- **Troll state:** a new enum on the Game instance, `TrollState { Unmet, Slain, Outwitted }`, starting at `Unmet`. It plays the same role for the Troll that `MerchantMet` plays for the Merchant: a new Game means a new Troll.
- **Encounter menu:** `a` attack, `r` riddle, `f` flee, checked case-insensitively. `f` is used for flee so that `r` is free for riddle. The dragon keeps `r` for retreat. Invalid input asks again. Closed input (end of stream) counts as fleeing, matching how combat treats closed input as a retreat.
- **Fleeing:** before the fight returns to the crossroads with no change. Mid-fight retreat returns to the crossroads with Health Points as they are. In both cases the state stays `Unmet`. This differs on purpose from the dragon, where retreating ends the game.
- **Winning the fight:** state becomes `Slain`. The Player gains **d10 + 5** Gold (6–15) and Troll Hide.
- **Troll Hide:** if the Player has no armor, they get armor with Protection **3**. If they already have armor, its Protection rises by **2**.
- **Losing the fight:** Troll-specific death text, then the existing `defeat` game-over ending.
- **Riddles:** a pool of three, picked with a **d3** roll. Each is multiple choice with options `a`, `b`, `c` and one fixed correct answer. Invalid input asks again. Closed input counts as a wrong answer. English versions:
  1. "What has to be broken before you can use it?" a) an egg (correct), b) a promise, c) a bridge.
  2. "The more you take, the more you leave behind. What am I?" a) gold, b) footsteps (correct), c) breath.
  3. "I speak without a mouth and hear without ears. What am I?" a) a ghost, b) the wind, c) an echo (correct).
- **Right answer:** state becomes `Outwitted`. The Player gains **d6** Gold (1–6) and no hide. Fight Gold's minimum (6) equals riddle Gold's maximum, so fighting never pays less.
- **Wrong answer:** the fight starts at once, under the normal fight rules, including mid-fight fleeing.
- **Returning west:** `Slain` prints "The troll's body lies by the bridge." `Outwitted` prints "The troll waves you across." Both then return to the crossroads with no other effect.
- **No healing:** Health Points lost to the Troll stay lost for the dragon fight.
- **Localisation:** every new message, the Troll's weapon name, and all riddle questions and options are added to all four languages, in the file's existing no-accent style. Riddle answers are letters, so no answer matching is language-specific.

## Testing Decisions

- **What makes a good test:** tests check what a Player could observe: Gold, Health Points, armor Protection, Troll state, and the text printed. They don't check how the code stores things internally. Dice are fixed so outcomes are exact.
- **Seams:**
  1. **Monster attack**: a fixed die and a Player, asserting on Health Points and the returned combat text, for both Dragon and Troll.
  2. **Combat against any Monster**: the existing combat tests, run unchanged against the refactored `Combat`, plus Troll fights.
  3. **The Game's Troll encounter**: scripted typed input via console redirection, asserting on printed output, Player state and Troll state.
  4. **Riddle pick and Gold rolls**: a fixed die selects the riddle and the Gold amount.
- **Covered cases:**
  - All existing dragon, combat and game tests pass after the Monster refactor.
  - `w` and `west` are valid paths. The crossroads prompt mentions West in all four languages.
  - Troll stats are within 1–12 and the club deals at most 8.
  - Fleeing before the fight returns to the crossroads with no change and the Troll still `Unmet`. A second trip west meets the Troll again.
  - Fleeing mid-fight keeps the damage taken and leaves the Troll `Unmet`.
  - Winning gives 6 Gold at the lowest roll and 15 at the highest, plus Protection 3 with no prior armor, or +2 on existing armor (3 becomes 5).
  - Losing prints the Troll death text and ends the game.
  - Each of the three riddles (fixed d3 roll) accepts only its correct letter. A right answer gives 1–6 Gold, no hide, and `Outwitted`. A wrong answer starts the fight.
  - After `Slain` or `Outwitted`, going west prints the matching aftermath line and returns to the crossroads.
  - Damage taken from the Troll is still missing at the start of the dragon fight.
  - Invalid menu or riddle input asks again. Closed input flees from the menu and counts as wrong in a riddle.
  - Every new message exists in all four languages and every formatted message formats without error.
- **Prior art:** the fixed-die stat and combat tests, the Merchant encounter console-redirection tests, and the existing per-language message-key tests.

## Out of Scope

- Healing, potions, or any way to recover Health Points.
- A new area beyond the bridge. West is a detour, not a gate.
- Paying a toll to the Troll with Gold.
- Free-text riddle answers.
- More than three riddles, or riddles elsewhere in the game.
- Changing how the dragon's retreat works.

## Further Notes

- These loose ends were found during planning and are deliberately left for later:
  - Retreating from the dragon mid-fight prints only `retreat_combat` and ends the game without "The End".
  - `EndGame`'s retreat branch is never reached, because it is never called with `playerRetreated = true`.
  - "You have no weapon to attack with." (`Player.cs`) is hard-coded English and not translated.
  - The English `retreat` text ("come home poor but alive") doesn't match the Spanish and French versions.
- The Spanish, French and Italian text should be checked by a fluent speaker.
- New domain terms (West path, Troll, Troll Encounter, Grumblegut, Monster, Hoard, Troll Hide, Riddle, Outwitted, Slain) should be added to the project glossary.

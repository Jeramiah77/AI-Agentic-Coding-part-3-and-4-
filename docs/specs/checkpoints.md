# Spec: Checkpoints and Retries

_Status: agreed, not yet implemented._

## Problem Statement

Every Defeat ends the run. A Player who spends several minutes on character creation, visits the Merchant and fights their way past the Troll can lose everything to one bad roll, and the only way to try again is to start over: language menu, race, name, occupation and four stat rolls. The Troll fight and the dragon fight are both gambles by design, but at the moment a lost gamble costs the whole run, not just the fight. This makes Players avoid risks the game wants them to take.

## Solution

The game takes a **Checkpoint** automatically every time the Player arrives at the crossroads: right after character creation, after each return from the South path or the West path, and after each Retry. A Checkpoint is a snapshot of the Player (stats, Gold, Weapon, Armor) and of the world (whether the Merchant was met, and the Troll's state). Checkpoints live in memory for the current run only.

A **Defeat** is either the Player's Health Points reaching 0 in a fight with any Monster, or the Player fleeing or retreating from any Monster: the Monster always strikes them down from behind. After a Defeat the game shows the defeat text (or, for an escape, the struck-down text) and then offers two choices:

1. Return to last Checkpoint.
2. Quit.

Returning (a **Retry**) puts the Player back at the crossroads as they were at the Checkpoint, at **Full Health**, with the Troll and the dragon also at Full Health. A short return message is shown, then the normal path prompt. Retries are unlimited: the Player may Retry after every Defeat. Quitting ends the game.

The game now ends only through a victory over the dragon or Quit. Dice are not rewound, so a Retry plays out differently. All new text appears in English, Spanish, French and Italian.

## User Stories

1. As a Player, I want a Defeat to offer me a way back, so that one bad roll doesn't cost me the whole run.
2. As a Player, I want Checkpoints taken automatically each time I reach the crossroads, so that I never have to remember to save.
3. As a Player, I want a Checkpoint taken right after character creation, so that a Defeat on my first trip never sends me back through race, name, occupation and stat rolls.
4. As a Player, I want a Checkpoint taken after each return from the South path or the West path, so that the Merchant purchases and Troll rewards I earned are kept.
5. As a Player, I want my stats, Gold, Weapon and Armor restored to their Checkpoint values when I return, so that the Defeat is fully undone.
6. As a Player, I want to return at Full Health, so that a Retry is a real fresh attempt.
7. As a Player, I want the Merchant and the Troll's state restored to how they were at the Checkpoint, so that the world matches my character.
8. As a Player, I want to never be able to buy from the Merchant or take the Troll's Hoard twice by using a Retry, so that Retries can't be used to farm rewards.
9. As a Player who returns, I want the Troll back at Full Health, so that every Retry faces the same Troll.
10. As a Player defeated by the dragon, I want the dragon back at Full Health when I return, so that the fight starts fresh.
11. As a Player, I want a Retry to play out differently, so that returning is a real second chance and not a replay of the same Defeat.
12. As a Player defeated in a fight, I want to see the usual defeat text first, so that I still know what beat me.
13. As a Player who flees the Troll or retreats from the Troll or the dragon, I want to be told that the Monster struck me down from behind, so that I understand why fleeing counted as a Defeat.
14. As a Player, I want running away to be a Defeat, so that fleeing is a real choice with a real cost.
15. As a Player, I want to choose Quit instead, so that I can stop playing after a Defeat if I want.
16. As a Player who mistypes the choice, I want to be asked again, so that a typo never quits or returns me by accident.
17. As a Player who returns, I want a short message saying I wake at the crossroads, so that I understand where I am.
18. As a Player who returns, I want to go straight to the path prompt, without the language menu or character creation, so that returning is quick.
19. As a Player, I want unlimited Retries, so that a Defeat costs me time and progress, never the whole run.
20. As a Player who quits, I want the game to end, so that I can stop whenever I'm beaten.
21. As a Spanish, French or Italian speaker, I want all Checkpoint, Retry and struck-down text in my language, so that the game never falls back to English or shows a `[missing key]`.
22. As a developer, I want taking and restoring a Checkpoint to be testable without scripting console input, so that the snapshot rules are easy to verify.
23. As a developer, I want a test that fails if any language is missing a Checkpoint, Retry or struck-down message, so that translation gaps are caught before release.

## Implementation Decisions

- **Defeat** is reported, not handled, by the encounters. `HandleTrollEncounter` already returns true when the Player loses the fight; it must also return true when the Player flees before the fight or retreats mid-fight. `HandleDragonEncounter` currently returns `void` and calls `EndGame` itself; it needs to report a Defeat to the main loop in the same way (for example by returning a result), for a lost fight and for a retreat before or during the fight. Victory over the dragon keeps its current behaviour of ending the game.
- **Struck down while fleeing:** every escape (fleeing the Troll before the fight, retreating mid-fight from the Troll, retreating from the dragon before or during the fight) is always fatal. There is no roll. The game prints one new message naming the Monster (English: "As you turn to flee, the {0} strikes you in the back. You fall.") and goes straight to the Defeat prompt, without `troll_defeat` or `defeat`. Closed input on the Troll menu still counts as fleeing, so it leads to a Defeat, and then to Quit, because closed input at the prompt quits.
- **Full Health:** the Health Points a Player or Monster had when created. `Player` stores it when `GenerateHealthPoints` rolls Health Points (today that method only overwrites `HealthPoints`). Each Monster stores it when its Health Points are rolled in its constructor. Nothing raises Health Points above Full Health.
- **Checkpoint** is a new in-memory snapshot. It holds the Player's Strength, Agility, Gold, Weapon and Armor, plus `MerchantMet` and `TrollState`. It holds no Health Points: the Player, Troll and dragon all return at Full Health. Name, Race and Occupation never change after creation, but may be included for completeness.
- **Copies, not references:** the Weapon is enchanted in place by `Weapon.Enchant`, so a Checkpoint must store a copy of the Weapon (type, MaxDamage, ASCII art), not a reference to the Player's object. `Armor` is immutable (Protection has no setter; Troll Hide replaces it with a new `Armor`), so it can be stored by reference or copied.
- **Taking a Checkpoint:** at the top of the `inAdventureMenu` loop in `StartGame`, before `PromptForPath`. This one place covers arriving after character creation, after the Merchant Encounter, after a Troll Encounter that isn't a Defeat, and after a Retry.
- **Restoring a Checkpoint:** writes the stored values back onto the existing `Player` and `Game` (`MerchantMet`, `TrollState`), sets the Player's Health Points to Full Health, and sets the Troll and the dragon to their Full Health. `MerchantMet` has a private setter today; restoring happens inside `Game`, so this stays private. The Troll is the same lazily created `Troll` instance (`_troll ??= new Troll(_die)`), keeping its stats and name; if it hasn't been created yet, it stays uncreated. The dragon is the one created in `StartGame`; its Strength, Agility and name are unchanged.
- **Troll wounds:** between trips without a Defeat the Troll keeps its wounds, as today. Since every escape is now a Defeat, the Player can only leave a Troll fight wounded-but-alive by winning it. Any Retry resets the Troll to Full Health.
- **Merchant and Troll can't be farmed:** buying from the Merchant sets `MerchantMet` and the Troll's rewards set `TrollState`. Both always return the Player to the crossroads, where a Checkpoint is taken, so any later Retry restores a state where the reward was already taken.
- **Retries are unlimited:** there is no counter. Every Defeat offers the prompt.
- **Dice are not rewound:** the injected `Die` keeps rolling from where it is. Tests can still fix outcomes because the `Die` injection is unchanged.
- **Defeat prompt:** after the defeat text (`troll_defeat` for a lost Troll fight, `defeat` for a lost dragon fight) or the struck-down text, show `1. Return to last Checkpoint` and `2. Quit`, then `enter_choice`. `1` returns; `2` quits. Any other input prints `invalid` and asks again. Closed input (null, end of stream) counts as Quit.
- **Returning:** prints a short return message (English: "You wake at the crossroads, aching but alive."), restores the Checkpoint, and continues the `inAdventureMenu` loop, so the next thing the Player sees is the normal path prompt. No language menu, main menu or character creation.
- **Quit:** ends the game: `running` and `inAdventureMenu` become false and nothing further is printed.
- **Game end:** the game ends only through a victory over the dragon or Quit. The `playerRetreated` path in `EndGame` is removed.
- **Localisation:** the two prompt options, the return message and the struck-down message (with a `{0}` placeholder for the Monster's name) are added to all four languages in `language_data.json`, in the file's existing no-accent style. The escape texts `troll_flee`, `troll_retreat_to_crossroads` and `retreat` are removed from all four languages.
- **Decision record:** fleeing as a Defeat together with unlimited Retries is recorded in `docs/adr/0001-fleeing-is-a-defeat-and-retries-are-unlimited.md`.

## Testing Decisions

- **What makes a good test:** tests check what a Player could observe: Health Points, Gold, Weapon damage, Armor Protection, Merchant and Troll state, and the text printed. They don't check how a Checkpoint stores things internally. Dice are fixed so outcomes are exact.
- **Seams:**
  1. **Checkpoint take and restore**, the primary seam: take a Checkpoint from a Player and Game, change them, restore, and assert the observable state is back, with no console.
  2. **The Game's main loop**: scripted typed input via console redirection, driving a Defeat by the Troll or the dragon (in a fight or by fleeing) with a fixed die, then the defeat prompt, asserting on printed output and Player state.
- **Covered cases:**
  - Restoring brings back Strength, Agility, Gold, Weapon (including an enchantment made after the Checkpoint being undone), Armor, `MerchantMet` and `TrollState`.
  - After a Retry the Player's Health Points equal Full Health, even if they were lower at the Checkpoint.
  - After a Retry the Troll's and the dragon's Health Points equal their Full Health, even if the Troll was wounded at the Checkpoint.
  - An enchantment or Armor bought before the Checkpoint is kept after restoring.
  - Changing the Player's Weapon after a Checkpoint doesn't change the Checkpoint.
  - A Defeat in the Troll fight shows `troll_defeat`, then the prompt. Choosing `1` prints the return message, then the path prompt, with the Player and Troll as described above.
  - A Defeat in the dragon fight returns to the crossroads with the dragon at Full Health on the next trip north.
  - Fleeing the Troll, retreating mid-fight from the Troll, and retreating from the dragon before and during the fight each print the struck-down text with the Monster's name, then the prompt, and never `troll_defeat` or `defeat`.
  - Several Defeats in a row each offer the prompt again; Retries never run out.
  - Choosing `2` ends the game. Invalid input asks again. Closed input quits.
  - After buying from the Merchant or beating the Troll and then suffering a Defeat, a Retry leaves the Merchant gone and the Troll's state unchanged: the reward can't be taken twice.
  - Every new message exists in all four languages, every formatted message formats without error, and the removed keys are gone.
- **Prior art:** the fixed-die combat tests, the Merchant and Troll encounter console-redirection tests (run without parallelism), and the existing per-language message-key tests.

## Out of Scope

- Saving Checkpoints to disk, or resuming a run after the program closes.
- Choosing a Checkpoint other than the last one, or taking one manually.
- Limiting Retries.
- Rewinding the dice.
- A chance to survive fleeing.

## Further Notes

- **A Defeat is a free full heal:** because a Retry returns the Player at Full Health with their Checkpoint Gold and gear, a deliberate Defeat (for example fleeing on purpose) works as "undo since the last Checkpoint" and fully heals the Player. Little is gained between the crossroads and a Defeat, so this was accepted as intended: a Defeat costs time and any progress since the Checkpoint, never the run.
- The existing defeat texts end with "The End." (`troll_defeat` in every language, `defeat` in English and Italian). That line is now always followed by the Retry prompt. The text is kept as agreed, but a later pass could split the ending from the defeat narrative.
- The dragon encounter menu (`HandleDragonEncounter`) treats closed input as invalid and asks again, so it loops forever at end of stream. This is an existing issue, left alone here, but scripted tests that reach the dragon must supply input until the fight starts.
- The Spanish, French and Italian text should be checked by a fluent speaker.
- Domain terms used here (Defeat, Checkpoint, Retry, Full Health) are defined in the project glossary under Progress.

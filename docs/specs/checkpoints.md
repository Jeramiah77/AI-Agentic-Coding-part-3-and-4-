# Spec: Checkpoints and Retries

_Status: agreed, not yet implemented._

## Problem Statement

Every Defeat ends the run. A Player who spends several minutes on character creation, visits the Merchant and fights their way past the Troll can lose everything to one bad roll, and the only way to try again is to start over: language menu, race, name, occupation and four stat rolls. The Troll fight and the dragon fight are both gambles by design, but at the moment a lost gamble costs the whole run, not just the fight. This makes Players avoid risks the game wants them to take.

## Solution

The game takes a **Checkpoint** automatically every time the Player arrives at the crossroads: right after character creation, and again after each return from the South path or the West path. A Checkpoint is a full snapshot of the Player (stats, Health Points, Gold, Weapon, Armor) and of the world (whether the Merchant was met, the Troll's state, and the Troll's current Health Points). Checkpoints live in memory for the current run only.

After a **Defeat** (the Player's Health Points reach 0 in a fight with any Monster, Troll or dragon) the game shows the existing defeat text and then offers two choices:

1. Return to the last Checkpoint, showing how many Retries are left.
2. Quit.

Returning puts the Player back at the crossroads exactly as they were at the Checkpoint, with a short return message, then the normal path prompt. Each Player has **3 Retries** per run, never refilled. Spending a Retry is permanent: the Retry count is not part of the Checkpoint. A Defeat with no Retries left shows the defeat text and a "No retries remain" line, then the game ends with no prompt. Quitting ends the game exactly as a Defeat does today.

Dice are not rewound, so a Retry plays out differently. Fleeing or retreating is never a Defeat and never offers a Retry. All new text appears in English, Spanish, French and Italian.

## User Stories

1. As a Player, I want a Defeat to offer me a way back, so that one bad roll doesn't cost me the whole run.
2. As a Player, I want Checkpoints taken automatically each time I reach the crossroads, so that I never have to remember to save.
3. As a Player, I want a Checkpoint taken right after character creation, so that a Defeat on my first trip never sends me back through race, name, occupation and stat rolls.
4. As a Player, I want a Checkpoint taken after each return from the South path or the West path, so that the Merchant purchases and Troll rewards I earned are kept.
5. As a Player, I want my stats, Health Points, Gold, Weapon and Armor restored to their Checkpoint values when I return, so that the Defeat is fully undone.
6. As a Player, I want the Merchant and the Troll restored to how they were at the Checkpoint, so that the world matches my character.
7. As a Player, I want to never be able to buy from the Merchant or take the Troll's Hoard twice by using a Retry, so that Retries can't be used to farm rewards.
8. As a Player defeated by the Troll, I want the wounds I dealt it in the losing fight undone, so that the Troll is exactly as it was at the Checkpoint.
9. As a Player who wounded the Troll on an earlier trip and fled, I want those earlier wounds kept after a Retry, so that progress made before the Checkpoint stays.
10. As a Player defeated by the dragon, I want the dragon back at full health when I return, so that the fight starts fresh.
11. As a Player, I want a Retry to play out differently, so that returning is a real second chance and not a replay of the same Defeat.
12. As a Player, I want to see the usual defeat text first, so that I still know what beat me.
13. As a Player, I want the prompt to show how many Retries I have left, so that I know what I'm spending.
14. As a Player, I want to choose Quit instead, so that I can stop playing after a Defeat if I want.
15. As a Player who mistypes the choice, I want to be asked again, so that a typo never quits or spends a Retry.
16. As a Player who returns, I want a short message saying I wake at the crossroads, so that I understand where I am.
17. As a Player who returns, I want to go straight to the path prompt, without the language menu or character creation, so that returning is quick.
18. As a Player, I want 3 Retries for the whole run, so that Defeat still matters.
19. As a Player, I want a spent Retry to stay spent even though I return to an earlier Checkpoint, so that Retries are a real, limited resource.
20. As a Player defeated with no Retries left, I want to be told that no retries remain, and the game then to end, so that I'm not shown a choice I can't take.
21. As a Player who quits, I want the game to end the same way a Defeat ends it today, so that quitting feels familiar.
22. As a Player who flees from the Troll or retreats from a fight, I want that never to count as a Defeat, so that I'm never offered or charged a Retry for running away.
23. As a Spanish, French or Italian speaker, I want all Checkpoint and Retry text in my language, so that the game never falls back to English or shows a `[missing key]`.
24. As a developer, I want taking and restoring a Checkpoint to be testable without scripting console input, so that the snapshot rules are easy to verify.
25. As a developer, I want a test that fails if any language is missing a Checkpoint or Retry message, so that translation gaps are caught before release.

## Implementation Decisions

- **Defeat** is reported, not handled, by the encounters. `HandleTrollEncounter` already returns true when the Player loses the fight. `HandleDragonEncounter` currently returns `void` and calls `EndGame` itself; it needs to report a Defeat to the main loop in the same way (for example by returning a result), while victory and retreat keep their current behaviour of ending the game. Retreating or fleeing never reports a Defeat.
- **Checkpoint** is a new in-memory snapshot. It holds the Player's Strength, Agility, Health Points, Gold, Weapon and Armor, plus `MerchantMet`, `TrollState` and the Troll's Health Points. Name, Race and Occupation never change after creation, but may be included for completeness.
- **Copies, not references:** the Weapon is enchanted in place by `Weapon.Enchant`, so a Checkpoint must store a copy of the Weapon (type, MaxDamage, ASCII art), not a reference to the Player's object. `Armor` is immutable (Protection has no setter; Troll Hide replaces it with a new `Armor`), so it can be stored by reference or copied.
- **Taking a Checkpoint:** at the top of the `inAdventureMenu` loop in `StartGame`, before `PromptForPath`. This one place covers arriving after character creation, after the Merchant Encounter, after the Troll Encounter (any outcome other than Defeat), and after a Retry.
- **Restoring a Checkpoint:** writes the stored values back onto the existing `Player` and `Game` (`MerchantMet`, `TrollState`, and `Troll.HealthPoints`). `MerchantMet` has a private setter today; restoring happens inside `Game`, so this stays private. The Troll is the same lazily created `Troll` instance (`_troll ??= new Troll(_die)`), keeping its stats and name; only its Health Points are rolled back. If the Troll has not been created yet at the Checkpoint, it stays as it is.
- **Troll wounds:** because the Checkpoint stores the Troll's current Health Points, wounds dealt on earlier trips that ended in fleeing (and so passed through the crossroads) are kept, and only wounds dealt since the Checkpoint are undone.
- **Dragon:** after a Defeat by the dragon and a Retry, the dragon is back at full health. The dragon is created once in `StartGame` with a d20 Health Points roll; "full health" means the Health Points it was created with, not a new roll. Its Strength, Agility and name are unchanged. The dragon is not part of the Checkpoint because it can only be fought once per arrival and always ends the game otherwise.
- **Merchant and Troll can't be farmed:** buying from the Merchant sets `MerchantMet` and the Troll's rewards set `TrollState`. Both always return the Player to the crossroads, where a Checkpoint is taken, so any later Retry restores a state where the reward was already taken.
- **Retries:** a counter on the Game instance, starting at **3**, counted down by one each time the Player chooses to return. It is not part of the Checkpoint and is never refilled. A new Game means 3 new Retries.
- **Dice are not rewound:** the injected `Die` keeps rolling from where it is. Tests can still fix outcomes because the `Die` injection is unchanged.
- **Defeat prompt:** after the existing defeat text (`troll_defeat` for the Troll, `defeat` for the dragon), if Retries remain, show `1. Return to last Checkpoint ({0} retries left)` and `2. Quit`, then `enter_choice`. `1` spends a Retry and returns; `2` quits. Any other input prints `invalid` and asks again. Closed input (null, end of stream) counts as Quit, matching how the Merchant and Troll menus treat closed input as leaving instead of looping forever.
- **Returning:** prints a short return message (English: "You wake at the crossroads, aching but alive."), restores the Checkpoint, and continues the `inAdventureMenu` loop, so the next thing the Player sees is the normal path prompt. No language menu, main menu or character creation.
- **No Retries left:** after the defeat text, print a "No retries remain" line and end the game with no prompt, setting `running` and `inAdventureMenu` to false as a Defeat does today.
- **Quit:** ends the game exactly as a Defeat does today: `running` and `inAdventureMenu` become false and nothing further is printed.
- **North path:** still ends the game after victory or retreat. Only a dragon Defeat with a Retry spent returns to the crossroads.
- **Localisation:** the defeat prompt (with a `{0}` placeholder for Retries left), the Quit option, the return message and the "No retries remain" line are added to all four languages in `language_data.json`, in the file's existing no-accent style.

## Testing Decisions

- **What makes a good test:** tests check what a Player could observe: Health Points, Gold, Weapon damage, Armor Protection, Merchant and Troll state, Retries left, and the text printed. They don't check how a Checkpoint stores things internally. Dice are fixed so outcomes are exact.
- **Seams:**
  1. **Checkpoint take and restore**, the primary seam: take a Checkpoint from a Player and Game, change them, restore, and assert the observable state is back, with no console.
  2. **The Game's main loop**: scripted typed input via console redirection, driving a Defeat by the Troll or the dragon with a fixed die, then the defeat prompt, asserting on printed output, Player state and Retries left.
- **Covered cases:**
  - Restoring brings back Strength, Agility, Health Points, Gold, Weapon (including an enchantment made after the Checkpoint being undone), Armor, `MerchantMet`, `TrollState` and the Troll's Health Points.
  - An enchantment or Armor bought before the Checkpoint is kept after restoring.
  - Changing the Player's Weapon after a Checkpoint doesn't change the Checkpoint.
  - A Defeat by the Troll with Retries left shows `troll_defeat`, then the prompt with the right count. Choosing `1` prints the return message, then the path prompt, with the Player and Troll as they were at the Checkpoint.
  - A Defeat by the dragon with Retries left returns to the crossroads with the dragon at its starting Health Points on the next trip north.
  - Retries go 3, 2, 1, 0 across Defeats and are not restored by returning to a Checkpoint.
  - A Defeat with 0 Retries left prints the defeat text and "No retries remain", shows no prompt, and ends the game.
  - Choosing `2` ends the game. Invalid input asks again. Closed input quits.
  - Fleeing the Troll, retreating mid-fight from the Troll, and retreating from the dragon never show the defeat prompt or spend a Retry.
  - After buying from the Merchant or beating the Troll and then losing to a Monster, a Retry leaves the Merchant gone and the Troll's state unchanged: the reward can't be taken twice.
  - Every new message exists in all four languages and every formatted message formats without error.
- **Prior art:** the fixed-die combat tests, the Merchant and Troll encounter console-redirection tests (run without parallelism), and the existing per-language message-key tests.

## Out of Scope

- Saving Checkpoints or Retries to disk, or resuming a run after the program closes.
- Choosing a Checkpoint other than the last one, or taking one manually.
- Earning extra Retries.
- Rewinding the dice.
- Changing how retreating from the dragon works: it still ends the game, and it is not a Defeat.

## Further Notes

- The existing defeat texts end with "The End." (`troll_defeat` in every language, `defeat` in English and Italian). With Retries left, that line is followed by the Retry prompt. The text is kept as agreed, but a later pass could split the ending from the defeat narrative.
- The dragon encounter menu (`HandleDragonEncounter`) treats closed input as invalid and asks again, so it loops forever at end of stream. This is an existing issue, left alone here, but scripted tests that reach the dragon must supply input until the fight starts.
- The Spanish, French and Italian text should be checked by a fluent speaker.
- Domain terms used here (Defeat, Checkpoint, Retry) are defined in the project glossary under Progress.

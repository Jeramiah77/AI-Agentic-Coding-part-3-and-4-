# Spec: Merchant Encounter on the South Path

_Status: implemented on `main`._

## Problem Statement

The South path is a dead end. A Player who takes it reads that nothing adventurous happens and that they return home bored, yet the game then sends them back to the crossroads anyway. Going south has no effect on the story or on the Player's chances against the dragon, so there is no reason to ever choose it. The Player also has no way to improve their character after creation: their weapon is fixed by occupation, and nothing protects them from the dragon's claws besides their Agility roll.

## Solution

On their first trip down the South path, the Player meets a **Merchant** who sells two pieces of magic for **Gold**:

- **Magical Armor**, which gives **Protection** that reduces the damage of every dragon hit that lands.
- A **Magical Weapon**: the Merchant enchants the Player's own occupation weapon so it deals more damage.

Every Player now rolls Gold during character creation. They may buy either item, both if they can afford them, or neither. After the **Merchant Encounter** they return to the crossroads and can still go north to face the dragon, now better equipped. Later trips south find that the Merchant has moved on. All new text appears in English, Spanish, French and Italian.

## User Stories

1. As a Player, I want to roll for Gold during character creation, so that I have something to spend on my adventure.
2. As a Player, I want my Gold roll to use the same "type `roll`" prompt as my other stats, so that character creation feels consistent.
3. As a Player, I want to see the result straight after each roll (Strength, Agility, Health Points, Gold), so that I know my character without opening the stats screen.
4. As a Player of any race, I want Gold to be rolled the same way for everyone, so that no race is penalised at the Merchant.
5. As a Player, I want my Gold shown in my character summary, so that I can plan whether to visit the Merchant.
6. As a Player, I want to meet a Merchant the first time I take the South path, so that going south is worth doing.
7. As a Player, I want the South path's story to describe the Merchant's clearing instead of saying I went home bored, so that the narrative matches what happens.
8. As a Player, I want the Merchant to tell me how much Gold I have, so that I know what I can afford.
9. As a Player, I want the Merchant's menu to show each item's price and effect, so that I can choose with full information.
10. As a Player, I want to buy Magical Armor, so that dragon hits hurt me less.
11. As a Player, I want to buy a Magical Weapon that enchants my own weapon, so that my occupation's weapon stays mine but deals more damage.
12. As a thief, archer, fighter or magician, I want the enchantment to raise my weapon's damage by the same amount, so that every occupation gains the same benefit.
13. As a Player with enough Gold, I want to buy both items, so that a lucky Gold roll pays off.
14. As a Player, I want an item I've bought to disappear from the menu, so that I can't pay twice for the same thing.
15. As a Player, I want a clear "not enough Gold" message when I pick something I can't afford, so that I understand why nothing happened, and I keep my Gold.
16. As a Player, I want to leave the Merchant without buying anything, so that I'm never forced to spend.
17. As a Player, I want the encounter to end by itself once I've bought both items, so that I don't have to choose Leave from an empty shop.
18. As a Player who mistypes a menu choice, I want to be asked again, so that a typo never buys or skips something.
19. As a Player, I want to return to the crossroads after the Merchant, so that I can still go north and fight the dragon.
20. As a Player who goes south a second time, I want to learn that the Merchant has moved on, so that the encounter feels like a one-time opportunity.
21. As a Player wearing Magical Armor, I want the armor to absorb part of every dragon hit that lands, so that I survive longer.
22. As a Player wearing Magical Armor, I want a weak hit to be absorbed completely, so that small hits don't cost me Health Points.
23. As a Player wearing Magical Armor, I want combat text to tell me how much damage my armor absorbed, so that I can see it working.
24. As a Player without armor, I want combat to work exactly as before, so that skipping the Merchant changes nothing.
25. As a Player, I want my Armor Protection and Gold shown in my character summary and the pre-combat stats, so that I can see my whole loadout before fighting.
26. As a Player with a Magical Weapon, I want it to appear as "magical <weapon>" everywhere (stats, attack text), so that I'm reminded it's enchanted.
27. As a Spanish, French or Italian speaker, I want all Merchant, Gold and Armor text in my language, including magical weapon names, so that the game never falls back to English or shows a `[missing key]`.
28. As a developer, I want the Merchant's rules to be testable without scripting console input, so that pricing and purchase rules are easy to verify.
29. As a developer, I want a test that fails if any language is missing a Merchant message, so that translation gaps are caught before release.

## Implementation Decisions

- **Gold** is a new Player stat: 10 + d20 (11–30), rolled after Health Points with no race modifier, using the Player's injected die.
- **Prices:** Magical Armor costs 12 Gold and a Magical Weapon costs 15 Gold. Every Player can afford at least one item. Only rolls of 17 or more (27+ Gold, about 20% of Players) can afford both.
- **Armor** is a new item the Player may own (at most one). It has a single value, **Protection = 3**. When the dragon's attack lands and the Player's Agility block fails, damage = claw roll − Protection, with a minimum of 0. Combat reports the absorbed amount, or that the armor absorbed the entire blow, in which case no Health Points are lost.
- **Magical Weapon** is the Player's own weapon, enchanted in place: its type gains a "magical " prefix and its maximum damage rises by **4**. A weapon is enchanted at most once. A Player with no weapon is not offered the enchantment and cannot buy it.
- **Merchant** is a new module holding the shop rules only: prices, whether an item is on offer to a Player, whether they can afford it, and buying (which takes the Gold and applies the item, or changes nothing and reports failure). It does no console input or output.
- **The menu loop** lives in the Game, alongside the existing dragon encounter. It uses fixed numbers (1 Armor, 2 Weapon, 3 Leave) and hides items the Player already owns. Choosing a hidden item counts as invalid input. Closed input (end of stream) leaves the shop, matching how combat treats closed input as a retreat.
- **Once only:** the Game remembers that the Merchant was met. The flag lives on the Game instance, so a new Game means a new Merchant.
- **Displays:** the character summary and pre-combat stats show Gold always, and Armor Protection only when armor is owned.
- **Localisation:** every new message and the four magical weapon display names are added to all four languages. Translations follow the file's existing no-accent style. Magical weapon names are separate translation entries rather than a "magical {0}" pattern, so gendered languages can agree correctly (e.g. *daga magica*, *arco largo magico*).
- **Roll results:** after each character-creation roll, the final stat (after race modifiers) is printed.

## Testing Decisions

- **What makes a good test:** tests check what a Player could observe: Gold totals, Health Points, what's owned, and the text printed. They don't check how the code stores things internally. Dice are fixed so outcomes are exact rather than "within a range".
- **Seams (confirmed with the developer):**
  1. **Merchant rules**, the primary seam: offered, afford and buy checks against a Player directly, with no console.
  2. **The Game's merchant menu**: scripted typed input via console redirection, asserting on printed output and the Player's resulting state.
  3. **The dragon's attack**: a die fixed so the hit lands and the claw roll is known, asserting on Health Points and the returned combat text.
  4. **Player stat generation**: the Gold roll with a fixed die.
- **Covered cases:**
  - Gold is 11 at the lowest roll and 30 at the highest, for every race.
  - Armor bought with exactly 12 Gold leaves 0 Gold and Protection 3. With 11 Gold the purchase fails and nothing changes.
  - Weapon bought with exactly 15 Gold gives +4 damage for all four occupations. With 14 Gold the purchase fails.
  - Both items bought with 27 Gold leaves 0. Buying either item a second time is refused.
  - A Player with no weapon is not offered the enchantment and is refused.
  - With armor, a claw roll of 5 costs 2 Health Points, and claw rolls of 1–3 cost none. Without armor, the full roll is taken.
  - The Merchant appears on the first trip only. Leave, closed input, invalid input, not enough Gold and bought-item-drops-off-menu all behave as described. Buying both ends the encounter.
  - The character summary and combat stats show Gold and Armor.
  - Every new message and magical weapon name exists in all four languages, every formatted message formats without error, and every roll-result message shows the value.
- **Prior art:** the fixed-die stat tests (a die subclass returning set values per die size), the combat input tests (console redirection, run without parallelism), and the existing per-language message-key tests.

## Out of Scope

- Selling items back, haggling, or a Merchant inventory beyond the two items.
- Any other use for leftover Gold (e.g. the dragon's hoard adding to it).
- Armor or enchantments for the dragon.
- Multiple Merchant visits, restocking, or randomised prices.
- Text input such as "armor" or "weapon" in the menu (numbers only, like the main menu).
- Accented characters in translations, which would be a separate, file-wide decision.

## Further Notes

- Going north still always ends the game, even after a retreat, so the Merchant can't be visited after facing the dragon.
- The Spanish, French and Italian text was written without a native-speaker review and should be checked by a fluent speaker.
- Domain terms used here (Gold, Merchant, Merchant Encounter, Armor, Protection, Magical Weapon) are defined in the project glossary.

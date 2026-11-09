# 05: Buying both ends the encounter

**What to build:** A Player with 27 or more Gold can buy both Magical Armor and a Magical Weapon in one visit. Once nothing is left to sell, the Merchant says farewell and the Player is back at the crossroads without having to choose Leave. Also adds a safety check that every Merchant message formats correctly in every language.

**Blocked by:** 03: Buy Magical Armor, 04: Buy Magical Weapon

**Status:** done (implemented on `main`)

- [x] With 27 Gold, buying both items succeeds and leaves 0 Gold
- [x] The encounter ends by itself once both items are bought
- [x] Every Merchant message and magical weapon name exists in all four languages
- [x] Every formatted Merchant message formats without error in all four languages
- [x] Tests: buy both with 27 Gold; scripted "1 then 2" ends the encounter; language-key and format checks

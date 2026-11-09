# 04: Buy Magical Weapon

**What to build:** At the Merchant, menu option 2 enchants the Player's own weapon for 15 Gold. It becomes "magical <weapon>" with +4 maximum damage. The enchanted name is translated in every language and appears in stats and attack text. If the Player can't afford it, the Merchant says so and nothing changes. Once bought, it drops off the menu. A Player with no weapon is never offered the enchantment.

**Blocked by:** 02: Merchant Encounter with Leave only

**Status:** done (implemented on `main`)

- [x] Option 2 enchants the Player's own weapon for 15 Gold: "magical" name, +4 maximum damage
- [x] Works for all four occupations (long sword, lightning bolt spell, dagger, long bow)
- [x] With 14 Gold the purchase fails with a "not enough Gold" message, and nothing changes
- [x] A weapon is enchanted at most once, and the option drops off the menu once bought
- [x] A Player with no weapon is not offered the enchantment and cannot buy it
- [x] Magical weapon names are translated in all four languages, with correct gender agreement
- [x] Tests: +4 for each occupation with exact Gold; one Gold short; second purchase refused; Player with no weapon refused; scripted "not enough Gold" message

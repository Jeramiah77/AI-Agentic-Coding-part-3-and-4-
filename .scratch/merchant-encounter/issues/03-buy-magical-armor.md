# 03: Buy Magical Armor

**What to build:** At the Merchant, menu option 1 sells Magical Armor for 12 Gold. It gives Protection 3. If the Player can't afford it, the Merchant says so and nothing changes. Once bought, the armor drops off the menu. In combat, when the dragon's hit lands and the Player's Agility block fails, the armor absorbs its Protection from the damage, never below zero. The combat text says how much was absorbed, or that the armor absorbed the entire blow. Armor Protection shows in the character summary and pre-combat stats.

**Blocked by:** 02: Merchant Encounter with Leave only

**Status:** done (implemented on `main`)

- [x] Option 1 sells Magical Armor for 12 Gold; the Player gets Protection 3 and loses 12 Gold
- [x] With 11 Gold the purchase fails with a "not enough Gold" message, and nothing changes
- [x] Bought armor drops off the menu, and choosing it again is invalid
- [x] A landed dragon hit is reduced by Protection, never below zero
- [x] Combat text shows the absorbed amount, or that the entire blow was absorbed (no Health Points lost)
- [x] A Player without armor takes damage exactly as before
- [x] Armor Protection shows in both stats displays, only when owned
- [x] All new text exists in all four languages
- [x] Tests: buy with exact Gold; one Gold short; second purchase refused; claw roll 5 costs 2 HP; claw rolls 1–3 cost none; no armor takes the full roll; stats displays show armor

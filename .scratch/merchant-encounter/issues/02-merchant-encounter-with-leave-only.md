# 02: Merchant Encounter with Leave only

**What to build:** The first time the Player takes the South path, they read the new clearing narrative and meet the Merchant. The Merchant greets them, shows their current Gold, and offers a menu where only Leave works. Leaving returns the Player to the crossroads, where they can still go north. Any later trip south shows that the Merchant has moved on, and the Player is back at the crossroads. This is the first complete path through the encounter, before anything can be bought.

**Blocked by:** 01: Gold at character creation (the Merchant shows the Player's Gold)

**Status:** done (implemented on `main`)

- [x] The "return home, poor and bored" South narrative is replaced by the Merchant's clearing in all four languages
- [x] The first trip south shows the Merchant's greeting and the Player's Gold
- [x] Choosing Leave ends the encounter and returns the Player to the crossroads
- [x] Invalid input shows the existing invalid message and the menu again
- [x] Closed input (end of stream) leaves the shop instead of looping forever
- [x] Later trips south show "the Merchant has moved on" and no menu
- [x] The Merchant Encounter is remembered per game
- [x] Tests (scripted console input): first trip shows the Merchant and the second shows "moved on"; Leave buys nothing; closed input leaves; invalid input asks again

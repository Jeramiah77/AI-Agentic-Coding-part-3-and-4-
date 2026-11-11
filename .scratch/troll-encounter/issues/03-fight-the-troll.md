# 03: Fight the Troll

**What to build:** Choosing `a` starts combat against the Troll using the shared `Combat`. Winning sets `Slain` and gives d10 + 5 Gold and Troll Hide: armor with Protection 3, or +2 Protection on existing armor. Losing prints Troll-specific death text and then the existing game-over ending. Retreating mid-fight returns to the crossroads with the damage kept and the Troll still `Unmet`. Health Points carry over to the dragon fight.

**Blocked by:** 02

**Status:** done (implemented on `main`)

- [x] `a` starts the fight
- [x] Win: `Slain`, d10 + 5 Gold, Troll Hide
- [x] Lose: Troll death text, and the game ends (the dragon-specific `defeat` text is skipped)
- [x] Mid-fight retreat: back to the crossroads, damage kept, state `Unmet`
- [x] Troll taunts, damage replies, reward and death text in all four languages
- [x] Tests: Gold is 6 at the lowest roll and 15 at the highest
- [x] Tests: hide gives Protection 3 with no armor, and turns existing Protection 3 into 5
- [x] Tests: losing ends the game with the Troll death text
- [x] Tests: mid-fight retreat keeps the damage and leaves the Troll `Unmet`
- [x] Tests: damage from the Troll is still missing at the start of the dragon fight

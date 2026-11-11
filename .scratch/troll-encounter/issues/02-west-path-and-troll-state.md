# 02: West path, Troll state and aftermath

**What to build:** The crossroads accepts `w`/`west`, and the prompt lists three paths. The Game holds a `TrollState` (`Unmet`, `Slain`, `Outwitted`), starting at `Unmet`. Going west while `Unmet` shows the Troll intro (Grumblegut, red text, ASCII art, stats) and a menu: `a` attack, `r` riddle, `f` flee. In this ticket only flee works: it returns to the crossroads unharmed with the Troll still `Unmet`. Attack and riddle arrive in tickets 03 and 04. Until then, they print a placeholder and act like flee. Going west while `Slain` or `Outwitted` prints the matching aftermath line and returns to the crossroads.

**Blocked by:** 01

**Status:** not started

- [ ] `Troll : Monster` with Strength, Agility and HP each a d12, and a club with MaxDamage 8
- [ ] `w`/`west` accepted by `PromptForPath` and `IsValidPathChoice`. `e` stays exit
- [ ] Crossroads prompt text mentions West
- [ ] `TrollState` on the Game. A new Game starts at `Unmet`
- [ ] Encounter menu: invalid input asks again, closed input flees
- [ ] Aftermath lines for `Slain` and `Outwitted`
- [ ] All new text in English, Spanish, French and Italian
- [ ] Tests: `GameTests.cs:168` changed on purpose so that "west" is valid
- [ ] Tests: flee returns to the crossroads with no change, and a second trip west meets the Troll again
- [ ] Tests: aftermath for each resolved state, setting the state directly
- [ ] Tests: Troll stats are within 1–12
- [ ] Tests: new keys exist in all four languages

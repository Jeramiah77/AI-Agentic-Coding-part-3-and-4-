# 01: Gold at character creation

**What to build:** During character creation, after rolling Health Points, the Player rolls for Gold with the usual "type `roll`" prompt and sees how much Gold they have. Every character-creation roll (Strength, Agility, Health Points, Gold) now prints its result straight away, already including race modifiers. Gold appears in the character summary and in the pre-combat stats.

**Blocked by:** None (can start immediately)

**Status:** done (implemented on `main`)

- [x] Gold is 10 + d20 (11–30), with no race modifier
- [x] The Gold roll uses the same "type `roll`" prompt as the other stats, after Health Points
- [x] Each roll prints its final value (Strength, Agility, Health Points, Gold)
- [x] The character summary and pre-combat stats show Gold
- [x] Roll prompt and result messages exist in English, Spanish, French and Italian
- [x] Tests: Gold is 11 at the lowest roll and 30 at the highest, for every race
- [x] Tests: every roll-result message exists and shows the value in all four languages

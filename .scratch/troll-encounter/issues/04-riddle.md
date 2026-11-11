# 04: Answer the Troll's riddle

**What to build:** Choosing `r` picks one of three riddles with a d3 roll and shows it with options `a`/`b`/`c`. A right answer sets `Outwitted` and gives d6 Gold, with no hide. A wrong answer starts the fight from ticket 03 immediately. Invalid input asks again, and closed input counts as wrong. After fleeing, a later visit can try a riddle again with a new roll. The riddle text and answer keys are in the spec.

**Blocked by:** 03

**Status:** not started

- [ ] Riddle pool of three, picked with the injected die
- [ ] Right answer: `Outwitted`, d6 Gold, no hide
- [ ] Wrong answer starts the fight
- [ ] Invalid input asks again, and closed input counts as wrong
- [ ] All riddle questions and options in all four languages
- [ ] Tests: each riddle, picked with a fixed d3, accepts only its correct letter
- [ ] Tests: a right answer gives 1–6 Gold, no armor change, and `Outwitted`
- [ ] Tests: a wrong answer starts combat

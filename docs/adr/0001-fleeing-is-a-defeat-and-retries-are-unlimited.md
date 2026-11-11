# 1. Fleeing is a Defeat, and Retries are unlimited

_Status: accepted. Date: 2026-11-11._

## Context

Before Checkpoints, any Defeat ended the run, but fleeing the Troll or retreating from a fight was a safe way out. The first Checkpoint spec gave each run 3 Retries and kept fleeing as never a Defeat.

During design review two problems showed up:

- With limited Retries, a run could still be lost to bad rolls after a long session, which is the problem Checkpoints were meant to solve.
- A Retry returns the Player at Full Health. If fleeing stayed safe while losing healed the Player, the game would reward losing over running away, and fleeing would cost nothing.

## Decision

- Retries are unlimited. There is no counter; every Defeat offers `Return to last Checkpoint` or `Quit`.
- Fleeing or retreating from any Monster (Troll or dragon, before or during the fight) is always a Defeat: the Monster strikes the Player down from behind. There is no roll to survive it.
- A Retry restores the Checkpoint and puts the Player, the Troll and the dragon at Full Health.
- The game ends only through a victory over the dragon or Quit.

## Consequences

- A Defeat, including a deliberate flee, works as "undo since the last Checkpoint" with a full heal. This is accepted: a Defeat costs time and any progress since the Checkpoint, never the run.
- Fleeing is no longer a way to keep wounds and progress. The escape texts `troll_flee`, `troll_retreat_to_crossroads` and `retreat` and the `playerRetreated` path in `EndGame` go away.
- Retreating from the dragon no longer ends the game.
- Reversing either part later (adding a Retry limit, or a chance to survive fleeing) is a small code change, but it would change the game's balance and the meaning of Defeat in the glossary, so it should be a new ADR.

See `docs/specs/checkpoints.md` and the Progress section of `GLOSSARY.md`.

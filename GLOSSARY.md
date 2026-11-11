# Choose Your Own Adventure

A console adventure where a Player creates a character, chooses a path at the crossroads, and faces what waits down it.

## Language

### Economy

**Gold**:
The Player's currency, used to pay the Merchant.
_Avoid_: Money, coins, currency

**Merchant**:
The trader the Player meets on the South path, who sells Magical Armor and Magical Weapons for Gold.
_Avoid_: Shopkeeper, vendor, trader, shop

**Merchant Encounter**:
The single meeting with the Merchant, on the Player's first trip down the South path; later trips find the Merchant gone.
_Avoid_: Shop visit, store

### Equipment

**Armor**:
Protective gear worn by the Player that reduces the damage they take from a landed hit.
_Avoid_: Shield, defense

**Protection**:
The amount of damage Armor removes from each landed hit, never reducing damage below zero.
_Avoid_: Defense, armor class, AC

**Magical Weapon**:
An enchanted version of the Player's own occupation Weapon, with higher Max Damage.
_Avoid_: New weapon, upgraded weapon

**Troll Hide**:
The reward for slaying the Troll: Armor with Protection 3, or +2 Protection on Armor the Player already has.
_Avoid_: Troll skin, troll armor

### Enemies

**Monster**:
Any enemy the Player can fight. Every Monster uses the same combat rules for hitting, blocking, damage and Armor.
_Avoid_: Enemy, creature, mob

**Troll**:
Grumblegut, the Monster guarding the bridge on the West path. Weaker than the dragon: d12 stats and a club.
_Avoid_: Ogre, bridge guard

### Places

**West path**:
The crossroads path, chosen with `w`, that leads to the Troll's bridge. A detour: the Player always returns to the crossroads unless they die.
_Avoid_: Bridge path, east

**Troll Encounter**:
Meeting the Troll on the West path, where the Player chooses to attack, answer a Riddle, or flee. It stays open until the Troll is Slain or Outwitted.
_Avoid_: Bridge fight

**Riddle**:
A multiple-choice question from the Troll, one of three picked at random. A right answer Outwits the Troll; a wrong one starts the fight.
_Avoid_: Puzzle, quiz

**Hoard**:
The Troll's Gold: d10 + 5 for slaying it, d6 for outwitting it.
_Avoid_: Loot, treasure, toll

**Slain**:
The Troll was beaten in a fight. Later trips west show its body by the bridge.

**Outwitted**:
The Troll was beaten with a right Riddle answer. Later trips west show it waving the Player across.

### Progress

**Defeat**:
The Player's HealthPoints reaching 0 in a fight with any Monster. Retreating or fleeing is never a Defeat.
_Avoid_: Death, game over, losing

**Checkpoint**:
A snapshot of the Player and the world, taken each time the Player arrives at the crossroads. After a Defeat the Player may return to the last Checkpoint. It lasts only for the current run.
_Avoid_: Save, save point, respawn

**Retry**:
One return to the last Checkpoint after a Defeat. A Player has 3 per run; spending one is permanent, and a Defeat with none left ends the game.
_Avoid_: Life, continue, respawn

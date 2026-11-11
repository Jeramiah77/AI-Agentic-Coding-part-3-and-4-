// An enemy the Player can fight. Holds the rules every enemy shares:
// hit on d20 <= Strength, the Player blocks on d20 <= Agility, and a
// landed hit deals d(weapon MaxDamage) minus armor Protection.
// Subclasses supply their stats and the message keys for their own text.
public abstract class Monster
{
    protected readonly Die _die;

    public string Name { get; set; } = "";
    public int Strength { get; set; }
    public int Agility { get; set; }
    public int HealthPoints { get; set; }
    public Weapon Weapon { get; set; }

    protected Monster(Die die, Weapon weapon)
    {
        _die = die;
        Weapon = weapon;
    }

    // Text that differs per monster.
    protected abstract string[] DefenseTauntKeys { get; }
    protected abstract string[] DamageReplyKeys { get; }
    public abstract string PlayerAttackIntroKey { get; }
    public abstract string DefeatedNarrativeKey { get; }
    public abstract string StatsHeaderKey { get; }
    public abstract string CombatHeaderKey { get; }

    public int RollAttack()
    {
        return _die.Roll(20);
    }

    public bool AttackHits(int attackRoll)
    {
        return attackRoll <= Strength;
    }

    public int RollDamage()
    {
        return _die.Roll(Weapon.MaxDamage);
    }

    public string Attack(Player player, Messages messages)
    {
        var output = new List<string>();

        output.Add(string.Format(
            messages.GetMessage("dragon_attack_intro"),
            Name,
            player.Name,
            messages.TranslateWeaponForDisplay(Weapon.Type)
        ));

        if (!string.IsNullOrWhiteSpace(Weapon.AsciiArt))
            output.Add(Weapon.AsciiArt);

        int attackRoll = RollAttack();
        output.Add(string.Format(messages.GetMessage("dragon_attack_roll"), Name, attackRoll, Strength));

        if (!AttackHits(attackRoll))
        {
            output.Add(string.Format(messages.GetMessage("dragon_missed_player"), Name, player.Name));
            return string.Join(Environment.NewLine, output);
        }

        output.Add(string.Format(messages.GetMessage("dragon_hit_player"), Name, player.Name));

        int defenseRoll = _die.Roll(20);
        output.Add(string.Format(messages.GetMessage("block_roll"), player.Name, defenseRoll, player.Agility));

        if (defenseRoll <= player.Agility)
        {
            output.Add(string.Format(messages.GetMessage("player_defended_dragon_attack"), player.Name));
            return string.Join(Environment.NewLine, output);
        }

        int damage = RollDamage();

        // ARMOR: Protection is taken off the hit, never below zero.
        if (player.Armor != null)
        {
            int absorbed = player.Armor.Absorb(damage);
            damage -= absorbed;

            if (damage == 0)
            {
                output.Add(messages.GetMessage("armor_absorbed_all"));
                return string.Join(Environment.NewLine, output);
            }

            if (absorbed > 0)
                output.Add(string.Format(messages.GetMessage("armor_absorbed"), absorbed));
        }

        player.HealthPoints -= damage;

        if (player.HealthPoints < 0)
            player.HealthPoints = 0;

        output.Add(string.Format(messages.GetMessage("dragon_damage_dealt"), Name, damage));
        output.Add(string.Format(messages.GetMessage("player_health_now"), player.Name, player.HealthPoints));

        if (player.HealthPoints <= 0)
            output.Add(string.Format(messages.GetMessage("player_defeated_narrative"), player.Name));

        return string.Join(Environment.NewLine, output);
    }

    public string GetRandomDefenseTaunt(Messages messages)
    {
        int index = _die.Roll(DefenseTauntKeys.Length) - 1;
        return messages.GetMessage(DefenseTauntKeys[index]);
    }

    public string GetRandomDamageReply(Messages messages)
    {
        int index = _die.Roll(DamageReplyKeys.Length) - 1;
        return messages.GetMessage(DamageReplyKeys[index]);
    }

    public void DisplayStats(Messages messages)
    {
        Console.WriteLine(messages.GetMessage(StatsHeaderKey));
        Console.WriteLine(messages.GetMessage("stats_name") + Name);
        Console.WriteLine(messages.GetMessage("stats_strength") + Strength);
        Console.WriteLine(messages.GetMessage("stats_agility") + Agility);
        Console.WriteLine(messages.GetMessage("stats_health") + HealthPoints);
        Console.WriteLine(messages.GetMessage("stats_weapon") + messages.TranslateWeaponForDisplay(Weapon.Type));
        Console.WriteLine(messages.GetMessage("stats_damage") + Weapon.MaxDamage);
    }
}

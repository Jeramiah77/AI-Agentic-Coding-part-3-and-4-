public class Player
{
    // Stats are kept between 1 and 20 so race modifiers can't produce
    // a character who can never hit (Strength <= 0), starts dead
    // (HealthPoints <= 0), or can never miss/never be hurt (stat > 20).
    public const int MinStat = 1;
    public const int MaxStat = 20;

    private readonly Die _die;

    public Player() : this(new Die()) { }

    // Lets tests pass in a die with fixed rolls.
    public Player(Die die)
    {
        _die = die;
    }

    public string Name { get; set; } = "";
    public string Race { get; set; } = "";
    public string Occupation { get; set; } = "";

    public int Strength { get; set; }
    public int Agility { get; set; }
    public int HealthPoints { get; set; }

    public Weapon? Weapon { get; set; }
    public Armor? Armor { get; set; }

    // Gold is spent at the Merchant. It is 10 + d20, with no race modifier.
    public const int BaseGold = 10;
    public int Gold { get; set; }

    public void CreateCharacter(Messages messages)
    {
        Race = PromptForRace(messages);
        Name = PromptForName(messages);
        Occupation = PromptForOccupation(messages);

        AssignWeaponByOccupation();

        int strengthModifier = RollStrength(messages);
        RollAgility(messages);
        RollHealthPoints(messages, strengthModifier);
        RollGold(messages);

        PromptForNextAction(messages);
    }

    private string PromptForRace(Messages messages)
    {
        while (true)
        {
            Console.WriteLine(messages.GetMessage("race_prompt"));
            string? input = Console.ReadLine()?.Trim();

            if (messages.IsValidRace(input))
                return messages.NormalizeRace(input!);

            Console.WriteLine(messages.GetMessage("race_invalid"));
        }
    }

    private string PromptForName(Messages messages)
    {
        while (true)
        {
            Console.WriteLine(messages.GetMessage("name_prompt"));
            string? input = Console.ReadLine()?.Trim();

            if (!string.IsNullOrWhiteSpace(input))
                return input;

            Console.WriteLine(messages.GetMessage("name_invalid"));
        }
    }

    private string PromptForOccupation(Messages messages)
    {
        while (true)
        {
            Console.WriteLine(messages.GetMessage("occupation_prompt"));
            string? input = Console.ReadLine()?.Trim();

            if (messages.IsValidOccupation(input))
                return messages.NormalizeOccupation(input!);

            Console.WriteLine(messages.GetMessage("occupation_invalid"));
        }
    }

    private void AssignWeaponByOccupation()
    {
        switch (Occupation.ToLower())
        {
            case "fighter":
                Weapon = new Weapon("long sword", 12, "-)=====>");
                break;
            case "magician":
                Weapon = new Weapon("lightning bolt spell", 12, "zap~~~~~~");
                break;
            case "thief":
                Weapon = new Weapon("dagger", 6, "-)==>");
                break;
            case "archer":
                Weapon = new Weapon("long bow", 8, "}    -->");
                break;
        }
    }

    private int RollStrength(Messages messages)
    {
        PromptRoll(messages.GetMessage("roll_strength"), messages);
        int modifier = GenerateStrength();
        Console.WriteLine(string.Format(messages.GetMessage("rolled_strength"), Strength));
        return modifier;
    }

    private void RollAgility(Messages messages)
    {
        PromptRoll(messages.GetMessage("roll_agility"), messages);
        GenerateAgility();
        Console.WriteLine(string.Format(messages.GetMessage("rolled_agility"), Agility));
    }

    private void RollHealthPoints(Messages messages, int strengthModifier)
    {
        PromptRoll(messages.GetMessage("roll_health"), messages);
        GenerateHealthPoints(strengthModifier);
        Console.WriteLine(string.Format(messages.GetMessage("rolled_health"), HealthPoints));
    }

    private void RollGold(Messages messages)
    {
        PromptRoll(messages.GetMessage("roll_gold"), messages);
        GenerateGold();
        Console.WriteLine(string.Format(messages.GetMessage("rolled_gold"), Gold));
    }

    // --- Dice math (no console input, so it can be unit tested) ---

    // Rolls d20 for Strength plus a d4 modifier: Halflings subtract it,
    // every other race adds it. Returns the modifier, which also applies
    // to HealthPoints.
    public int GenerateStrength()
    {
        int roll = _die.Roll(20);
        int modifier = _die.Roll(4);

        if (Race == "Halfling")
            modifier = -modifier;

        Strength = Math.Clamp(roll + modifier, MinStat, MaxStat);
        return modifier;
    }

    // Rolls d20 for Agility; Halflings and Elves add a d4 bonus.
    public void GenerateAgility()
    {
        int agility = _die.Roll(20);

        if (Race == "Halfling" || Race == "Elf")
            agility += _die.Roll(4);

        Agility = Math.Clamp(agility, MinStat, MaxStat);
    }

    // Rolls d20 for HealthPoints plus the Strength modifier.
    // Only a minimum applies: extra health doesn't break combat.
    public void GenerateHealthPoints(int strengthModifier)
    {
        HealthPoints = Math.Max(MinStat, _die.Roll(20) + strengthModifier);
    }

    // Rolls d20 and adds BaseGold, giving 11 to 30 Gold for every race.
    public void GenerateGold()
    {
        Gold = BaseGold + _die.Roll(20);
    }

    private void PromptForNextAction(Messages messages)
    {
        while (true)
        {
            Console.WriteLine(messages.GetMessage("next_action"));
            string? input = Console.ReadLine()?.Trim();

            if (input == "1")
                DisplayStats(messages);
            else if (input == "2")
                return;
            else
                Console.WriteLine(messages.GetMessage("invalid"));
        }
    }

    public void DisplayStats(Messages messages)
    {
        Console.WriteLine(messages.GetMessage("stats_header"));
        Console.WriteLine(messages.GetMessage("stats_name") + Name);
        Console.WriteLine(messages.GetMessage("stats_race") + messages.TranslateRaceForDisplay(Race));
        Console.WriteLine(messages.GetMessage("stats_occupation") + messages.TranslateOccupationForDisplay(Occupation));
        Console.WriteLine(messages.GetMessage("stats_strength") + Strength);
        Console.WriteLine(messages.GetMessage("stats_agility") + Agility);
        Console.WriteLine(messages.GetMessage("stats_health") + HealthPoints);

        if (Weapon != null)
        {
            Console.WriteLine(messages.GetMessage("stats_weapon") + messages.TranslateWeaponForDisplay(Weapon.Type));
            Console.WriteLine(messages.GetMessage("stats_damage") + Weapon.MaxDamage);
        }

        if (Armor != null)
            Console.WriteLine(messages.GetMessage("stats_armor") + Armor.Protection);

        Console.WriteLine(messages.GetMessage("stats_gold") + Gold);
    }

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
        if (Weapon == null)
            return 0;

        return _die.Roll(Weapon.MaxDamage);
    }

    public string Attack(Monster monster, Messages messages)
    {
        if (Weapon == null)
            return "You have no weapon to attack with.";

        var output = new List<string>();

        output.Add(string.Format(
            messages.GetMessage(monster.PlayerAttackIntroKey),
            messages.TranslateOccupationForDisplay(Occupation),
            messages.TranslateWeaponForDisplay(Weapon.Type)
        ));

        // If this fails to compile, change AsciiArt to whatever exists in Weapon.cs
        output.Add(Weapon.AsciiArt);

        int attackRoll = RollAttack();
        output.Add(string.Format(messages.GetMessage("player_attack_roll"), attackRoll, Strength));

        // MISS
        if (!AttackHits(attackRoll))
        {
            output.Add(string.Format(messages.GetMessage("player_missed_dragon"), monster.Name));
            return string.Join(Environment.NewLine, output);
        }

        output.Add(string.Format(messages.GetMessage("player_hit_dragon"), monster.Name));

        int defenseRoll = _die.Roll(20);
        output.Add(string.Format(messages.GetMessage("block_roll"), monster.Name, defenseRoll, monster.Agility));

        // DEFENSE
        if (defenseRoll <= monster.Agility)
        {
            output.Add(string.Format(messages.GetMessage("dragon_defended_player_attack"), monster.Name));
            output.Add(string.Format(
                monster.GetRandomDefenseTaunt(messages),
                messages.TranslateRaceForDisplay(Race)
            ));
            return string.Join(Environment.NewLine, output);
        }

        // DAMAGE
        int damage = RollDamage();
        monster.HealthPoints -= damage;

        output.Add(string.Format(messages.GetMessage("player_damage_dealt"), damage));
        output.Add(string.Format(messages.GetMessage("dragon_health_now"), monster.Name, monster.HealthPoints));

        // END
        if (monster.HealthPoints <= 0)
            output.Add(messages.GetMessage(monster.DefeatedNarrativeKey));
        else
            output.Add(monster.GetRandomDamageReply(messages));

        return string.Join(Environment.NewLine, output);
    }

    private void PromptRoll(string prompt, Messages messages)
    {
        while (true)
        {
            Console.WriteLine(prompt);
            string? input = Console.ReadLine()?.Trim().ToLower();

            if (input == "roll")
                return;

            Console.WriteLine(messages.GetMessage("roll_invalid"));
        }
    }
}
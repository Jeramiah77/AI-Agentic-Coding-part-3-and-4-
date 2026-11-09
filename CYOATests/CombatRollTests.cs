// Combat text shows each attack roll, the number it had to beat, and,
// when an attack lands, the defender's block roll.
[TestClass]
public class CombatRollTests
{
    private static Messages GetMessages(string language = "English")
    {
        Messages messages = new Messages();
        messages.SetCurrentLanguage(language);
        messages.ReadDictionary();
        return messages;
    }

    // Every d20 rolls d20Roll; damage dice roll 1.
    private static Die FixedDie(int d20Roll) => new SidesDie(sides => sides == 20 ? d20Roll : 1);

    private static Player MakePlayer(Die die, int strength, int agility) => new Player(die)
    {
        Name = "TestHero",
        Race = "Human",
        Occupation = "Fighter",
        Strength = strength,
        Agility = agility,
        HealthPoints = 20,
        Weapon = new Weapon("long sword", 12, "-)=====>")
    };

    private static Dragon MakeDragon(Die die, int strength, int agility)
    {
        Dragon dragon = new Dragon(die);
        dragon.Strength = strength;
        dragon.Agility = agility;
        dragon.HealthPoints = 20;
        return dragon;
    }

    [TestMethod]
    public void PlayerAttack_Hit_ShowsAttackRollAndDragonBlockRoll()
    {
        Die die = FixedDie(5);
        Player player = MakePlayer(die, strength: 10, agility: 10);
        Dragon dragon = MakeDragon(die, strength: 10, agility: 3);
        Messages messages = GetMessages();

        string output = player.Attack(dragon, messages);

        StringAssert.Contains(output, string.Format(messages.GetMessage("player_attack_roll"), 5, 10));
        StringAssert.Contains(output, string.Format(messages.GetMessage("player_hit_dragon"), dragon.Name));
        StringAssert.Contains(output, string.Format(messages.GetMessage("block_roll"), dragon.Name, 5, 3));
    }

    [TestMethod]
    public void PlayerAttack_Miss_ShowsAttackRollAndNoBlockRoll()
    {
        Die die = FixedDie(15);
        Player player = MakePlayer(die, strength: 10, agility: 10);
        Dragon dragon = MakeDragon(die, strength: 10, agility: 3);
        Messages messages = GetMessages();

        string output = player.Attack(dragon, messages);

        StringAssert.Contains(output, string.Format(messages.GetMessage("player_attack_roll"), 15, 10));
        StringAssert.Contains(output, string.Format(messages.GetMessage("player_missed_dragon"), dragon.Name));
        Assert.IsFalse(output.Contains("to block"));
    }

    [TestMethod]
    public void DragonAttack_Hit_ShowsAttackRollAndPlayerBlockRoll()
    {
        Die die = FixedDie(5);
        Player player = MakePlayer(die, strength: 10, agility: 8);
        Dragon dragon = MakeDragon(die, strength: 10, agility: 10);
        Messages messages = GetMessages();

        string output = dragon.Attack(player, messages);

        StringAssert.Contains(output, string.Format(messages.GetMessage("dragon_attack_roll"), dragon.Name, 5, 10));
        StringAssert.Contains(output, string.Format(messages.GetMessage("dragon_hit_player"), dragon.Name, player.Name));
        StringAssert.Contains(output, string.Format(messages.GetMessage("block_roll"), player.Name, 5, 8));
        StringAssert.Contains(output, string.Format(messages.GetMessage("player_defended_dragon_attack"), player.Name));
    }

    [TestMethod]
    public void DragonAttack_Miss_ShowsAttackRollAndNoBlockRoll()
    {
        Die die = FixedDie(15);
        Player player = MakePlayer(die, strength: 10, agility: 8);
        Dragon dragon = MakeDragon(die, strength: 10, agility: 10);
        Messages messages = GetMessages();

        string output = dragon.Attack(player, messages);

        StringAssert.Contains(output, string.Format(messages.GetMessage("dragon_attack_roll"), dragon.Name, 15, 10));
        StringAssert.Contains(output, string.Format(messages.GetMessage("dragon_missed_player"), dragon.Name, player.Name));
        Assert.IsFalse(output.Contains("to block"));
    }

    [TestMethod]
    [DataRow("English")]
    [DataRow("Spanish")]
    [DataRow("French")]
    [DataRow("Italian")]
    public void RollMessages_ExistAndShowTheRoll(string language)
    {
        Messages messages = GetMessages(language);

        string playerRoll = string.Format(messages.GetMessage("player_attack_roll"), 17, 9);
        string dragonRoll = string.Format(messages.GetMessage("dragon_attack_roll"), "Smolderfang", 17, 9);
        string blockRoll = string.Format(messages.GetMessage("block_roll"), "Smolderfang", 17, 9);

        foreach (string text in new[] { playerRoll, dragonRoll, blockRoll })
        {
            Assert.IsFalse(text.StartsWith("["), $"{language} is missing a roll message.");
            StringAssert.Contains(text, "17");
            StringAssert.Contains(text, "9");
        }
    }
}

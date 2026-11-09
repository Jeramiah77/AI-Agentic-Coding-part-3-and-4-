// A die that always returns the same result for each size of die,
// so tests can force the lowest and highest possible rolls.
public class FixedDie : Die
{
    private readonly int _d20;
    private readonly int _d4;

    public FixedDie(int d20, int d4)
    {
        _d20 = d20;
        _d4 = d4;
    }

    public override int Roll(int sides) => sides switch
    {
        20 => _d20,
        4 => _d4,
        _ => 1
    };
}

[TestClass]
public class PlayerStatTests
{
    private static Player MakePlayer(string race, int d20, int d4) =>
        new Player(new FixedDie(d20, d4)) { Race = race };

    // --- Halfling minimums (the original bug) ---

    [TestMethod]
    public void GenerateStrength_HalflingWorstRoll_IsAtLeastOne()
    {
        // d20 = 1, d4 = 4 used to give Strength -3 (could never hit).
        Player player = MakePlayer("Halfling", d20: 1, d4: 4);
        player.GenerateStrength();
        Assert.AreEqual(1, player.Strength);
    }

    [TestMethod]
    public void GenerateStrength_Halfling_ReturnsNegativeModifier()
    {
        Player player = MakePlayer("Halfling", d20: 10, d4: 3);
        int modifier = player.GenerateStrength();
        Assert.AreEqual(-3, modifier);
        Assert.AreEqual(7, player.Strength);
    }

    [TestMethod]
    public void GenerateHealthPoints_HalflingWorstRoll_IsAtLeastOne()
    {
        // d20 = 1 with a -4 strength modifier used to give HealthPoints -3 (started dead).
        Player player = MakePlayer("Halfling", d20: 1, d4: 4);
        int modifier = player.GenerateStrength();
        player.GenerateHealthPoints(modifier);
        Assert.AreEqual(1, player.HealthPoints);
    }

    [TestMethod]
    public void GenerateHealthPoints_AppliesStrengthModifier_WhenAboveMinimum()
    {
        Player player = MakePlayer("Halfling", d20: 15, d4: 2);
        player.GenerateHealthPoints(-2);
        Assert.AreEqual(13, player.HealthPoints);
    }

    // --- Maximums ---

    [TestMethod]
    public void GenerateStrength_HumanBestRoll_IsAtMostTwenty()
    {
        // d20 = 20, d4 = 4 used to give Strength 24 (every attack hit).
        Player player = MakePlayer("Human", d20: 20, d4: 4);
        int modifier = player.GenerateStrength();
        Assert.AreEqual(20, player.Strength);
        Assert.AreEqual(4, modifier, "The modifier still applies to health even when Strength is capped.");
    }

    [TestMethod]
    public void GenerateAgility_ElfBestRoll_IsAtMostTwenty()
    {
        // d20 = 20, d4 = 4 used to give Agility 24 (dragon could never do damage).
        Player player = MakePlayer("Elf", d20: 20, d4: 4);
        player.GenerateAgility();
        Assert.AreEqual(20, player.Agility);
    }

    [TestMethod]
    public void GenerateAgility_HalflingBestRoll_IsAtMostTwenty()
    {
        Player player = MakePlayer("Halfling", d20: 19, d4: 3);
        player.GenerateAgility();
        Assert.AreEqual(20, player.Agility);
    }

    [TestMethod]
    public void GenerateHealthPoints_HasNoMaximum()
    {
        Player player = MakePlayer("Human", d20: 20, d4: 4);
        player.GenerateHealthPoints(4);
        Assert.AreEqual(24, player.HealthPoints);
    }

    // --- Normal values are unchanged ---

    [TestMethod]
    public void GenerateStrength_Human_AddsModifier()
    {
        Player player = MakePlayer("Human", d20: 10, d4: 3);
        Assert.AreEqual(3, player.GenerateStrength());
        Assert.AreEqual(13, player.Strength);
    }

    [TestMethod]
    public void GenerateAgility_Dwarf_GetsNoBonus()
    {
        Player player = MakePlayer("Dwarf", d20: 12, d4: 4);
        player.GenerateAgility();
        Assert.AreEqual(12, player.Agility);
    }

    // --- Every race, every possible roll, stays in range ---

    [TestMethod]
    public void AllRacesAndRolls_StayWithinLimits()
    {
        foreach (string race in new[] { "Human", "Elf", "Dwarf", "Halfling" })
            for (int d20 = 1; d20 <= 20; d20++)
                for (int d4 = 1; d4 <= 4; d4++)
                {
                    Player player = MakePlayer(race, d20, d4);
                    int modifier = player.GenerateStrength();
                    player.GenerateAgility();
                    player.GenerateHealthPoints(modifier);

                    string roll = $"{race}, d20={d20}, d4={d4}";
                    Assert.IsTrue(player.Strength is >= 1 and <= 20, $"Strength {player.Strength} out of range for {roll}");
                    Assert.IsTrue(player.Agility is >= 1 and <= 20, $"Agility {player.Agility} out of range for {roll}");
                    Assert.IsTrue(player.HealthPoints >= 1, $"HealthPoints {player.HealthPoints} below 1 for {roll}");
                }
    }
}

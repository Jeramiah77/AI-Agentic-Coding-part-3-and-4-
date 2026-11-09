using System.Text.Json;

// A die whose result depends only on the number of sides, so a test can
// say "every d20 rolls 1, every d12 rolls 5".
public class SidesDie : Die
{
    private readonly Func<int, int> _rollFor;

    public SidesDie(Func<int, int> rollFor)
    {
        _rollFor = rollFor;
    }

    public override int Roll(int sides) => _rollFor(sides);
}

[TestClass]
public class MerchantTests
{
    private static readonly string[] Languages = { "English", "Spanish", "French", "Italian" };

    private static Player MakePlayer(int gold, Weapon? weapon = null) => new Player
    {
        Name = "TestHero",
        Race = "Human",
        Occupation = "Fighter",
        Gold = gold,
        Weapon = weapon ?? new Weapon("long sword", 12, "-)=====>")
    };

    // --- Gold roll ---

    [TestMethod]
    [DataRow("Human")]
    [DataRow("Elf")]
    [DataRow("Dwarf")]
    [DataRow("Halfling")]
    public void GenerateGold_LowestRoll_IsEleven_ForEveryRace(string race)
    {
        Player player = new Player(new FixedDie(d20: 1, d4: 4)) { Race = race };
        player.GenerateGold();
        Assert.AreEqual(11, player.Gold);
    }

    [TestMethod]
    [DataRow("Human")]
    [DataRow("Elf")]
    [DataRow("Dwarf")]
    [DataRow("Halfling")]
    public void GenerateGold_HighestRoll_IsThirty_ForEveryRace(string race)
    {
        Player player = new Player(new FixedDie(d20: 20, d4: 4)) { Race = race };
        player.GenerateGold();
        Assert.AreEqual(30, player.Gold);
    }

    // --- Buying ---

    [TestMethod]
    public void Buy_Armor_WithExactGold_LeavesZeroGoldAndProtectionThree()
    {
        Player player = MakePlayer(gold: 12);

        bool bought = new Merchant().Buy(player, MerchantItem.MagicalArmor);

        Assert.IsTrue(bought);
        Assert.AreEqual(0, player.Gold);
        Assert.IsNotNull(player.Armor);
        Assert.AreEqual(3, player.Armor.Protection);
    }

    [TestMethod]
    [DataRow("long sword", 12)]
    [DataRow("lightning bolt spell", 12)]
    [DataRow("dagger", 6)]
    [DataRow("long bow", 8)]
    public void Buy_Weapon_WithExactGold_EnchantsOwnWeaponPlusFour(string type, int maxDamage)
    {
        Player player = MakePlayer(gold: 15, weapon: new Weapon(type, maxDamage, "art"));

        bool bought = new Merchant().Buy(player, MerchantItem.MagicalWeapon);

        Assert.IsTrue(bought);
        Assert.AreEqual(0, player.Gold);
        Assert.AreEqual(maxDamage + 4, player.Weapon!.MaxDamage);
        Assert.AreEqual("magical " + type, player.Weapon.Type);
        Assert.IsTrue(player.Weapon.IsMagical);
    }

    [TestMethod]
    public void Buy_Armor_OneGoldShort_FailsAndChangesNothing()
    {
        Player player = MakePlayer(gold: 11);

        bool bought = new Merchant().Buy(player, MerchantItem.MagicalArmor);

        Assert.IsFalse(bought);
        Assert.AreEqual(11, player.Gold);
        Assert.IsNull(player.Armor);
    }

    [TestMethod]
    public void Buy_Weapon_OneGoldShort_FailsAndChangesNothing()
    {
        Player player = MakePlayer(gold: 14);

        bool bought = new Merchant().Buy(player, MerchantItem.MagicalWeapon);

        Assert.IsFalse(bought);
        Assert.AreEqual(14, player.Gold);
        Assert.AreEqual(12, player.Weapon!.MaxDamage);
        Assert.IsFalse(player.Weapon.IsMagical);
    }

    [TestMethod]
    public void Buy_Both_WithTwentySevenGold_LeavesZero()
    {
        Player player = MakePlayer(gold: 27);
        Merchant merchant = new Merchant();

        Assert.IsTrue(merchant.Buy(player, MerchantItem.MagicalArmor));
        Assert.IsTrue(merchant.Buy(player, MerchantItem.MagicalWeapon));

        Assert.AreEqual(0, player.Gold);
        Assert.AreEqual(3, player.Armor!.Protection);
        Assert.AreEqual(16, player.Weapon!.MaxDamage);
    }

    [TestMethod]
    public void Buy_ArmorTwice_SecondPurchaseRefused()
    {
        Player player = MakePlayer(gold: 30);
        Merchant merchant = new Merchant();
        merchant.Buy(player, MerchantItem.MagicalArmor);

        bool boughtAgain = merchant.Buy(player, MerchantItem.MagicalArmor);

        Assert.IsFalse(boughtAgain);
        Assert.AreEqual(18, player.Gold);
        Assert.IsFalse(merchant.IsOffered(player, MerchantItem.MagicalArmor));
    }

    [TestMethod]
    public void Buy_WeaponTwice_SecondPurchaseRefused()
    {
        Player player = MakePlayer(gold: 30);
        Merchant merchant = new Merchant();
        merchant.Buy(player, MerchantItem.MagicalWeapon);

        bool boughtAgain = merchant.Buy(player, MerchantItem.MagicalWeapon);

        Assert.IsFalse(boughtAgain);
        Assert.AreEqual(15, player.Gold);
        Assert.AreEqual(16, player.Weapon!.MaxDamage);
        Assert.AreEqual("magical long sword", player.Weapon.Type);
    }

    [TestMethod]
    public void Buy_Weapon_PlayerWithNoWeapon_IsNotOfferedAndRefused()
    {
        Player player = MakePlayer(gold: 30);
        player.Weapon = null;
        Merchant merchant = new Merchant();

        Assert.IsFalse(merchant.IsOffered(player, MerchantItem.MagicalWeapon));
        Assert.IsFalse(merchant.Buy(player, MerchantItem.MagicalWeapon));
        Assert.AreEqual(30, player.Gold);
        Assert.IsNull(player.Weapon);
    }

    // --- Armor in combat ---

    // Every d20 rolls 1: the dragon (Strength 20) always hits, and the
    // player's block (Agility 0) always fails. The claw roll is d12.
    private static (Player player, Dragon dragon) SetUpLandedHit(int clawRoll, Armor? armor)
    {
        Die die = new SidesDie(sides => sides == 12 ? clawRoll : 1);
        Dragon dragon = new Dragon(die) { Strength = 20 };
        Player player = MakePlayer(gold: 0);
        player.Agility = 0;
        player.HealthPoints = 20;
        player.Armor = armor;
        return (player, dragon);
    }

    private static Messages GetMessages(string language = "English")
    {
        Messages messages = new Messages();
        messages.SetCurrentLanguage(language);
        messages.ReadDictionary();
        return messages;
    }

    [TestMethod]
    public void DragonAttack_WithArmor_ClawRollFive_PlayerLosesTwo()
    {
        var (player, dragon) = SetUpLandedHit(clawRoll: 5, armor: new Armor(3));
        Messages messages = GetMessages();

        string output = dragon.Attack(player, messages);

        Assert.AreEqual(18, player.HealthPoints);
        StringAssert.Contains(output, string.Format(messages.GetMessage("armor_absorbed"), 3));
        StringAssert.Contains(output, string.Format(messages.GetMessage("dragon_damage_dealt"), dragon.Name, 2));
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    public void DragonAttack_WithArmor_ClawRollThreeOrLess_PlayerLosesNothing(int clawRoll)
    {
        var (player, dragon) = SetUpLandedHit(clawRoll, armor: new Armor(3));
        Messages messages = GetMessages();

        string output = dragon.Attack(player, messages);

        Assert.AreEqual(20, player.HealthPoints);
        StringAssert.Contains(output, messages.GetMessage("armor_absorbed_all"));
    }

    [TestMethod]
    public void DragonAttack_WithoutArmor_TakesFullClawRoll()
    {
        var (player, dragon) = SetUpLandedHit(clawRoll: 5, armor: null);
        Messages messages = GetMessages();

        string output = dragon.Attack(player, messages);

        Assert.AreEqual(15, player.HealthPoints);
        Assert.IsFalse(output.Contains(string.Format(messages.GetMessage("armor_absorbed"), 3)));
    }

    // --- Displays ---

    [TestMethod]
    public void CombatStatsDisplay_ShowsGoldAndArmor()
    {
        Messages messages = GetMessages();
        Player player = MakePlayer(gold: 7);
        player.Armor = new Armor(3);
        Combat combat = new Combat(player, new Dragon(new Die()), messages);

        string output = combat.GetCombatStatsDisplay();

        StringAssert.Contains(output, messages.GetMessage("stats_gold") + 7);
        StringAssert.Contains(output, messages.GetMessage("stats_armor") + 3);
    }

    [TestMethod]
    public void CombatStatsDisplay_WithoutArmor_HidesArmorLine()
    {
        Messages messages = GetMessages();
        Combat combat = new Combat(MakePlayer(gold: 7), new Dragon(new Die()), messages);

        string output = combat.GetCombatStatsDisplay();

        Assert.IsFalse(output.Contains(messages.GetMessage("stats_armor")));
    }

    // --- Language data ---

    private static readonly string[] MerchantKeys =
    {
        "roll_gold", "stats_gold", "stats_armor", "south_path_narrative",
        "merchant_greeting", "merchant_gone", "merchant_gold",
        "merchant_option_armor", "merchant_option_weapon", "merchant_option_leave",
        "merchant_not_enough_gold", "merchant_bought_armor", "merchant_bought_weapon",
        "merchant_farewell", "armor_absorbed", "armor_absorbed_all"
    };

    private static readonly string[] MagicalWeapons =
    {
        "magical long sword", "magical lightning bolt spell", "magical dagger", "magical long bow"
    };

    [TestMethod]
    public void LanguageFile_HasEveryMerchantKey_InAllFourLanguages()
    {
        string json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "language_data.json"));
        using JsonDocument doc = JsonDocument.Parse(json);

        foreach (string language in Languages)
        {
            JsonElement section = doc.RootElement.GetProperty(language);
            JsonElement dictionary = section.GetProperty("dictionary");
            JsonElement weapons = section.GetProperty("displayWeaponMap");

            foreach (string key in MerchantKeys)
                Assert.IsTrue(dictionary.TryGetProperty(key, out _), $"{language} is missing '{key}'.");

            foreach (string weapon in MagicalWeapons)
                Assert.IsTrue(weapons.TryGetProperty(weapon, out _), $"{language} is missing weapon '{weapon}'.");
        }
    }

    [TestMethod]
    public void MerchantFormatStrings_FormatWithoutError_InAllFourLanguages()
    {
        foreach (string language in Languages)
        {
            Messages messages = GetMessages(language);

            // string.Format throws if a translation uses a placeholder
            // the code doesn't supply (e.g. {3}).
            string.Format(messages.GetMessage("merchant_gold"), 20);
            string.Format(messages.GetMessage("merchant_option_armor"), 3, 12);
            string.Format(messages.GetMessage("merchant_option_weapon"), "dagger", 4, 15);
            string.Format(messages.GetMessage("merchant_bought_weapon"), "magical dagger");
            string.Format(messages.GetMessage("armor_absorbed"), 3);
        }
    }
}

[TestClass]
[DoNotParallelize]
public class MerchantEncounterInputTests
{
    private TextReader? _originalIn;
    private TextWriter? _originalOut;

    [TestInitialize]
    public void Setup()
    {
        _originalIn = Console.In;
        _originalOut = Console.Out;
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (_originalIn != null)
            Console.SetIn(_originalIn);

        if (_originalOut != null)
            Console.SetOut(_originalOut);
    }

    private static Messages GetMessages()
    {
        Messages messages = new Messages();
        messages.SetCurrentLanguage("English");
        messages.ReadDictionary();
        return messages;
    }

    private static Player MakePlayer(int gold) => new Player
    {
        Name = "TestHero",
        Race = "Human",
        Occupation = "Fighter",
        Gold = gold,
        Weapon = new Weapon("long sword", 12, "-)=====>")
    };

    // Runs one Merchant Encounter with the given typed input and
    // returns everything printed.
    private static string RunEncounter(Game game, Player player, Messages messages, string input)
    {
        Console.SetIn(new StringReader(input));
        using StringWriter writer = new StringWriter();
        Console.SetOut(writer);

        game.HandleMerchantEncounter(player, messages);

        return writer.ToString();
    }

    [TestMethod]
    public void FirstTripSouth_ShowsMerchant_SecondTrip_MerchantHasMovedOn()
    {
        Messages messages = GetMessages();
        Game game = new Game();
        Player player = MakePlayer(gold: 20);

        string first = RunEncounter(game, player, messages, "3\n");
        string second = RunEncounter(game, player, messages, "1\n");

        StringAssert.Contains(first, messages.GetMessage("merchant_greeting"));
        Assert.IsTrue(game.MerchantMet);
        StringAssert.Contains(second, messages.GetMessage("merchant_gone"));
        Assert.IsFalse(second.Contains(messages.GetMessage("merchant_greeting")));
        Assert.IsNull(player.Armor, "The second trip must not sell anything.");
    }

    [TestMethod]
    public void Leave_BuysNothing()
    {
        Messages messages = GetMessages();
        Player player = MakePlayer(gold: 20);

        string output = RunEncounter(new Game(), player, messages, "3\n");

        Assert.AreEqual(20, player.Gold);
        Assert.IsNull(player.Armor);
        Assert.IsFalse(player.Weapon!.IsMagical);
        StringAssert.Contains(output, messages.GetMessage("merchant_farewell"));
    }

    [TestMethod]
    public void ClosedInput_LeavesShopInsteadOfHanging()
    {
        Messages messages = GetMessages();
        Player player = MakePlayer(gold: 20);

        string output = RunEncounter(new Game(), player, messages, "");

        Assert.AreEqual(20, player.Gold);
        StringAssert.Contains(output, messages.GetMessage("merchant_farewell"));
    }

    [TestMethod]
    public void InvalidInput_RepromptsAndBuysNothing()
    {
        Messages messages = GetMessages();
        Player player = MakePlayer(gold: 20);

        string output = RunEncounter(new Game(), player, messages, "xyz\n3\n");

        StringAssert.Contains(output, messages.GetMessage("invalid"));
        Assert.AreEqual(20, player.Gold);
        Assert.IsNull(player.Armor);
    }

    [TestMethod]
    public void BuyingBoth_EndsEncounterWithoutChoosingLeave()
    {
        Messages messages = GetMessages();
        Player player = MakePlayer(gold: 27);

        string output = RunEncounter(new Game(), player, messages, "1\n2\n");

        Assert.AreEqual(0, player.Gold);
        Assert.IsNotNull(player.Armor);
        Assert.IsTrue(player.Weapon!.IsMagical);
        StringAssert.Contains(output, messages.GetMessage("merchant_farewell"));
    }

    [TestMethod]
    public void BoughtItem_DropsOffMenu_ChoosingItAgainIsInvalid()
    {
        Messages messages = GetMessages();
        Player player = MakePlayer(gold: 30);

        string output = RunEncounter(new Game(), player, messages, "1\n1\n3\n");

        Assert.AreEqual(18, player.Gold);
        StringAssert.Contains(output, messages.GetMessage("invalid"));
    }

    [TestMethod]
    public void CannotAfford_ShowsNotEnoughGold_AndBuysNothing()
    {
        Messages messages = GetMessages();
        Player player = MakePlayer(gold: 11);

        string output = RunEncounter(new Game(), player, messages, "2\n3\n");

        StringAssert.Contains(output, messages.GetMessage("merchant_not_enough_gold"));
        Assert.AreEqual(11, player.Gold);
        Assert.IsFalse(player.Weapon!.IsMagical);
    }

    [TestMethod]
    public void DisplayStats_ShowsGoldAndArmor()
    {
        Messages messages = GetMessages();
        Player player = MakePlayer(gold: 9);
        player.Armor = new Armor(3);

        using StringWriter writer = new StringWriter();
        Console.SetOut(writer);
        player.DisplayStats(messages);
        string output = writer.ToString();

        StringAssert.Contains(output, messages.GetMessage("stats_gold") + 9);
        StringAssert.Contains(output, messages.GetMessage("stats_armor") + 3);
    }
}

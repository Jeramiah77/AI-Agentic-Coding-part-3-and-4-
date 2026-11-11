using System.Text.Json;

[TestClass]
public class TrollTests
{
    private static readonly string[] Languages = { "English", "Spanish", "French", "Italian" };

    private static Messages GetMessages(string language = "English")
    {
        Messages messages = new Messages();
        messages.SetCurrentLanguage(language);
        messages.ReadDictionary();
        return messages;
    }

    [TestMethod]
    public void NewTroll_RollsD12Stats_AndCarriesAClub()
    {
        // Each die returns its number of sides, so a d12 roll shows up as 12.
        Troll troll = new Troll(new SidesDie(sides => sides));

        Assert.AreEqual("Grumblegut", troll.Name);
        Assert.AreEqual(12, troll.Strength);
        Assert.AreEqual(12, troll.Agility);
        Assert.AreEqual(12, troll.HealthPoints);
        Assert.AreEqual("club", troll.Weapon.Type);
        Assert.AreEqual(8, troll.Weapon.MaxDamage);
    }

    [TestMethod]
    public void TrollAttack_LandedHit_UsesClubDamageAndArmor()
    {
        // d20s roll 1 (troll hits, player block fails); club rolls 6.
        Troll troll = new Troll(new SidesDie(sides => sides == 8 ? 6 : 1)) { Strength = 12 };
        Player player = new Player { Name = "TestHero", Agility = 0, HealthPoints = 20, Armor = new Armor(3) };

        troll.Attack(player, GetMessages());

        Assert.AreEqual(17, player.HealthPoints);
    }

    private static readonly string[] TrollKeys =
    {
        "west_path_narrative", "troll_intro", "troll_stats_intro", "troll_stats_header",
        "combat_troll_header", "troll_encounter_prompt", "troll_flee", "troll_retreat_to_crossroads",
        "player_attack_intro_troll", "troll_defeated_narrative",
        "troll_defense_taunt_1", "troll_defense_taunt_2", "troll_defense_taunt_3",
        "troll_damage_reply_1", "troll_damage_reply_2", "troll_damage_reply_3",
        "troll_victory", "troll_hide_new", "troll_hide_upgrade", "troll_return", "troll_defeat",
        "troll_riddle_intro", "troll_riddle_1", "troll_riddle_1_options", "troll_riddle_2",
        "troll_riddle_2_options", "troll_riddle_3", "troll_riddle_3_options", "troll_riddle_prompt",
        "troll_riddle_right", "troll_riddle_wrong", "troll_aftermath_slain", "troll_aftermath_outwitted"
    };

    [TestMethod]
    public void LanguageFile_HasEveryTrollKey_InAllFourLanguages()
    {
        string json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "language_data.json"));
        using JsonDocument doc = JsonDocument.Parse(json);

        foreach (string language in Languages)
        {
            JsonElement section = doc.RootElement.GetProperty(language);
            JsonElement dictionary = section.GetProperty("dictionary");

            foreach (string key in TrollKeys)
                Assert.IsTrue(dictionary.TryGetProperty(key, out _), $"{language} is missing '{key}'.");

            Assert.IsTrue(section.GetProperty("displayWeaponMap").TryGetProperty("club", out _),
                $"{language} is missing weapon 'club'.");
        }
    }

    [TestMethod]
    public void TrollFormatStrings_FormatWithoutError_InAllFourLanguages()
    {
        foreach (string language in Languages)
        {
            Messages messages = GetMessages(language);

            string.Format(messages.GetMessage("troll_intro"), "Grumblegut");
            string.Format(messages.GetMessage("player_attack_intro_troll"), "Fighter", "long sword");
            string.Format(messages.GetMessage("troll_defense_taunt_2"), "Elf");
            string.Format(messages.GetMessage("troll_victory"), 10);
            string.Format(messages.GetMessage("troll_hide_new"), 3);
            string.Format(messages.GetMessage("troll_hide_upgrade"), 5);
            string.Format(messages.GetMessage("troll_riddle_right"), 4);
        }
    }

    [TestMethod]
    public void PathPrompt_MentionsWest_InAllFourLanguages()
    {
        foreach (string language in Languages)
            StringAssert.Contains(GetMessages(language).GetMessage("path_prompt_full"), "'w'", language);
    }
}

[TestClass]
[DoNotParallelize]
public class TrollEncounterInputTests
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

    private static Player MakePlayer(int strength = 20, int healthPoints = 20) => new Player
    {
        Name = "TestHero",
        Race = "Human",
        Occupation = "Fighter",
        Strength = strength,
        Agility = 0,
        HealthPoints = healthPoints,
        Gold = 0,
        Weapon = new Weapon("long sword", 12, "-)=====>")
    };

    // Every d20 rolls 1, so whoever has Strength 20 always hits and a
    // defender with Agility 0 never blocks. Other dice are set per test.
    private static Game MakeGame(int d3 = 1, int d6 = 1, int d8 = 1, int d10 = 1, int d12 = 5)
    {
        Die die = new SidesDie(sides => sides switch
        {
            3 => d3,
            6 => d6,
            8 => d8,
            10 => d10,
            12 => d12,
            _ => 1
        });
        return new Game(die);
    }

    // A Troll that falls to one landed hit and never lands one itself.
    private static void MakeTrollWeak(Game game)
    {
        game.Troll.Strength = 0;
        game.Troll.Agility = 0;
        game.Troll.HealthPoints = 1;
    }

    // A Troll that always hits and can't be hit by a Strength-0 Player.
    private static void MakeTrollStrong(Game game)
    {
        game.Troll.Strength = 20;
        game.Troll.Agility = 0;
        game.Troll.HealthPoints = 12;
    }

    private static (bool died, string output) Run(Game game, Player player, Messages messages, string input)
    {
        Console.SetIn(new StringReader(input));
        using StringWriter writer = new StringWriter();
        Console.SetOut(writer);

        bool died = game.HandleTrollEncounter(player, messages);

        return (died, writer.ToString());
    }

    // --- Meeting and fleeing ---

    [TestMethod]
    public void FirstTripWest_ShowsTrollIntroAndMenu()
    {
        Messages messages = GetMessages();
        Game game = MakeGame();

        var (_, output) = Run(game, MakePlayer(), messages, "f\n");

        StringAssert.Contains(output, messages.GetMessage("west_path_narrative"));
        StringAssert.Contains(output, string.Format(messages.GetMessage("troll_intro"), "Grumblegut"));
        StringAssert.Contains(output, messages.GetMessage("troll_stats_header"));
        StringAssert.Contains(output, messages.GetMessage("troll_encounter_prompt"));
    }

    [TestMethod]
    public void Flee_ReturnsUnharmed_AndTrollWaitsNextTime()
    {
        Messages messages = GetMessages();
        Game game = MakeGame();
        Player player = MakePlayer();

        var (died, first) = Run(game, player, messages, "f\n");
        var (_, second) = Run(game, player, messages, "flee\n");

        Assert.IsFalse(died);
        StringAssert.Contains(first, messages.GetMessage("troll_flee"));
        Assert.AreEqual(20, player.HealthPoints);
        Assert.AreEqual(0, player.Gold);
        Assert.AreEqual(TrollState.Unmet, game.TrollState);
        StringAssert.Contains(second, messages.GetMessage("troll_encounter_prompt"));
    }

    [TestMethod]
    public void ClosedInput_FleesInsteadOfHanging()
    {
        Messages messages = GetMessages();
        Game game = MakeGame();

        var (died, output) = Run(game, MakePlayer(), messages, "");

        Assert.IsFalse(died);
        StringAssert.Contains(output, messages.GetMessage("troll_flee"));
    }

    [TestMethod]
    public void InvalidInput_AsksAgain()
    {
        Messages messages = GetMessages();
        Game game = MakeGame();

        var (_, output) = Run(game, MakePlayer(), messages, "xyz\nf\n");

        StringAssert.Contains(output, messages.GetMessage("invalid"));
        Assert.AreEqual(TrollState.Unmet, game.TrollState);
    }

    // --- Fighting ---

    [TestMethod]
    [DataRow(1, 6)]
    [DataRow(10, 15)]
    public void WinningFight_GivesD10Plus5Gold_AndTrollHide(int d10, int expectedGold)
    {
        Messages messages = GetMessages();
        Game game = MakeGame(d10: d10);
        MakeTrollWeak(game);
        Player player = MakePlayer();

        var (died, output) = Run(game, player, messages, "a\n");

        Assert.IsFalse(died);
        Assert.AreEqual(TrollState.Slain, game.TrollState);
        Assert.AreEqual(expectedGold, player.Gold);
        Assert.AreEqual(3, player.Armor!.Protection);
        StringAssert.Contains(output, string.Format(messages.GetMessage("troll_victory"), expectedGold));
        StringAssert.Contains(output, messages.GetMessage("troll_defeated_narrative"));
        StringAssert.Contains(output, messages.GetMessage("troll_return"));
    }

    [TestMethod]
    public void WinningFight_WithArmor_AddsTwoProtection()
    {
        Messages messages = GetMessages();
        Game game = MakeGame();
        MakeTrollWeak(game);
        Player player = MakePlayer();
        player.Armor = new Armor(3);

        var (_, output) = Run(game, player, messages, "a\n");

        Assert.AreEqual(5, player.Armor!.Protection);
        StringAssert.Contains(output, string.Format(messages.GetMessage("troll_hide_upgrade"), 5));
    }

    [TestMethod]
    public void LosingFight_EndsTheGame_WithTrollDeathText()
    {
        Messages messages = GetMessages();
        Game game = MakeGame(d8: 8);
        MakeTrollStrong(game);
        Player player = MakePlayer(strength: 0, healthPoints: 1);

        var (died, output) = Run(game, player, messages, "a\n");

        Assert.IsTrue(died);
        Assert.AreEqual(0, player.HealthPoints);
        StringAssert.Contains(output, messages.GetMessage("troll_defeat"));
        Assert.AreEqual(TrollState.Unmet, game.TrollState);
    }

    [TestMethod]
    public void RetreatMidFight_KeepsDamage_AndTrollStaysUnmet()
    {
        Messages messages = GetMessages();
        Game game = MakeGame(d8: 2);
        MakeTrollStrong(game);
        Player player = MakePlayer(strength: 0);

        var (died, output) = Run(game, player, messages, "a\nr\n");

        Assert.IsFalse(died);
        Assert.AreEqual(18, player.HealthPoints);
        Assert.AreEqual(TrollState.Unmet, game.TrollState);
        StringAssert.Contains(output, messages.GetMessage("troll_retreat_to_crossroads"));
    }

    [TestMethod]
    public void TrollDamage_CarriesOverToTheDragonFight()
    {
        Messages messages = GetMessages();
        Game game = MakeGame(d8: 2);
        MakeTrollStrong(game);
        Player player = MakePlayer(strength: 0);

        Run(game, player, messages, "a\nr\n");
        string dragonStats = new Combat(player, new Dragon(new Die()), messages).GetCombatStatsDisplay();

        StringAssert.Contains(dragonStats, messages.GetMessage("stats_health") + 18);
    }

    // --- Riddles ---

    [TestMethod]
    [DataRow(1, "a")]
    [DataRow(2, "b")]
    [DataRow(3, "c")]
    public void RightAnswer_OutwitsTroll_ForD6Gold_AndNoHide(int riddle, string answer)
    {
        Messages messages = GetMessages();
        Game game = MakeGame(d3: riddle, d6: 4);
        Player player = MakePlayer();

        var (died, output) = Run(game, player, messages, $"r\n{answer}\n");

        Assert.IsFalse(died);
        Assert.AreEqual(TrollState.Outwitted, game.TrollState);
        Assert.AreEqual(4, player.Gold);
        Assert.IsNull(player.Armor);
        StringAssert.Contains(output, messages.GetMessage($"troll_riddle_{riddle}"));
        StringAssert.Contains(output, string.Format(messages.GetMessage("troll_riddle_right"), 4));
    }

    [TestMethod]
    [DataRow(1, "b")]
    [DataRow(1, "c")]
    [DataRow(2, "a")]
    [DataRow(3, "a")]
    public void WrongAnswer_StartsTheFight(int riddle, string answer)
    {
        Messages messages = GetMessages();
        Game game = MakeGame(d3: riddle);
        MakeTrollWeak(game);
        Player player = MakePlayer();

        var (_, output) = Run(game, player, messages, $"r\n{answer}\n");

        StringAssert.Contains(output, messages.GetMessage("troll_riddle_wrong"));
        StringAssert.Contains(output, messages.GetMessage("combat_troll_header"));
        Assert.AreEqual(TrollState.Slain, game.TrollState);
    }

    [TestMethod]
    public void InvalidRiddleAnswer_AsksAgain()
    {
        Messages messages = GetMessages();
        Game game = MakeGame(d3: 1);

        var (_, output) = Run(game, MakePlayer(), messages, "r\nd\na\n");

        StringAssert.Contains(output, messages.GetMessage("invalid"));
        Assert.AreEqual(TrollState.Outwitted, game.TrollState);
    }

    // --- Aftermath ---

    [TestMethod]
    [DataRow(TrollState.Slain, "troll_aftermath_slain")]
    [DataRow(TrollState.Outwitted, "troll_aftermath_outwitted")]
    public void ResolvedTroll_ShowsAftermath_AndChangesNothing(TrollState state, string key)
    {
        Messages messages = GetMessages();
        Game game = MakeGame();
        game.TrollState = state;
        Player player = MakePlayer();

        var (died, output) = Run(game, player, messages, "a\n");

        Assert.IsFalse(died);
        StringAssert.Contains(output, messages.GetMessage(key));
        Assert.IsFalse(output.Contains(messages.GetMessage("troll_encounter_prompt")));
        Assert.AreEqual(20, player.HealthPoints);
        Assert.AreEqual(0, player.Gold);
    }
}

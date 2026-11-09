[TestClass]
public class ItalianLanguageTests
{
    private Messages GetItalianMessages()
    {
        Messages messages = new Messages();
        messages.SetCurrentLanguage("Italian");
        messages.ReadDictionary();
        return messages;
    }

    [TestMethod]
    public void SetCurrentLanguage_SetsLanguageToItalian()
    {
        Messages messages = new Messages();
        messages.SetCurrentLanguage("Italian");
        Assert.AreEqual("Italian", messages.CurrentLanguage);
    }

    [TestMethod]
    public void ReadDictionary_LoadsItalianWelcomeMessage()
    {
        Assert.AreEqual("Benvenuto in Scegli la Tua Avventura", GetItalianMessages().GetMessage("welcome"));
    }

    [TestMethod]
    public void ReadDictionary_LoadsItalianGoodbyeMessage()
    {
        Assert.AreEqual("Arrivederci!", GetItalianMessages().GetMessage("goodbye"));
    }

    [TestMethod]
    public void ReadDictionary_ItalianHasEveryEnglishKey()
    {
        // Every message the game uses in English must also exist in Italian.
        string[] keys =
        {
            "welcome", "menu", "choose_path", "path_north", "path_south", "path_exit",
            "attack_prompt", "victory", "defeat", "retreat", "retreat_combat", "invalid",
            "goodbye", "race_prompt", "race_invalid", "name_prompt", "name_invalid",
            "occupation_prompt", "occupation_invalid", "roll_strength", "roll_agility",
            "roll_health", "roll_invalid", "next_action", "proceeding", "stats_header",
            "stats_name", "stats_race", "stats_occupation", "stats_strength",
            "stats_agility", "stats_health", "stats_weapon", "stats_damage",
            "adventure_ends", "path_prompt_full", "path_invalid", "south_path_narrative",
            "north_path_narrative", "dragon_intro", "dragon_stats_intro",
            "dragon_encounter_prompt", "dragon_stats_header", "combat_stats_header",
            "combat_player_header", "combat_dragon_header", "player_attack_intro",
            "player_missed_dragon", "player_hit_dragon", "dragon_defended_player_attack",
            "player_damage_dealt", "dragon_health_now", "dragon_defeated_narrative",
            "dragon_attack_intro", "dragon_missed_player", "dragon_hit_player",
            "player_defended_dragon_attack", "dragon_damage_dealt", "player_health_now",
            "player_defeated_narrative", "dragon_defense_taunt_1", "dragon_defense_taunt_2",
            "dragon_defense_taunt_3", "dragon_defense_taunt_4", "dragon_damage_reply_1",
            "dragon_damage_reply_2", "dragon_damage_reply_3", "dragon_damage_reply_4",
            "enter_choice"
        };

        Messages messages = GetItalianMessages();
        foreach (string key in keys)
            Assert.AreNotEqual($"[{key}]", messages.GetMessage(key), $"Key '{key}' is missing from Italian dictionary.");
    }

    [TestMethod]
    public void IsValidRace_AcceptsItalianRaceNames()
    {
        Messages messages = GetItalianMessages();
        Assert.IsTrue(messages.IsValidRace("Umano"));
        Assert.IsTrue(messages.IsValidRace("MEZZUOMO"));
        Assert.IsFalse(messages.IsValidRace("orco"));
    }

    [TestMethod]
    public void NormalizeRace_MapsItalianToCanonicalRace()
    {
        Assert.AreEqual("Halfling", GetItalianMessages().NormalizeRace("mezzuomo"));
    }

    [TestMethod]
    public void NormalizeOccupation_MapsItalianToCanonicalOccupation()
    {
        Messages messages = GetItalianMessages();
        Assert.AreEqual("Fighter", messages.NormalizeOccupation("guerriero"));
        Assert.AreEqual("Archer", messages.NormalizeOccupation("ARCIERE"));
    }

    [TestMethod]
    public void TranslateForDisplay_ShowsItalianNames()
    {
        Messages messages = GetItalianMessages();
        Assert.AreEqual("Mezzuomo", messages.TranslateRaceForDisplay("Halfling"));
        Assert.AreEqual("Ladro", messages.TranslateOccupationForDisplay("Thief"));
        Assert.AreEqual("spada lunga", messages.TranslateWeaponForDisplay("long sword"));
        Assert.AreEqual("artigli", messages.TranslateWeaponForDisplay("claws"));
    }

    [TestMethod]
    public void EndGame_DoesNotThrow_InItalian()
    {
        Game game = new Game();
        Messages messages = GetItalianMessages();
        game.EndGame(true, false, messages);
        game.EndGame(false, false, messages);
        game.EndGame(false, true, messages);
    }
}

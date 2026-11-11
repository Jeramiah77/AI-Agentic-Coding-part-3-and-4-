public class Game
{
    private readonly Merchant _merchant = new Merchant();

    // The Merchant Encounter happens only on the first trip south.
    public bool MerchantMet { get; private set; }

    // The Troll Encounter is resolved once the Troll is slain or outwitted.
    // Fleeing leaves it Unmet, so the Troll is waiting next time.
    public TrollState TrollState { get; set; } = TrollState.Unmet;

    // Troll rewards and limits.
    public const int TrollHideProtection = 3;
    public const int TrollHideUpgrade = 2;

    // The correct letter for each riddle, by riddle number (1-3).
    private static readonly string[] RiddleAnswers = { "", "a", "b", "c" };

    private readonly Die _die;
    private Troll? _troll;

    // The same Troll waits each time, keeping any wounds from earlier fights.
    public Troll Troll => _troll ??= new Troll(_die);

    public Game() : this(new Die())
    {
    }

    public Game(Die die)
    {
        _die = die;
    }

    public void StartGame()
    {
        // Title screen
        Console.WriteLine("=====================================");
        Console.WriteLine("Welcome to Choose Your Adventure Game");
        Console.WriteLine("=====================================\n");

        Dragon dragon = new Dragon(_die);

        // Language selection shown before Messages dictionary is loaded
        Console.WriteLine("Select language / Selecciona idioma / Choisissez la langue / Scegli la lingua:");
        Console.WriteLine("1. English  2. Espanol  3. Francais  4. Italiano");
        Console.Write("Enter choice: ");
        string? langInput = Console.ReadLine()?.Trim();
        string selectedLanguage = langInput switch { "2" => "Spanish", "3" => "French", "4" => "Italian", _ => "English" };

        // Initialize Messages with selected language (per sequence diagram)

        Messages messages = new Messages();
        messages.SetCurrentLanguage(selectedLanguage);
        messages.ReadDictionary();
        // Display welcome message
        Console.WriteLine();
        Console.WriteLine(messages.GetMessage("welcome"));
        Console.WriteLine();

        // Main menu loop
        bool running = true;
        while (running)
        {
            Console.WriteLine(messages.GetMessage("menu"));
            Console.Write(messages.GetMessage("enter_choice"));
            string? choice = Console.ReadLine()?.Trim();

            if (choice == "1")
            {
                Player player = new Player();
                player.CreateCharacter(messages);

                bool inAdventureMenu = true;
                while (inAdventureMenu)
                {
                    string selectedPath = PromptForPath(messages);

                    if (selectedPath == "exit")
                    {
                        Console.WriteLine(messages.GetMessage("adventure_ends"));
                        running = false;
                        inAdventureMenu = false;
                    }
                    else if (selectedPath == "south")
                    {
                        HandleMerchantEncounter(player, messages);
                    }
                    else if (selectedPath == "west")
                    {
                        bool playerDied = HandleTrollEncounter(player, messages);

                        if (playerDied)
                        {
                            running = false;
                            inAdventureMenu = false;
                        }
                    }
                    else if (selectedPath == "north")
                    {
                        HandleDragonEncounter(player, dragon, messages);

                        running = false;
                        inAdventureMenu = false;
                    }
                }
            }
            else if (choice == "2")
            {
                Console.WriteLine(messages.GetMessage("goodbye"));
                running = false;
            }
            else
            {
                Console.WriteLine(messages.GetMessage("invalid"));
            }
        }
    }

    public void EndGame(bool playerWon, bool playerRetreated, Messages messages)
    {
        if (playerRetreated)
            Console.WriteLine(messages.GetMessage("retreat"));
        else if (playerWon)
            Console.WriteLine(messages.GetMessage("victory"));
        else
            Console.WriteLine(messages.GetMessage("defeat"));
    }

    public bool IsValidMenuChoice(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        string trimmed = input.Trim();
        return trimmed == "1" || trimmed == "2";
    }

    public bool IsValidPathChoice(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        string trimmed = input.Trim().ToLower();
        return trimmed == "n" || trimmed == "north" ||
               trimmed == "s" || trimmed == "south" ||
               trimmed == "w" || trimmed == "west" ||
               trimmed == "e" || trimmed == "exit";
    }

    public bool IsValidCombatChoice(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        string trimmed = input.Trim().ToLower();
        return trimmed == "a" || trimmed == "attack" ||
               trimmed == "r" || trimmed == "retreat";
    }

    private string PromptForPath(Messages messages)
    {
        while (true)
        {
            Console.WriteLine(messages.GetMessage("path_prompt_full"));
            Console.Write(messages.GetMessage("enter_choice"));
            string? input = Console.ReadLine()?.Trim().ToLower();

            if (input == "north" || input == "n")
                return "north";

            if (input == "south" || input == "s")
                return "south";

            if (input == "west" || input == "w")
                return "west";

            if (input == "exit" || input == "e")
                return "exit";

            Console.WriteLine(messages.GetMessage("path_invalid"));
        }
    }

    // Runs the shop menu. Items use fixed numbers (1 armor, 2 weapon,
    // 3 leave); an item the Player already owns drops off the menu.
    // Afterwards the Player is back at the crossroads.
    public void HandleMerchantEncounter(Player player, Messages messages)
    {
        if (MerchantMet)
        {
            Console.WriteLine(messages.GetMessage("merchant_gone"));
            return;
        }

        MerchantMet = true;
        Console.WriteLine(messages.GetMessage("south_path_narrative"));
        Console.WriteLine(messages.GetMessage("merchant_greeting"));

        while (true)
        {
            bool armorOffered = _merchant.IsOffered(player, MerchantItem.MagicalArmor);
            bool weaponOffered = _merchant.IsOffered(player, MerchantItem.MagicalWeapon);

            if (!armorOffered && !weaponOffered)
            {
                Console.WriteLine(messages.GetMessage("merchant_farewell"));
                return;
            }

            Console.WriteLine(string.Format(messages.GetMessage("merchant_gold"), player.Gold));

            if (armorOffered)
                Console.WriteLine(string.Format(messages.GetMessage("merchant_option_armor"),
                    Merchant.ArmorProtection, Merchant.ArmorPrice));

            if (weaponOffered)
                Console.WriteLine(string.Format(messages.GetMessage("merchant_option_weapon"),
                    messages.TranslateWeaponForDisplay(player.Weapon!.Type), Merchant.WeaponDamageBonus, Merchant.WeaponPrice));

            Console.WriteLine(messages.GetMessage("merchant_option_leave"));
            Console.Write(messages.GetMessage("enter_choice"));
            string? input = Console.ReadLine()?.Trim();

            // End of input (e.g. stream closed): leave instead of looping forever.
            if (input == null || input == "3")
            {
                Console.WriteLine(messages.GetMessage("merchant_farewell"));
                return;
            }

            MerchantItem? item = input switch
            {
                "1" when armorOffered => MerchantItem.MagicalArmor,
                "2" when weaponOffered => MerchantItem.MagicalWeapon,
                _ => null
            };

            if (item == null)
                Console.WriteLine(messages.GetMessage("invalid"));
            else if (!_merchant.Buy(player, item.Value))
                Console.WriteLine(messages.GetMessage("merchant_not_enough_gold"));
            else if (item == MerchantItem.MagicalArmor)
                Console.WriteLine(messages.GetMessage("merchant_bought_armor"));
            else
                Console.WriteLine(string.Format(messages.GetMessage("merchant_bought_weapon"),
                    messages.TranslateWeaponForDisplay(player.Weapon!.Type)));
        }
    }

    // Runs the Troll Encounter on the West path. Returns true if the
    // Player died (game over); otherwise they are back at the crossroads.
    public bool HandleTrollEncounter(Player player, Messages messages)
    {
        if (TrollState == TrollState.Slain)
        {
            Console.WriteLine(messages.GetMessage("troll_aftermath_slain"));
            return false;
        }

        if (TrollState == TrollState.Outwitted)
        {
            Console.WriteLine(messages.GetMessage("troll_aftermath_outwitted"));
            return false;
        }

        Console.WriteLine(messages.GetMessage("west_path_narrative"));

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(string.Format(messages.GetMessage("troll_intro"), Troll.Name));
        Console.ResetColor();

        Console.WriteLine(messages.GetMessage("troll_stats_intro"));
        Troll.DisplayStats(messages);

        while (true)
        {
            Console.WriteLine(messages.GetMessage("troll_encounter_prompt"));
            Console.Write(messages.GetMessage("enter_choice"));
            string? input = Console.ReadLine()?.Trim().ToLower();

            // End of input (e.g. stream closed): flee instead of looping forever.
            if (input == null || input == "f" || input == "flee")
            {
                Console.WriteLine(messages.GetMessage("troll_flee"));
                return false;
            }

            if (input == "a" || input == "attack")
                return FightTroll(player, messages);

            if (input == "r" || input == "riddle")
            {
                if (AskRiddle(messages))
                {
                    TrollState = TrollState.Outwitted;
                    int gold = _die.Roll(6);
                    player.Gold += gold;
                    Console.WriteLine(string.Format(messages.GetMessage("troll_riddle_right"), gold));
                    return false;
                }

                Console.WriteLine(messages.GetMessage("troll_riddle_wrong"));
                return FightTroll(player, messages);
            }

            Console.WriteLine(messages.GetMessage("invalid"));
        }
    }

    // Asks one riddle picked with a d3. Returns true for the right answer.
    // Closed input counts as a wrong answer.
    private bool AskRiddle(Messages messages)
    {
        int riddle = _die.Roll(3);

        Console.WriteLine(messages.GetMessage("troll_riddle_intro"));
        Console.WriteLine(messages.GetMessage($"troll_riddle_{riddle}"));
        Console.WriteLine(messages.GetMessage($"troll_riddle_{riddle}_options"));

        while (true)
        {
            Console.WriteLine(messages.GetMessage("troll_riddle_prompt"));
            Console.Write(messages.GetMessage("enter_choice"));
            string? input = Console.ReadLine()?.Trim().ToLower();

            if (input == null)
                return false;

            if (input == "a" || input == "b" || input == "c")
                return input == RiddleAnswers[riddle];

            Console.WriteLine(messages.GetMessage("invalid"));
        }
    }

    // Returns true if the Player died.
    private bool FightTroll(Player player, Messages messages)
    {
        Combat combat = new Combat(player, Troll, messages);
        bool playerWon = combat.StartCombat();

        if (combat.PlayerRetreated)
        {
            Console.WriteLine(messages.GetMessage("troll_retreat_to_crossroads"));
            return false;
        }

        if (!playerWon)
        {
            Console.WriteLine(messages.GetMessage("troll_defeat"));
            return true;
        }

        TrollState = TrollState.Slain;

        int gold = _die.Roll(10) + 5;
        player.Gold += gold;
        Console.WriteLine(string.Format(messages.GetMessage("troll_victory"), gold));

        if (player.Armor == null)
        {
            player.Armor = new Armor(TrollHideProtection);
            Console.WriteLine(string.Format(messages.GetMessage("troll_hide_new"), TrollHideProtection));
        }
        else
        {
            player.Armor = new Armor(player.Armor.Protection + TrollHideUpgrade);
            Console.WriteLine(string.Format(messages.GetMessage("troll_hide_upgrade"), player.Armor.Protection));
        }

        Console.WriteLine(messages.GetMessage("troll_return"));
        return false;
    }

    private void HandleDragonEncounter(Player player, Dragon dragon, Messages messages)
    {
        Console.WriteLine(messages.GetMessage("north_path_narrative"));

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(string.Format(messages.GetMessage("dragon_intro"), dragon.Name));
        Console.ResetColor();

        Console.WriteLine(messages.GetMessage("dragon_stats_intro"));
        dragon.DisplayStats(messages);
        while (true)
        {
            Console.WriteLine(messages.GetMessage("dragon_encounter_prompt"));
            Console.Write(messages.GetMessage("enter_choice"));
            string? input = Console.ReadLine()?.Trim().ToLower();

            if (input == "a" || input == "attack")
            {
                Combat combat = new Combat(player, dragon, messages);
                bool playerWon = combat.StartCombat();

                if (combat.PlayerRetreated)
                {
                    return; //combat already handled retreat message
                }
                else
                {
                   EndGame(playerWon, false, messages); 
                }
                
                return;
            }

        if (input == "r" || input == "retreat")
        {
            Console.WriteLine(messages.GetMessage("retreat"));
            return;
        }

        Console.WriteLine(messages.GetMessage("invalid"));
    }
}
}

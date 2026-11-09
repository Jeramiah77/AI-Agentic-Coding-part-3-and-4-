using System.Text.Json;

public class Messages
{
    private const string DefaultLanguage = "English";
    private const string LanguageFileName = "language_data.json";

    // Private setter: change the language only through SetCurrentLanguage
    // so the loaded text can never get out of sync with CurrentLanguage.
    public string CurrentLanguage { get; private set; }

    private bool _isLoaded;
    private Dictionary<string, string> _dictionary;
    private Dictionary<string, string> _raceMap;
    private Dictionary<string, string> _occupationMap;
    private Dictionary<string, string> _displayRaceMap;
    private Dictionary<string, string> _displayOccupationMap;
    private Dictionary<string, string> _displayWeaponMap;

    public Messages()
    {
        CurrentLanguage = DefaultLanguage;

        // Every map starts empty so no method can hit a null reference,
        // even if it is called before ReadDictionary().
        _dictionary = new Dictionary<string, string>();
        _raceMap = new Dictionary<string, string>();
        _occupationMap = new Dictionary<string, string>();
        _displayRaceMap = new Dictionary<string, string>();
        _displayOccupationMap = new Dictionary<string, string>();
        _displayWeaponMap = new Dictionary<string, string>();
    }

    public void SetCurrentLanguage(string language)
    {
        string newLanguage = string.IsNullOrWhiteSpace(language) ? DefaultLanguage : language.Trim();
        bool changed = !string.Equals(newLanguage, CurrentLanguage, StringComparison.OrdinalIgnoreCase);

        CurrentLanguage = newLanguage;

        // If text was already loaded, reload it so we never show the old language.
        if (changed && _isLoaded)
            ReadDictionary();
    }

    public void ReadDictionary()
    {
        string jsonPath = Path.Combine(AppContext.BaseDirectory, LanguageFileName);

        string jsonText;
        try
        {
            jsonText = File.ReadAllText(jsonPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException(
                $"Could not read the language file '{jsonPath}'. Make sure {LanguageFileName} is copied to the output folder.", ex);
        }

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(jsonText);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"The language file '{jsonPath}' is not valid JSON: {ex.Message}", ex);
        }

        using (doc)
        {
            JsonElement langSection = FindLanguageSection(doc.RootElement);

            // Load everything into locals first, so a failure part-way through
            // doesn't leave this object with a mix of old and new languages.
            var dictionary = LoadMap(langSection, "dictionary");
            var raceMap = LoadMap(langSection, "raceMap");
            var occupationMap = LoadMap(langSection, "occupationMap");
            var displayRaceMap = LoadMap(langSection, "displayRaceMap");
            var displayOccupationMap = LoadMap(langSection, "displayOccupationMap");
            var displayWeaponMap = LoadMap(langSection, "displayWeaponMap");

            _dictionary = dictionary;
            _raceMap = raceMap;
            _occupationMap = occupationMap;
            _displayRaceMap = displayRaceMap;
            _displayOccupationMap = displayOccupationMap;
            _displayWeaponMap = displayWeaponMap;
            _isLoaded = true;
        }
    }

    // Finds the section for CurrentLanguage, ignoring case ("french" works).
    // Falls back to English if the language isn't in the file.
    private JsonElement FindLanguageSection(JsonElement root)
    {
        foreach (JsonProperty language in root.EnumerateObject())
        {
            if (string.Equals(language.Name, CurrentLanguage, StringComparison.OrdinalIgnoreCase))
            {
                CurrentLanguage = language.Name; // use the exact spelling from the file
                return language.Value;
            }
        }

        if (root.TryGetProperty(DefaultLanguage, out JsonElement fallback))
        {
            Console.Error.WriteLine($"Language '{CurrentLanguage}' not found. Using {DefaultLanguage}.");
            CurrentLanguage = DefaultLanguage;
            return fallback;
        }

        throw new InvalidOperationException(
            $"Language '{CurrentLanguage}' and the default '{DefaultLanguage}' are both missing from {LanguageFileName}.");
    }

    // Reads one section (e.g. "raceMap") into a dictionary.
    // Replaces the six copy-pasted loops that were here before.
    private Dictionary<string, string> LoadMap(JsonElement langSection, string sectionName)
    {
        if (!langSection.TryGetProperty(sectionName, out JsonElement section) ||
            section.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException(
                $"Section '{sectionName}' is missing for language '{CurrentLanguage}' in {LanguageFileName}.");
        }

        var map = new Dictionary<string, string>();
        foreach (JsonProperty entry in section.EnumerateObject())
        {
            // GetString() throws on numbers/booleans, so handle those safely.
            map[entry.Name] = entry.Value.ValueKind switch
            {
                JsonValueKind.String => entry.Value.GetString() ?? string.Empty,
                JsonValueKind.Null => string.Empty,
                _ => entry.Value.ToString()
            };
        }
        return map;
    }

    public string GetMessage(string key)
    {
        // Missing keys show as "[key]" so typos are visible on screen.
        return _dictionary.TryGetValue(key, out string? message) ? message : $"[{key}]";
    }

    public bool IsValidRace(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        return _raceMap.ContainsKey(input.Trim().ToLower());
    }

    public string NormalizeRace(string input)
    {
        return _raceMap.TryGetValue(input.Trim().ToLower(), out string? canonical) ? canonical : input;
    }

    public bool IsValidOccupation(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        return _occupationMap.ContainsKey(input.Trim().ToLower());
    }

    public string NormalizeOccupation(string input)
    {
        return _occupationMap.TryGetValue(input.Trim().ToLower(), out string? canonical) ? canonical : input;
    }

    public string TranslateRaceForDisplay(string race)
    {
        return _displayRaceMap.TryGetValue(race, out string? translated) ? translated : race;
    }

    public string TranslateOccupationForDisplay(string occupation)
    {
        return _displayOccupationMap.TryGetValue(occupation, out string? translated) ? translated : occupation;
    }

    public string TranslateWeaponForDisplay(string weaponType)
    {
        return _displayWeaponMap.TryGetValue(weaponType, out string? translated) ? translated : weaponType;
    }
}

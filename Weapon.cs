public class Weapon
{
    public const string MagicalPrefix = "magical ";

    public string Type { get; set; }
    public int MaxDamage { get; set; }
    public string AsciiArt { get; set; }

    public bool IsMagical => Type.StartsWith(MagicalPrefix);

    public Weapon(string type, int maxDamage, string asciiArt)
    {
        Type = type;
        MaxDamage = maxDamage;
        AsciiArt = asciiArt;
    }

    // Turns this weapon into its magical version: "dagger" becomes
    // "magical dagger" with extra damage. A weapon is enchanted only once.
    public void Enchant(int damageBonus)
    {
        if (IsMagical)
            return;

        Type = MagicalPrefix + Type;
        MaxDamage += damageBonus;
    }
}

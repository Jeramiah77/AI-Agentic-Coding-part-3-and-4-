// Protective gear worn by the Player. Each landed hit is reduced by
// Protection, but never below zero damage.
public class Armor
{
    public int Protection { get; }

    public Armor(int protection)
    {
        Protection = protection;
    }

    // Returns how much of an incoming hit the armor absorbs.
    public int Absorb(int damage)
    {
        return Math.Clamp(Protection, 0, Math.Max(damage, 0));
    }
}

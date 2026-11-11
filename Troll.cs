// How the Troll Encounter has been resolved so far.
public enum TrollState
{
    Unmet,
    Slain,
    Outwitted
}

// Grumblegut, the Troll guarding the bridge on the West path.
// A mid-tier fight: d12 stats and a club, against the dragon's d20 and claws.
public class Troll : Monster
{
    private static readonly string[] _defenseTauntKeys =
    {
        "troll_defense_taunt_1",
        "troll_defense_taunt_2",
        "troll_defense_taunt_3"
    };

    private static readonly string[] _damageReplyKeys =
    {
        "troll_damage_reply_1",
        "troll_damage_reply_2",
        "troll_damage_reply_3"
    };

    protected override string[] DefenseTauntKeys => _defenseTauntKeys;
    protected override string[] DamageReplyKeys => _damageReplyKeys;
    public override string PlayerAttackIntroKey => "player_attack_intro_troll";
    public override string DefeatedNarrativeKey => "troll_defeated_narrative";
    public override string StatsHeaderKey => "troll_stats_header";
    public override string CombatHeaderKey => "combat_troll_header";

    public Troll(Die die) : base(die, new Weapon("club", 8, "o==== club"))
    {
        Name = "Grumblegut";
        Strength = _die.Roll(12);
        Agility = _die.Roll(12);
        HealthPoints = _die.Roll(12);
    }
}

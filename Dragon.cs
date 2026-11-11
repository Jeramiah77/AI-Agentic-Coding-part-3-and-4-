public class Dragon : Monster
{
    private static readonly string[] _defenseTauntKeys =
    {
        "dragon_defense_taunt_1",
        "dragon_defense_taunt_2",
        "dragon_defense_taunt_3",
        "dragon_defense_taunt_4"
    };

    private static readonly string[] _damageReplyKeys =
    {
        "dragon_damage_reply_1",
        "dragon_damage_reply_2",
        "dragon_damage_reply_3",
        "dragon_damage_reply_4"
    };

    protected override string[] DefenseTauntKeys => _defenseTauntKeys;
    protected override string[] DamageReplyKeys => _damageReplyKeys;
    public override string PlayerAttackIntroKey => "player_attack_intro";
    public override string DefeatedNarrativeKey => "dragon_defeated_narrative";
    public override string StatsHeaderKey => "dragon_stats_header";
    public override string CombatHeaderKey => "combat_dragon_header";

    public Dragon(Die die) : base(die, new Weapon("claws", 12, "<<< claws >>>"))
    {
        Name = "Smolderfang";
        Strength = _die.Roll(20);
        Agility = _die.Roll(20);
        HealthPoints = _die.Roll(20);
    }
}
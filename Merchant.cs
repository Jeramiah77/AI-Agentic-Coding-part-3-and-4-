public enum MerchantItem
{
    MagicalArmor,
    MagicalWeapon
}

// The trader on the South path. Holds the shop rules only (prices, what
// a Player may buy, and what a purchase does) with no console input or
// output, so every rule can be unit tested. Game runs the menu.
public class Merchant
{
    public const int ArmorPrice = 12;
    public const int WeaponPrice = 15;
    public const int ArmorProtection = 3;
    public const int WeaponDamageBonus = 4;

    public int PriceOf(MerchantItem item) => item switch
    {
        MerchantItem.MagicalArmor => ArmorPrice,
        MerchantItem.MagicalWeapon => WeaponPrice,
        _ => throw new ArgumentOutOfRangeException(nameof(item))
    };

    // An item is on offer if the Player doesn't already own it.
    // A Player with no weapon has nothing to enchant.
    public bool IsOffered(Player player, MerchantItem item) => item switch
    {
        MerchantItem.MagicalArmor => player.Armor == null,
        MerchantItem.MagicalWeapon => player.Weapon != null && !player.Weapon.IsMagical,
        _ => false
    };

    public bool CanAfford(Player player, MerchantItem item)
    {
        return player.Gold >= PriceOf(item);
    }

    // Takes the Gold and hands over the item. Returns false, changing
    // nothing, if the item isn't on offer or the Player can't afford it.
    public bool Buy(Player player, MerchantItem item)
    {
        if (!IsOffered(player, item) || !CanAfford(player, item))
            return false;

        player.Gold -= PriceOf(item);

        if (item == MerchantItem.MagicalArmor)
            player.Armor = new Armor(ArmorProtection);
        else
            player.Weapon!.Enchant(WeaponDamageBonus);

        return true;
    }
}

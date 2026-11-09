using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class WeaponTests
{
    [TestMethod]
    public void NewWeapon_SetsPropertiesCorrectly()
    {
        Weapon weapon = new Weapon("long sword", 12, "-)====>");

        Assert.AreEqual("long sword", weapon.Type);
        Assert.AreEqual(12, weapon.MaxDamage);
        Assert.AreEqual("-)====>", weapon.AsciiArt);
    }
}
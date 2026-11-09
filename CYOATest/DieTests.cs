using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class DieTests
{
    [TestMethod]
    public void Roll_ReturnsValueWithinRange_ForTwentySidedDie()
    {
        Die die = new Die();
        int result = die.Roll(20);

        Assert.IsTrue(result >= 1 && result <= 20);
    }
}
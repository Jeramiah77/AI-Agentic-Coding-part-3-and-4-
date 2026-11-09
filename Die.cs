using System;

public class Die
{
    private readonly Random _random = new Random();

    // virtual so tests can substitute a die with fixed results
    public virtual int Roll(int sides)
    {
        return _random.Next(1, sides + 1);
    }
}
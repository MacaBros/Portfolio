using Xunit;

namespace DailyKata.Day01.Tests;

public class DamageHandlerTests
{
    [Fact]
    public void CalculateDamageTaken_Normal_ReturnsBaseDamage()
    {
        var handler = new DamageHandler();
        var damage = handler.CalculateDamageTaken(new Normal(), 10);
        Assert.Equal(10, damage);
    }

    [Fact]
    public void CalculateDamageTaken_Poisoned_ReturnsBaseDamagePlusTicksTimesTwo()
    {
        var handler = new DamageHandler();
        var damage = handler.CalculateDamageTaken(new Poisoned(2), 10);
        Assert.Equal(14, damage);
    }

    [Fact]
    public void CalculateDamageTaken_Shielded_ReturnsBaseDamageMinusStrength()
    {
        var handler = new DamageHandler();
        var damage = handler.CalculateDamageTaken(new Shielded(5), 10);
        Assert.Equal(5, damage);
    }

    [Fact]
    public void CalculateDamageTaken_ShieldedStrongerThanBaseDamage_ReturnsZero()
    {
        var handler = new DamageHandler();
        var damage = handler.CalculateDamageTaken(new Shielded(10), 5);
        Assert.Equal(0, damage);
    }
}

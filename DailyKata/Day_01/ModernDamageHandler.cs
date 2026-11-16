// C# Kata Day 1 — Modern C# 12 / .NET 8 Refactor
// Abstract record hierarchy + primary constructor + pattern-matching switch expression.

namespace DailyKata.Day01;

public abstract record StatusEffect;
public record Normal : StatusEffect;
public record Poisoned(int Ticks) : StatusEffect;
public record Shielded(int Strength) : StatusEffect;

public class DamageHandler()
{
    public int CalculateDamageTaken(StatusEffect se, int baseDamage) => se switch
    {
        Shielded s when s.Strength >= baseDamage => 0,
        Shielded s => baseDamage - s.Strength,

        Poisoned { Ticks: var ticks } => baseDamage + ticks * 2,

        Normal => baseDamage,

        _ => baseDamage
    };
}

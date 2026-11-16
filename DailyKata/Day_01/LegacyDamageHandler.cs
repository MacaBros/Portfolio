// C# Kata Day 1 — Legacy Unity-style (C# 4)
// At risk of becoming a monolithic class with magic numbers & nested conditionals.

public class PlayerState
{
    public int StateType; // 0 = Normal, 1 = Poisoned, 2 = Shielded
    public int PoisonTicks;
    public int ShieldStrength;
}

public class DamageHandler
{
    public int CalculateDamageTaken(PlayerState state, int incomingDamage)
    {
        if (state.StateType == 2) // Shielded
        {
            if (state.ShieldStrength >= incomingDamage)
                return 0;
            else
                return incomingDamage - state.ShieldStrength;
        }
        else if (state.StateType == 1) // Poisoned
        {
            return incomingDamage + (state.PoisonTicks * 2);
        }

        return incomingDamage; // Normal fallback
    }
}

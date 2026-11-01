using Microsoft.Xna.Framework;

namespace HarveyOverhaul.InjuryCare.Helpers;

public enum ExplosionSeverity
{
    Glancing,
    Close,
    Direct
}

/// <summary>An actual bomb hit, after vanilla immunity, blast bounds and damage reduction.</summary>
public readonly record struct ExplosionImpact(int Damage, ExplosionSeverity Severity, double DistanceRatio)
{
    public double ConsequenceChance => Severity switch
    {
        ExplosionSeverity.Glancing => 0.12,
        ExplosionSeverity.Close => 0.28,
        _ => 0.45
    };

    public static bool TryCreate(Rectangle area, Rectangle playerBox, int healthBefore, int healthAfter,
        bool isBomb, out ExplosionImpact impact)
    {
        impact = default;
        if (!isBomb || healthBefore <= 0 || healthAfter >= healthBefore
            || area.Width <= 0 || area.Height <= 0 || !area.Intersects(playerBox))
            return false;

        int damage = healthBefore - Math.Max(0, healthAfter);
        // Vanilla damages a square of tiles, including the corners. Match those bounds instead
        // of inventing a smaller circle; distance only changes the risk/severity of a confirmed hit.
        double distance = Math.Clamp(Math.Max(
            Math.Abs(playerBox.Center.X - area.Center.X) / (area.Width / 2.0),
            Math.Abs(playerBox.Center.Y - area.Center.Y) / (area.Height / 2.0)), 0.0, 1.0);
        ExplosionSeverity severity = damage < 8 || distance >= 0.70
            ? ExplosionSeverity.Glancing
            : damage >= 15 && distance <= 0.35 ? ExplosionSeverity.Direct : ExplosionSeverity.Close;
        impact = new ExplosionImpact(damage, severity, distance);
        return true;
    }

    /// <summary>One outcome per successful roll; shrapnel requires a strong direct hit.</summary>
    public string SelectInjury(double roll) => Severity switch
    {
        ExplosionSeverity.Glancing => "buffHurt",
        ExplosionSeverity.Close => roll < 0.50 ? "buffHurt" : roll < 0.80 ? "buffBurnWounds" : "buffDeepCuts",
        _ => roll < 0.35 ? "buffShrapnelWounds" : roll < 0.75 ? "buffBurnWounds" : "buffBadlyHurt"
    };

    /// <summary>Exclude this hit from the later health poll, preserving unrelated damage.</summary>
    public int ExcludeFromHealthSnapshot(int lastHealth, int currentHealth) =>
        Math.Max(currentHealth, lastHealth - Damage);
}

/// <summary>A grace period even after a failed roll, so chained bombs cannot farm consequences.</summary>
public sealed class ExplosionInjuryCooldown
{
    public const int GraceMinutes = 30;
    private int _day = -1;
    private int _minute = -1;

    public bool TryBegin(int day, int minute)
    {
        if (_day == day && minute >= _minute && minute - _minute < GraceMinutes)
            return false;
        _day = day;
        _minute = minute;
        return true;
    }

    public void Reset() => (_day, _minute) = (-1, -1);
}

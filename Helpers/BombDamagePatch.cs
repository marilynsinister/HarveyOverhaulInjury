using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Helpers;

/// <summary>
/// Observe the local delivery of vanilla's networked explosion damage. Scanning sprites detects
/// lit fuses and can miss explosions; patching explode itself misses delayed/multiplayer damage.
/// </summary>
public static class BombDamagePatch
{
    private static IMonitor? _monitor;
    private static Action<ExplosionImpact>? _onImpact;
    private static FieldInfo _isBomb = null!;
    private static FieldInfo _area = null!;

    public static void Apply(Harmony harmony, IMonitor monitor, Action<ExplosionImpact> onImpact)
    {
        _monitor = monitor;
        try
        {
            var argType = AccessTools.Inner(typeof(GameLocation), "DamagePlayersEventArg");
            var method = AccessTools.DeclaredMethod(typeof(GameLocation), "performDamagePlayers", new[] { argType });
            _isBomb = AccessTools.Field(argType, "IsBomb") ?? throw new MissingFieldException("DamagePlayersEventArg.IsBomb");
            _area = AccessTools.Field(argType, "Area") ?? throw new MissingFieldException("DamagePlayersEventArg.Area");
            if (method == null)
                throw new MissingMethodException("GameLocation.performDamagePlayers");

            _onImpact = onImpact;
            harmony.Patch(method,
                prefix: new HarmonyMethod(typeof(BombDamagePatch), nameof(Prefix)),
                postfix: new HarmonyMethod(typeof(BombDamagePatch), nameof(Postfix)));
            monitor.Log("[ExplosionInjury] Травмы привязаны к фактическому урону от взрыва.", LogLevel.Debug);
        }
        catch (Exception ex)
        {
            _onImpact = null;
            monitor.Log($"[ExplosionInjury] Не удалось подключить обработчик взрывов: {ex}", LogLevel.Error);
        }
    }

    private sealed record HitState(Farmer Player, int Health, Rectangle PlayerBox, Rectangle Area);

    private static void Prefix(GameLocation __instance, object __0, out HitState? __state)
    {
        __state = null;
        try
        {
            var player = Game1.player;
            if (_onImpact == null || player == null || !ReferenceEquals(player.currentLocation, __instance)
                || !(bool)_isBomb.GetValue(__0)!)
                return;

            __state = new HitState(player, player.health, player.GetBoundingBox(), (Rectangle)_area.GetValue(__0)!);
        }
        catch (Exception ex)
        {
            _monitor?.Log($"[ExplosionInjury] Ошибка чтения взрыва: {ex}", LogLevel.Error);
        }
    }

    private static void Postfix(HitState? __state)
    {
        if (__state == null || !ReferenceEquals(Game1.player, __state.Player))
            return;
        try
        {
            if (ExplosionImpact.TryCreate(__state.Area, __state.PlayerBox, __state.Health,
                __state.Player.health, isBomb: true, out var impact))
                _onImpact?.Invoke(impact);
        }
        catch (Exception ex)
        {
            // A medical consequence must never break the game's damage handling.
            _monitor?.Log($"[ExplosionInjury] Ошибка обработки попадания: {ex}", LogLevel.Error);
        }
    }
}

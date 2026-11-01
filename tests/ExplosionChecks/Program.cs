using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Runtime.Serialization;
using HarmonyLib;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.EventHandlers;
using HarveyOverhaul.InjuryCare.Helpers;
using HarveyOverhaul.InjuryCare.Managers;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using InjuryBuffManager = HarveyOverhaul.InjuryCare.Managers.BuffManager;

var root = new DirectoryInfo(AppContext.BaseDirectory);
while (root != null && !File.Exists(Path.Combine(root.FullName, "HarveyOverhaulInjury.csproj")))
    root = root.Parent;
string mod = root?.FullName ?? throw new Exception("Cannot find mod sources.");
string game = @"D:\Games\Steam\steamapps\common\Stardew Valley";
AssemblyLoadContext.Default.Resolving += (_, name) =>
{
    foreach (string dir in new[] { Path.Combine(mod, "bin/Debug/net6.0"),
        Path.GetFullPath(Path.Combine(mod, "../HarveyOverhaulCore/bin/Debug/net6.0")), game,
        Path.Combine(game, "smapi-internal") })
    {
        string path = Path.Combine(dir, name.Name + ".dll");
        if (File.Exists(path)) return AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
    }
    return null;
};
// Resolve game assemblies before the CLR loads Checks' game-typed static fields.
try { Assembly.GetExecutingAssembly().GetType("Checks")!.GetMethod("Run")!.Invoke(null, null); }
catch (Exception ex) { Console.Error.WriteLine(ex); Environment.ExitCode = 1; }

static class Checks
{
    static int _checks;
    static int _callbacks;
    static ExplosionImpact _lastHit;
    static Rectangle _playerBox = new(0, 0, 32, 32);
    static bool _blockDamage;
    static int _bruiseUpgradeAttempts;
    static readonly List<string> Hud = new();
    const BindingFlags Fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    static void Check(bool ok, string message)
    {
        if (!ok) throw new Exception("FAIL: " + message);
        _checks++;
    }
    static T Empty<T>() => (T)FormatterServices.GetUninitializedObject(typeof(T));
    static void Set(object target, string name, object? value) =>
        AccessTools.Field(target.GetType(), name).SetValue(target, value);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Run()
    {
        var area = new Rectangle(-320, -320, 640, 640);
        var center = new Rectangle(-16, -16, 32, 32);
        var edge = new Rectangle(280, -16, 32, 32);
        Check(!ExplosionImpact.TryCreate(area, center, 100, 100, true, out _), "immunity/no health loss has no consequence");
        Check(!ExplosionImpact.TryCreate(area, center, 100, 110, true, out _), "healing has no consequence");
        Check(!ExplosionImpact.TryCreate(area, center, 100, 80, false, out _), "non-bomb damage ignored");
        Check(!ExplosionImpact.TryCreate(area, new Rectangle(320, 0, 32, 32), 100, 80, true, out _), "touching outer boundary is outside");
        Check(!ExplosionImpact.TryCreate(area, new Rectangle(700, 0, 32, 32), 100, 80, true, out _), "remote explosion ignored");
        Check(ExplosionImpact.TryCreate(area, center, 100, 80, true, out var direct) && direct.Severity == ExplosionSeverity.Direct, "strong central hit");
        Check(ExplosionImpact.TryCreate(area, center, 100, 93, true, out var protectedHit) && protectedHit.Severity == ExplosionSeverity.Glancing, "damage reduction lowers severity");
        Check(ExplosionImpact.TryCreate(area, edge, 100, 80, true, out var fringe) && fringe.Severity == ExplosionSeverity.Glancing, "edge hit cannot give shrapnel");
        Check(ExplosionImpact.TryCreate(area, new Rectangle(155, 155, 32, 32), 100, 80, true, out var close) && close.Severity == ExplosionSeverity.Close, "off-center hit is intermediate");
        Check(ExplosionImpact.TryCreate(area, new Rectangle(310, 310, 32, 32), 100, 80, true, out _), "vanilla square corner remains a valid hit");
        Check(fringe.ConsequenceChance < close.ConsequenceChance && close.ConsequenceChance < direct.ConsequenceChance, "risk grows with severity");
        Check(fringe.SelectInjury(0.0) == "buffHurt" && fringe.SelectInjury(0.99) == "buffHurt", "edge only gives a bruise");
        Check(Enumerable.Range(0, 100).All(i => close.SelectInjury(i / 100.0) != "buffShrapnelWounds"), "intermediate hits never give shrapnel");
        Check(direct.SelectInjury(0.1) == "buffShrapnelWounds", "strong direct hit can give shrapnel");
        Check(direct.ExcludeFromHealthSnapshot(100, 80) == 80, "bomb damage removed from health poll");
        Check(direct.ExcludeFromHealthSnapshot(100, 70) == 80, "nearby monster's 10 damage retained");
        Check(direct.ExcludeFromHealthSnapshot(60, 80) == 80, "healing cannot make stale damage");
        var cooldown = new ExplosionInjuryCooldown();
        Check(cooldown.TryBegin(77, 600), "first hit gets a roll");
        Check(!cooldown.TryBegin(77, 600) && !cooldown.TryBegin(77, 629), "chain hits receive grace");
        Check(cooldown.TryBegin(77, 630), "risk returns at 30 minutes");
        Check(cooldown.TryBegin(78, 360), "new day clears grace");
        cooldown.Reset();
        Check(cooldown.TryBegin(78, 360), "loading another save clears grace");

        var monitor = DispatchProxy.Create<IMonitor, TestMonitor>();
        var harmony = new Harmony("Harvey.ExplosionChecks");
        BombDamagePatch.Apply(harmony, monitor, hit => { _callbacks++; _lastHit = hit; });
        var method = AccessTools.DeclaredMethod(typeof(GameLocation), "performDamagePlayers");
        Check(Harmony.GetPatchInfo(method)?.Owners.Contains(harmony.Id) == true, "production Harmony patch installs on the actual game");

        // Execute the installed game's actual network delivery handler. Only the low-level HP
        // mutation is replaced; its bounds, bridge, bomb immunity and book logic remain vanilla.
        harmony.Patch(AccessTools.Method(typeof(Farmer), nameof(Farmer.takeDamage)), prefix: new HarmonyMethod(typeof(Checks), nameof(Damage)));
        harmony.Patch(AccessTools.Method(typeof(Farmer), nameof(Farmer.GetBoundingBox)), prefix: new HarmonyMethod(typeof(Checks), nameof(Bounds)));
        harmony.Patch(AccessTools.Method(typeof(Game1), nameof(Game1.addHUDMessage), new[] { typeof(HUDMessage) }), prefix: new HarmonyMethod(typeof(Checks), nameof(CaptureHud)));
        var player = Empty<Farmer>();
        AccessTools.Field(typeof(Game1), "_player").SetValue(null, player);
        InitField(player, "buffs", player);
        InitField(player, "stats");
        InitField(player, "onBridge");
        InitField(player, "activeDialogueEvents");
        InitField(player, "mailReceived");
        InitField(player, "currentLocationRef");
        var location = Empty<GameLocation>();
        var gameInstance = Empty<Game1>();
        Set(gameInstance, "instanceGameLocation", location);
        AccessTools.Field(typeof(Game1), "game1").SetValue(null, gameInstance);
        Set(AccessTools.Field(typeof(Character), "currentLocationRef").GetValue(player)!, "_gameLocation", location);
        Set(AccessTools.Field(typeof(Character), "currentLocationRef").GetValue(player)!, "_dirty", false);
        player.health = 100;
        _playerBox = center;
        Check(ReferenceEquals(Game1.player, player) && ReferenceEquals(player.currentLocation, location), "local player fixture");
        Check(!player.hasBuff("dwarfStatue_3"), "buff fixture");
        Check(player.stats.Get("Book_Bombs") == 0, "stats fixture");
        Check(player.GetBoundingBox() == center, "bounds fixture");
        Check(!player.onBridge.Value, "bridge fixture");
        object Arg(bool bomb, Rectangle bounds, int damage)
        {
            var arg = Activator.CreateInstance(AccessTools.Inner(typeof(GameLocation), "DamagePlayersEventArg"))!;
            Set(arg, "IsBomb", bomb); Set(arg, "Area", bounds); Set(arg, "Damage", damage);
            return arg;
        }
        void Deliver(bool bomb = true, Rectangle? bounds = null, int damage = 20) =>
            method.Invoke(location, new[] { Arg(bomb, bounds ?? area, damage) });
        Deliver();
        Check(_callbacks == 1 && _lastHit.Damage == 20 && player.health == 80, "actual bomb delivery yields exactly one confirmed hit");
        Deliver(bounds: new Rectangle(1000, 1000, 64, 64));
        Check(_callbacks == 1 && player.health == 80, "distant delivery causes no consequence");
        Deliver(bomb: false);
        Check(_callbacks == 1 && player.health == 60, "other area damage never counts as a bomb");
        _blockDamage = true; Deliver(); _blockDamage = false;
        Check(_callbacks == 1 && player.health == 60, "invulnerability prevents medical consequences");
        var buffIds = (ICollection<string>)AccessTools.Field(player.buffs.GetType(), "AppliedBuffIds").GetValue(player.buffs)!;
        buffIds.Add("dwarfStatue_3"); Deliver(); buffIds.Remove("dwarfStatue_3");
        Check(_callbacks == 1 && player.health == 60, "dwarf statue bomb immunity respected");
        SetNetBool(player, "onBridge", true); Deliver(); SetNetBool(player, "onBridge", false);
        Check(_callbacks == 1 && player.health == 60, "bridge immunity respected");
        method.Invoke(Empty<GameLocation>(), new[] { Arg(true, area, 20) });
        Check(_callbacks == 1, "another location/player never produces a local consequence");
        player.stats.Set("Book_Bombs", 1); Deliver();
        Check(_callbacks == 2 && _lastHit.Damage == 15 && player.health == 45, "actual bomb book damage reduction used");
        _playerBox = edge; Deliver();
        Check(_callbacks == 3 && _lastHit.Severity == ExplosionSeverity.Glancing, "actual edge delivery classified as glancing");

        var stateManager = new StateManager(DispatchProxy.Create<IDataHelper, TestMonitor>(), monitor);
        var buffs = new InjuryBuffManager(monitor, null!);
        var config = new ModConfig();
        var injuries = new InjuryManager(monitor, stateManager, buffs, null!, null!, config);
        var selfCare = new SelfCareManager(monitor, stateManager, buffs, null!, null!, null!);
        var complications = new ComplicationManager(monitor, config, stateManager, buffs, Empty<DialogueManager>(), injuries, null!, selfCare);
        // Buff visuals and dialogue need a running game, so suppress just those presentation
        // operations. The real complication eligibility/state/HUD code is still executed.
        harmony.Patch(AccessTools.Method(typeof(InjuryBuffManager), nameof(InjuryBuffManager.AddBuff)), prefix: new HarmonyMethod(typeof(Checks), nameof(Skip)));
        harmony.Patch(AccessTools.Method(typeof(DialogueManager), nameof(DialogueManager.EnsureComplicationDialogueTopics)), prefix: new HarmonyMethod(typeof(Checks), nameof(Skip)));
        int hudBefore = Hud.Count;
        Check(!complications.TryApplyExplosionComplication(1), "healthy farmer cannot gain a complication");
        void Main(string id)
        {
            stateManager.State.ActiveDebuffs.Clear(); stateManager.State.ActiveComplications.Clear(); buffIds.Clear();
            stateManager.State.MainInjuryId = id;
            stateManager.State.ActiveDebuffs[id] = new DebuffState { BuffId = id, InjuryStartDay = 0 };
            buffIds.Add(id);
        }
        Main(InjuryBuffs.Cold);
        Check(!complications.TryApplyExplosionComplication(1) && Hud.Count == hudBefore, "cold does not create a false deterioration HUD");
        Main("buffFracturedBone");
        Check(complications.TryApplyExplosionComplication(1) && stateManager.State.ActiveComplications.ContainsKey(InjuryBuffs.PainFlare), "confirmed impact worsens an old fracture even outside mines");
        Check(!complications.TryApplyExplosionComplication(1) && Hud.Count == hudBefore + 1, "active pain flare never resets or repeats HUD");
        Main("buffDeepCuts");
        Check(complications.TryApplyExplosionComplication(1) && stateManager.State.ActiveComplications.ContainsKey(InjuryBuffs.DirtyWound), "existing open wound can collect blast dust");
        Check(!complications.TryApplyExplosionComplication(1) && Hud.Count == hudBefore + 2, "active dirty wound never resets or repeats HUD");
        Main("buffDeepCuts"); stateManager.State.NeglectLadder.PausedUntilDay = int.MaxValue;
        Check(!complications.TryApplyExplosionComplication(1), "promised medical grace prevents contamination escalation");
        stateManager.State.NeglectLadder.PausedUntilDay = -1;
        buffIds.Clear();
        Check(!complications.TryApplyExplosionComplication(1), "stale injury state without an active buff cannot cause HUD");

        Game1.random = new FixedRandom(0.45);
        Main("buffDeepCuts");
        stateManager.State.SelfCareProtections[SelfCareProtectionTypes.Antiseptic] = GameUtils.Today();
        Check(!complications.TryApplyExplosionComplication(0.8), "antiseptic halves dust risk");
        stateManager.State.SelfCareProtections.Clear();
        void DressWound()
        {
            var wound = stateManager.State.ActiveDebuffs["buffDeepCuts"];
            wound.TreatmentStarted = true; wound.TotalPhases = 3; wound.CurrentPhase = 1;
            buffIds.Clear(); buffIds.Add(injuries.GetPhaseBuffId("buffDeepCuts", 1));
        }
        DressWound();
        Check(!complications.TryApplyExplosionComplication(0.8), "actual treatment dressing halves dust risk");
        Game1.random = new FixedRandom(0.30);
        stateManager.State.SelfCareProtections[SelfCareProtectionTypes.Antiseptic] = GameUtils.Today();
        Check(!complications.TryApplyExplosionComplication(0.8), "dressing and antiseptic protections combine");
        stateManager.State.SelfCareProtections.Clear();
        Check(complications.TryApplyExplosionComplication(0.8), "dressing reduces risk without making wounds immune");

        // Exercise the actual player handler, including its health bookkeeping and failed-roll
        // grace period. Headless SMAPI has no loaded save, so replace just that readiness query.
        harmony.Patch(AccessTools.PropertyGetter(typeof(Context), nameof(Context.IsWorldReady)), prefix: new HarmonyMethod(typeof(Checks), nameof(WorldReady)));
        var hospital = new HospitalizationManager(monitor, config, Empty<DialogueManager>(), stateManager, buffs);
        var services = new Dictionary<Type, object>
        {
            [typeof(IMonitor)] = monitor, [typeof(ModConfig)] = config, [typeof(StateManager)] = stateManager,
            [typeof(InjuryBuffManager)] = buffs, [typeof(InjuryManager)] = injuries,
            [typeof(ComplicationManager)] = complications, [typeof(HospitalizationManager)] = hospital
        };
        var constructor = typeof(PlayerEventHandler).GetConstructors().Single();
        var handler = (PlayerEventHandler)constructor.Invoke(constructor.GetParameters()
            .Select(p => services.TryGetValue(p.ParameterType, out var service) ? service : null).ToArray());
        config.Difficulty = DifficultyPresets.Custom; config.CustomInjuryChanceMultiplier = 3;
        Game1.timeOfDay = 1000;
        Main("buffFracturedBone"); stateManager.State.LastHealth = 100; player.health = 70;
        handler.OnBombDamage(direct);
        Check(stateManager.State.LastHealth == 80, "handler excludes bomb HP while preserving monster damage");
        Check(stateManager.State.ActiveComplications.ContainsKey(InjuryBuffs.PainFlare), "handler applies one compatible consequence");
        Main("buffDeepCuts"); Game1.timeOfDay = 1010; stateManager.State.LastHealth = 100; player.health = 80;
        handler.OnBombDamage(direct);
        Check(stateManager.State.ActiveComplications.Count == 0 && stateManager.State.LastHealth == 80, "grace suppresses another outcome while still accounting for bomb HP");
        Game1.timeOfDay = 1030; handler.OnBombDamage(direct);
        Check(stateManager.State.ActiveComplications.ContainsKey(InjuryBuffs.DirtyWound), "handler permits another roll after grace");
        handler.ResetExplosionTracking(); Game1.timeOfDay = 1000;
        stateManager.State.ActiveDebuffs.Clear(); stateManager.State.ActiveComplications.Clear();
        stateManager.State.MainInjuryId = null; buffIds.Clear();
        config.CustomInjuryChanceMultiplier = 0; handler.OnBombDamage(direct);
        Check(stateManager.State.ActiveDebuffs.Count == 0, "disabled custom injury chance stays disabled for bombs");
        config.CustomInjuryChanceMultiplier = 3; Main("buffFracturedBone"); Game1.timeOfDay = 1010;
        handler.OnBombDamage(direct);
        Check(stateManager.State.ActiveComplications.Count == 0, "failed roll also starts the grace period");
        handler.ResetExplosionTracking(); player.health = 0;
        handler.OnBombDamage(direct);
        Check(stateManager.State.ActiveComplications.Count == 0, "lethal hit is left to pass-out/rescue handling");
        harmony.Patch(AccessTools.Method(typeof(InjuryManager), nameof(InjuryManager.ApplyBadlyHurtSafe)), prefix: new HarmonyMethod(typeof(Checks), nameof(BruiseUpgrade)));
        Main("buffHurt"); player.health = 60; handler.ResetExplosionTracking();
        handler.OnBombDamage(direct);
        Check(_bruiseUpgradeAttempts == 1, "strong direct hit may worsen an untreated bruise");
        handler.ResetExplosionTracking(); stateManager.State.ActiveDebuffs["buffHurt"].TreatmentStarted = true;
        handler.OnBombDamage(direct);
        Check(_bruiseUpgradeAttempts == 1, "treated bruise is not silently replaced");
        handler.ResetExplosionTracking(); stateManager.State.ActiveDebuffs["buffHurt"].TreatmentStarted = false;
        handler.OnBombDamage(fringe);
        Check(_bruiseUpgradeAttempts == 1, "glancing hit cannot upgrade an existing bruise");
        harmony.UnpatchAll(harmony.Id);
        Console.WriteLine($"PASS: {_checks} explosion checks against the production mod and installed game.");
    }

    static void InitField(object target, string name, Farmer? player = null)
    {
        var field = AccessTools.Field(target.GetType(), name);
        var ctor = field.FieldType.GetConstructors().OrderBy(c => c.GetParameters().Length).First();
        field.SetValue(target, ctor.Invoke(ctor.GetParameters().Select(p => p.ParameterType == typeof(Farmer) ? player : p.HasDefaultValue ? p.DefaultValue : null).ToArray()));
    }
    static void SetNetBool(object target, string name, bool value)
    {
        object net = AccessTools.Field(target.GetType(), name).GetValue(target)!;
        net.GetType().GetProperty("Value")!.SetValue(net, value);
    }
    static bool Damage(Farmer __instance, int damage) { if (!_blockDamage) __instance.health = Math.Max(0, __instance.health - damage); return false; }
    static bool Bounds(ref Rectangle __result) { __result = _playerBox; return false; }
    static bool CaptureHud(HUDMessage message) { Hud.Add(message.message); return false; }
    static bool WorldReady(ref bool __result) { __result = true; return false; }
    static bool BruiseUpgrade() { _bruiseUpgradeAttempts++; return false; }
    static bool Skip() => false;
}

sealed class FixedRandom : Random
{
    private readonly double _value;
    public FixedRandom(double value) => _value = value;
    public override double NextDouble() => _value;
}

public class TestMonitor : DispatchProxy
{
    protected override object? Invoke(MethodInfo? method, object?[]? args)
    {
        if (method?.Name == "Log" && args?.Length >= 2 && (LogLevel)args[1]! == LogLevel.Error)
            Console.Error.WriteLine(args[0]);
        return method!.ReturnType != typeof(void) && method.ReturnType.IsValueType
            ? Activator.CreateInstance(method.ReturnType) : null;
    }
}

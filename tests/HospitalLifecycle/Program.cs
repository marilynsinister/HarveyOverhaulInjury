using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Runtime.Serialization;
using HarveyOverhaul.Core.Services;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using HarveyOverhaul.InjuryCare.EventHandlers;
using HarveyOverhaul.InjuryCare.Managers;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework;

string workspace = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".."));
string game = @"D:\Games\Steam\steamapps\common\Stardew Valley";
AssemblyLoadContext.Default.Resolving += (_, name) =>
{
    foreach (string folder in new[] { Path.Combine(workspace, "HarveyOverhaulCore/bin/Debug/net6.0"),
        Path.Combine(workspace, "HarveyOverhaulInjury/bin/Debug/net6.0"), game, Path.Combine(game, "smapi-internal") })
    {
        string path = Path.Combine(folder, name.Name + ".dll");
        if (File.Exists(path)) return AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
    }
    return null;
};
try { Checks.Run(args); }
catch (Exception ex) { Console.Error.WriteLine(ex); Environment.ExitCode = 1; }

static class Checks
{
    const BindingFlags Fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    public static T Empty<T>() => (T)FormatterServices.GetUninitializedObject(typeof(T));
    public static void Set(object target, string name, object? value) => target.GetType().GetField(name, Fields)!.SetValue(target, value);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Run(string[] args)
    {
        int assertions = 0;
        void Check(bool condition, string message)
        {
            assertions++;
            if (!condition) throw new Exception(message);
        }
        var monitor = DispatchProxy.Create<IMonitor, SilentProxy>();
        var guard = new HarveyEventDialogueGuard(null!, monitor);
        var transition = typeof(HarveyEventDialogueGuard).GetMethod("ProcessMenuChange", Fields)!;
        DialogueBox? Change(Event? evt, IClickableMenu? oldMenu, IClickableMenu? newMenu) =>
            (DialogueBox?)transition.Invoke(guard, new object?[] { evt, oldMenu, newMenu });
        Event Scene(int command = 4)
        {
            var evt = Empty<Event>();
            evt.id = "HarveyHospitalFixture";
            evt.eventCommands = Enumerable.Repeat("pause 1", 20).ToArray();
            evt.CurrentCommand = command;
            return evt;
        }
        DialogueBox Box(string text = "Unfinished question")
        {
            var box = Empty<DialogueBox>();
            box.dialogues = new() { text };
            return box;
        }
        var scene = Scene();
        var consumed = Box("Lost money message");
        Change(scene, null, consumed);
        consumed.dialogues.Clear(); // Actual DialogueBox.receiveLeftClick consumes this list.
        var lostItems = Empty<ItemListMenu>();
        Change(scene, consumed, lostItems);
        Check(Change(scene, lostItems, null) == null, "Never restore the consumed hospitaldeath message after the loss list closes.");
        Check(consumed.getCurrentString() == "", "The original failure really produces an empty game DialogueBox.");

        var first = Box("Narration");
        var next = Box("Next event line");
        Change(scene, null, first);
        scene.CurrentCommand++;
        Change(scene, first, next);
        Check(Change(scene, next, null) == null, "Normal message -> speak transitions must not resurrect the previous page.");

        var question = Box();
        question.isQuestion = true;
        var foreign = Empty<ItemListMenu>();
        Change(scene, null, question);
        Change(scene, question, foreign);
        Check(ReferenceEquals(Change(scene, foreign, null), question), "Resume a live unanswered question interrupted within the same event command.");
        Check(Change(scene, question, null) == null, "A resumed question must not be restored a second time after answering.");

        var closing = Box();
        Change(scene, null, closing);
        closing.transitioning = true;
        closing.transitioningBigger = false;
        Change(scene, closing, foreign);
        Check(Change(scene, foreign, null) == null, "Do not revive a box already playing its closing animation.");

        var finished = Box();
        Change(scene, null, finished);
        finished.dialogueFinished = true;
        Change(scene, finished, foreign);
        Check(Change(scene, foreign, null) == null, "Do not revive a completed NPC dialogue.");

        var interrupted = Box();
        Change(scene, null, interrupted);
        Change(scene, interrupted, foreign);
        scene.CurrentCommand++;
        Check(Change(scene, foreign, null) == null, "Discard suspended text once the scene advances.");

        Change(scene, null, interrupted);
        Change(scene, interrupted, foreign);
        Check(Change(Scene(), foreign, null) == null, "A different scene must never inherit a suspended window.");
        Change(scene, null, interrupted);
        Change(scene, interrupted, foreign);
        Check(Change(null, foreign, null) == null, "Ending the event releases suspended windows.");

        var nested = Empty<ItemListMenu>();
        Change(scene, null, question);
        Change(scene, question, foreign);
        Change(scene, foreign, nested);
        Check(ReferenceEquals(Change(scene, nested, null), question), "A replacement foreign menu must preserve the original unanswered question.");
        Console.WriteLine($"PASS: {assertions} real-menu dialogue lifecycle checks.");

        const string homeScript = "continue/2 7/farmer 2 6 1 Harvey 3 6 3/skippable/viewport 2 7 true/speak Harvey \"Rest\"/viewport -1000 -1000/end dialogue Harvey \"Good night\"";
        var bedCompose = typeof(HarveyEventStaging).GetMethod("ComposeAtBed", Fields | BindingFlags.Static)!;
        string staged = (string)bedCompose.Invoke(null, new object[] { homeScript, new Point(31, 8), new Point(30, 8), "pause 200/" })!;
        var commands = HarveyEventStaging.SplitCommands(staged);
        Check(commands[2] == "farmer 31 8 2 Harvey 30 8 1", "Care scenes must use the real bed coordinates, not the player's entry tile.");
        Check(commands.Contains("showFrame farmer 107") && commands.Contains("ignoreCollisions farmer"), "The patient lies on the bed rather than standing at the door.");
        Check(commands[3] == "ignoreEventTileOffset" && commands.Contains("viewport 31 8 true"), "Moved beds and upgraded homes use absolute runtime coordinates.");
        Check(commands[^2] == "stopAnimation farmer" && commands[^1].StartsWith("end dialogue"), "A held bed pose must release before control returns.");
        Check(commands.Contains("viewport -1000 -1000"), "Preserve the closing fade while moving the camera to the bed.");
        string moved = (string)bedCompose.Invoke(null, new object[] { homeScript, new Point(12, 25), new Point(13, 25), "" })!;
        Check(moved.Contains("farmer 12 25 2 Harvey 13 25 3"), "Moving the bed must move both care actors and reverse Harvey's facing when needed.");

        var episode = new InjuryState { NeedsHarveyAfterExternalRescueHomeEvent = true,
            NeedsHarveyAfterHospitalDischargeHomeEvent = true, NeedsSevereNightRoundEvent = true,
            NeedsHarveyMorningAfterExhaustionEvent = true };
        typeof(HarveyHomeCareEventLauncher).GetMethod("CompleteCareEpisode", Fields | BindingFlags.Static)!.Invoke(null, new object[] { episode, 77, "buffBadlyHurt" });
        Check(HarveyHomeCareEventLauncher.HasHomeCareVisitToday(episode, 77), "A visit survives save/reload through a persisted day marker.");
        Check(!episode.NeedsHarveyAfterHospitalDischargeHomeEvent && !episode.NeedsSevereNightRoundEvent
            && !episode.NeedsHarveyAfterExternalRescueHomeEvent && !episode.NeedsHarveyMorningAfterExhaustionEvent,
            "One recovery visit consumes overlapping discharge/rescue/night-round requests instead of chaining scenes.");
        Check(!HarveyHomeCareEventLauncher.HasHomeCareVisitToday(episode, 78), "A future day's new care visit is still allowed.");
        episode.LastHomeCareVisitDay = -1;
        episode.LastNightRoundDay = -1;
        episode.HarveyAfterHospitalDischargeShownDay = 77;
        Check(HarveyHomeCareEventLauncher.HasHomeCareVisitToday(episode, 77), "Older saves with a shown-day marker must not replay their visit after updating.");

        var gameInstance = Empty<Game1>();
        typeof(Game1).GetField("game1", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!.SetValue(null, gameInstance);
        Set(gameInstance, "instanceGameLocation", Empty<GameLocation>());
        var player = Empty<Farmer>();
        typeof(Game1).GetField("_player", BindingFlags.Static | BindingFlags.NonPublic)!.SetValue(null, player);
        foreach (string name in new[] { "buffs", "activeDialogueEvents", "mailReceived", "stats" })
        {
            var field = typeof(Farmer).GetField(name)!;
            var ctor = field.FieldType.GetConstructors().OrderBy(c => c.GetParameters().Length).First();
            object?[] values = ctor.GetParameters().Select(p => p.ParameterType == typeof(Farmer) ? player : p.HasDefaultValue ? p.DefaultValue : null).ToArray();
            field.SetValue(player, ctor.Invoke(values));
        }
        var worldField = typeof(Game1).GetField("netWorldState")!;
        var worldRoot = Activator.CreateInstance(worldField.FieldType)!;
        var world = Activator.CreateInstance(worldField.FieldType.GenericTypeArguments[0])!;
        worldField.FieldType.GetMethod("Set")!.Invoke(worldRoot, new[] { world });
        worldField.SetValue(null, worldRoot);
        Game1.stats.DaysPlayed = 77;
        Game1.timeOfDay = 900;
        var data = DispatchProxy.Create<IDataHelper, SilentProxy>();
        var stateManager = new StateManager(data, monitor);
        var state = stateManager.State;
        var buffs = new HarveyOverhaul.InjuryCare.Managers.BuffManager(monitor, null!);
        var hospital = new HospitalizationManager(monitor, new ModConfig(), new DialogueManager(monitor), stateManager, buffs);
        void Admit(int progress = 0, int minimum = 120, string mode = "day")
        {
            state.IsHospitalized = true;
            state.HospitalizedInjuryId = "buffFracturedBone";
            state.HospitalizationReason = "general";
            state.HospitalStayProgressMinutes = progress;
            state.HospitalMinStayMinutes = minimum;
            state.LastHospitalTimeOfDay = 900;
            state.HospitalStayMode = mode;
            state.HospitalAdmissionDay = 77;
            Set(hospital, "_hospitalHoldActive", true);
            Set(hospital, "_dischargeAllowed", false);
        }
        Admit(30);
        hospital.StartForcedHospitalizationWithExplanation("buffFracturedBone", null, "mine_rescue");
        Check(state.HospitalStayProgressMinutes == 30 && state.HospitalizationReason == "general", "Repeated admission must preserve the existing stay and clock.");
        hospital.UpdateHospitalStayProgress(900, 910);
        Check(state.HospitalStayProgressMinutes == 40, "Count ordinary clinic minutes.");
        hospital.UpdateHospitalStayProgress(910, 900);
        Check(state.HospitalStayProgressMinutes == 40, "Do not subtract progress when time moves backwards.");
        hospital.UpdateHospitalStayProgress(900, 1800);
        Check(state.HospitalStayProgressMinutes == 40, "Ignore an arbitrary large console clock jump.");
        Admit(110);
        hospital.UpdateHospitalStayProgress(900, 910);
        Check(hospital.CanDischarge(), "The exact minimum stay must unlock the exit.");
        Admit(120, 120, "overnight");
        Check(!hospital.CanDischarge(), "An overnight admission must wait for morning even after its minute minimum.");
        state.HospitalOvernightCreditDay = 77;
        Check(hospital.CanDischarge(), "An overnight credit must unlock the exit.");
        state.HospitalOvernightCreditDay = -1;

        Admit(0, 1000);
        Game1.timeOfDay = 930;
        hospital.UpdateHospitalStayProgress(900, 930); // synchronous clock callback during a nap
        hospital.ApplyRestedMinutes(30, 0);
        Check(state.HospitalStayProgressMinutes == 30, "A nap must not count time twice when clock callbacks already ran.");
        Admit(10, 1000);
        hospital.ApplyRestedMinutes(30, 10); // callback arrives after the nap
        hospital.UpdateHospitalStayProgress(900, 930);
        Check(state.HospitalStayProgressMinutes == 40, "Late TimeChanged after resting must not count the same minutes twice.");
        Admit(0, 1000);
        hospital.UpdateHospitalStayProgress(900, 910);
        hospital.ApplyRestedMinutes(30, 0);
        Check(state.HospitalStayProgressMinutes == 30, "Partially delivered clock callbacks credit only the uncounted nap minutes.");

        Admit(50);
        state.HospitalizationCaseId = "case77";
        state.SavedActiveBuffs.Add(StatusBuffs.Hospitalized);
        Set(hospital, "_pendingReturnToHospital", true);
        Set(hospital, "_admissionIntroReason", "general");
        typeof(HospitalizationManager).GetMethod("ClearHospitalizationState", Fields)!.Invoke(hospital, new object?[] { false, null });
        Check(!hospital.IsHospitalized && !hospital.HasPendingReturnToHospital && state.HospitalizationCaseId == "", "Clearing the stay releases its lock and case.");
        Check(!state.SavedActiveBuffs.Contains(StatusBuffs.Hospitalized), "An old hospitalized buff must not return from the saved snapshot.");
        Check(typeof(HospitalizationManager).GetField("_admissionIntroReason", Fields)!.GetValue(hospital) == null, "Discharge must cancel delayed admission dialogue.");

        Game1.eventUp = true;
        Check(LocationEventLauncher.IsSceneBusy(), "An active event must defer a new rescue scene.");
        Game1.eventUp = false;
        typeof(Game1).GetField("_activeClickableMenu", BindingFlags.Static | BindingFlags.NonPublic)!.SetValue(null, lostItems);
        Check(LocationEventLauncher.IsSceneBusy(), "The loss list must finish before post-death care starts.");
        typeof(Game1).GetField("_activeClickableMenu", BindingFlags.Static | BindingFlags.NonPublic)!.SetValue(null, null);
        Game1.killScreen = true;
        Check(LocationEventLauncher.IsSceneBusy(), "Wait until the game's death transition completes.");
        Game1.killScreen = false;
        Check(LocationEventLauncher.EventKeyMatches("eventHarveyExhaustion/Time 600 2600", "eventHarveyExhaustion")
            && !LocationEventLauncher.EventKeyMatches("eventHarveyExhaustionOther", "eventHarveyExhaustion"), "Event IDs must match whole keys before their conditions.");

        Admit(20, 120, "overnight");
        state.HospitalAdmissionDay = 76;
        var overnight = typeof(HospitalizationManager).GetMethod("ApplyOvernightCreditIfNeeded", Fields)!;
        Check((bool)overnight.Invoke(hospital, null)!, "A new morning credits an unfinished overnight stay.");
        Check(state.HospitalStayProgressMinutes == 120 && hospital.CanDischarge(), "The morning credit completes the required stay and releases the exit.");
        Check(!(bool)overnight.Invoke(hospital, null)! && state.HospitalStayProgressMinutes == 120, "Repeated morning callbacks must not credit the same night twice.");
        state.HospitalizedInjuryId = ""; // Isolate discharge bookkeeping from the separate outpatient treatment system.
        state.HospitalizationCaseId = "case-night-76";
        hospital.Discharge();
        Check(!state.IsHospitalized && state.DischargedToday && state.HospitalizationCompletedCaseId == "case-night-76", "Actual discharge completes the case and releases the admission state.");

        var passOut = new PassOutHandler(monitor, new ModConfig(), stateManager, buffs, new DialogueManager(monitor), null!, null!);
        Game1.eventUp = true;
        state.PendingDeathRevive = true;
        state.PendingDeathLocation = "UndergroundMine121";
        passOut.TrackPassOut();
        Check(state.PendingDeathRevive && state.PendingDeathLocation == "UndergroundMine121", "A forced day end during death must retain the pending revive.");
        typeof(PassOutHandler).GetMethod("BeginHospitalPassOutWarp", Fields)!.Invoke(passOut, new object[] { "eventHarveyExhaustion", "exhaustion" });
        Check(state.PendingHospitalPassOutEventId == "eventHarveyExhaustion", "Busy admission must retain the hospital event for retry.");
        var retry = typeof(PassOutHandler).GetMethod("TryStartPendingAtLocation", Fields)!;
        Check(!(bool)retry.Invoke(passOut, new object?[] { null })! && state.PendingHospitalPassOutEventId == "eventHarveyExhaustion", "A busy warp callback must not consume a queued hospital scene.");
        typeof(PassOutHandler).GetMethod("BeginMineRescueMorningEvent", Fields)!.Invoke(passOut, new object[] { "eventHarveyMineRescueMorning" });
        Check(state.PendingMineRescueEventId == "eventHarveyMineRescueMorning", "A queued morning rescue survives an already open event.");
        Game1.eventUp = false;
        var guess = typeof(PassOutHandler).GetMethod("GuessVanillaReviveLocation", Fields)!;
        foreach (var (death, revive) in new[] { ("UndergroundMine1", "Mine"), ("UndergroundMine120", "Mine"),
            ("UndergroundMine121", "Hospital"), ("VolcanoDungeon3", "IslandSouth"), ("Town", "Hospital") })
            Check((string)guess.Invoke(passOut, new object[] { death })! == revive, $"Death route {death} -> {revive}.");
        Console.WriteLine($"PASS: {assertions} total dialogue, admission, clock, discharge and launch checks.");
    }
}

public class SilentProxy : DispatchProxy
{
    protected override object? Invoke(MethodInfo? method, object?[]? args) =>
        method!.ReturnType != typeof(void) && method.ReturnType.IsValueType ? Activator.CreateInstance(method.ReturnType) : null;
}


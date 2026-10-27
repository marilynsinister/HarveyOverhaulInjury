using System;
using HarveyOverhaul.InjuryCare.Core;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Triggers;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Записка Харви перед шахтой. При первом спуске за день (шахта, Череп, вулкан; Харви от 4 сердец)
    /// игра спрашивает, оставить ли ему записку, куда она ушла.
    ///   * Оставила — он знает, где искать: риск занести грязь в рану сегодня ниже (×<see cref="DirtyWoundMultiplier"/>),
    ///     а когда она возвращается домой — немного доверия, облегчение стресса и его реплика (тема <see cref="HomeTopic"/>).
    ///   * Не оставила — ничего плохого, просто без этого.
    ///   * «Больше не спрашивать» выключает вопрос (настройка AskMineNote, её можно вернуть в GMCM).
    /// </summary>
    public sealed class MineNoteManager
    {
        public const string HomeTopic = "HarveyMineNote_Home";
        public const double DirtyWoundMultiplier = 0.6;

        private const string NoteDayKey = "marilynsinister.HarveyOverhaul/MineNoteDay";
        private const string AskedDayKey = "marilynsinister.HarveyOverhaul/MineNoteAskedDay";
        private const string HomeDayKey = "marilynsinister.HarveyOverhaul/MineNoteHomeDay";
        private const string SourceId = "marilynsinister.HarveyOverhaul.Injury";
        private const int MinFriendship = 1000;
        private const int StressRelief = 6;
        private const int TrustPoints = 2;

        private readonly IMonitor _monitor;
        private readonly IModHelper _helper;
        private readonly ModConfig _config;
        private readonly CareTrustManager _careTrustManager;
        private readonly DialogueManager _dialogueManager;

        public MineNoteManager(IMonitor monitor, IModHelper helper, ModConfig config, CareTrustManager careTrustManager, DialogueManager dialogueManager)
        {
            _monitor = monitor;
            _helper = helper;
            _config = config;
            _careTrustManager = careTrustManager;
            _dialogueManager = dialogueManager;
        }

        private static string Today => Game1.stats.DaysPlayed.ToString();

        /// <summary>Сегодня записка оставлена (для риска грязной раны в PlayerEventHandler).</summary>
        public static bool LeftNoteToday
            => Game1.player?.modData.TryGetValue(NoteDayKey, out string? day) == true && day == Today;

        private static bool IsMine(GameLocation? location) => location is MineShaft or VolcanoDungeon;

        public void OnWarped(WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer)
                return;

            try
            {
                if (IsMine(e.NewLocation) && !IsMine(e.OldLocation))
                    TryAsk();
                else if (e.NewLocation is Farm or FarmHouse)
                    TryReturnHome();
            }
            catch (Exception ex)
            {
                _monitor.Log($"[MineNote] OnWarped error: {ex}", LogLevel.Error);
            }
        }

        private void TryAsk()
        {
            var player = Game1.player;
            if (!_config.AskMineNote || Game1.eventUp || Utility.isFestivalDay())
                return;

            if (player.modData.TryGetValue(AskedDayKey, out string? asked) && asked == Today)
                return;

            if (!player.friendshipData.TryGetValue("Harvey", out var friendship) || friendship.Points < MinFriendship)
                return;

            player.modData[AskedDayKey] = Today;
            string question = friendship.IsMarried()
                ? "Перед спуском ты вспоминаешь, о чём просил Харви: просто напиши, куда идёшь. Оставить ему записку на кухонном столе?"
                : "Харви как-то сказал: «Мне не нужно разрешение. Мне нужно знать, где тебя искать». Отправить ему записку, что ты в шахте?";

            // «Вернусь к ужину» — это про общий дом; пока не женаты, Харви ждёт её вечером не за своим столом.
            string noteText = friendship.IsMarried()
                ? "Да: «Ушла в шахту, вернусь к ужину»"
                : "Да: «Я в шахте, к вечеру поднимусь»";

            DelayedAction.functionAfterDelay(() =>
            {
                if (Game1.activeClickableMenu != null || Game1.eventUp)
                    return;

                Game1.currentLocation?.createQuestionDialogue(
                    question,
                    new[]
                    {
                        new Response("note", noteText),
                        new Response("skip", "Не сегодня"),
                        new Response("never", "Больше не спрашивать"),
                    },
                    (_, answer) => OnAnswer(answer));
            }, 400);
        }

        private void OnAnswer(string answer)
        {
            switch (answer)
            {
                case "note":
                    Game1.player.modData[NoteDayKey] = Today;
                    Game1.addHUDMessage(new HUDMessage("Записка ушла. Харви знает, где ты. Береги раны — и возвращайся.", HUDMessage.newQuest_type));
                    _monitor.Log("[MineNote] Записка оставлена", LogLevel.Debug);
                    break;
                case "never":
                    _config.AskMineNote = false;
                    _helper.WriteConfig(_config);
                    Game1.addHUDMessage(new HUDMessage("Больше не спрашиваю. Вернуть вопрос можно в настройках мода.", HUDMessage.newQuest_type));
                    break;
            }
        }

        private void TryReturnHome()
        {
            var player = Game1.player;
            if (!LeftNoteToday || (player.modData.TryGetValue(HomeDayKey, out string? home) && home == Today))
                return;

            player.modData[HomeDayKey] = Today;
            _careTrustManager.UnifiedTrust?.Award(SourceId, "mine_note_home", TrustPoints);
            TriggerActionManager.TryRunAction($"marilynsinister.HarveyStressMeter_ComfortRelief {StressRelief}", out _, out _);
            _dialogueManager.AddTopic(HomeTopic, 1);
            Game1.addHUDMessage(new HUDMessage("Ты дома, и Харви знает, что ты вернулась. Становится спокойнее.", HUDMessage.newQuest_type));
            _monitor.Log("[MineNote] Вернулась домой после записки: доверие и облегчение", LogLevel.Debug);
        }
    }
}

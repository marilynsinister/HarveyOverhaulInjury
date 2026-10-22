using HarveyOverhaul.Core.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Харви тоже устаёт и болеет. Иногда утром он «устал» (ночной вызов, долгий приём) или зимой «простыл».
    /// Фермерша может позаботиться о нём: подарить горячее блюдо или напиток (полная забота) или просто
    /// поговорить с ним (визит). Забота в ответ — дружба, немного общего доверия и бафф «Забота в ответ».
    /// Если не заметить — ничего плохого: просто упущенная возможность.
    ///
    /// Для CP: флаги-письма <see cref="TiredFlag"/> / <see cref="SickFlag"/> на сегодня и темы разговора
    /// HarveyWellbeing_* (реплики — assets/Code/dialogues/harvey_wellbeing.json).
    /// </summary>
    public sealed class HarveyWellbeingManager
    {
        public const string TiredFlag = "HarveyState_Tired";
        public const string SickFlag = "HarveyState_Sick";
        public const string CaredFlag = "HarveyState_Cared";
        /// <summary>Наутро после гибели или обморока фермерши — Харви не спал и всё ещё тревожится.</summary>
        public const string WorriedFlag = "HarveyState_Worried";

        public const string TiredTopic = "HarveyWellbeing_Tired";
        public const string SickTopic = "HarveyWellbeing_Sick";
        public const string WorriedTopic = "HarveyWellbeing_Worried";
        public const string ReliefTopic = "HarveyWellbeing_Relieved";
        public const string VisitTopic = "HarveyWellbeing_Visited";

        private const string GratefulBuffId = "HarveyWellbeing_Grateful";
        private const string LastStateDayKey = "marilynsinister.HarveyOverhaul/WellbeingLastDay";
        private const string SourceId = "marilynsinister.HarveyOverhaul.Injury";

        private const int CooldownDays = 5;
        private const int MinHearts = 6;
        private const double TiredChance = 0.12;
        private const double TiredChanceAfterPatientNight = 0.30;
        private const double SickChanceWinter = 0.07;
        private const double SickChanceFall = 0.03;

        /// <summary>Чем можно позаботиться о Харви, кроме любого приготовленного блюда.</summary>
        private static readonly HashSet<string> ReliefDrinks = new(StringComparer.Ordinal)
        {
            "(O)395",  // кофе
            "(O)253",  // тройной эспрессо
            "(O)614",  // зелёный чай
            "(O)903",  // имбирный эль
            "(O)marilynsinister.HarveyOverhaul_HerbalTea",
        };

        private readonly IMonitor _monitor;
        private readonly CareTrustManager _careTrustManager;
        private readonly DialogueManager _dialogueManager;
        private readonly StateManager _stateManager;

        private string? _pendingGiftItemId;
        private int _pendingGiftsBefore;
        private int _pendingGiftTicks;
        private bool _talkedBefore;

        public HarveyWellbeingManager(
            IMonitor monitor,
            CareTrustManager careTrustManager,
            DialogueManager dialogueManager,
            StateManager stateManager)
        {
            _monitor = monitor;
            _careTrustManager = careTrustManager;
            _dialogueManager = dialogueManager;
            _stateManager = stateManager;
        }

        public static bool IsTired => Game1.player?.mailReceived.Contains(TiredFlag) == true;
        public static bool IsSick => Game1.player?.mailReceived.Contains(SickFlag) == true;
        public static bool IsWorried => Game1.player?.mailReceived.Contains(WorriedFlag) == true;
        private static bool NeedsCare => IsTired || IsSick || IsWorried;
        private static bool IsCared => Game1.player?.mailReceived.Contains(CaredFlag) == true;

        public void OnDayStarted()
        {
            var player = Game1.player;
            player.mailReceived.Remove(TiredFlag);
            player.mailReceived.Remove(SickFlag);
            player.mailReceived.Remove(CaredFlag);
            player.mailReceived.Remove(WorriedFlag);
            _pendingGiftItemId = null;
            _talkedBefore = false;

            if (!CanHaveState(player) || Utility.isFestivalDay())
                return;

            int today = (int)Game1.stats.DaysPlayed;

            // Вчера она потеряла сознание — сегодня Харви тревожится (без кулдауна и случайности).
            if (_stateManager.State.RecentCollapseDays.Contains(today - 1))
            {
                player.mailReceived.Add(WorriedFlag);
                _dialogueManager.AddTopic(WorriedTopic, 1);
                player.modData[LastStateDayKey] = today.ToString();
                _monitor.Log("[Wellbeing] Харви сегодня: тревожится после вчерашнего обморока", LogLevel.Debug);
                return;
            }

            if (player.modData.TryGetValue(LastStateDayKey, out string? raw)
                && int.TryParse(raw, out int lastDay)
                && today - lastDay < CooldownDays)
            {
                return;
            }

            var random = Utility.CreateDaySaveRandom(1307);
            double sickChance = Game1.IsWinter ? SickChanceWinter : Game1.IsFall ? SickChanceFall : 0;
            // Ночь в клинике (госпитализация, ночной обход) — Харви не спал вместе с ней.
            var state = _stateManager.State;
            double tiredChance = state.LastNightRoundDay >= today - 1 || state.LastHospitalDischargeDay >= today - 1
                ? TiredChanceAfterPatientNight
                : TiredChance;

            string? flag = null, topic = null;
            if (random.NextDouble() < sickChance)
                (flag, topic) = (SickFlag, SickTopic);
            else if (random.NextDouble() < tiredChance)
                (flag, topic) = (TiredFlag, TiredTopic);

            if (flag == null)
                return;

            player.mailReceived.Add(flag);
            _dialogueManager.AddTopic(topic!, 1);
            player.modData[LastStateDayKey] = today.ToString();
            _monitor.Log($"[Wellbeing] Харви сегодня: {flag}", LogLevel.Debug);
        }

        /// <summary>Действие с предметом рядом с Харви — запоминаем, что, возможно, это подарок.</summary>
        public void OnButtonPressed(ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree || !e.Button.IsActionButton() || !NeedsCare || IsCared)
                return;

            if (Game1.player.ActiveObject is not { } item)
                return;

            var harvey = Game1.currentLocation?.isCharacterAtTile(e.Cursor.GrabTile) ?? Game1.currentLocation?.isCharacterAtTile(e.Cursor.Tile);
            if (harvey?.Name != "Harvey" || !Game1.player.friendshipData.TryGetValue("Harvey", out var friendship))
                return;

            _pendingGiftItemId = item.QualifiedItemId;
            _pendingGiftsBefore = friendship.GiftsToday;
            _pendingGiftTicks = 0;
            _pendingCategory = item.Category;
        }

        private int _pendingCategory;

        /// <summary>Подарок засчитан игрой (GiftsToday вырос) → смотрим, было ли это заботой. Плюс первый разговор за день.</summary>
        public void OnUpdateTicked()
        {
            if (!Context.IsWorldReady || !NeedsCare || IsCared)
                return;

            if (!Game1.player.friendshipData.TryGetValue("Harvey", out var friendship))
                return;

            if (_pendingGiftItemId != null)
            {
                if (friendship.GiftsToday > _pendingGiftsBefore)
                {
                    bool relief = _pendingCategory == StardewValley.Object.CookingCategory
                                  || ReliefDrinks.Contains(_pendingGiftItemId);
                    _pendingGiftItemId = null;
                    if (relief)
                    {
                        Care(full: true);
                        return;
                    }
                }
                else if (++_pendingGiftTicks > 120)
                {
                    _pendingGiftItemId = null;
                }
            }

            // Просто зашла поговорить — тоже забота, но поменьше. Ждём, пока закроется его реплика.
            if (friendship.TalkedToToday && !_talkedBefore && Game1.activeClickableMenu == null && !Game1.dialogueUp)
            {
                _talkedBefore = true;
                Care(full: false);
            }
        }

        private void Care(bool full)
        {
            var player = Game1.player;
            if (full)
            {
                player.mailReceived.Add(CaredFlag);
                player.changeFriendship(40, Game1.getCharacterFromName("Harvey"));
                _careTrustManager.UnifiedTrust?.Award(SourceId, "harvey_wellbeing_care", 3);
                player.applyBuff(new Buff(GratefulBuffId));
                _dialogueManager.AddTopic(ReliefTopic, 1);
                Game1.addHUDMessage(new HUDMessage(
                    IsWorried
                        ? "Харви долго не отпускал твою руку. Кажется, он наконец выдохнул."
                        : IsSick
                        ? "Харви согрелся. Кажется, впервые за день он позволил позаботиться о себе."
                        : "Харви выдохнул и улыбнулся. Усталость не ушла, но стала легче.",
                    HUDMessage.achievement_type));
                _monitor.Log("[Wellbeing] Фермерша позаботилась о Харви (подарок).", LogLevel.Debug);
                return;
            }

            player.changeFriendship(15, Game1.getCharacterFromName("Harvey"));
            _dialogueManager.AddTopic(VisitTopic, 1);
        }

        /// <summary>Секция для «Обзора»: подсказка, что Харви сегодня нужна забота.</summary>
        public HarveyPanelSectionDto? BuildOverviewSection()
        {
            if (!Context.IsWorldReady || !NeedsCare)
                return null;

            if (IsCared)
            {
                return new HarveyPanelSectionDto
                {
                    Title = "Ты позаботилась о Харви",
                    Body = "Сегодня он чуть меньше врач и чуть больше человек, которому тоже бывает тяжело.",
                    Priority = 900,
                    Severity = HarveyPanelSeverity.Success,
                };
            }

            return new HarveyPanelSectionDto
            {
                Title = IsWorried ? "Харви не спал из-за тебя" : IsSick ? "Харви простыл" : "Харви сегодня устал",
                Body = IsWorried
                    ? "Вчера ты потеряла сознание, и он всю ночь прислушивался к телефону. Зайди показать, что ты в порядке, — или принеси ему чай и что-нибудь тёплое."
                    : IsSick
                    ? "Он всё равно пошёл на приём. Горячий суп, чай или любое приготовленное блюдо — лучше любых слов. Можно и просто зайти."
                    : "Ночь была длинной. Кофе, чай или приготовленное блюдо помогут. Можно и просто зайти поговорить.",
                Priority = 900,
                Severity = HarveyPanelSeverity.Warning,
            };
        }

        private static bool CanHaveState(Farmer player)
        {
            if (!player.friendshipData.TryGetValue("Harvey", out var friendship))
                return false;

            return friendship.IsDating() || friendship.IsMarried() || friendship.Points >= MinHearts * 250;
        }
    }
}

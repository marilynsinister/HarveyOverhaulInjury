using HarveyOverhaul.InjuryCare.Core;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Старые ожоги на руках (прошлое фермерши, TraumaBackstory): зимой на холоде рубцы стягивает —
    /// лёгкий дебафф до конца дня. Мазь Харви (сцена C3 «Мазь для рубцов») снимает его и защищает до вечера.
    /// Первый постоянный след прошлого в геймплее, кроме грозы, — поэтому мягкий и отключаемый (EnableScarTightness).
    ///
    /// Работает только при флаге <see cref="EnabledFlag"/>: его ставит CP-триггер, если TraumaBackstory = true.
    /// </summary>
    public sealed class ScarCareManager
    {
        public const string EnabledFlag = "HarveyScars_Enabled";
        public const string TightnessBuffId = "HarveyScars_Tightness";
        public const string SalveBuffId = "HarveyScars_Salved";

        /// <summary>Тема для реплики Харви «рубцы тянет?» (CP: dialogues/harvey_scars_topics.json).</summary>
        public const string TightnessTopic = "HarveyScars_TightToday";
        public const string SalveUsedTopic = "HarveyScars_SalveUsed";

        private readonly IMonitor _monitor;
        private readonly ModConfig _config;
        private readonly DialogueManager _dialogueManager;

        private bool _appliedToday;

        public ScarCareManager(IMonitor monitor, ModConfig config, DialogueManager dialogueManager)
        {
            _monitor = monitor;
            _config = config;
            _dialogueManager = dialogueManager;
        }

        private static bool IsCold => Game1.IsWinter || Game1.isSnowing;

        private static bool HasBuff(string id) => Game1.player.buffs.IsApplied(id);

        public void OnDayStarted()
        {
            _appliedToday = false;
        }

        /// <summary>Проверка при каждой смене локации и каждые 10 минут: вышла на холод без мази — рубцы тянет.</summary>
        public void Check()
        {
            if (_appliedToday || !_config.EnableScarTightness || !Context.IsWorldReady || Game1.eventUp)
                return;

            var player = Game1.player;
            if (!player.mailReceived.Contains(EnabledFlag) || !IsCold)
                return;

            if (Game1.currentLocation?.IsOutdoors != true || HasBuff(SalveBuffId))
                return;

            _appliedToday = true;
            player.applyBuff(new Buff(TightnessBuffId));
            _dialogueManager.AddTopic(TightnessTopic, 1);
            Game1.addHUDMessage(new HUDMessage(
                "Холод стягивает старые рубцы на руках. Пальцы слушаются хуже.",
                HUDMessage.error_type));
            _monitor.Log("[Scars] Рубцы стягивает (зима, улица).", LogLevel.Debug);
        }

        /// <summary>Мазь в руках + кнопка действия. true — мазь потрачена.</summary>
        public bool UseSalve()
        {
            var player = Game1.player;
            bool tight = HasBuff(TightnessBuffId);

            if (!tight && HasBuff(SalveBuffId))
                return Refuse("Мазь уже впиталась. Харви сказал: один раз в день, не больше.");

            if (!tight && !IsCold)
                return Refuse("Сейчас рубцы не тянет. Мазь пригодится зимой.");

            player.buffs.Remove(TightnessBuffId);
            player.applyBuff(new Buff(SalveBuffId));
            _appliedToday = true;
            _dialogueManager.AddTopic(SalveUsedTopic, 2);

            Game1.playSound("slosh");
            Game1.addHUDMessage(new HUDMessage(
                tight
                    ? "Мазь тёплая и пахнет травами. Кожа отпускает — пальцы снова слушаются."
                    : "Ты втираешь мазь заранее, как учил Харви. Сегодня холод рубцы не стянет.",
                HUDMessage.health_type));
            return true;
        }

        private static bool Refuse(string message)
        {
            Game1.playSound("cancel");
            Game1.addHUDMessage(new HUDMessage(message, HUDMessage.error_type));
            return false;
        }
    }
}

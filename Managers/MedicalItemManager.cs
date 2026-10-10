using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Helpers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Медицинские предметы самопомощи (CP: assets/Code/medicalItems.json).
    /// Бинт, антисептик, обезболивающее и аптечка используются кнопкой действия с предметом в руках;
    /// травяной сбор — обычным питьём.
    /// </summary>
    public sealed class MedicalItemManager
    {
        private const int TopicDays = 3;
        private const int PainkillerOveruseWindowDays = 3;
        private const int PainkillerOveruseCount = 2;
        private const int FirstAidKitHeal = 40;
        private const string MinorInjuryId = "buffHurt";

        private readonly IMonitor _monitor;
        private readonly IInputHelper _input;
        private readonly StateManager _stateManager;
        private readonly BuffManager _buffManager;
        private readonly DialogueManager _dialogueManager;
        private readonly ComplianceManager _complianceManager;
        private readonly ComplicationManager _complicationManager;
        private readonly SelfCareManager _selfCareManager;
        private readonly TreatmentManager _treatmentManager;
        private readonly MishapManager _mishapManager;
        private readonly MedicalCardManager _medicalCardManager;

        private StardewValley.Object? _itemBeingEaten;

        public MedicalItemManager(
            IMonitor monitor,
            IInputHelper input,
            StateManager stateManager,
            BuffManager buffManager,
            DialogueManager dialogueManager,
            ComplianceManager complianceManager,
            ComplicationManager complicationManager,
            SelfCareManager selfCareManager,
            TreatmentManager treatmentManager,
            MishapManager mishapManager,
            MedicalCardManager medicalCardManager)
        {
            _monitor = monitor;
            _input = input;
            _stateManager = stateManager;
            _buffManager = buffManager;
            _dialogueManager = dialogueManager;
            _complianceManager = complianceManager;
            _complicationManager = complicationManager;
            _selfCareManager = selfCareManager;
            _treatmentManager = treatmentManager;
            _mishapManager = mishapManager;
            _medicalCardManager = medicalCardManager;
        }

        public void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree || !e.Button.IsActionButton())
                return;

            string? itemId = Game1.player.ActiveObject?.QualifiedItemId;
            if (itemId is not (MedicalItems.CleanBandage or MedicalItems.Antiseptic
                or MedicalItems.Painkiller or MedicalItems.FirstAidKit))
            {
                return;
            }

            // Клик по персонажу — обычный разговор/подарок, а не использование предмета.
            var location = Game1.currentLocation;
            if (location?.isCharacterAtTile(e.Cursor.GrabTile) != null || location?.isCharacterAtTile(e.Cursor.Tile) != null)
                return;

            _input.Suppress(e.Button);

            bool used = itemId switch
            {
                MedicalItems.CleanBandage => UseCleanBandage(),
                MedicalItems.Antiseptic => UseAntiseptic(),
                MedicalItems.Painkiller => UsePainkiller(),
                MedicalItems.FirstAidKit => UseFirstAidKit(),
                _ => false,
            };

            if (used)
                Game1.player.reduceActiveItemByOne();
        }

        public void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            var player = Game1.player;
            if (player.isEating)
            {
                if (player.itemToEat is StardewValley.Object eating)
                    _itemBeingEaten = eating;
                return;
            }

            if (_itemBeingEaten == null)
                return;

            var consumed = _itemBeingEaten;
            _itemBeingEaten = null;
            _mishapManager.OnConsumed(consumed);
            if (consumed.QualifiedItemId == MedicalItems.HerbalTea && _selfCareManager.ApplyWarmTea(requireHome: false))
                _monitor.Log("[MedicalItem] Травяной сбор: простуда облегчена", LogLevel.Info);
        }

        /// <summary>Предмет не нужен для основной цели — может, им можно снять мелкую неприятность.</summary>
        private bool TryCureMishap(string message, params string[] mishapIds)
        {
            if (_mishapManager.TryCure(mishapIds) == null)
                return false;

            Game1.addHUDMessage(new HUDMessage(message, HUDMessage.health_type));
            return true;
        }

        private bool UseCleanBandage()
        {
            string? blockReason = _selfCareManager.GetCleanBandageBlockReason();
            if (blockReason != null)
            {
                if (TryCureMishap("Ты вытащила занозу и заклеила палец.", MishapBuffs.Splinter))
                {
                    Game1.playSound("leafrustle");
                    return true;
                }

                return Refuse(blockReason);
            }

            if (!_selfCareManager.ApplyCleanBandage(requireHome: false))
                return Refuse("Сейчас перевязывать нечего.");

            Game1.playSound("leafrustle");
            return true;
        }

        private bool UseAntiseptic()
        {
            var state = _stateManager.State;
            int today = GameUtils.Today();
            if (state.LastAntisepticDay == today)
                return Refuse("Рану сегодня уже обработали.");

            bool removedDirtyWound = _complicationManager.RemoveComplicationBySelfCare(InjuryBuffs.DirtyWound, "antiseptic");
            if (!removedDirtyWound && !_complicationManager.CanReceiveMineDirtyWound())
            {
                if (TryCureMishap("Ты обработала ранку антисептиком. Щиплет, но заживёт быстро.", MishapBuffs.Splinter, MishapBuffs.BeeSting))
                {
                    Game1.playSound("waterSlosh");
                    return true;
                }

                return Refuse("Сейчас обрабатывать нечего.");
            }

            state.LastAntisepticDay = today;
            state.SelfCareProtections[SelfCareProtectionTypes.Antiseptic] = today;
            _dialogueManager.AddTopic(ConversationTopics.UsedAntiseptic, TopicDays);
            if (removedDirtyWound)
                _complianceManager.AddCompliance(+1, "selfcare_antiseptic");
            _stateManager.Save();

            Game1.playSound("waterSlosh");
            Game1.addHUDMessage(new HUDMessage(
                removedDirtyWound
                    ? "Ты промыла рану антисептиком. Грязи больше нет — щиплет, но так правильно."
                    : "Ты обработала рану антисептиком. Сегодня в шахте риск занести грязь ниже.",
                HUDMessage.health_type));
            return true;
        }

        private bool UsePainkiller()
        {
            if (!_complicationManager.HasComplication(InjuryBuffs.PainFlare) && !_buffManager.HasBuff(InjuryBuffs.PainFlare))
            {
                if (TryCureMishap("Таблетка сняла отёк от укуса.", MishapBuffs.BeeSting))
                {
                    Game1.playSound("smallSelect");
                    return true;
                }

                return Refuse("Сильной боли сейчас нет. Без нужды таблетки лучше не пить.");
            }

            _complicationManager.RemoveComplicationBySelfCare(InjuryBuffs.PainFlare, "painkiller");

            var state = _stateManager.State;
            int today = GameUtils.Today();
            state.PainkillerUseDays.RemoveAll(day => day <= today - PainkillerOveruseWindowDays);
            state.PainkillerUseDays.Add(today);

            bool overuse = state.PainkillerUseDays.Count >= PainkillerOveruseCount;
            if (overuse)
            {
                _dialogueManager.AddTopic(ConversationTopics.PainkillerOveruse, TopicDays);
                _complianceManager.AddCompliance(-1, "painkiller_overuse");
            }

            _stateManager.Save();

            Game1.playSound("smallSelect");
            Game1.addHUDMessage(new HUDMessage(
                overuse
                    ? "Боль утихла. Но это уже не первая таблетка за несколько дней — Харви это заметит."
                    : "Боль утихла. Обезболивающее не лечит причину, но даёт передышку.",
                overuse ? HUDMessage.error_type : HUDMessage.health_type));
            _monitor.Log($"[MedicalItem] Обезболивающее, приёмов за {PainkillerOveruseWindowDays} дн.: {state.PainkillerUseDays.Count}", LogLevel.Info);
            return true;
        }

        private bool UseFirstAidKit()
        {
            var ds = _stateManager.GetDebuffState(MinorInjuryId);
            bool untreatedMinorInjury = _buffManager.HasBuff(MinorInjuryId) && ds?.TreatmentStarted != true;

            if (untreatedMinorInjury && _treatmentManager.ApplySelfTreatmentForMinorInjury(MinorInjuryId))
            {
                _medicalCardManager.Unlock(MedicalAchievements.SelfReliant);
                _dialogueManager.AddTopic(ConversationTopics.UsedFirstAidKit, TopicDays);
                _stateManager.Save();
                Game1.playSound("powerup");
                Game1.addHUDMessage(new HUDMessage(
                    "Ты сама обработала рану по правилам Харви. Лёгкая травма под контролем.",
                    HUDMessage.health_type));
                return true;
            }

            if (TryCureMishap("Аптечка пригодилась: с мелкой бедой справилась сама.",
                    MishapBuffs.Splinter, MishapBuffs.BeeSting, MishapBuffs.Sunstroke, MishapBuffs.Frostbite))
            {
                Game1.playSound("powerup");
                return true;
            }

            var player = Game1.player;
            if (player.health >= player.maxHealth)
                return Refuse("Аптечка сейчас не нужна.");

            player.health = System.Math.Min(player.maxHealth, player.health + FirstAidKitHeal);
            Game1.playSound("powerup");
            Game1.addHUDMessage(new HUDMessage("Ты обработала ссадины. Стало легче.", HUDMessage.health_type));
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

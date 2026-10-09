using HarveyOverhaul.Core.Api;
using HarveyOverhaul.Core.Models;
using HarveyOverhaul.Core.Services;
using HarveyOverhaul.InjuryCare.Core;
using HarveyOverhaul.InjuryCare.Core.Models;
using HarveyOverhaul.InjuryCare.Helpers;
using HarveyOverhaul.InjuryCare.Managers;
using StardewModdingAPI;
using StardewValley;

namespace HarveyOverhaul.InjuryCare.Services;

/// <summary>Факты травм и recovery plan → HarveyCareDirective (без buff/topic/quest id в UI).</summary>
public sealed class InjuryCareDirectiveProvider : IHarveyCareDirectiveProvider
{
    public string ProviderId => HarveyProviderRegistry.InjuryProviderId;

    private readonly ModConfig _config;
    private readonly StateManager _stateManager;
    private readonly InjuryManager _injuryManager;
    private readonly RecoveryPlanManager _recoveryPlanManager;

    public InjuryCareDirectiveProvider(
        ModConfig config,
        StateManager stateManager,
        InjuryManager injuryManager,
        RecoveryPlanManager recoveryPlanManager)
    {
        _config = config;
        _stateManager = stateManager;
        _injuryManager = injuryManager;
        _recoveryPlanManager = recoveryPlanManager;
    }

    public IReadOnlyList<HarveyCareDirective> GetCareDirectives()
    {
        _recoveryPlanManager.EnsurePlanFreshForDisplay();

        var directives = new List<HarveyCareDirective>();
        var state = _stateManager.State;
        var recoveryVm = _recoveryPlanManager.BuildViewModel();
        int today = Context.IsWorldReady ? (int)Game1.stats.DaysPlayed : -1;

        AppendComplicationDirectives(state, directives);
        AppendUnnoticedInjuryHint(directives);
        AppendTreatmentDirectives(state, recoveryVm, directives);
        AppendRecoveryPlanTasks(recoveryVm, directives);
        AppendPrescriptionDirectives(state, today, directives);
        AppendMineForbidden(state, today, directives);
        AppendFailureReasons(recoveryVm, directives);
        AppendWarnings(recoveryVm, directives);

        return directives;
    }

    private void AppendComplicationDirectives(InjuryState state, List<HarveyCareDirective> directives)
    {
        foreach (string id in InjurySets.ComplicationPriorityOrder)
        {
            if (!state.ActiveComplications.ContainsKey(id))
                continue;

            AppendComplication(id, directives);
        }

        foreach (string id in state.ActiveComplications.Keys)
        {
            if (InjurySets.ComplicationPriorityOrder.Contains(id))
                continue;

            AppendComplication(id, directives);
        }
    }

    /// <summary>
    /// Травма, о которой Харви не знает: без подсказки игрок не догадывается, что лечение начинается с разговора.
    /// Название травмы не раскрываем (Харви лишь «что-то заметил»). Если сегодня игрок решил скрыть — не напоминаем.
    /// </summary>
    private void AppendUnnoticedInjuryHint(List<HarveyCareDirective> directives)
    {
        if (!Context.IsWorldReady)
            return;

        bool hasUnnoticed = _stateManager.GetAllActiveDebuffStates().Any(d =>
            d.HiddenFromHarvey
            && !d.HarveyAware
            && !d.TreatmentStarted
            && !d.PlayerDeniedInjuryToday
            && InjurySets.HarveyTreatable.Contains(d.BuffId)
            && (_injuryManager.HasInjuryOrPhase(d.BuffId) || Game1.player.hasBuff(d.BuffId)));

        if (!hasUnnoticed)
            return;

        directives.Add(new HarveyCareDirective
        {
            Id = "injury.unnoticed.talk",
            Source = HarveyCareDirectiveSource.Injury,
            Type = HarveyCareDirectiveType.Appointment,
            Title = "Поговори с Харви",
            Text = "Харви поглядывает на тебя с беспокойством — он спросит, всё ли в порядке.",
            Reason = "У тебя есть травма, о которой Харви ещё не знает.",
            NextStep = "в разговоре можно признаться — тогда сразу начнётся лечение — или промолчать.",
            Priority = HarveyCareDirectivePriority.Normal,
            HarveyTone = HarveyCareDirectiveTone.Calm,
        });
    }

    private static void AppendComplication(string buffId, List<HarveyCareDirective> directives)
    {
        var copy = GetComplicationDirective(buffId);
        if (copy != null)
            directives.Add(copy);
    }

    private void AppendTreatmentDirectives(
        InjuryState state,
        RecoveryPlanViewModel recoveryVm,
        List<HarveyCareDirective> directives)
    {
        foreach (var (injuryId, debuff) in _injuryManager.GetInjuriesForHarveyPanel())
        {
            // Id и текст — по конкретной травме: при двух травмах игрок видит, с чем именно идти к Харви.
            string injuryName = _injuryManager.GetInjuryName(injuryId);

            if (debuff.ReadyForRecovery)
            {
                directives.Add(new HarveyCareDirective
                {
                    Id = $"injury.ready_recovery.{injuryId}",
                    Source = HarveyCareDirectiveSource.Injury,
                    Type = HarveyCareDirectiveType.Appointment,
                    Title = $"Финальный осмотр: {injuryName}",
                    Text = "Похоже, лечение можно завершить, но Харви должен убедиться, что всё в порядке. Зайди к нему в клинику.",
                    Reason = "Лечение подходит к концу.",
                    NextStep = "если Харви всё устроит, травма будет закрыта и ограничения снимутся.",
                    Priority = HarveyCareDirectivePriority.High,
                    HarveyTone = HarveyCareDirectiveTone.Calm,
                });
            }
            else if (debuff.ReadyForNextPhase)
            {
                directives.Add(new HarveyCareDirective
                {
                    Id = $"injury.ready_next_phase.{injuryId}",
                    Source = HarveyCareDirectiveSource.Injury,
                    Type = HarveyCareDirectiveType.Appointment,
                    Title = $"Контрольный осмотр: {injuryName}",
                    Text = "Текущая фаза лечения завершена. Харви должен подтвердить переход к следующей — поговори с ним.",
                    Reason = "Текущая фаза лечения пройдена.",
                    NextStep = "Харви назначит следующую фазу и обновит план.",
                    Priority = HarveyCareDirectivePriority.High,
                    HarveyTone = HarveyCareDirectiveTone.Calm,
                });
            }
            else if (!debuff.TreatmentStarted && HarveyInjuryAwarenessHelper.IsInjuryHarveyAware(debuff))
            {
                directives.Add(new HarveyCareDirective
                {
                    Id = $"injury.start_treatment.{injuryId}",
                    Source = HarveyCareDirectiveSource.Injury,
                    Type = HarveyCareDirectiveType.Appointment,
                    Title = $"Начать лечение: {injuryName}",
                    Text = "Харви знает о травме. Поговори с ним, чтобы он назначил лечение.",
                    Reason = $"Травма «{injuryName}» без лечения может осложниться.",
                    NextStep = "после разговора в плане появится режим на каждый день.",
                    Priority = HarveyCareDirectivePriority.High,
                    HarveyTone = HarveyCareDirectiveTone.Worried,
                });
            }
            else if (!debuff.TreatmentStarted && !debuff.IsInTreatment)
            {
                // Травма требует Харви, но он о ней ещё не знает — без подсказки игрок видел «всё спокойно».
                // Скрытые от Харви травмы сюда не попадают (их отфильтровывает GetInjuriesForHarveyPanel).
                if (InjurySets.HarveyTreatable.Contains(injuryId))
                {
                    directives.Add(new HarveyCareDirective
                    {
                        Id = $"injury.unreported.{injuryId}",
                        Source = HarveyCareDirectiveSource.Injury,
                        Type = HarveyCareDirectiveType.Appointment,
                        Title = $"Покажи травму Харви: {injuryName}",
                        Text = "Харви пока не знает о травме. Без осмотра лечение не начнётся.",
                        Reason = $"Травма «{injuryName}» без осмотра может осложниться.",
                        NextStep = "Харви осмотрит рану и назначит лечение.",
                        Priority = HarveyCareDirectivePriority.Normal,
                        HarveyTone = HarveyCareDirectiveTone.Calm,
                    });
                }
            }
            else
            {
                string phaseText = debuff.TotalPhases > 0 && debuff.CurrentPhase > 0
                    ? TreatmentManager.GetPhaseDisplayName(injuryId, debuff.CurrentPhase, debuff.TotalPhases)
                    : "лечение";

                directives.Add(new HarveyCareDirective
                {
                    Id = $"injury.treatment.{injuryId}",
                    Source = HarveyCareDirectiveSource.Injury,
                    Type = HarveyCareDirectiveType.TodayRule,
                    Title = injuryName,
                    Text = $"Сейчас {phaseText.ToLowerInvariant()}. Следуй режиму и не форсируй нагрузку.",
                    Reason = "Травма заживает, пока соблюдается режим.",
                    NextStep = "когда фаза закончится, Харви позовёт на осмотр — пункт появится в плане.",
                    Priority = HarveyCareDirectivePriority.Normal,
                    HarveyTone = HarveyCareDirectiveTone.Calm,
                });
            }

            if (IsInfectionTreatment(injuryId, debuff))
            {
                directives.Add(new HarveyCareDirective
                {
                    Id = "injury.infection_regime",
                    Source = HarveyCareDirectiveSource.Injury,
                    Type = HarveyCareDirectiveType.TodayRule,
                    Title = "Продолжай лечение инфекции",
                    Text = "Рана ещё воспалена. Нужен щадящий режим и контроль у Харви.",
                    Reason = "В рану попала инфекция.",
                    Priority = HarveyCareDirectivePriority.High,
                    HarveyTone = HarveyCareDirectiveTone.Worried,
                });
            }

            if (IsSevereInjury(injuryId))
            {
                directives.Add(new HarveyCareDirective
                {
                    Id = "injury.severe_regime",
                    Source = HarveyCareDirectiveSource.Injury,
                    Type = HarveyCareDirectiveType.TodayRule,
                    Title = "Щадящий режим",
                    Text = "Избегай тяжёлой работы, шахт, поздней ночи и падения здоровья.",
                    Reason = $"«{injuryName}» — тяжёлая травма: любая нагрузка замедляет заживление.",
                    Priority = HarveyCareDirectivePriority.High,
                    HarveyTone = HarveyCareDirectiveTone.Worried,
                });
            }
        }
    }

    private static void AppendRecoveryPlanTasks(RecoveryPlanViewModel vm, List<HarveyCareDirective> directives)
    {
        // Конкретные осложнения уже добавлены отдельными визитами — общая задача «покажи осложнение» их дублирует.
        bool hasComplicationAppointments = directives.Any(IsComplicationAppointment);
        // То же для «зайди к Харви»: визит по конкретной травме (осмотр/финал) уже есть.
        bool hasPhaseAppointments = directives.Any(d =>
            d.Id.StartsWith("injury.ready_next_phase.", StringComparison.Ordinal)
            || d.Id.StartsWith("injury.ready_recovery.", StringComparison.Ordinal));

        foreach (RecoveryPlanTask task in vm.Tasks)
        {
            if (!task.IsFailed
                && ((hasComplicationAppointments && task.Id == RecoveryPlanTaskIds.TreatComplications)
                    || (hasPhaseAppointments && task.Id == RecoveryPlanTaskIds.VisitHarveyIfReady)))
            {
                continue;
            }

            var directive = MapRecoveryTask(task, vm);
            if (directive != null)
                directives.Add(directive);
        }

        if (vm.RequiresHarveyTalk && !directives.Any(d => d.Type == HarveyCareDirectiveType.Appointment))
        {
            directives.Add(new HarveyCareDirective
            {
                Id = "injury.harvey_checkup",
                Source = HarveyCareDirectiveSource.Injury,
                Type = HarveyCareDirectiveType.Appointment,
                Title = "Контрольный осмотр у Харви",
                Text = "Харви ждёт контрольный осмотр, прежде чем продолжить план.",
                Reason = "Без осмотра Харви не может продолжить план.",
                Priority = HarveyCareDirectivePriority.High,
                HarveyTone = HarveyCareDirectiveTone.Worried,
            });
        }
    }

    private static HarveyCareDirective? MapRecoveryTask(RecoveryPlanTask task, RecoveryPlanViewModel vm)
    {
        string state = task.IsFailed
            ? HarveyCareDirectiveState.Failed
            : task.IsCompleted
                ? HarveyCareDirectiveState.Done
                : HarveyCareDirectiveState.Active;

        return task.Id switch
        {
            RecoveryPlanTaskIds.SleepBeforeMidnight => new HarveyCareDirective
            {
                Id = "injury.rule.sleep",
                Source = HarveyCareDirectiveSource.Injury,
                Type = HarveyCareDirectiveType.TodayRule,
                Title = "Лечь спать вовремя",
                Text = "Харви просил лечь до полуночи, чтобы день восстановления засчитался.",
                Reason = "Во сне организм восстанавливается быстрее всего.",
                Priority = HarveyCareDirectivePriority.Normal,
                State = state,
                CanFailDay = true,
                FailureText = "если ляжешь после полуночи",
                HarveyTone = HarveyCareDirectiveTone.Calm,
            },
            RecoveryPlanTaskIds.AvoidMines => new HarveyCareDirective
            {
                Id = "injury.avoid.mines",
                Source = HarveyCareDirectiveSource.Injury,
                Type = HarveyCareDirectiveType.Avoid,
                Title = "Не ходи в шахту",
                Text = "Сейчас любая драка или грязь могут сорвать восстановление.",
                Reason = "В шахте легко снова пораниться или занести грязь в рану.",
                Priority = HarveyCareDirectivePriority.Critical,
                State = state,
                CanFailDay = true,
                FailureText = "если зайдёшь в шахту или вулкан",
                HarveyTone = HarveyCareDirectiveTone.Strict,
            },
            RecoveryPlanTaskIds.KeepStaminaAbove15 => new HarveyCareDirective
            {
                Id = "injury.rule.stamina",
                Source = HarveyCareDirectiveSource.Injury,
                Type = HarveyCareDirectiveType.TodayRule,
                Title = "Не опускать стамину ниже 15%",
                Text = "Харви просит не доводить выносливость до предела.",
                Reason = "Когда силы на нуле, рана заживает медленнее.",
                Priority = HarveyCareDirectivePriority.Normal,
                State = state,
                CanFailDay = true,
                FailureText = "если выносливость упадёт слишком низко",
            },
            RecoveryPlanTaskIds.ReturnIfLowHealth => new HarveyCareDirective
            {
                Id = "injury.rule.health",
                Source = HarveyCareDirectiveSource.Injury,
                Type = HarveyCareDirectiveType.TodayRule,
                Title = "Вернуться при низком здоровье",
                Text = "Если здоровье падает — домой или в клинику.",
                Reason = "Новые удары по ослабленному телу грозят осложнением.",
                Priority = HarveyCareDirectivePriority.High,
                State = state,
                CanFailDay = true,
                FailureText = "если здоровье упадёт слишком низко",
            },
            RecoveryPlanTaskIds.KeepBandageDry => new HarveyCareDirective
            {
                Id = "injury.avoid.wet_bandage",
                Source = HarveyCareDirectiveSource.Injury,
                Type = HarveyCareDirectiveType.Avoid,
                Title = "Не выходи под дождь с повязкой",
                Text = "Повязка должна оставаться сухой — дождь и вода повышают риск осложнения.",
                Reason = "Мокрая повязка — прямой путь к инфекции.",
                Priority = HarveyCareDirectivePriority.High,
                State = state,
                CanFailDay = true,
                FailureText = "если повязка промокнет",
            },
            RecoveryPlanTaskIds.VisitHarveyIfReady => new HarveyCareDirective
            {
                Id = "injury.appointment.visit",
                Source = HarveyCareDirectiveSource.Injury,
                Type = HarveyCareDirectiveType.Appointment,
                Title = "Поговори с Харви",
                Text = RecoveryPlanTexts.Tasks.VisitPhaseDescription,
                Priority = HarveyCareDirectivePriority.High,
                State = state,
            },
            RecoveryPlanTaskIds.TreatComplications => new HarveyCareDirective
            {
                Id = "injury.appointment.complication",
                Source = HarveyCareDirectiveSource.Injury,
                Type = HarveyCareDirectiveType.Appointment,
                Title = "Покажи осложнение Харви",
                Text = RecoveryPlanTexts.Tasks.ComplicationDescription,
                Priority = HarveyCareDirectivePriority.Critical,
                State = state,
            },
            _ => null,
        };
    }

    private void AppendPrescriptionDirectives(InjuryState state, int today, List<HarveyCareDirective> directives)
    {
        if (state.ActivePrescriptions == null)
            return;

        foreach (var (id, prescription) in state.ActivePrescriptions)
        {
            if (prescription.IsExpired(today))
                continue;

            var mapped = MapPrescription(id);
            if (mapped == null)
                continue;

            // Назначение уже отражено задачей плана (у неё есть отметка выполнения) — второй пункт только путает.
            if (PrescriptionCoveredByPlanTask.TryGetValue(mapped.Id, out string? taskDirectiveId)
                && directives.Any(d => d.Id == taskDirectiveId))
            {
                continue;
            }

            directives.Add(mapped);
        }
    }

    private static readonly Dictionary<string, string> PrescriptionCoveredByPlanTask = new(StringComparer.Ordinal)
    {
        ["injury.rx.no_mine"] = "injury.avoid.mines",
        ["injury.rx.keep_dry"] = "injury.avoid.wet_bandage",
        ["injury.rx.rest"] = "injury.rule.sleep",
    };

    private void AppendMineForbidden(InjuryState state, int today, List<HarveyCareDirective> directives)
    {
        if (directives.Any(d => d.Id is "injury.avoid.mines" or "injury.rx.no_mine"))
            return;

        if (!MineForbiddenHelper.IsMineForbiddenActive(state, _config, today))
        {
            // Мягкое ограничение (бафф синхронизирует MineForbiddenHelper) — тоже показываем в плане.
            if (Context.IsWorldReady && Game1.player.hasBuff(InjuryBuffs.MineRestricted))
            {
                directives.Add(new HarveyCareDirective
                {
                    Id = "injury.avoid.mines_restricted",
                    Source = HarveyCareDirectiveSource.Injury,
                    Type = HarveyCareDirectiveType.Avoid,
                    Title = "Шахта — только с осторожностью",
                    Text = "Рана ещё не зажила. Один спуск в день Харви стерпит, повторный грозит обострением или полным запретом.",
                    Reason = "Рана ещё не зажила.",
                    Priority = HarveyCareDirectivePriority.High,
                    CanFailDay = true,
                    FailureText = "если спустишься в шахту второй раз за день",
                    HarveyTone = HarveyCareDirectiveTone.Worried,
                });
            }

            return;
        }

        directives.Add(new HarveyCareDirective
        {
            Id = "injury.avoid.mines_forbidden",
            Source = HarveyCareDirectiveSource.Injury,
            Type = HarveyCareDirectiveType.Avoid,
            Title = "Не ходи в шахту",
            Text = "Харви запретил шахту и вулкан до разрешения.",
            Reason = "Рана слишком свежая для драк и подземной грязи.",
            NextStep = "запрет снимет Харви на одном из осмотров.",
            Priority = HarveyCareDirectivePriority.Critical,
            CanFailDay = true,
            FailureText = "если зайдёшь в шахту",
            HarveyTone = HarveyCareDirectiveTone.Strict,
        });
    }

    private static void AppendFailureReasons(RecoveryPlanViewModel vm, List<HarveyCareDirective> directives)
    {
        if (vm.TodayFailed || vm.TodayViolationReasons.Count > 0)
        {
            foreach (string reason in RecoveryPlanViolationReasonTexts.FormatReasons(vm.TodayViolationReasons))
            {
                directives.Add(new HarveyCareDirective
                {
                    Id = $"injury.fail.{reason.GetHashCode(StringComparison.Ordinal):X}",
                    Source = HarveyCareDirectiveSource.Injury,
                    Type = HarveyCareDirectiveType.FailureReason,
                    Title = reason,
                    Text = reason,
                    Priority = HarveyCareDirectivePriority.High,
                    State = HarveyCareDirectiveState.Failed,
                });
            }
        }

        foreach (var assignment in vm.Assignments.Where(a =>
                     !a.IsCompleted
                     && a.Goal > 0
                     && !IsStressOwnedAssignment(a.Id)))
        {
            if (assignment.Progress < assignment.Goal)
            {
                directives.Add(new HarveyCareDirective
                {
                    Id = $"injury.fail.pending.{assignment.Id}",
                    Source = HarveyCareDirectiveSource.Injury,
                    Type = HarveyCareDirectiveType.FailureReason,
                    Title = assignment.Title,
                    Text = assignment.Description,
                    FailureText = $"«{assignment.Title}» ещё не выполнено",
                    Priority = HarveyCareDirectivePriority.Normal,
                    CanFailDay = true,
                    Current = assignment.Progress,
                    Goal = assignment.Goal,
                });
            }
        }
    }

    private static void AppendWarnings(RecoveryPlanViewModel vm, List<HarveyCareDirective> directives)
    {
        foreach (string warning in vm.TodayWarnings)
        {
            directives.Add(new HarveyCareDirective
            {
                Id = $"injury.warn.{warning.GetHashCode(StringComparison.Ordinal):X}",
                Source = HarveyCareDirectiveSource.Injury,
                Type = HarveyCareDirectiveType.Warning,
                Title = "Предупреждение",
                Text = warning,
                Priority = HarveyCareDirectivePriority.Normal,
                State = HarveyCareDirectiveState.Warning,
                HarveyTone = HarveyCareDirectiveTone.Worried,
            });
        }

        // Сводка нужна, только если осложнения не показаны отдельными визитами к Харви.
        if (!string.IsNullOrWhiteSpace(vm.ComplicationSummary) && !directives.Any(IsComplicationAppointment))
        {
            directives.Add(new HarveyCareDirective
            {
                Id = "injury.warn.complications",
                Source = HarveyCareDirectiveSource.Injury,
                Type = HarveyCareDirectiveType.Warning,
                Title = "Осложнения",
                Text = vm.ComplicationSummary,
                Priority = HarveyCareDirectivePriority.High,
                HarveyTone = HarveyCareDirectiveTone.Worried,
            });
        }
    }

    private static HarveyCareDirective? GetComplicationDirective(string buffId) => buffId switch
    {
        var id when id.Equals(InjuryBuffs.WetBandage, StringComparison.OrdinalIgnoreCase) => new HarveyCareDirective
        {
            Id = "injury.appointment.wet_bandage",
            Source = HarveyCareDirectiveSource.Injury,
            Type = HarveyCareDirectiveType.Appointment,
            Title = "Смени повязку у Харви",
            Text = "Повязка промокла. Её нужно сменить, иначе есть риск осложнения.",
            Reason = "Повязка промокла.",
            Priority = HarveyCareDirectivePriority.High,
            HarveyTone = HarveyCareDirectiveTone.Worried,
        },
        var id when id.Equals(InjuryBuffs.DirtyWound, StringComparison.OrdinalIgnoreCase) => new HarveyCareDirective
        {
            Id = "injury.appointment.dirty_wound",
            Source = HarveyCareDirectiveSource.Injury,
            Type = HarveyCareDirectiveType.Appointment,
            Title = "Не откладывай обработку раны",
            Text = "Рана загрязнилась. Чем дольше ждать, тем выше риск инфекции.",
            Reason = "Рана загрязнилась.",
            Priority = HarveyCareDirectivePriority.Critical,
            HarveyTone = HarveyCareDirectiveTone.Strict,
        },
        _ => new HarveyCareDirective
        {
            Id = $"injury.complication.{buffId}",
            Source = HarveyCareDirectiveSource.Injury,
            Type = HarveyCareDirectiveType.Appointment,
            Title = "Покажи осложнение Харви",
            Text = "Есть осложнение — нужен осмотр.",
            Reason = "Появилось осложнение.",
            Priority = HarveyCareDirectivePriority.High,
            HarveyTone = HarveyCareDirectiveTone.Worried,
        },
    };

    private static HarveyCareDirective? MapPrescription(string prescriptionId) => prescriptionId switch
    {
        var id when id.Equals(PrescriptionIds.NoMine, StringComparison.Ordinal) => new HarveyCareDirective
        {
            Id = "injury.rx.no_mine",
            Source = HarveyCareDirectiveSource.Injury,
            Type = HarveyCareDirectiveType.Avoid,
            Title = "Не ходи в шахту",
            Text = RecoveryPlanTexts.Tasks.MinesDescription,
            Priority = HarveyCareDirectivePriority.Critical,
            CanFailDay = true,
        },
        var id when id.Equals(PrescriptionIds.KeepDry, StringComparison.Ordinal) => new HarveyCareDirective
        {
            Id = "injury.rx.keep_dry",
            Source = HarveyCareDirectiveSource.Injury,
            Type = HarveyCareDirectiveType.Avoid,
            Title = "Держи повязку сухой",
            Text = RecoveryPlanTexts.Tasks.BandageDescription,
            Priority = HarveyCareDirectivePriority.High,
        },
        var id when id.Equals(PrescriptionIds.Rest, StringComparison.Ordinal) => new HarveyCareDirective
        {
            Id = "injury.rx.rest",
            Source = HarveyCareDirectiveSource.Injury,
            Type = HarveyCareDirectiveType.TodayRule,
            Title = "Отдых по назначению",
            Text = RecoveryPlanTexts.Tasks.SleepDescription,
            Priority = HarveyCareDirectivePriority.Normal,
        },
        _ => null,
    };

    private static bool IsInfectionTreatment(string injuryId, DebuffState debuff) =>
        string.Equals(injuryId, "buffInfectedWound", StringComparison.OrdinalIgnoreCase)
        || debuff.CurrentPhase > 0 && injuryId.Contains("Infect", StringComparison.OrdinalIgnoreCase);

    private static bool IsSevereInjury(string injuryId) =>
        InjurySets.Severe.Contains(injuryId)
        || MineForbiddenHelper.SevereAcutePhase1Treatment.Contains(injuryId);

    private static bool IsComplicationAppointment(HarveyCareDirective d) =>
        d.Type == HarveyCareDirectiveType.Appointment
        && (d.Id is "injury.appointment.wet_bandage" or "injury.appointment.dirty_wound"
            || d.Id.StartsWith("injury.complication.", StringComparison.Ordinal));

    private static bool IsStressOwnedAssignment(string assignmentId) =>
        string.Equals(assignmentId, RecoveryPlanAssignmentIds.FindSafePlace, StringComparison.OrdinalIgnoreCase)
        || string.Equals(assignmentId, RecoveryPlanAssignmentIds.DontStayAlone, StringComparison.OrdinalIgnoreCase);
}

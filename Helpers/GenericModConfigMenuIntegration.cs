using System;
using HarveyOverhaul.InjuryCare.Core;
using StardewModdingAPI;

namespace HarveyOverhaul.InjuryCare.Helpers
{
    /// <summary>Подмножество API Generic Mod Config Menu (spacechase0.GenericModConfigMenu).</summary>
    public interface IGenericModConfigMenuApi
    {
        void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);

        void AddSectionTitle(IManifest mod, Func<string> text, Func<string>? tooltip = null);

        void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<string>? tooltip = null, string? fieldId = null);

        void AddNumberOption(IManifest mod, Func<float> getValue, Action<float> setValue, Func<string> name, Func<string>? tooltip = null, float? min = null, float? max = null, float? interval = null, Func<float, string>? formatValue = null, string? fieldId = null);

        void AddTextOption(IManifest mod, Func<string> getValue, Action<string> setValue, Func<string> name, Func<string>? tooltip = null, string[]? allowedValues = null, Func<string, string>? formatAllowedValue = null, string? fieldId = null);
    }

    /// <summary>
    /// Меню настроек в GMCM (необязательная зависимость). Меняет тот же экземпляр ModConfig,
    /// который держат менеджеры, поэтому настройки применяются сразу.
    /// </summary>
    internal static class GenericModConfigMenuIntegration
    {
        public static void Register(IModHelper helper, IManifest manifest, ModConfig config)
        {
            var gmcm = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (gmcm == null)
                return;

            gmcm.Register(
                manifest,
                reset: () => ResetToDefaults(config),
                save: () => helper.WriteConfig(config));

            gmcm.AddSectionTitle(manifest, () => "Сложность");
            gmcm.AddTextOption(
                manifest,
                () => config.Difficulty,
                value => config.Difficulty = value,
                () => "Пресет",
                () => "Уютно: травмы вдвое реже и заживают вдвое быстрее.\nРеалистично: как задумано.\nХардкор: травмы в 1.5 раза чаще, лечение на 25% дольше.\nСвоё: множители ниже.",
                DifficultyPresets.All,
                FormatDifficulty);
            gmcm.AddNumberOption(
                manifest,
                () => (float)config.CustomInjuryChanceMultiplier,
                value => config.CustomInjuryChanceMultiplier = value,
                () => "Своё: шанс травм ×",
                () => "Только для пресета «Своё». Бой, взрывы, работа на ферме, простуда.",
                min: 0f, max: 3f, interval: 0.05f);
            gmcm.AddNumberOption(
                manifest,
                () => (float)config.CustomPhaseDurationMultiplier,
                value => config.CustomPhaseDurationMultiplier = value,
                () => "Своё: длительность лечения ×",
                () => "Только для пресета «Своё». Применяется к новым травмам.",
                min: 0.25f, max: 3f, interval: 0.05f);

            gmcm.AddSectionTitle(manifest, () => "Лечение");
            gmcm.AddBoolOption(
                manifest,
                () => config.RegimenAffectsRecoveryPace,
                value => config.RegimenAffectsRecoveryPace = value,
                () => "Режим влияет на сроки",
                () => "Соблюдение предписаний сокращает фазу лечения, нарушения — удлиняют.");
            gmcm.AddBoolOption(
                manifest,
                () => config.ForceHospitalization,
                value => config.ForceHospitalization = value,
                () => "Госпитализация",
                () => "После тяжёлой травмы в шахте Харви может оставить в палате.");
            gmcm.AddBoolOption(
                manifest,
                () => config.EnableFarmingToolUseInjuries,
                value => config.EnableFarmingToolUseInjuries = value,
                () => "Травмы от работы на ферме",
                () => "Растяжения и порезы при долгой работе инструментами с низкой энергией.");
            gmcm.AddBoolOption(
                manifest,
                () => config.EnableMinorMishaps,
                value => config.EnableMinorMishaps = value,
                () => "Мелкие неприятности",
                () => "Заноза, укус пчелы, солнечный удар, обморожение. Проходят за ночь или лечатся предметами.");
            gmcm.AddNumberOption(
                manifest,
                () => config.MineForbiddenDurationDays,
                value => config.MineForbiddenDurationDays = (int)value,
                () => "Запрет шахты (дней)",
                min: 1f, max: 7f, interval: 1f);

            gmcm.AddSectionTitle(manifest, () => "Письма и забота");
            gmcm.AddTextOption(
                manifest,
                () => config.MedicalLetters.ToString(),
                value => config.MedicalLetters = Enum.Parse<MedicalLetterMode>(value),
                () => "Медицинские письма",
                allowedValues: Enum.GetNames<MedicalLetterMode>());
            gmcm.AddBoolOption(
                manifest,
                () => config.SendRomanticCareLetters,
                value => config.SendRomanticCareLetters = value,
                () => "Романтические письма");
            gmcm.AddBoolOption(
                manifest,
                () => config.EnableSpouseDomesticCare,
                value => config.EnableSpouseDomesticCare = value,
                () => "Домашняя забота супруга");
        }

        private static string FormatDifficulty(string value) => value switch
        {
            DifficultyPresets.Cozy => "Уютно",
            DifficultyPresets.Realistic => "Реалистично",
            DifficultyPresets.Hardcore => "Хардкор",
            DifficultyPresets.Custom => "Своё",
            _ => value,
        };

        private static void ResetToDefaults(ModConfig config)
        {
            var defaults = new ModConfig();
            config.Difficulty = defaults.Difficulty;
            config.CustomInjuryChanceMultiplier = defaults.CustomInjuryChanceMultiplier;
            config.CustomPhaseDurationMultiplier = defaults.CustomPhaseDurationMultiplier;
            config.RegimenAffectsRecoveryPace = defaults.RegimenAffectsRecoveryPace;
            config.ForceHospitalization = defaults.ForceHospitalization;
            config.EnableFarmingToolUseInjuries = defaults.EnableFarmingToolUseInjuries;
            config.EnableMinorMishaps = defaults.EnableMinorMishaps;
            config.MineForbiddenDurationDays = defaults.MineForbiddenDurationDays;
            config.MedicalLetters = defaults.MedicalLetters;
            config.SendRomanticCareLetters = defaults.SendRomanticCareLetters;
            config.EnableSpouseDomesticCare = defaults.EnableSpouseDomesticCare;
        }
    }
}

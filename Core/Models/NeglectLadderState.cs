namespace HarveyOverhaul.InjuryCare.Core.Models
{
    /// <summary>
    /// Лестница последствий для запущенной травмы, ступени 4–5 (ступени 1–3 — письма, осложнения и «Небрежность»
    /// в <c>ComplicationManager</c>). Один «случай» = одна main-травма с её днём начала.
    /// </summary>
    public class NeglectLadderState
    {
        /// <summary>Травма случая. Пусто — случая нет.</summary>
        public string CaseInjuryId { get; set; } = "";

        /// <summary>InjuryStartDay травмы случая: та же травма, полученная заново, — новый случай.</summary>
        public int CaseInjuryStartDay { get; set; } = -1;

        /// <summary>День, когда Харви написал «загляну утром». -1 — письма не было.</summary>
        public int LetterDay { get; set; } = -1;

        /// <summary>С этого дня Харви ждёт фермершу утром на ферме. -1 — визит не назначен.</summary>
        public int HouseCallDay { get; set; } = -1;

        /// <summary>Сколько раз фермерша попросила «не сейчас» (0, 1, 2).</summary>
        public int Deferrals { get; set; }

        /// <summary>День последнего визита (событие сыграно). Не больше одного визита в день.</summary>
        public int LastVisitDay { get; set; } = -1;

        /// <summary>До этого дня (не включая) ухудшение на паузе: Харви оставил бинт и дал пару дней.</summary>
        public int PausedUntilDay { get; set; } = -1;

        /// <summary>С этого дня после второго отказа возможен обморок. -1 — не взведён.</summary>
        public int CollapseFromDay { get; set; } = -1;

        /// <summary>Обморок в этом случае уже был — второй раз не случится.</summary>
        public bool CollapseDone { get; set; }

        public bool HasCase => !string.IsNullOrEmpty(CaseInjuryId);

        /// <summary>Новый случай: всё с нуля.</summary>
        public void StartCase(string injuryId, int injuryStartDay)
        {
            CaseInjuryId = injuryId;
            CaseInjuryStartDay = injuryStartDay;
            CollapseDone = false;
            ResetProgress();
        }

        /// <summary>
        /// Харви осмотрел (визит, клиника, госпитализация): лестница начинается заново, если фермерша снова
        /// забросит лечение. CollapseDone остаётся — обморок не больше одного раза на случай.
        /// </summary>
        public void ResetProgress()
        {
            LetterDay = -1;
            HouseCallDay = -1;
            Deferrals = 0;
            LastVisitDay = -1;
            PausedUntilDay = -1;
            CollapseFromDay = -1;
        }

        public void Clear()
        {
            CaseInjuryId = "";
            CaseInjuryStartDay = -1;
            CollapseDone = false;
            ResetProgress();
        }
    }
}

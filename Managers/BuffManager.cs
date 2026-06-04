using System;
using System.Collections.Generic;
using System.Linq;
using HarveyOverhaul.InjuryCare.Core;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Buffs;

namespace HarveyOverhaul.InjuryCare.Managers
{
    /// <summary>
    /// Управление баффами игрока
    /// </summary>
    public class BuffManager
    {
        private readonly IMonitor _monitor;
        private readonly IModHelper _helper;
        private Dictionary<string, BuffData> _allBuffs = new(StringComparer.OrdinalIgnoreCase);

        public BuffManager(IMonitor monitor, IModHelper helper)
        {
            _monitor = monitor;
            _helper = helper;
        }

        /// <summary>
        /// Загрузить данные баффов из Data/Buffs
        /// </summary>
        public void LoadBuffData()
        {
            try
            {
                // Подхватывает базу игры + пропатченные Content Patcher'ом buffsInjury.json / buffsCure.json
                var data = _helper.GameContent.Load<Dictionary<string, BuffData>>("Data/Buffs");
                _allBuffs = data ?? new Dictionary<string, BuffData>(StringComparer.OrdinalIgnoreCase);

                _monitor.Log($"[Buffs] Загружено {_allBuffs.Count} записей из Data/Buffs.", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"[Buffs] Ошибка загрузки Data/Buffs: {ex}", LogLevel.Error);
                _allBuffs = new Dictionary<string, BuffData>(StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Проверить существование баффа
        /// </summary>
        public bool BuffExists(string buffId)
        {
            return _allBuffs.ContainsKey(buffId);
        }

        /// <summary>
        /// Проверить наличие баффов
        /// </summary>
        public bool HasBuff(params string[] ids)
        {
            return ids.All(id => Game1.player.hasBuff(id));
        }

        /// <summary>
        /// Проверить наличие хотя бы одного из баффов
        /// </summary>
        public bool HasAnyBuff(params string[] ids)
        {
            return ids.Any(id => Game1.player.hasBuff(id));
        }

        /// <summary>
        /// Добавить бафф игроку
        /// </summary>
        /// <param name="id">ID баффа</param>
        /// <param name="minutes">Длительность в игровых минутах (-2 = целый день)</param>
        public void AddBuff(string id, int minutes = -2)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    _monitor.Log($"AddBuff: пустой ID", LogLevel.Error);
                    return;
                }

                int duration = minutes == -2 ? -2 : minutes * 60 * 1000;
                var buff = new Buff(id, duration: duration);
                Game1.player.applyBuff(buff);

                _monitor.Log($"AddBuff: {id}", LogLevel.Trace);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Ошибка AddBuff({id}): {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Удалить баффы
        /// </summary>
        public void RemoveBuff(params string[] ids)
        {
            foreach (string id in ids)
            {
                if (!Game1.player.hasBuff(id))
                {
                    _monitor.Log($"RemoveBuff({id}): не активен", LogLevel.Trace);
                    continue;
                }

                Game1.player.buffs.Remove(id);

                if (Game1.player.hasBuff(id))
                    _monitor.Log($"RemoveBuff({id}): не удалось удалить", LogLevel.Error);
                else
                    _monitor.Log($"RemoveBuff: {id}", LogLevel.Trace);
            }
        }

        /// <summary>
        /// Удалить все баффы из списка
        /// </summary>
        public void RemoveAllBuffs(System.Collections.Generic.IEnumerable<string> ids)
        {
            foreach (var id in ids)
            {
                RemoveBuff(id);
            }
        }

        /// <summary>
        /// Получить все активные баффы игрока
        /// </summary>
        public List<string> GetActiveBuffs()
        {
            return Game1.player?.buffs?.AppliedBuffs?.Keys.ToList() ?? new();
        }

        /// <summary>
        /// Получить активные баффы, принадлежащие моду (зарегистрированные в Data/Buffs через CP)
        /// </summary>
        public List<string> GetActiveModBuffs()
        {
            var applied = Game1.player?.buffs?.AppliedBuffs;
            if (applied == null) return new();

            var result = new List<string>();
            foreach (var id in applied.Keys)
            {
                if (string.Equals(id, ReminderBuffs.DoctorVisitNeeded, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (_allBuffs.ContainsKey(id))
                    result.Add(id);
            }
            return result;
        }
    }
}


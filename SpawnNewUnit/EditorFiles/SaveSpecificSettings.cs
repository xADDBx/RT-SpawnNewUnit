using HarmonyLib;
using Kingmaker;
using Kingmaker.EntitySystem.Persistence;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace SpawnNewUnit {

    public class SaveSpecificSettings {
        #region Infrastructure
        private static bool m_IsInitialized = false;
        private static string m_SaveStringKey = "SpawnNewUnit.SaveSpecificSettings";
        internal static void Initialize() {
            if (m_IsInitialized) {
                return;
            }
            _ = Main.HarmonyInstance.Patch(AccessTools.Method(typeof(SaveManager), nameof(SaveManager.SaveRoutine)), new(AccessTools.Method(typeof(SaveSpecificSettings), nameof(SaveManager_SaveRoutine_Patch))));
            _ = Main.HarmonyInstance.Patch(AccessTools.Method(typeof(SaveManager), nameof(SaveManager.LoadFolderSave)), new(AccessTools.Method(typeof(SaveSpecificSettings), nameof(SaveManager_LoadRoutine_Patch))));
            _ = Main.HarmonyInstance.Patch(AccessTools.Method(typeof(ThreadedGameLoader), "DeserializeInGameSettings"), null, new(AccessTools.Method(typeof(SaveSpecificSettings), nameof(ThreadedGameLoader_DeserializeInGameSettings_Patch))));
            m_IsInitialized = true;
        }
        private static void TryLoadSaveSpecificSettings(InGameSettings? maybeSettings) {
            var settingsList = maybeSettings?.List ?? Game.Instance?.State?.InGameSettings?.List;
            if (settingsList == null) {
                return;
            }
            SaveSpecificSettings? loaded = null;
            if (settingsList.TryGetValue(m_SaveStringKey, out var obj) && obj is string json) {
                try {
                    loaded = JsonConvert.DeserializeObject<SaveSpecificSettings>(json);
                } catch (Exception ex) {
                    Main.Logger.Log($"[Error] Deserialization of SaveSpecificSettings failed:\n{ex}");
                }
            }
            if (loaded == null) {
                Main.Logger.Log("[Warn] SaveSpecificSettings not found, creating new...");
                loaded = new();
                loaded.Save();
            }
            Instance = loaded;
        }
        private static SaveSpecificSettings? m_Instance = null;
        public static SaveSpecificSettings? Instance {
            get {
                if (m_Instance == null) {
                    TryLoadSaveSpecificSettings(null);
                }
                return m_Instance;
            }
            private set {
                m_Instance = value;
            }
        }
        public void Save() {
            var list = Game.Instance?.State?.InGameSettings?.List;
            if (list == null) {
                return;
            }
            var json = JsonConvert.SerializeObject(this);
            list[m_SaveStringKey] = json;
        }
        private static void SaveManager_SaveRoutine_Patch() {
            Instance?.Save();
        }
        private static void SaveManager_LoadRoutine_Patch() {
            Instance = null;
        }
        private static void ThreadedGameLoader_DeserializeInGameSettings_Patch(ref Task<InGameSettings> __result) {
            __result = __result.ContinueWith(t => {
                TryLoadSaveSpecificSettings(t.Result);
                return t.Result;
            });
        }
        #endregion Infrastructure
        public bool WasSpawned = false;
    }
}
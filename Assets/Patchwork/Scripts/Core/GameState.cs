using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Patchwork
{
    // Game progress (form, hat, golden buttons, finished levels, tutorial cards seen), saved as a
    // small JSON file with a backup copy. Settings stay in PlayerPrefs (see GameSettings).
    // Older saves that lived in PlayerPrefs are moved over the first time the game loads.
    public static class GameState
    {
        const int Version = 1;
        const string FileName = "save.json";

        [System.Serializable]
        class SaveData
        {
            public int version = Version;
            public int form = (int)FormType.Yarn;
            public int unlockedForms = 0b111; // all three forms are unlocked in the prototype
            public int hat = 1;
            public List<string> collected = new List<string>();
            public List<string> completedLevels = new List<string>();
            public List<string> hintsSeen = new List<string>();
        }

        static SaveData data;
        static bool dirty;

        static string SavePath { get { return Path.Combine(Application.persistentDataPath, FileName); } }
        static string BackupPath { get { return SavePath + ".bak"; } }

        static SaveData Data
        {
            get
            {
                if (data == null) Load();
                return data;
            }
        }

        public static void ResetStatics()
        {
            data = null;
            dirty = false;
        }

        // ----- Forms -----
        public static FormType CurrentForm
        {
            get { return (FormType)Mathf.Clamp(Data.form, 0, 2); }
            set { if (Data.form != (int)value) { Data.form = (int)value; dirty = true; } }
        }

        public static bool IsFormUnlocked(FormType form)
        {
            return (Data.unlockedForms & (1 << (int)form)) != 0;
        }

        public static void SetFormUnlocked(FormType form, bool unlocked)
        {
            int mask = Data.unlockedForms;
            if (unlocked) mask |= 1 << (int)form; else mask &= ~(1 << (int)form);
            if (mask != Data.unlockedForms) { Data.unlockedForms = mask; dirty = true; }
        }

        // ----- Cosmetics (independent from forms) -----
        public static int HatIndex
        {
            get { return Data.hat; }
            set { if (Data.hat != value) { Data.hat = value; dirty = true; } }
        }

        // ----- Collectibles -----
        public static bool IsCollected(string id)
        {
            return Data.collected.Contains(id);
        }

        public static void SetCollected(string id)
        {
            if (Data.collected.Contains(id)) return;
            Data.collected.Add(id);
            dirty = true;
            Save();
        }

        // ----- Level completion -----
        public static bool IsLevelComplete(string levelId)
        {
            return Data.completedLevels.Contains(levelId);
        }

        public static void SetLevelComplete(string levelId)
        {
            if (Data.completedLevels.Contains(levelId)) return;
            Data.completedLevels.Add(levelId);
            dirty = true;
            Save();
        }

        // ----- Tutorial cards (each level card shows once) -----
        public static bool IsHintSeen(string id)
        {
            return Data.hintsSeen.Contains(id);
        }

        public static void SetHintSeen(string id)
        {
            if (Data.hintsSeen.Contains(id)) return;
            Data.hintsSeen.Add(id);
            dirty = true;
        }

        // ----- Saving -----

        // Writes the file if anything changed. Writes to a temporary file first and keeps the
        // previous save as a backup, so a crash halfway through can never wipe progress.
        public static void Save()
        {
            if (data == null || !dirty) return;
            try
            {
                string json = JsonUtility.ToJson(data, true);
                string tmp = SavePath + ".tmp";
                File.WriteAllText(tmp, json);
                if (File.Exists(SavePath)) File.Copy(SavePath, BackupPath, true);
                File.Copy(tmp, SavePath, true);
                File.Delete(tmp);
                dirty = false;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[GameState] Couldn't save: " + e.Message);
            }
        }

        // Clears all progress (used by the editor's Reset Save Data).
        public static void DeleteSave()
        {
            try
            {
                if (File.Exists(SavePath)) File.Delete(SavePath);
                if (File.Exists(BackupPath)) File.Delete(BackupPath);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[GameState] Couldn't delete save: " + e.Message);
            }
            data = null;
            dirty = false;
        }

        static void Load()
        {
            data = TryRead(SavePath);
            if (data == null)
            {
                data = TryRead(BackupPath);
                if (data != null) Debug.LogWarning("[GameState] Save file was damaged; loaded the backup.");
            }
            if (data == null)
            {
                data = FromOldPlayerPrefs();
                dirty = true;
                Save();
            }
            if (data.collected == null) data.collected = new List<string>();
            if (data.completedLevels == null) data.completedLevels = new List<string>();
            if (data.hintsSeen == null) data.hintsSeen = new List<string>();
        }

        static SaveData TryRead(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                var d = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
                if (d == null || d.version < 1) return null;
                return d;
            }
            catch
            {
                return null;
            }
        }

        // Saves made before the JSON file existed were spread over PlayerPrefs keys.
        static SaveData FromOldPlayerPrefs()
        {
            var d = new SaveData();
            d.form = PlayerPrefs.GetInt("pw_current_form", d.form);
            d.unlockedForms = PlayerPrefs.GetInt("pw_unlocked_forms", d.unlockedForms);
            d.hat = PlayerPrefs.GetInt("pw_cosmetic_hat", d.hat);
            foreach (var id in Level1Builder.CollectibleIds)
                if (PlayerPrefs.GetInt("pw_collect_" + id, 0) == 1) d.collected.Add(id);
            foreach (var level in new[] { HubBuilder.SceneName, Level1Builder.SceneName })
                if (PlayerPrefs.GetInt("pw_complete_" + level, 0) == 1) d.completedLevels.Add(level);
            return d;
        }
    }
}

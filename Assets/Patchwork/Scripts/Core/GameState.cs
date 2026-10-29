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
        // 2: Level 1 became Toybox Town, so finishing the old garden level no longer counts.
        const int Version = 2;
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
            public List<string> bestThreads = new List<string>();   // "Level1=23/40"
            public List<int> unlockedHats = new List<int>();         // hats found in the world
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

        // ----- Threads and hat unlocks -----

        // Best number of thread spools collected in one visit to a level, and how many it has.
        public static int BestThreads(string levelId, out int total)
        {
            total = 0;
            foreach (var e in Data.bestThreads)
            {
                if (!e.StartsWith(levelId + "=")) continue;
                var parts = e.Substring(levelId.Length + 1).Split('/');
                int best;
                if (parts.Length == 2 && int.TryParse(parts[0], out best) && int.TryParse(parts[1], out total)) return best;
            }
            return 0;
        }

        // Records a finished visit. Returns true if it's a new best.
        public static bool RecordThreads(string levelId, int count, int total)
        {
            int oldTotal;
            int best = BestThreads(levelId, out oldTotal);
            if (count <= best && total == oldTotal) return false;
            Data.bestThreads.RemoveAll(e => e.StartsWith(levelId + "="));
            Data.bestThreads.Add(levelId + "=" + Mathf.Max(best, count) + "/" + total);
            dirty = true;
            Save();
            return count > best;
        }

        // Hats: 0 none and 1 beanie are free. 2 top hat = half of Level 1's threads in one go,
        // 3 daisy = all of them, 4 crown = found in the hub's Button Box.
        public static bool IsHatUnlocked(int hat)
        {
            if (hat <= 1) return true;
            if (hat == 2 || hat == 3)
            {
                int total;
                int best = BestThreads(Level1Builder.SceneName, out total);
                if (total <= 0) return false;
                return hat == 2 ? best * 2 >= total : best >= total;
            }
            return Data.unlockedHats.Contains(hat);
        }

        public static string HatRequirement(int hat)
        {
            switch (hat)
            {
                case 2: return "Collect half of Level 1's threads in one run.";
                case 3: return "Collect every thread in Level 1 in one run.";
                case 4: return "Inside the captain's cabin on the Skiff.";
                default: return "";
            }
        }

        public static void UnlockHat(int hat)
        {
            if (Data.unlockedHats.Contains(hat)) return;
            Data.unlockedHats.Add(hat);
            dirty = true;
            Save();
        }

        public static int GoldenButtonCount()
        {
            int n = 0;
            foreach (var id in Level1Builder.CollectibleIds) if (IsCollected(id)) n++;
            return n;
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
                // Very old saves lived in PlayerPrefs; clear those too, or they'd come back.
                foreach (var key in new[] { "pw_current_form", "pw_unlocked_forms", "pw_cosmetic_hat" }) PlayerPrefs.DeleteKey(key);
                foreach (var id in Level1Builder.CollectibleIds) PlayerPrefs.DeleteKey("pw_collect_" + id);
                foreach (var level in new[] { HubBuilder.SceneName, Level1Builder.SceneName }) PlayerPrefs.DeleteKey("pw_complete_" + level);
                PlayerPrefs.Save();
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
                data.version = 1;   // so the version upgrade below still runs on it
                dirty = true;
            }
            if (data.collected == null) data.collected = new List<string>();
            if (data.completedLevels == null) data.completedLevels = new List<string>();
            if (data.version < 2)
            {
                // The old Level 1 (the Sewing Basket Garden) is gone; Toybox Town starts fresh.
                data.completedLevels.Remove(Level1Builder.SceneName);
                // Its Heartstrings were in different places, so those start fresh too.
                data.collected.RemoveAll(id => id.StartsWith("L1_hs_"));
                data.version = 2;
                dirty = true;
            }
            if (data.hintsSeen == null) data.hintsSeen = new List<string>();
            if (data.bestThreads == null) data.bestThreads = new List<string>();
            if (data.unlockedHats == null) data.unlockedHats = new List<int>();
            if (dirty) Save();
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

using UnityEngine;

namespace Patchwork
{
    /// <summary>
    /// Small persistent save layer (PlayerPrefs). Keeps form state, cosmetics and
    /// collectibles in separate keys so changing one never erases another.
    /// </summary>
    public static class GameState
    {
        const string KeyForm = "pw_current_form";
        const string KeyUnlocked = "pw_unlocked_forms";
        const string KeyHat = "pw_cosmetic_hat";
        const string KeyCollectPrefix = "pw_collect_";
        const string KeyCompletePrefix = "pw_complete_";

        // ----- Forms -----
        public static FormType CurrentForm
        {
            get { return (FormType)Mathf.Clamp(PlayerPrefs.GetInt(KeyForm, (int)FormType.Yarn), 0, 2); }
            set { PlayerPrefs.SetInt(KeyForm, (int)value); }
        }

        /// <summary>Bitmask of unlocked forms. All three are unlocked by default in the prototype.</summary>
        static int UnlockedMask
        {
            get { return PlayerPrefs.GetInt(KeyUnlocked, 0b111); }
            set { PlayerPrefs.SetInt(KeyUnlocked, value); }
        }

        public static bool IsFormUnlocked(FormType form)
        {
            return (UnlockedMask & (1 << (int)form)) != 0;
        }

        public static void SetFormUnlocked(FormType form, bool unlocked)
        {
            int mask = UnlockedMask;
            if (unlocked) mask |= 1 << (int)form; else mask &= ~(1 << (int)form);
            UnlockedMask = mask;
        }

        // ----- Cosmetics (independent from forms) -----
        public static int HatIndex
        {
            get { return PlayerPrefs.GetInt(KeyHat, 1); }
            set { PlayerPrefs.SetInt(KeyHat, value); }
        }

        // ----- Collectibles -----
        public static bool IsCollected(string id)
        {
            return PlayerPrefs.GetInt(KeyCollectPrefix + id, 0) == 1;
        }

        public static void SetCollected(string id)
        {
            PlayerPrefs.SetInt(KeyCollectPrefix + id, 1);
            PlayerPrefs.Save();
        }

        // ----- Level completion -----
        public static bool IsLevelComplete(string levelId)
        {
            return PlayerPrefs.GetInt(KeyCompletePrefix + levelId, 0) == 1;
        }

        public static void SetLevelComplete(string levelId)
        {
            PlayerPrefs.SetInt(KeyCompletePrefix + levelId, 1);
            PlayerPrefs.Save();
        }

        public static void Save()
        {
            PlayerPrefs.Save();
        }

        /// <summary>Clears collectibles/completion but keeps cosmetics.</summary>
        public static void ResetProgress(string[] collectibleIds, string[] levelIds)
        {
            foreach (var id in collectibleIds) PlayerPrefs.DeleteKey(KeyCollectPrefix + id);
            foreach (var id in levelIds) PlayerPrefs.DeleteKey(KeyCompletePrefix + id);
            PlayerPrefs.Save();
        }
    }
}

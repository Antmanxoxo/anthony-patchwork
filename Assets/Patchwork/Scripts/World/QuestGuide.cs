using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Whatever is running a level's story can tell the HUD where to go next (the gold pointer)
    // and what to do next (the "Next:" line). Level 1: Level1Guide. Level 2: Level2Quest.
    public interface IQuestGuide
    {
        bool ObjectiveTarget(out Vector3 at, out string label);
        string Objective();
        // Every quest in the level so far, for the quest menu: the main story first, then the
        // side quests you've been given, then the things there are to collect.
        void BuildLog(List<QuestEntry> log);
    }

    // One quest in the quest menu.
    public class QuestEntry
    {
        public enum Kind { Main, Side, Collection }
        public string Id;
        public Kind Type;
        public string Title;
        public string Giver;        // who gave it (or where it's from)
        public string Summary;      // what it's about, in a sentence or two
        public bool Done;
        public readonly List<QuestStep> Steps = new List<QuestStep>();
        // Where the pointer should go while this quest is tracked (null: it can't point at it).
        public System.Func<Vector3?> Target;
        public string TargetLabel;

        public QuestEntry Step(string text, bool done) { Steps.Add(new QuestStep { Text = text, Done = done }); return this; }
        public int StepsDone { get { int n = 0; foreach (var s in Steps) if (s.Done) n++; return n; } }
    }

    public struct QuestStep
    {
        public string Text;
        public bool Done;
    }

    public static class QuestGuide
    {
        public static IQuestGuide Current;

        // The quest the pointer follows (by Id, per level). Empty: the main story.
        static string TrackKey()
        {
            var game = PatchworkGame.Instance;
            return "pw_track_" + (game != null && game.Level != null ? game.Level.Id : "none");
        }

        public static string Tracked
        {
            get { return PlayerPrefs.GetString(TrackKey(), ""); }
            set { PlayerPrefs.SetString(TrackKey(), value ?? ""); PlayerPrefs.Save(); }
        }

        // The quest log, rebuilt at most a few times a second (the pointer asks every frame).
        static readonly List<QuestEntry> cache = new List<QuestEntry>();
        static float cacheTime = -10f;
        static IQuestGuide cacheFor;

        public static List<QuestEntry> Log(bool fresh = false)
        {
            if (Current == null) { cache.Clear(); return cache; }
            if (fresh || cacheFor != Current || Time.unscaledTime - cacheTime > 0.4f)
            {
                cache.Clear();
                Current.BuildLog(cache);
                cacheFor = Current;
                cacheTime = Time.unscaledTime;
            }
            return cache;
        }

        public static QuestEntry Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var q in Log()) if (q.Id == id) return q;
            return null;
        }

        // Where the gold pointer goes: the tracked side quest, if it can say where it is and
        // isn't finished, otherwise the next step of the main story. Nothing if the marker is
        // switched off in the quest menu.
        public static bool Target(out Vector3 at, out string label)
        {
            at = Vector3.zero;
            label = null;
            if (Current == null || !GameSettings.QuestMarker) return false;
            var q = Find(Tracked);
            if (q != null && q.Type != QuestEntry.Kind.Main && !q.Done && q.Target != null)
            {
                var p = q.Target();
                if (p.HasValue) { at = p.Value; label = q.TargetLabel ?? q.Title; return true; }
            }
            return Current.ObjectiveTarget(out at, out label);
        }

        // Shows "Next: ..." when the step changes, unless that's switched off.
        public static void NextToast(PatchworkGame game, string objective)
        {
            if (game != null && GameSettings.QuestPopups) game.ShowToast("Next: " + objective);
        }

        // Handy for quest targets: whoever's called this.
        public static Vector3? NpcAt(string name)
        {
            foreach (var n in Object.FindObjectsByType<ToyNpc>(FindObjectsInactive.Exclude))
                if (n != null && n.Name == name) return n.transform.position;
            return null;
        }
    }
}

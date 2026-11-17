using UnityEngine;

namespace Patchwork
{
    // Whatever is running a level's story can tell the HUD where to go next (the gold pointer)
    // and what to do next (the "Next:" line). Level 1: Level1Guide. Level 2: Level2Quest.
    public interface IQuestGuide
    {
        bool ObjectiveTarget(out Vector3 at, out string label);
        string Objective();
    }

    public static class QuestGuide
    {
        public static IQuestGuide Current;
    }
}

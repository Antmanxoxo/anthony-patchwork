using UnityEngine;

namespace Patchwork
{
    // Anyone you can talk to. The dialogue box asks them for their name, lines and voice.
    public abstract class Npc : MonoBehaviour
    {
        public abstract string DisplayNameText { get; }
        public virtual Color NameColor { get { return new Color(0.62f, 0.27f, 0.3f); } }
        public abstract Vector3 HeadPosition { get; }
        public bool Talking { get; set; }

        // What they say this time (one card per line).
        public abstract string[] NextLines();
        // One syllable of their voice.
        public abstract void Blip();
        // When Patch walks up to them.
        public virtual void Greet() { }
        // Called when a chat with them finishes (used to start things like the wedding).
        public virtual void OnTalkEnd() { }
    }
}

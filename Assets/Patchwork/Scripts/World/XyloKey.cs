using UnityEngine;

namespace Patchwork
{
    // One key of a walk-on xylophone. Stepping onto it plays its note (and the key dips and
    // glows a little). The bridge it belongs to notices if you play the whole scale in a row.
    public class XyloKey : PlayerVolume
    {
        public int Semitones;           // above C5
        public System.Action<XyloKey> Struck;
        public int Index;
        Transform key;
        Vector3 rest;
        float dip;

        public void Setup(Transform keyVisual, int index, int semitones)
        {
            key = keyVisual;
            rest = key.localPosition;
            Index = index;
            Semitones = semitones;
        }

        void Update()
        {
            dip = Mathf.MoveTowards(dip, 0f, Time.deltaTime * 4f);
            if (key != null) key.localPosition = rest + Vector3.down * 0.06f * Mathf.Sin(dip * Mathf.PI);
        }

        protected override void OnPlayerEnter(PatchworkPlayer player)
        {
            dip = 1f;
            Sfx.PlayPitched(SfxId.Xylo, 0.5f, Mathf.Pow(2f, Semitones / 12f));
            FxBurst.Spawn(FxBurst.Kind.Sparkle, transform.position + Vector3.up * 0.2f, Color.Lerp(new Color(1f, 0.5f, 0.6f), new Color(0.5f, 0.7f, 1f), Index / 8f));
            if (Struck != null) Struck(this);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // One corner of a level that the Fray has got its threads into. The Fray's tears, tendrils and
    // grey static belong to it, and so do the flowers that grow back once it's mended.
    // Mending shrinks the Fray away and blooms the flowers in.
    public class FrayRegion : MonoBehaviour
    {
        public string HeartstringId;
        readonly List<Transform> fray = new List<Transform>();
        readonly List<Vector3> frayScale = new List<Vector3>();
        readonly List<Transform> bloom = new List<Transform>();
        readonly List<Vector3> bloomScale = new List<Vector3>();
        float clearT = -1f;   // < 0 not mending; 0..1 mending; > 1 done
        float growT = -1f;    // the intro: the Fray spreading in
        public bool Mended { get; private set; }

        // Toybox Town's intro starts with the town still clean: the Fray is hidden, then grows in.
        public void HideFray()
        {
            if (Mended) return;
            foreach (var t in fray) if (t != null) t.gameObject.SetActive(false);
        }

        public void GrowFray()
        {
            if (Mended) return;
            growT = 0f;
            for (int i = 0; i < fray.Count; i++)
            {
                if (fray[i] == null) continue;
                fray[i].gameObject.SetActive(true);
                fray[i].localScale = Vector3.zero;
            }
        }

        public void ShowFrayNow()
        {
            if (Mended) return;
            growT = -1f;
            for (int i = 0; i < fray.Count; i++)
            {
                if (fray[i] == null) continue;
                fray[i].gameObject.SetActive(true);
                fray[i].localScale = frayScale[i];
            }
        }

        public void AddFray(Transform t)
        {
            fray.Add(t);
            frayScale.Add(t.localScale);
        }

        public void AddBloom(Transform t)
        {
            bloom.Add(t);
            bloomScale.Add(t.localScale);
            t.localScale = Vector3.zero;
            t.gameObject.SetActive(false);
        }

        public void Mend(bool instant)
        {
            if (Mended) return;
            Mended = true;
            if (instant)
            {
                foreach (var t in fray) if (t != null) t.gameObject.SetActive(false);
                for (int i = 0; i < bloom.Count; i++) { bloom[i].gameObject.SetActive(true); bloom[i].localScale = bloomScale[i]; }
                clearT = 2f;
            }
            else
            {
                clearT = 0f;
                foreach (var b in bloom) b.gameObject.SetActive(true);
            }
        }

        void Update()
        {
            if (growT >= 0f && !Mended)
            {
                growT += Time.deltaTime / 1.6f;
                for (int i = 0; i < fray.Count; i++)
                {
                    if (fray[i] == null) continue;
                    float k = Mathf.Clamp01((growT - (i % 5) * 0.08f) / 0.6f);
                    fray[i].localScale = frayScale[i] * UIKit.EaseOutBack(k);
                }
                if (growT > 1.5f) growT = -1f;
            }
            if (clearT < 0f || clearT > 1.5f) return;
            clearT += Time.deltaTime / 2.2f;
            for (int i = 0; i < fray.Count; i++)
            {
                if (fray[i] == null) continue;
                // Each piece goes at its own moment, so it peels away rather than vanishing at once.
                float k = Mathf.Clamp01((clearT - (i % 7) * 0.06f) / 0.5f);
                fray[i].localScale = frayScale[i] * (1f - Mathf.SmoothStep(0f, 1f, k));
                if (k >= 1f && fray[i].gameObject.activeSelf)
                {
                    fray[i].gameObject.SetActive(false);
                    if (i % 3 == 0) FxBurst.Spawn(FxBurst.Kind.Sparkle, fray[i].position, new Color(1f, 0.85f, 0.5f));
                }
            }
            for (int i = 0; i < bloom.Count; i++)
            {
                float k = Mathf.Clamp01((clearT - 0.35f - (i % 9) * 0.05f) / 0.4f);
                bloom[i].localScale = bloomScale[i] * UIKit.EaseOutBack(k);
            }
            if (clearT >= 1.5f) clearT = 2f;
        }
    }
}

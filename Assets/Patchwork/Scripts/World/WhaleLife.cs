using UnityEngine;

namespace Patchwork
{
    // The Great Whale over the Observatory (and her calf): more than going round and round.
    //   - Every half a minute or so she sings: a long, low song you hear through the glass, the
    //     stars on her back brighten and pulse, her light swells, and a ring of bubbles rises
    //     from her blowhole.
    //   - Now and then she rolls right over, slowly, like somebody turning in their sleep.
    //   - When somebody is looking at her through the Night Glass (Watched), she slows right
    //     down and sings more often.
    public class WhaleLife : MonoBehaviour
    {
        public FishSwim Swim;
        public Transform Stars;
        public Light Glow;
        public bool Calf;
        public bool Watched;
        public float Size = 1f;

        AudioSource voice;
        float nextSong, songT = -1f, rollT = -1f, nextRoll, baseSpeed, baseGlow;
        Vector3 starScale;
        Transform[] bubbles;
        float[] bubbleT;
        Vector3[] bubbleFrom;

        public bool Singing { get { return songT >= 0f; } }

        void Start()
        {
            if (Swim == null) Swim = GetComponent<FishSwim>();
            baseSpeed = Swim != null ? Swim.Speed : 0f;
            nextSong = Calf ? 9999f : Random.Range(6f, 12f);
            nextRoll = Random.Range(25f, 40f) + (Calf ? 9f : 0f);
            if (Stars != null) starScale = Stars.localScale;
            if (Glow != null) baseGlow = Glow.intensity;
            voice = gameObject.AddComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.spatialBlend = 1f;
            voice.rolloffMode = AudioRolloffMode.Linear;
            voice.minDistance = 25f;
            voice.maxDistance = 120f;
            voice.dopplerLevel = 0f;
            // The bubbles live in the world (not on the whale), so they rise straight up behind her.
            int n = 9;
            bubbles = new Transform[n];
            bubbleT = new float[n];
            bubbleFrom = new Vector3[n];
            var mat = PatchworkAssets.Glow(new Color(0.8f, 0.95f, 1f), 0.8f);
            for (int i = 0; i < n; i++)
            {
                var b = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(b.GetComponent<Collider>());
                b.name = "WhaleBubble";
                b.transform.SetParent(transform.parent, false);
                var r = b.GetComponent<Renderer>();
                r.sharedMaterial = mat;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                b.SetActive(false);
                bubbles[i] = b.transform;
                bubbleT[i] = -1f;
            }
        }

        // A song, now.
        public void Sing()
        {
            songT = 0f;
            if (voice != null && Sfx.Volume > 0)
            {
                voice.pitch = Calf ? 1.7f : 1f;
                voice.volume = (Calf ? 0.3f : 0.85f) * Sfx.Volume / 10f;
                voice.PlayOneShot(Sfx.WhaleSong);
            }
            // A ring of bubbles out of the blowhole.
            Vector3 hole = transform.position + transform.up * 1.1f * Size + transform.forward * 1.6f * Size;
            for (int i = 0; i < bubbles.Length; i++)
            {
                float a = i / (float)bubbles.Length * Mathf.PI * 2f;
                bubbleFrom[i] = hole + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 0.5f * Size;
                bubbleT[i] = -i * 0.12f;
            }
        }

        void Update()
        {
            float dt = Time.deltaTime;
            nextSong -= dt;
            if (nextSong <= 0f && !Calf)
            {
                Sing();
                nextSong = Watched ? 7.5f : Random.Range(22f, 32f);
            }
            if (Swim != null) Swim.Speed = Mathf.MoveTowards(Swim.Speed, Watched ? baseSpeed * 0.25f : baseSpeed, dt * 2f);

            // While she sings, the stars on her back pulse and her light swells.
            float pulse = 0f;
            if (songT >= 0f)
            {
                songT += dt;
                pulse = Mathf.Sin(Mathf.PI * Mathf.Clamp01(songT / 6.5f));
                if (songT > 6.5f) songT = -1f;
            }
            if (Stars != null) Stars.localScale = starScale * (1f + pulse * (0.12f + 0.08f * Mathf.Sin(Time.time * 7f)));
            if (Glow != null) Glow.intensity = baseGlow + pulse * (Calf ? 0.6f : 2.4f) + (Watched ? 0.8f : 0f);

            nextRoll -= dt;
            if (nextRoll <= 0f && rollT < 0f) rollT = 0f;
            if (rollT >= 0f)
            {
                rollT += dt;
                if (rollT > 6f) { rollT = -1f; nextRoll = Random.Range(35f, 55f); }
            }

            for (int i = 0; i < bubbles.Length; i++)
            {
                if (bubbleT[i] == -1f) continue;
                bubbleT[i] += dt;
                float k = bubbleT[i] / 3.2f;
                bool on = k >= 0f && k < 1f;
                if (bubbles[i].gameObject.activeSelf != on) bubbles[i].gameObject.SetActive(on);
                if (k >= 1f) { bubbleT[i] = -1f; continue; }
                if (!on) continue;
                bubbles[i].position = bubbleFrom[i] + new Vector3(Mathf.Sin(k * 9f + i) * 0.3f, k * 7f * Size, 0f);
                bubbles[i].localScale = Vector3.one * Mathf.Lerp(0.18f, 0.55f, k) * Size;
            }
        }

        // A slow roll right over, once in a while (after the swim has set the heading).
        void LateUpdate()
        {
            if (rollT < 0f) return;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(rollT / 6f));
            transform.rotation = transform.rotation * Quaternion.Euler(0f, 0f, k * 360f);
        }
    }
}

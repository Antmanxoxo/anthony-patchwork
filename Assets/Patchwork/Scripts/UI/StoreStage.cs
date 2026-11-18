using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // The Store's little theatre: a felt stage with a curtain and footlights, built a long way
    // under the level where nothing else can see it, and the three forms standing on it (Water,
    // Yarn, Wind) to model whatever you're looking at. Its own camera films them into a picture
    // the Store draws on screen. A fourth Patch off to one side, with a camera of its own, poses
    // for the little pictures on the outfit and hat cards, one a frame.
    public class StoreStage
    {
        public const int Layer = 29;
        static readonly Vector3 Origin = new Vector3(0f, -600f, 0f);
        static readonly Vector3 ThumbSpot = new Vector3(30f, 0f, 0f);

        class Model
        {
            public FormType Form;
            public Transform Holder;
            public PlayerMotor Motor;
            public FormVisuals Visuals;
            public Vector3 Home;
            public float Hop = -1f, Delay, Spin, Dance;
        }

        readonly GameObject root;
        readonly List<Model> models = new List<Model>();
        readonly Model thumb;
        readonly Camera cam, thumbCam;
        public RenderTexture View { get; private set; }
        float time, userYaw, userYawVel;

        // Pictures for the cards, keyed by "cat:index"; and the ones still to take.
        readonly Dictionary<string, RenderTexture> thumbs = new Dictionary<string, RenderTexture>();
        readonly Queue<KeyValuePair<string, int[]>> thumbQueue = new Queue<KeyValuePair<string, int[]>>();
        string thumbTaking;

        public StoreStage()
        {
            root = new GameObject("StoreStage");
            root.transform.position = Origin;
            BuildSet();

            models.Add(MakeModel(FormType.Water, new Vector3(-1.2f, 0f, 0.15f), 14f));
            models.Add(MakeModel(FormType.Yarn, new Vector3(0f, 0f, -0.1f), 0f));
            models.Add(MakeModel(FormType.Wind, new Vector3(1.2f, 0f, 0.15f), -14f));
            thumb = MakeModel(FormType.Yarn, ThumbSpot, 0f);

            View = new RenderTexture(1100, 760, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4, name = "StoreView" };
            cam = MakeCamera("StoreCamera", View);
            cam.transform.localPosition = new Vector3(0f, 1.15f, 4.6f);
            cam.transform.LookAt(root.transform.TransformPoint(new Vector3(0f, 0.68f, 0f)));
            cam.fieldOfView = 30f;
            thumbCam = MakeCamera("StoreThumbCamera", null);
            thumbCam.enabled = false;
            thumbCam.fieldOfView = 26f;

            // The game's own camera never needs to see any of this.
            foreach (var c in Camera.allCameras)
                if (c != cam && c != thumbCam) c.cullingMask &= ~(1 << Layer);
            SetLayer(root.transform);
        }

        Camera MakeCamera(string name, RenderTexture target)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root.transform, false);
            var c = go.AddComponent<Camera>();
            c.cullingMask = 1 << Layer;
            c.clearFlags = CameraClearFlags.SolidColor;
            c.backgroundColor = new Color(0.32f, 0.16f, 0.26f);
            c.nearClipPlane = 0.05f;
            c.farClipPlane = 30f;
            c.targetTexture = target;
            c.depth = -50f;
            return c;
        }

        Model MakeModel(FormType form, Vector3 at, float yaw)
        {
            var go = new GameObject("Model_" + form);
            go.transform.SetParent(root.transform, false);
            go.transform.localPosition = at;
            go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            var motor = go.AddComponent<PlayerMotor>();
            var cc = go.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            motor.Params = FormLibrary.Get(form);
            var m = new Model { Form = form, Holder = go.transform, Motor = motor, Home = at, Spin = yaw };
            m.Visuals = new FormVisuals(go.transform);
            m.Visuals.SetForm(form, false);
            m.Visuals.UseUnscaledTime();
            return m;
        }

        // The theatre: a round felt stage with a stitched edge, a pleated curtain behind,
        // bunting, and a row of footlights.
        void BuildSet()
        {
            var t = root.transform;
            var stageFelt = PatchworkAssets.Felt(new Color(0.95f, 0.78f, 0.5f));
            var edge = PatchworkAssets.Felt(new Color(0.85f, 0.4f, 0.48f));
            var curtain = PatchworkAssets.Felt(new Color(0.72f, 0.16f, 0.26f));
            var curtainDark = PatchworkAssets.Felt(new Color(0.55f, 0.1f, 0.2f));
            var gold = PatchworkAssets.Glow(new Color(1f, 0.82f, 0.4f), 0.9f);
            Part(t, PrimitiveType.Cylinder, new Vector3(0f, -0.06f, 0f), new Vector3(5.2f, 0.06f, 3.4f), stageFelt);
            Part(t, PrimitiveType.Cylinder, new Vector3(0f, -0.1f, 0f), new Vector3(5.5f, 0.06f, 3.7f), edge);
            for (int i = 0; i < 28; i++)
            {
                float a = i / 28f * Mathf.PI * 2f;
                Part(t, PrimitiveType.Cube, new Vector3(Mathf.Cos(a) * 2.5f, 0.002f, Mathf.Sin(a) * 1.62f), new Vector3(0.16f, 0.01f, 0.04f), PatchworkAssets.Felt(new Color(1f, 0.95f, 0.85f)), new Vector3(0f, -a * Mathf.Rad2Deg + 90f, 0f));
            }
            // The curtain: a row of soft pleats.
            for (int i = -12; i <= 12; i++)
                Part(t, PrimitiveType.Cylinder, new Vector3(i * 0.32f, 2.2f, -1.75f), new Vector3(0.36f, 2.6f, 0.2f), i % 2 == 0 ? curtain : curtainDark);
            // The pelmet, with gold tassels.
            Part(t, PrimitiveType.Cube, new Vector3(0f, 3.75f, -1.55f), new Vector3(8.4f, 0.55f, 0.18f), curtainDark);
            for (int i = -8; i <= 8; i++)
            {
                Part(t, PrimitiveType.Sphere, new Vector3(i * 0.5f, 3.42f, -1.45f), new Vector3(0.12f, 0.16f, 0.12f), gold);
                Part(t, PrimitiveType.Cylinder, new Vector3(i * 0.5f, 3.3f, -1.45f), new Vector3(0.03f, 0.08f, 0.03f), gold);
            }
            // Bunting across the top.
            Color[] flags = { new Color(0.95f, 0.45f, 0.55f), new Color(1f, 0.8f, 0.35f), new Color(0.5f, 0.75f, 0.95f), new Color(0.6f, 0.85f, 0.55f) };
            for (int i = 0; i < 13; i++)
            {
                float x = -3f + i * 0.5f, sag = Mathf.Sin(i / 12f * Mathf.PI) * 0.35f;
                Part(t, PrimitiveType.Cube, new Vector3(x, 3.1f - sag, -1.2f), new Vector3(0.26f, 0.26f, 0.02f), PatchworkAssets.Felt(flags[i % flags.Length]), new Vector3(0f, 0f, 45f));
            }
            // Footlights.
            for (int i = -4; i <= 4; i++)
            {
                float x = i * 0.55f;
                Part(t, PrimitiveType.Sphere, new Vector3(x, 0.02f, 1.55f - Mathf.Abs(i) * 0.05f), new Vector3(0.14f, 0.1f, 0.14f), gold);
            }
            AddLight(t, new Vector3(1.8f, 2.8f, 2.8f), new Color(1f, 0.92f, 0.8f), 2.2f, 10f);
            AddLight(t, new Vector3(-2.2f, 1.8f, 2.4f), new Color(0.75f, 0.85f, 1f), 1.2f, 9f);
            AddLight(t, new Vector3(0f, 2.6f, -1.1f), new Color(1f, 0.75f, 0.85f), 1.5f, 5f);
            AddLight(t, new Vector3(0f, 0.4f, 2.2f), new Color(1f, 0.85f, 0.55f), 0.8f, 4f);
            // A softer light for the picture-taking corner.
            AddLight(t, ThumbSpot + new Vector3(0.8f, 2f, 2.4f), new Color(1f, 0.95f, 0.88f), 2f, 8f);
            AddLight(t, ThumbSpot + new Vector3(-1.4f, 1.4f, 1.6f), new Color(0.8f, 0.88f, 1f), 1f, 7f);
        }

        static void Part(Transform parent, PrimitiveType type, Vector3 at, Vector3 scale, Material mat, Vector3 euler = default(Vector3))
        {
            var go = GameObject.CreatePrimitive(type);
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = at;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.transform.localScale = scale;
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        static void AddLight(Transform parent, Vector3 at, Color c, float intensity, float range)
        {
            var go = new GameObject("StageLight");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = at;
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = c;
            l.intensity = intensity;
            l.range = range;
            l.shadows = LightShadows.None;
        }

        void SetLayer(Transform t)
        {
            t.gameObject.layer = Layer;
            for (int i = 0; i < t.childCount; i++) SetLayer(t.GetChild(i));
        }

        // Puts a look on models: everything as it is now, but with one thing swapped in. The
        // wardrobe reads what he's wearing from the save, so it's swapped there for a moment and
        // put straight back.
        static void Dress(IEnumerable<Model> on, int colour, int fabric, int outfit, int hat)
        {
            int c = GameState.ColourScheme, f = GameState.Fabric, o = GameState.Outfit;
            GameState.ColourScheme = colour;
            GameState.Fabric = fabric;
            GameState.Outfit = outfit;
            try
            {
                foreach (var m in on)
                {
                    m.Visuals.ApplyLook();
                    m.Visuals.SetHat(hat);
                }
            }
            finally
            {
                GameState.ColourScheme = c;
                GameState.Fabric = f;
                GameState.Outfit = o;
            }
        }

        // Show this look on the stage, and have them show it off: a hop each, one after another.
        public void Preview(int colour, int fabric, int outfit, int hat)
        {
            Dress(models, colour, fabric, outfit, hat);
            SetLayer(root.transform);
            for (int i = 0; i < models.Count; i++) { models[i].Hop = 0f; models[i].Delay = i * 0.12f; }
        }

        // Bought it: a little dance, all three.
        public void Celebrate()
        {
            foreach (var m in models) { m.Dance = 2.6f; m.Visuals.SetDancing(true); }
        }

        // Spin them round to see the back (the right stick).
        public void Turn(float amount) { userYawVel = amount * 220f; }

        // The picture on a card (null until it's been taken).
        public Texture Thumb(int cat, int index)
        {
            RenderTexture rt;
            return thumbs.TryGetValue(cat + ":" + index, out rt) && rt != null && thumbTaking != cat + ":" + index ? rt : null;
        }

        // Asks for a card's picture: look = {colour, fabric, outfit, hat}; cat 2 frames the body,
        // anything else the head.
        public void WantThumb(int cat, int index, int[] look)
        {
            string key = cat + ":" + index;
            if (thumbs.ContainsKey(key)) return;
            thumbs[key] = null;
            thumbQueue.Enqueue(new KeyValuePair<string, int[]>(key, look));
        }

        public void Tick(float dt)
        {
            time += dt;
            userYaw += userYawVel * dt;
            userYawVel = Mathf.MoveTowards(userYawVel, 0f, dt * 600f);
            // When no one's spinning them, they drift back to face the front.
            if (Mathf.Abs(userYawVel) < 1f) userYaw = Mathf.LerpAngle(userYaw, 0f, 1f - Mathf.Exp(-1.5f * dt));

            for (int i = 0; i < models.Count; i++)
            {
                var m = models[i];
                m.Motor.HoldForDisplay();
                if (m.Dance > 0f)
                {
                    m.Dance -= dt;
                    m.Visuals.DanceBeat += dt * 2.2f;
                    if (m.Dance <= 0f) m.Visuals.SetDancing(false);
                }
                m.Visuals.Tick(m.Motor, dt);
                // The hop: up, a half-twirl, and down with a squash.
                float lift = 0f, twirl = 0f;
                if (m.Hop >= 0f)
                {
                    float k = (m.Hop - m.Delay) / 0.55f;
                    if (k >= 0f)
                    {
                        if (k < 1f) { lift = Mathf.Sin(k * Mathf.PI) * 0.32f; twirl = Mathf.SmoothStep(0f, 1f, k) * 360f; }
                        if (k >= 1f && k - dt / 0.55f < 1f) m.Visuals.PunchScale(new Vector3(1.2f, 0.8f, 1.2f));
                        if (k > 1.2f) m.Hop = -1f;
                    }
                    if (m.Hop >= 0f) m.Hop += dt;
                }
                float sway = Mathf.Sin(time * 0.7f + i * 1.7f) * 12f;
                m.Holder.localPosition = m.Home + Vector3.up * lift;
                m.Holder.localRotation = Quaternion.Euler(0f, m.Spin + sway + twirl + userYaw, 0f);
            }

            // The picture-taking Patch: dress him for the next card and point the camera, which
            // takes the picture when the frame is drawn.
            thumb.Motor.HoldForDisplay();
            thumb.Visuals.Tick(thumb.Motor, dt);
            thumbTaking = null;
            if (thumbQueue.Count > 0)
            {
                var job = thumbQueue.Dequeue();
                var rt = new RenderTexture(220, 220, 24, RenderTextureFormat.ARGB32) { antiAliasing = 2, name = "StoreThumb" };
                thumbs[job.Key] = rt;
                var look = job.Value;
                Dress(new[] { thumb }, look[0], look[1], look[2], look[3]);
                SetLayer(thumb.Holder);
                bool body = job.Key.StartsWith("2:");
                float headY = thumb.Visuals.FormHeight;
                Vector3 aim = ThumbSpot + Vector3.up * (body ? headY * 0.55f : headY * 0.92f);
                thumbCam.transform.localPosition = aim + new Vector3(0.35f, 0.18f, body ? 2.9f : 1.75f);
                thumbCam.transform.LookAt(root.transform.TransformPoint(aim));
                thumbCam.targetTexture = rt;
                thumbCam.enabled = true;
                thumbTaking = job.Key;
            }
            else if (thumbCam.enabled) thumbCam.enabled = false;
        }

        public void Destroy()
        {
            if (cam != null) cam.targetTexture = null;
            if (thumbCam != null) thumbCam.targetTexture = null;
            if (View != null) { View.Release(); Object.Destroy(View); }
            foreach (var rt in thumbs.Values) if (rt != null) { rt.Release(); Object.Destroy(rt); }
            thumbs.Clear();
            if (root != null) Object.Destroy(root);
        }
    }
}

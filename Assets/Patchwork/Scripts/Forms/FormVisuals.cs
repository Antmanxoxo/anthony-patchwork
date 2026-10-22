using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    /// <summary>
    /// Builds and animates Patch's three bodies and the transitions between them:
    ///  - Yarn: a little knitted toy with legs and arms; walks, jumps, and reaches a yarn arm up to swing.
    ///  - Water: a wobbly, see-through bubble with big eyes that jiggles when it moves.
    ///  - Wind: a happy little cloud that floats above the ground with a smile and drifting wisps.
    /// Transitions play a short element-specific animation (yarn soaks → water, water steams → wind,
    /// wind rains → water, etc.). Cosmetics live on their own transform and never reset with form.
    /// </summary>
    public class FormVisuals
    {
        public static readonly string[] HatNames = { "No Hat", "Red Beanie", "Felt Top Hat", "Daisy Clip" };

        class Body
        {
            public Transform Root;
            public readonly List<Renderer> Renderers = new List<Renderer>();
            public readonly List<Color> Colors = new List<Color>();
        }

        const float Phase1 = 0.32f;   // old body reacts (soak / steam / rain / unravel)
        const float Phase2 = 0.30f;   // new body pops in

        readonly Transform root;
        readonly Body yarn = new Body(), water = new Body(), wind = new Body();
        readonly Transform accessoryRoot;
        readonly GameObject[] hats = new GameObject[4];
        static MaterialPropertyBlock mpb;

        // Yarn rig
        Transform yarnTorso, yarnHead, hipL, hipR, shoulderL, shoulderR, armR, handR;
        // Water rig
        Transform bubble, bubbleEyes;
        Vector3 bubbleEyesBase;
        readonly Transform[] miniBubbles = new Transform[2];
        // Wind rig
        Transform cloud;
        readonly Transform[] wisps = new Transform[3];

        FormType form;
        Vector3 punch = Vector3.one;
        float time;
        float walkPhase;
        float jiggle;
        Vector3 prevVelocity;
        bool gliding;
        bool tethered;
        Vector3 tetherTarget;
        float warp = 1f;

        // Transition state
        bool transitioning;
        FormType fromForm, toForm;
        float transT;
        bool swapped;

        public FormVisuals(Transform player)
        {
            if (mpb == null) mpb = new MaterialPropertyBlock();
            root = new GameObject("Visuals").transform;
            root.SetParent(player, false);

            BuildYarn();
            BuildWater();
            BuildWind();

            accessoryRoot = new GameObject("Accessories").transform;
            accessoryRoot.SetParent(root, false);
            BuildHats();
            SetHat(GameState.HatIndex);
        }

        // ------------------------------------------------------------------ API

        /// <summary>World position of Yarn's right hand (where the tether starts).</summary>
        public Vector3 HandPosition
        {
            get { return handR != null ? handR.position : root.position + Vector3.up; }
        }

        /// <summary>Instantly show a form (used at spawn).</summary>
        public void SetForm(FormType newForm, bool punchIt)
        {
            EndTransition();
            form = newForm;
            ShowOnly(form);
            if (punchIt) PunchScale(new Vector3(0.6f, 1.4f, 0.6f));
        }

        /// <summary>Plays the element-specific transformation from one body to the other.</summary>
        public void BeginTransition(FormType from, FormType to)
        {
            EndTransition();
            fromForm = from;
            toForm = to;
            form = to;
            transT = 0f;
            swapped = false;
            transitioning = true;

            BodyFor(from).Root.gameObject.SetActive(true);
            var target = BodyFor(to).Root;
            target.gameObject.SetActive(true);
            target.localScale = Vector3.zero;
            BodyFor(OtherForm(from, to)).Root.gameObject.SetActive(false);

            Vector3 feet = root.position;
            Color blue = FormLibrary.Get(FormType.Water).PrimaryColor;
            Color pink = FormLibrary.Get(FormType.Yarn).PrimaryColor;
            if (from == FormType.Yarn && to == FormType.Water) { FxBurst.Spawn(FxBurst.Kind.Drips, feet, blue); Sfx.Play(SfxId.Soak, 0.6f); }
            else if (from == FormType.Water && to == FormType.Wind) { FxBurst.Spawn(FxBurst.Kind.Steam, feet, Color.white); Sfx.Play(SfxId.Steam, 0.6f); }
            else if (from == FormType.Wind && to == FormType.Water) { FxBurst.Spawn(FxBurst.Kind.Rain, feet, blue); Sfx.Play(SfxId.Soak, 0.5f); }
            else if (from == FormType.Wind && to == FormType.Yarn) { FxBurst.Spawn(FxBurst.Kind.Threads, feet, pink); Sfx.Play(SfxId.Unravel, 0.6f); }
            else if (from == FormType.Yarn && to == FormType.Wind) { FxBurst.Spawn(FxBurst.Kind.Threads, feet, pink); Sfx.Play(SfxId.Unravel, 0.6f); }
            else { FxBurst.Spawn(FxBurst.Kind.Splash, feet + Vector3.up * 0.3f, blue); Sfx.Play(SfxId.Soak, 0.5f); }
        }

        public void SetHat(int index)
        {
            index = ((index % hats.Length) + hats.Length) % hats.Length;
            for (int i = 0; i < hats.Length; i++)
                if (hats[i] != null) hats[i].SetActive(i == index);
        }

        public void SetGliding(bool on) { gliding = on; }

        public void SetTether(bool on, Vector3 target)
        {
            tethered = on;
            tetherTarget = target;
        }

        /// <summary>1 = normal, 0 = fully shrunk (portal warp).</summary>
        public void SetWarp(float scale) { warp = scale; }

        public void PunchScale(Vector3 s) { punch = s; }

        // ------------------------------------------------------------------ Animation

        public void Tick(PlayerMotor motor, float dt)
        {
            time += dt;
            punch = Vector3.Lerp(punch, Vector3.one, 1f - Mathf.Exp(-9f * dt));

            Vector3 v = motor.Velocity;
            Vector3 dv = v - prevVelocity;
            prevVelocity = v;
            Vector3 localVel = motor.transform.InverseTransformDirection(v);
            float speed = new Vector3(v.x, 0f, v.z).magnitude;

            root.localScale = punch * warp;
            if (warp < 0.999f) root.localRotation = Quaternion.Euler(0f, (1f - warp) * 900f, 0f);
            else root.localRotation = Quaternion.identity;

            AnimateYarn(motor, speed, localVel, dt);
            AnimateWater(motor, dv, dt);
            AnimateWind(motor, dt);
            if (transitioning) TickTransition(dt);

            // Hat follows the head of the current body (the cloud floats, so its head moves).
            float hatY = FormLibrary.Get(form).HeadHeight;
            if (form == FormType.Wind && cloud != null) hatY += cloud.localPosition.y;
            if (form == FormType.Yarn && yarnHead != null) hatY += yarnTorso.localPosition.y;
            if (form == FormType.Water && bubble != null) hatY *= bubble.localScale.y;
            Vector3 ap = accessoryRoot.localPosition;
            ap.y = Mathf.Lerp(ap.y, hatY, 1f - Mathf.Exp(-14f * dt));
            accessoryRoot.localPosition = ap;
        }

        void AnimateYarn(PlayerMotor motor, float speed, Vector3 localVel, float dt)
        {
            if (!yarn.Root.gameObject.activeSelf) return;
            bool air = !motor.Grounded;
            float stride = Mathf.Clamp01(speed / 3f);
            walkPhase += speed * dt * 2.6f;

            float legSwing = air ? 0f : Mathf.Sin(walkPhase) * 38f * stride;
            float legTuck = air ? -30f : 0f;
            hipL.localRotation = Quaternion.Euler(legSwing + legTuck, 0f, 0f);
            hipR.localRotation = Quaternion.Euler(-legSwing + legTuck, 0f, 0f);

            float bounce = air ? 0f : Mathf.Abs(Mathf.Sin(walkPhase)) * 0.05f * stride + Mathf.Sin(time * 3f) * 0.012f;
            yarnTorso.localPosition = new Vector3(0f, bounce, 0f);
            yarnHead.localRotation = Quaternion.Euler(-localVel.z * 1.2f, 0f, Mathf.Sin(time * 1.7f) * 4f);

            // Arms: swing while walking, flail up while airborne.
            Quaternion armL, armRrot;
            if (air)
            {
                float flap = Mathf.Sin(time * 14f) * 12f;
                armL = Quaternion.Euler(0f, 0f, -125f + flap);
                armRrot = Quaternion.Euler(0f, 0f, 125f - flap);
            }
            else
            {
                armL = Quaternion.Euler(-legSwing * 0.8f, 0f, -12f);
                armRrot = Quaternion.Euler(legSwing * 0.8f, 0f, 12f);
            }
            shoulderL.localRotation = Quaternion.Slerp(shoulderL.localRotation, armL, 1f - Mathf.Exp(-16f * dt));

            if (tethered)
            {
                // Reach the yarn arm straight at the anchor and stretch it a little.
                Vector3 dirWorld = (tetherTarget - shoulderR.position).normalized;
                Vector3 dirLocal = shoulderR.parent.InverseTransformDirection(dirWorld);
                shoulderR.localRotation = Quaternion.FromToRotation(Vector3.down, dirLocal);
                armR.localScale = new Vector3(1f, Mathf.Lerp(armR.localScale.y, 1.6f, 1f - Mathf.Exp(-20f * dt)), 1f);
            }
            else
            {
                shoulderR.localRotation = Quaternion.Slerp(shoulderR.localRotation, armRrot, 1f - Mathf.Exp(-16f * dt));
                armR.localScale = new Vector3(1f, Mathf.Lerp(armR.localScale.y, 1f, 1f - Mathf.Exp(-12f * dt)), 1f);
            }
        }

        void AnimateWater(PlayerMotor motor, Vector3 dv, float dt)
        {
            if (!water.Root.gameObject.activeSelf) return;
            jiggle = Mathf.Min(0.32f, jiggle + dv.magnitude * 0.03f);
            jiggle = Mathf.MoveTowards(jiggle, 0.035f, dt * 0.5f);
            float w = Mathf.Sin(time * 13f) * jiggle;
            float w2 = Mathf.Sin(time * 9f + 1.3f) * jiggle * 0.6f;
            float stretch = Mathf.Clamp(motor.Velocity.y * 0.015f, -0.12f, 0.15f);
            bubble.localScale = new Vector3(1f + w - stretch * 0.5f, 1f - w + stretch, 1f + w2 - stretch * 0.5f);
            bubbleEyes.localPosition = bubbleEyesBase + new Vector3(0f, w * 0.06f, 0f);
            // Blink every few seconds.
            float blink = Mathf.Repeat(time, 3.7f) < 0.12f ? 0.15f : 1f;
            bubbleEyes.localScale = new Vector3(1f, blink, 1f);
            for (int i = 0; i < miniBubbles.Length; i++)
            {
                float a = time * (1.5f + i * 0.7f) + i * 3f;
                miniBubbles[i].localPosition = new Vector3(Mathf.Cos(a) * 0.5f, 0.5f + Mathf.Sin(time * 2f + i) * 0.2f, Mathf.Sin(a) * 0.5f);
            }
        }

        void AnimateWind(PlayerMotor motor, float dt)
        {
            if (!wind.Root.gameObject.activeSelf) return;
            float floatY = 0.28f + Mathf.Sin(time * 2.2f) * 0.08f;
            cloud.localPosition = new Vector3(0f, floatY, 0f);
            Vector3 target = gliding ? new Vector3(1.3f, 0.72f, 1.3f) : Vector3.one;
            cloud.localScale = Vector3.Lerp(cloud.localScale, target, 1f - Mathf.Exp(-8f * dt));
            cloud.localRotation = Quaternion.Euler(Mathf.Sin(time * 1.3f) * 4f, 0f, Mathf.Sin(time * 1.7f) * 6f);
            for (int i = 0; i < wisps.Length; i++)
            {
                float a = time * (gliding ? 5f : 2.2f) + i * Mathf.PI * 2f / 3f;
                wisps[i].localPosition = new Vector3(Mathf.Cos(a) * 0.85f, 0.35f + i * 0.28f + Mathf.Sin(time * 2f + i) * 0.06f, Mathf.Sin(a) * 0.85f);
            }
        }

        void TickTransition(float dt)
        {
            transT += dt;
            var from = BodyFor(fromForm);
            var to = BodyFor(toForm);

            if (transT < Phase1)
            {
                float k = transT / Phase1;
                Color tint;
                Vector3 scale = Vector3.one;
                Vector3 offset = Vector3.zero;
                float spinDeg = 0f;
                if (fromForm == FormType.Yarn && toForm == FormType.Water)
                {   // Soaking: darkens, droops and drips.
                    tint = new Color(0.25f, 0.35f, 0.65f);
                    scale = new Vector3(1f + 0.18f * k, 1f - 0.3f * k, 1f + 0.18f * k);
                }
                else if (fromForm == FormType.Water && toForm == FormType.Wind)
                {   // Evaporating: lifts, whitens, thins out.
                    tint = Color.white;
                    scale = Vector3.one * (1f - 0.55f * k);
                    offset = Vector3.up * 0.6f * k;
                }
                else if (fromForm == FormType.Wind && toForm == FormType.Water)
                {   // Condensing: turns into a little grey rain cloud and shrinks.
                    tint = new Color(0.5f, 0.55f, 0.65f);
                    scale = Vector3.one * (1f - 0.4f * k);
                }
                else if (fromForm == FormType.Water && toForm == FormType.Yarn)
                {   // Being wrung out: squeezes thin and spins.
                    tint = FormLibrary.Get(FormType.Yarn).PrimaryColor;
                    scale = new Vector3(1f - 0.6f * k, 1f + 0.3f * k, 1f - 0.6f * k);
                    spinDeg = 540f * k * k;
                }
                else
                {   // Yarn ↔ Wind: unravelling / spinning up.
                    tint = fromForm == FormType.Yarn ? new Color(1f, 0.85f, 0.9f) : FormLibrary.Get(FormType.Yarn).PrimaryColor;
                    scale = Vector3.one * (1f - 0.7f * k);
                    spinDeg = 900f * k * k;
                }
                Tint(from, tint, k * 0.85f);
                from.Root.localScale = scale;
                from.Root.localPosition = offset;
                from.Root.localRotation = Quaternion.Euler(0f, spinDeg, 0f);
                to.Root.localScale = Vector3.zero;
            }
            else
            {
                if (!swapped)
                {
                    swapped = true;
                    ResetBody(from);
                    from.Root.gameObject.SetActive(false);
                    Vector3 feet = root.position;
                    var def = FormLibrary.Get(toForm);
                    switch (toForm)
                    {
                        case FormType.Water: FxBurst.Spawn(FxBurst.Kind.Splash, feet + Vector3.up * 0.3f, def.PrimaryColor); break;
                        case FormType.Wind: FxBurst.Spawn(FxBurst.Kind.Puff, feet + Vector3.up * 0.8f, Color.white); break;
                        default: FxBurst.Spawn(FxBurst.Kind.Scraps, feet + Vector3.up * 0.8f, def.PrimaryColor); break;
                    }
                    Sfx.Play(SfxId.Transform, 0.55f);
                }
                float k2 = Mathf.Clamp01((transT - Phase1) / Phase2);
                // Back-out overshoot: 0 → 1.15 → 1
                float s = 1f + 2.2f * Mathf.Pow(k2 - 1f, 3f) + 1.2f * Mathf.Pow(k2 - 1f, 2f);
                to.Root.localScale = Vector3.one * s;
                to.Root.localRotation = toForm == FormType.Yarn ? Quaternion.Euler(0f, (1f - k2) * 360f, 0f) : Quaternion.identity;
                if (transT >= Phase1 + Phase2) EndTransition();
            }
        }

        void EndTransition()
        {
            if (!transitioning) return;
            transitioning = false;
            ResetBody(BodyFor(fromForm));
            ResetBody(BodyFor(toForm));
            ShowOnly(form);
        }

        void ShowOnly(FormType f)
        {
            yarn.Root.gameObject.SetActive(f == FormType.Yarn);
            water.Root.gameObject.SetActive(f == FormType.Water);
            wind.Root.gameObject.SetActive(f == FormType.Wind);
        }

        Body BodyFor(FormType f)
        {
            switch (f)
            {
                case FormType.Water: return water;
                case FormType.Wind: return wind;
                default: return yarn;
            }
        }

        static FormType OtherForm(FormType a, FormType b)
        {
            foreach (var f in FormLibrary.AllForms)
                if (f != a && f != b) return f;
            return a;
        }

        static void Tint(Body body, Color tint, float k)
        {
            for (int i = 0; i < body.Renderers.Count; i++)
            {
                var r = body.Renderers[i];
                if (r == null) continue;
                Color orig = body.Colors[i];
                Color c = Color.Lerp(orig, tint, k);
                c.a = orig.a;
                mpb.Clear();
                mpb.SetColor("_BaseColor", c);
                r.SetPropertyBlock(mpb);
            }
        }

        static void ResetBody(Body body)
        {
            for (int i = 0; i < body.Renderers.Count; i++)
                if (body.Renderers[i] != null) body.Renderers[i].SetPropertyBlock(null);
            body.Root.localScale = Vector3.one;
            body.Root.localPosition = Vector3.zero;
            body.Root.localRotation = Quaternion.identity;
        }

        // ------------------------------------------------------------------ Builders

        Body building;

        Transform Part(PrimitiveType type, Transform parent, Vector3 pos, Vector3 scale, Material mat, Vector3 euler = default(Vector3))
        {
            var t = Prim(type, parent, pos, scale, mat, euler);
            if (building != null)
            {
                var r = t.GetComponent<Renderer>();
                building.Renderers.Add(r);
                building.Colors.Add(mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : mat.color);
            }
            return t;
        }

        static Transform Pivot(string name, Transform parent, Vector3 pos)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            t.localPosition = pos;
            return t;
        }

        void BuildYarn()
        {
            var def = FormLibrary.Get(FormType.Yarn);
            building = yarn;
            yarn.Root = Pivot("YarnForm", root, Vector3.zero);
            var body = PatchworkAssets.Knit(def.PrimaryColor);
            var cream = PatchworkAssets.Felt(def.SecondaryColor);
            var dark = PatchworkAssets.Knit(new Color(0.62f, 0.25f, 0.38f));

            // Legs (pivot at the hip so they swing).
            hipL = Pivot("HipL", yarn.Root, new Vector3(-0.16f, 0.4f, 0f));
            hipR = Pivot("HipR", yarn.Root, new Vector3(0.16f, 0.4f, 0f));
            foreach (var hip in new[] { hipL, hipR })
            {
                Part(PrimitiveType.Cylinder, hip, new Vector3(0f, -0.17f, 0f), new Vector3(0.17f, 0.17f, 0.17f), dark);
                Part(PrimitiveType.Sphere, hip, new Vector3(0f, -0.36f, 0.06f), new Vector3(0.24f, 0.13f, 0.32f), cream);
            }

            yarnTorso = Pivot("Torso", yarn.Root, Vector3.zero);
            // Round knitted body with a stitched patch and a thread wrapped around it.
            Part(PrimitiveType.Sphere, yarnTorso, new Vector3(0f, 0.68f, 0f), new Vector3(0.72f, 0.68f, 0.64f), body);
            Part(PrimitiveType.Cylinder, yarnTorso, new Vector3(0f, 0.62f, 0f), new Vector3(0.74f, 0.025f, 0.66f), dark, new Vector3(8f, 0f, 6f));
            Part(PrimitiveType.Cube, yarnTorso, new Vector3(0.12f, 0.62f, 0.31f), new Vector3(0.2f, 0.2f, 0.04f), PatchworkAssets.Fabric(def.SecondaryColor), new Vector3(0f, 15f, 10f));

            // Arms (pivot at the shoulder). The right arm becomes the yarn tether.
            shoulderL = Pivot("ShoulderL", yarnTorso, new Vector3(-0.36f, 0.82f, 0f));
            shoulderR = Pivot("ShoulderR", yarnTorso, new Vector3(0.36f, 0.82f, 0f));
            Part(PrimitiveType.Cylinder, shoulderL, new Vector3(0f, -0.17f, 0f), new Vector3(0.12f, 0.17f, 0.12f), dark);
            Part(PrimitiveType.Sphere, shoulderL, new Vector3(0f, -0.36f, 0f), Vector3.one * 0.15f, cream);
            armR = Pivot("ArmR", shoulderR, Vector3.zero);
            Part(PrimitiveType.Cylinder, armR, new Vector3(0f, -0.17f, 0f), new Vector3(0.12f, 0.17f, 0.12f), dark);
            handR = Part(PrimitiveType.Sphere, armR, new Vector3(0f, -0.36f, 0f), Vector3.one * 0.15f, cream);

            // Head: another yarn ball with button eyes, a stitched smile and a little topknot.
            yarnHead = Pivot("Head", yarnTorso, new Vector3(0f, 1.12f, 0f));
            Part(PrimitiveType.Sphere, yarnHead, Vector3.zero, new Vector3(0.62f, 0.58f, 0.58f), body);
            Part(PrimitiveType.Cylinder, yarnHead, new Vector3(0f, 0.02f, 0f), new Vector3(0.63f, 0.02f, 0.59f), dark, new Vector3(-10f, 0f, 0f));
            AddButtonEyes(yarnHead, 0.05f, 0.27f, 0.13f, 0.12f);
            AddSmile(yarnHead, new Vector3(0f, -0.1f, 0.28f), 0.09f, PatchworkAssets.Plain(new Color(0.35f, 0.12f, 0.2f), 0.2f), 0.035f);
            AddCheeks(yarnHead, new Vector3(0f, -0.06f, 0.24f), 0.2f, 0.1f);
            Part(PrimitiveType.Sphere, yarnHead, new Vector3(-0.06f, 0.33f, 0f), new Vector3(0.12f, 0.16f, 0.12f), body, new Vector3(0f, 0f, 25f));
            Part(PrimitiveType.Sphere, yarnHead, new Vector3(0.07f, 0.34f, -0.02f), new Vector3(0.1f, 0.14f, 0.1f), body, new Vector3(0f, 0f, -25f));

            // Loose thread tail.
            Part(PrimitiveType.Cylinder, yarnTorso, new Vector3(0.05f, 0.42f, -0.36f), new Vector3(0.04f, 0.14f, 0.04f), body, new Vector3(55f, 0f, 10f));
            Part(PrimitiveType.Cylinder, yarnTorso, new Vector3(0.12f, 0.3f, -0.52f), new Vector3(0.04f, 0.1f, 0.04f), body, new Vector3(95f, 30f, 0f));
            building = null;
        }

        void BuildWater()
        {
            var def = FormLibrary.Get(FormType.Water);
            building = water;
            water.Root = Pivot("WaterForm", root, Vector3.zero);
            bubble = Pivot("Bubble", water.Root, Vector3.zero);

            var shell = PatchworkAssets.Bubble(new Color(def.PrimaryColor.r, def.PrimaryColor.g, def.PrimaryColor.b, 0.45f));
            Part(PrimitiveType.Sphere, bubble, new Vector3(0f, 0.4f, 0f), new Vector3(0.8f, 0.78f, 0.8f), shell);
            // A soft inner core gives the bubble some depth.
            Part(PrimitiveType.Sphere, bubble, new Vector3(0f, 0.36f, -0.05f), new Vector3(0.5f, 0.45f, 0.5f),
                PatchworkAssets.Bubble(new Color(def.SecondaryColor.r, def.SecondaryColor.g, def.SecondaryColor.b, 0.35f)));
            // Shiny highlights.
            var shine = PatchworkAssets.Glow(Color.white, 0.8f);
            Part(PrimitiveType.Sphere, bubble, new Vector3(-0.18f, 0.6f, 0.24f), new Vector3(0.14f, 0.09f, 0.05f), shine, new Vector3(0f, 0f, 30f));
            Part(PrimitiveType.Sphere, bubble, new Vector3(-0.26f, 0.5f, 0.22f), Vector3.one * 0.04f, shine);

            // Big friendly cartoon eyes sitting ON the bubble's surface (not inside it), so they stay
            // crisp instead of looking cloudy through the see-through shell. They blink.
            Vector3 bubbleCenter = new Vector3(0f, 0.4f, 0f);
            const float bubbleRadius = 0.4f;
            const float eyeHeight = 0.07f;   // above the bubble centre
            bubbleEyes = Pivot("Eyes", bubble, bubbleCenter + Vector3.up * eyeHeight);
            bubbleEyesBase = bubbleEyes.localPosition;
            var white = PatchworkAssets.Plain(new Color(0.98f, 0.98f, 1f), 0.85f);
            var black = PatchworkAssets.Plain(new Color(0.05f, 0.07f, 0.14f), 0.85f);
            for (int side = -1; side <= 1; side += 2)
            {
                // Direction from the bubble centre to where this eye sits on the surface.
                Vector3 n = new Vector3(side * 0.36f, eyeHeight / bubbleRadius, 1f).normalized;
                Vector3 onSurface = n * (bubbleRadius - 0.02f);
                Vector3 local = onSurface - Vector3.up * eyeHeight;   // relative to the Eyes pivot
                Vector3 euler = Quaternion.LookRotation(n, Vector3.up).eulerAngles;
                // Eyeball, pupil (nudged slightly inward for a cute look), sparkle.
                Part(PrimitiveType.Sphere, bubbleEyes, local, new Vector3(0.17f, 0.22f, 0.08f), white, euler);
                Vector3 inward = new Vector3(-side * 0.012f, -0.01f, 0f);
                Part(PrimitiveType.Sphere, bubbleEyes, local + n * 0.025f + inward, new Vector3(0.11f, 0.15f, 0.06f), black, euler);
                Part(PrimitiveType.Sphere, bubbleEyes, local + n * 0.05f + new Vector3(0.025f, 0.035f, 0f), new Vector3(0.04f, 0.04f, 0.02f), shine, euler);
            }

            // Little smile on the surface, just below the eyes.
            for (int i = 0; i < 5; i++)
            {
                float t = (i / 4f) * 2f - 1f;
                Vector3 n = new Vector3(t * 0.13f, -0.2f - (1f - t * t) * 0.07f, 1f).normalized;
                Part(PrimitiveType.Sphere, bubble, bubbleCenter + n * (bubbleRadius - 0.005f), Vector3.one * 0.03f, black);
            }

            var tiny = PatchworkAssets.Bubble(new Color(0.8f, 0.92f, 1f, 0.5f));
            for (int i = 0; i < miniBubbles.Length; i++)
                miniBubbles[i] = Part(PrimitiveType.Sphere, water.Root, Vector3.zero, Vector3.one * (0.1f + i * 0.04f), tiny);
            building = null;
        }

        void BuildWind()
        {
            var def = FormLibrary.Get(FormType.Wind);
            building = wind;
            wind.Root = Pivot("WindForm", root, Vector3.zero);
            cloud = Pivot("Cloud", wind.Root, new Vector3(0f, 0.28f, 0f));

            var puff = PatchworkAssets.Felt(new Color(0.98f, 0.99f, 1f));
            var shade = PatchworkAssets.Felt(new Color(0.8f, 0.9f, 0.97f));
            Part(PrimitiveType.Sphere, cloud, new Vector3(0f, 0.62f, 0f), new Vector3(0.95f, 0.8f, 0.8f), puff);
            Part(PrimitiveType.Sphere, cloud, new Vector3(-0.42f, 0.5f, 0f), new Vector3(0.6f, 0.55f, 0.58f), puff);
            Part(PrimitiveType.Sphere, cloud, new Vector3(0.44f, 0.52f, 0f), new Vector3(0.62f, 0.56f, 0.58f), puff);
            Part(PrimitiveType.Sphere, cloud, new Vector3(-0.15f, 0.98f, -0.04f), new Vector3(0.55f, 0.5f, 0.5f), puff);
            Part(PrimitiveType.Sphere, cloud, new Vector3(0.2f, 0.93f, -0.08f), new Vector3(0.45f, 0.42f, 0.42f), puff);
            Part(PrimitiveType.Sphere, cloud, new Vector3(0f, 0.36f, -0.02f), new Vector3(1.05f, 0.32f, 0.7f), shade);

            // Happy face: shiny eyes, rosy cheeks, a wide smile.
            var black = PatchworkAssets.Plain(new Color(0.12f, 0.12f, 0.2f), 0.6f);
            var shine = PatchworkAssets.Glow(Color.white, 0.6f);
            for (int side = -1; side <= 1; side += 2)
            {
                Part(PrimitiveType.Sphere, cloud, new Vector3(side * 0.15f, 0.7f, 0.38f), new Vector3(0.1f, 0.14f, 0.05f), black);
                Part(PrimitiveType.Sphere, cloud, new Vector3(side * 0.15f + 0.025f, 0.74f, 0.405f), Vector3.one * 0.035f, shine);
            }
            AddCheeks(cloud, new Vector3(0f, 0.6f, 0.36f), 0.28f, 0.11f);
            AddSmile(cloud, new Vector3(0f, 0.57f, 0.4f), 0.1f, black, 0.03f);

            var wispMat = PatchworkAssets.Glow(def.SecondaryColor, 0.4f);
            for (int i = 0; i < wisps.Length; i++)
                wisps[i] = Part(PrimitiveType.Sphere, wind.Root, Vector3.zero, new Vector3(0.18f, 0.08f, 0.12f), wispMat);
            building = null;
        }

        void AddButtonEyes(Transform parent, float y, float z, float spacing, float size)
        {
            var eyeMat = PatchworkAssets.Plain(new Color(0.08f, 0.06f, 0.1f), 0.7f);
            var thread = PatchworkAssets.Plain(new Color(0.85f, 0.8f, 0.75f), 0.3f);
            for (int side = -1; side <= 1; side += 2)
            {
                Part(PrimitiveType.Cylinder, parent, new Vector3(side * spacing, y, z), new Vector3(size, 0.02f, size), eyeMat, new Vector3(90f, 0f, 0f));
                Part(PrimitiveType.Cube, parent, new Vector3(side * spacing, y, z + 0.022f), new Vector3(size * 0.5f, 0.012f, 0.01f), thread, new Vector3(0f, 0f, 45f));
                Part(PrimitiveType.Cube, parent, new Vector3(side * spacing, y, z + 0.022f), new Vector3(size * 0.5f, 0.012f, 0.01f), thread, new Vector3(0f, 0f, -45f));
            }
        }

        void AddSmile(Transform parent, Vector3 center, float width, Material mat, float dot)
        {
            for (int i = 0; i < 5; i++)
            {
                float t = (i / 4f) * 2f - 1f;          // -1..1
                float x = t * width;
                float y = -(1f - t * t) * width * 0.45f; // curve down in the middle
                Part(PrimitiveType.Sphere, parent, center + new Vector3(x, y, 0f), Vector3.one * dot, mat);
            }
        }

        void AddCheeks(Transform parent, Vector3 center, float spacing, float size)
        {
            var cheek = PatchworkAssets.Felt(new Color(1f, 0.62f, 0.68f));
            for (int side = -1; side <= 1; side += 2)
                Part(PrimitiveType.Sphere, parent, center + new Vector3(side * spacing, 0f, 0f), new Vector3(size, size * 0.6f, 0.04f), cheek);
        }

        void BuildHats()
        {
            accessoryRoot.localPosition = new Vector3(0f, FormLibrary.Get(FormType.Yarn).HeadHeight, 0f);
            hats[0] = new GameObject("Hat_None");
            hats[0].transform.SetParent(accessoryRoot, false);

            var beanie = Pivot("Hat_RedBeanie", accessoryRoot, Vector3.zero);
            var red = PatchworkAssets.Yarn(new Color(0.85f, 0.25f, 0.25f));
            Prim(PrimitiveType.Sphere, beanie, new Vector3(0f, -0.04f, -0.02f), new Vector3(0.52f, 0.34f, 0.52f), red);
            Prim(PrimitiveType.Cylinder, beanie, new Vector3(0f, -0.11f, -0.02f), new Vector3(0.54f, 0.05f, 0.54f), PatchworkAssets.Yarn(new Color(0.95f, 0.9f, 0.8f)));
            Prim(PrimitiveType.Sphere, beanie, new Vector3(0f, 0.17f, -0.02f), Vector3.one * 0.16f, PatchworkAssets.Yarn(new Color(0.95f, 0.9f, 0.8f)));
            hats[1] = beanie.gameObject;

            var topHat = Pivot("Hat_FeltTopHat", accessoryRoot, Vector3.zero);
            var green = PatchworkAssets.Felt(new Color(0.25f, 0.5f, 0.35f));
            Prim(PrimitiveType.Cylinder, topHat, new Vector3(0f, -0.06f, 0f), new Vector3(0.56f, 0.02f, 0.56f), green);
            Prim(PrimitiveType.Cylinder, topHat, new Vector3(0f, 0.11f, 0f), new Vector3(0.32f, 0.16f, 0.32f), green);
            Prim(PrimitiveType.Cylinder, topHat, new Vector3(0f, 0.01f, 0f), new Vector3(0.33f, 0.03f, 0.33f), PatchworkAssets.Felt(new Color(0.95f, 0.8f, 0.3f)));
            hats[2] = topHat.gameObject;

            var daisy = Pivot("Hat_Daisy", accessoryRoot, Vector3.zero);
            var petal = PatchworkAssets.Felt(Color.white);
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI / 3f;
                Prim(PrimitiveType.Sphere, daisy, new Vector3(0.2f + Mathf.Cos(a) * 0.1f, -0.08f + Mathf.Sin(a) * 0.1f, 0.12f), new Vector3(0.1f, 0.1f, 0.04f), petal);
            }
            Prim(PrimitiveType.Sphere, daisy, new Vector3(0.2f, -0.08f, 0.14f), new Vector3(0.09f, 0.09f, 0.05f), PatchworkAssets.Felt(new Color(1f, 0.8f, 0.2f)));
            hats[3] = daisy.gameObject;
        }

        /// <summary>Visual-only primitive (collider removed, player layer).</summary>
        public static Transform Prim(PrimitiveType type, Transform parent, Vector3 localPos, Vector3 localScale, Material mat, Vector3 euler = default(Vector3))
        {
            var go = GameObject.CreatePrimitive(type);
            var col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
            go.layer = 2;
            var t = go.transform;
            t.SetParent(parent, false);
            t.localPosition = localPos;
            t.localRotation = Quaternion.Euler(euler);
            t.localScale = localScale;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return t;
        }
    }
}

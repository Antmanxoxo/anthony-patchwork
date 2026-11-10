using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // Builds Patch's three bodies and animates them:
    // Yarn is the Blender-made knitted toy (PatchModel), with a block-built backup if it's missing,
    // Water is a little water sprite in a raincoat and wellies, Wind is a floating happy cloud.
    // Also does the short change animations between forms (soak, steam, rain, unravel...).
    // Hats sit on their own transform so changing form never resets them.
    public class FormVisuals
    {
        public static readonly string[] HatNames = { "No Hat", "Red Beanie", "Felt Top Hat", "Daisy Clip", "Golden Crown", "Party Hat", "Pirate Hat", "Propeller Cap", "Starry Wizard Hat" };

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
        readonly GameObject[] hats = new GameObject[9];
        static MaterialPropertyBlock mpb;

        // Yarn rig (the Blender model if it's there, otherwise the block-built toy)
        PatchModel patchModel;
        Transform yarnTorso, yarnHead, hipL, hipR, shoulderL, shoulderR, armR, handR;
        // Water rig
        Transform wHipL, wHipR, wArmL, wArmR, wBody, wHead;
        float waterPhase;
        Transform bubble, bubbleEyes;
        Vector3 bubbleEyesBase;
        readonly Transform[] miniBubbles = new Transform[2];
        // Happy / sad / angry faces for Water and Wind (Yarn's are on the Blender model).
        readonly GameObject[] waterFaces = new GameObject[3], windFaces = new GameObject[3];
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
            // The hat is placed after the final pose each frame, so it never trails behind his head.
            if (patchModel != null) patchModel.PoseApplied += PlaceHatOnHead;
            ShowFace(PatchModel.CurrentFace);
        }

        // ------------------------------------------------------------------ API

        // World position of Yarn's right hand (where the tether starts).
        public Vector3 HandPosition
        {
            get { return handR != null ? handR.position : root.position + Vector3.up; }
        }

        // Runs the callback each frame after Patch's final pose (only when the Blender model is loaded).
        public void OnPoseApplied(System.Action callback)
        {
            if (patchModel != null) patchModel.PoseApplied += callback;
        }

        // Instantly show a form (used at spawn).
        public void SetForm(FormType newForm, bool punchIt)
        {
            EndTransition();
            form = newForm;
            ShowOnly(form);
            if (punchIt) PunchScale(new Vector3(0.6f, 1.4f, 0.6f));
        }

        // Plays the element-specific transformation from one body to the other.
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

        // Dance easter egg. DanceBeat is the song position in beats, set every frame.
        public float DanceBeat;
        bool dancing;
        float danceWeight;

        public void SetDancing(bool on)
        {
            dancing = on;
            if (patchModel != null) patchModel.Dancing = on;
        }

        public void SetTether(bool on, Vector3 target)
        {
            tethered = on;
            tetherTarget = target;
            if (patchModel != null) patchModel.SetTether(on, target);
        }

        // Holding something (grab): his arms reach for it.
        public void SetGrip(bool on, Vector3 target)
        {
            if (patchModel != null) patchModel.SetGrip(on, target);
        }

        // Patch's face (happy / sad / angry) on every form. Kept when he changes form.
        public void SetFace(PatchModel.Face face)
        {
            if (patchModel != null) patchModel.SetFace(face);
            else PatchModel.CurrentFace = face;
            ShowFace(face);
        }

        void ShowFace(PatchModel.Face face)
        {
            for (int i = 0; i < 3; i++)
            {
                if (waterFaces[i] != null) waterFaces[i].SetActive(i == (int)face);
                if (windFaces[i] != null) windFaces[i].SetActive(i == (int)face);
            }
        }

        public void PunchScale(Vector3 s) { punch = s; }

        // How high Patch sits above his feet (riding a yarn horse); eased.
        public float RideHeight;
        float rideY;

        // Put on a different hat with a springy little pop.
        public void ChangeHat(int index) { SetHat(index); hatPop = 1f; }
        public Vector3 HatPosition { get { return accessoryRoot.position; } }
        float hatPop;
        float HatPopScale { get { return 1f + Mathf.Sin(hatPop * Mathf.PI * 3f) * 0.22f * hatPop; } }

        // Hides Patch (flickering after a hit, or while he unravels). Remembers which parts were
        // already switched off so they stay off afterwards.
        Renderer[] hideCache;
        bool[] hideWas;
        bool hidden;
        public void SetHidden(bool h)
        {
            if (h == hidden) return;
            hidden = h;
            if (h)
            {
                hideCache = root.GetComponentsInChildren<Renderer>(true);
                hideWas = new bool[hideCache.Length];
                for (int i = 0; i < hideCache.Length; i++)
                {
                    hideWas[i] = hideCache[i].enabled;
                    hideCache[i].enabled = false;
                }
            }
            else if (hideCache != null)
            {
                for (int i = 0; i < hideCache.Length; i++)
                    if (hideCache[i] != null) hideCache[i].enabled = hideWas[i];
            }
        }

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

            root.localScale = punch;
            root.localRotation = Quaternion.identity;

            danceWeight = Mathf.MoveTowards(danceWeight, dancing ? 1f : 0f, dt * 3f);
            if (danceWeight > 0.001f) ApplyDance(DanceBeat, danceWeight);
            else root.localPosition = Vector3.zero;
            // Sitting up on a yarn horse.
            rideY = Mathf.MoveTowards(rideY, RideHeight, dt * 5f);
            if (rideY > 0f) root.localPosition += Vector3.up * rideY;

            AnimateYarn(motor, speed, localVel, dt);
            AnimateWater(motor, dv, dt);
            AnimateWind(motor, dt);
            if (transitioning) TickTransition(dt);

            // Hat follows the head of the current body (the cloud floats, so its head moves).
            // Patch's Blender model places it after his pose instead (PlaceHatOnHead).
            hatPop = Mathf.MoveTowards(hatPop, 0f, dt * 2.2f);
            if (form == FormType.Yarn && patchModel != null) return;
            float hatY = FormLibrary.Get(form).HeadHeight;
            Quaternion hatTilt = Quaternion.identity;
            if (form == FormType.Wind && cloud != null) { hatY += cloud.localPosition.y; hatTilt = cloud.localRotation; }
            if (form == FormType.Yarn && yarnHead != null) { hatY += yarnTorso.localPosition.y; hatTilt = yarnHead.localRotation; }
            if (form == FormType.Water && bubble != null) hatY *= bubble.localScale.y;
            accessoryRoot.localPosition = new Vector3(0f, hatY, 0f);
            accessoryRoot.localRotation = hatTilt;
            accessoryRoot.localScale = Vector3.one * HatPopScale;
        }

        void PlaceHatOnHead()
        {
            if (form != FormType.Yarn || patchModel == null) return;
            Vector3 pos;
            Quaternion rot;
            float fit;
            patchModel.HatPose(root, out pos, out rot, out fit);
            accessoryRoot.localPosition = pos;
            accessoryRoot.localRotation = rot;
            accessoryRoot.localScale = Vector3.one * (fit * HatPopScale);
        }

        void AnimateYarn(PlayerMotor motor, float speed, Vector3 localVel, float dt)
        {
            if (!yarn.Root.gameObject.activeSelf) return;
            if (patchModel != null) { patchModel.Tick(motor, dt); return; }
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

        // Each form has its own dance, moving the whole body in time with its song.
        void ApplyDance(float b, float w)
        {
            Vector3 pos = Vector3.zero, scale = Vector3.one;
            Vector3 euler = Vector3.zero;
            float frac = b - Mathf.Floor(b);
            switch (form)
            {
                case FormType.Water:
                {   // Jelly bounce: a hop every beat, squashing flat on each landing, wiggling side to side.
                    float hop = Mathf.Abs(Mathf.Sin(Mathf.PI * b));
                    float squash = Mathf.Pow(Mathf.Max(0f, Mathf.Cos(Mathf.PI * 2f * b)), 4f);
                    pos.y = hop * 0.35f;
                    scale = new Vector3(1f + 0.25f * squash, 1f - 0.25f * squash + 0.12f * hop, 1f + 0.25f * squash);
                    euler.y = Mathf.Sin(Mathf.PI * b * 0.5f) * 40f;
                    euler.z = Mathf.Sin(Mathf.PI * b) * 12f;
                    // Every 8 beats: a big hop with a full spin.
                    if (Mathf.Repeat(b, 8f) >= 7f) { pos.y += Mathf.Sin(Mathf.PI * frac) * 0.6f; euler.y += frac * 360f; }
                    break;
                }
                case FormType.Wind:
                {   // Floaty waltz: twirls round and round, drifting in a figure of eight.
                    euler.y = b * 120f;
                    euler.z = Mathf.Cos(Mathf.PI * b / 1.5f) * 15f;
                    pos = new Vector3(Mathf.Sin(Mathf.PI * b / 3f) * 0.6f, 0.25f + Mathf.Sin(Mathf.PI * b / 1.5f) * 0.18f,
                                      Mathf.Sin(Mathf.PI * b * 2f / 3f) * 0.3f);
                    float swell = 1f + 0.08f * Mathf.Sin(Mathf.PI * 2f * b / 3f);
                    scale = new Vector3(swell, 1f / swell, swell);
                    break;
                }
                default:
                {   // Hoedown: hops on the beat and sways, with a spin every fourth beat.
                    pos.y = Mathf.Abs(Mathf.Sin(Mathf.PI * b)) * 0.2f;
                    euler.z = Mathf.Sin(Mathf.PI * b) * 10f;
                    if (Mathf.Repeat(b, 4f) >= 3f) euler.y = Mathf.SmoothStep(0f, 1f, frac) * 360f;
                    else euler.y = Mathf.Sin(Mathf.PI * b * 0.5f) * 20f;
                    break;
                }
            }
            root.localPosition = pos * w;
            root.localRotation = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(euler), w) * root.localRotation;
            root.localScale = Vector3.Scale(root.localScale, Vector3.Lerp(Vector3.one, scale, w));
        }

        void AnimateWater(PlayerMotor motor, Vector3 dv, float dt)
        {
            if (!water.Root.gameObject.activeSelf) return;
            // Still water at heart: the whole doll wobbles when it's knocked about, and
            // stretches a little as it rises and falls.
            jiggle = Mathf.Min(0.2f, jiggle + dv.magnitude * 0.02f);
            jiggle = Mathf.MoveTowards(jiggle, 0.02f, dt * 0.4f);
            float w = Mathf.Sin(time * 13f) * jiggle;
            float w2 = Mathf.Sin(time * 9f + 1.3f) * jiggle * 0.6f;
            float stretch = Mathf.Clamp(motor.Velocity.y * 0.012f, -0.1f, 0.12f);
            bubble.localScale = new Vector3(1f + w - stretch * 0.5f, 1f - w + stretch, 1f + w2 - stretch * 0.5f);

            // A quick little walk: short legs, so lots of steps, arms swinging the other way.
            Vector3 flat = motor.Velocity;
            flat.y = 0f;
            float speed = flat.magnitude;
            bool air = !motor.Grounded;
            float stride = Mathf.Clamp01(speed / 4.5f);
            waterPhase += speed * dt * 4.4f;
            float leg = air ? 0f : Mathf.Sin(waterPhase) * 44f * stride;
            float tuck = air ? (motor.Velocity.y > 0f ? 30f : -12f) : 0f;
            float ease = 1f - Mathf.Exp(-18f * dt);
            wHipL.localRotation = Quaternion.Slerp(wHipL.localRotation, Quaternion.Euler(leg + tuck, 0f, 0f), ease);
            wHipR.localRotation = Quaternion.Slerp(wHipR.localRotation, Quaternion.Euler(-leg + tuck * 0.6f, 0f, 0f), ease);
            // Arms: swinging on the ground, thrown up and out in the air.
            float lift = air ? (motor.Velocity.y > 0f ? 115f : 70f) : 14f + Mathf.Sin(time * 1.6f) * 3f;
            wArmL.localRotation = Quaternion.Slerp(wArmL.localRotation, Quaternion.Euler(-leg * 0.8f, 0f, -lift), ease);
            wArmR.localRotation = Quaternion.Slerp(wArmR.localRotation, Quaternion.Euler(leg * 0.8f, 0f, lift), ease);
            // The body bobs with each step and leans into a run; breathing when stood still.
            float bob = air ? 0f : Mathf.Abs(Mathf.Sin(waterPhase)) * 0.028f * stride + Mathf.Sin(time * 2.6f) * 0.006f;
            wBody.localPosition = new Vector3(0f, 0.2f + bob, 0f);
            wBody.localRotation = Quaternion.Euler(stride * 9f, 0f, Mathf.Sin(waterPhase) * 3f * stride);
            // The head lolls a touch behind the body, like a full water balloon.
            wHead.localRotation = Quaternion.Euler(-stride * 5f + Mathf.Sin(time * 1.7f) * 2.5f + w * 30f, Mathf.Sin(time * 0.6f) * 6f * (1f - stride), -Mathf.Sin(waterPhase) * 5f * stride + w2 * 30f);
            wHead.localScale = new Vector3(1f + w * 0.6f, 1f - w * 0.6f, 1f + w2 * 0.6f);

            bubbleEyes.localPosition = bubbleEyesBase + new Vector3(0f, w * 0.04f, 0f);
            // Blink every few seconds.
            float blink = Mathf.Repeat(time, 3.7f) < 0.12f ? 0.15f : 1f;
            bubbleEyes.localScale = new Vector3(1f, blink, 1f);
            for (int i = 0; i < miniBubbles.Length; i++)
            {
                float a = time * (1.5f + i * 0.7f) + i * 3f;
                miniBubbles[i].localPosition = new Vector3(Mathf.Cos(a) * 0.36f, 0.5f + Mathf.Sin(time * 2f + i) * 0.2f, Mathf.Sin(a) * 0.36f);
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
                {   // Yarn / Wind: unravelling / spinning up.
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
                    // A quick cloud puff covers the moment the bodies swap.
                    FxBurst.Spawn(FxBurst.Kind.Poof, feet + Vector3.up * (toForm == FormType.Water ? 0.45f : 0.7f), def.PrimaryColor);
                    switch (toForm)
                    {
                        case FormType.Water: FxBurst.Spawn(FxBurst.Kind.Splash, feet + Vector3.up * 0.3f, def.PrimaryColor); break;
                        case FormType.Wind: FxBurst.Spawn(FxBurst.Kind.Puff, feet + Vector3.up * 0.8f, Color.white); break;
                        default: FxBurst.Spawn(FxBurst.Kind.Scraps, feet + Vector3.up * 0.8f, def.PrimaryColor); break;
                    }
                    Sfx.Play(SfxId.Transform, 0.55f);
                }
                float k2 = Mathf.Clamp01((transT - Phase1) / Phase2);
                // Back-out overshoot: 0 -> 1.15 -> 1
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

            patchModel = PatchModel.Create(yarn.Root);
            if (patchModel != null)
            {
                foreach (var r in patchModel.Renderers) { yarn.Renderers.Add(r); yarn.Colors.Add(Color.white); }
                handR = patchModel.HandR;
                building = null;
                return;
            }

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

        // Water: a little water sprite, a doll like Patch but made of rain. A big jelly
        // water-drop head with a curl of droplets on top, a felt raincoat with two buttons and
        // a pocket, red wellies, and round water hands. Short enough to slip through a drain.
        void BuildWater()
        {
            var def = FormLibrary.Get(FormType.Water);
            building = water;
            water.Root = Pivot("WaterForm", root, Vector3.zero);
            bubble = Pivot("Bubble", water.Root, Vector3.zero);     // the whole doll wobbles from the feet

            var skin = PatchworkAssets.Plain(Color.Lerp(def.PrimaryColor, def.SecondaryColor, 0.35f), 0.9f);
            var shell = PatchworkAssets.Bubble(new Color(def.PrimaryColor.r, def.PrimaryColor.g, def.PrimaryColor.b, 0.42f));
            var coat = PatchworkAssets.Felt(new Color(1f, 0.82f, 0.32f));
            var coatDark = PatchworkAssets.Felt(new Color(0.92f, 0.68f, 0.22f));
            var boot = PatchworkAssets.Plain(new Color(0.92f, 0.3f, 0.32f), 0.85f);
            var shine = PatchworkAssets.Glow(Color.white, 0.8f);
            var white = PatchworkAssets.Plain(new Color(0.98f, 0.98f, 1f), 0.85f);
            var black = PatchworkAssets.Plain(new Color(0.05f, 0.07f, 0.14f), 0.85f);

            // Legs (they swing from the hip): a short water leg in a shiny red welly.
            wHipL = Pivot("HipL", bubble, new Vector3(-0.085f, 0.2f, 0f));
            wHipR = Pivot("HipR", bubble, new Vector3(0.085f, 0.2f, 0f));
            foreach (var hip in new[] { wHipL, wHipR })
            {
                Part(PrimitiveType.Cylinder, hip, new Vector3(0f, -0.05f, 0f), new Vector3(0.085f, 0.06f, 0.085f), skin);
                Part(PrimitiveType.Cylinder, hip, new Vector3(0f, -0.12f, 0f), new Vector3(0.115f, 0.05f, 0.115f), boot);
                Part(PrimitiveType.Sphere, hip, new Vector3(0f, -0.165f, 0.03f), new Vector3(0.13f, 0.075f, 0.19f), boot);
                Part(PrimitiveType.Cylinder, hip, new Vector3(0f, -0.075f, 0f), new Vector3(0.125f, 0.012f, 0.125f), white);
            }

            // The raincoat: a little felt bell, wider at the hem, with a collar, two buttons,
            // a seam down the front and a pocket.
            wBody = Pivot("Body", bubble, new Vector3(0f, 0.2f, 0f));
            Part(PrimitiveType.Sphere, wBody, new Vector3(0f, 0.11f, 0f), new Vector3(0.36f, 0.3f, 0.32f), coat);
            Part(PrimitiveType.Cylinder, wBody, new Vector3(0f, 0.02f, 0f), new Vector3(0.39f, 0.035f, 0.35f), coat);
            Part(PrimitiveType.Cylinder, wBody, new Vector3(0f, -0.012f, 0f), new Vector3(0.4f, 0.012f, 0.36f), coatDark);
            Part(PrimitiveType.Cylinder, wBody, new Vector3(0f, 0.235f, 0f), new Vector3(0.26f, 0.025f, 0.24f), coatDark);
            Part(PrimitiveType.Cube, wBody, new Vector3(0f, 0.11f, 0.158f), new Vector3(0.012f, 0.2f, 0.01f), coatDark);
            for (int i = 0; i < 2; i++)
                Part(PrimitiveType.Cylinder, wBody, new Vector3(0.035f, 0.07f + i * 0.085f, 0.157f - i * 0.006f), new Vector3(0.04f, 0.008f, 0.04f), boot, new Vector3(90f, 0f, 0f));
            Part(PrimitiveType.Cube, wBody, new Vector3(-0.085f, 0.055f, 0.146f), new Vector3(0.075f, 0.06f, 0.012f), coatDark, new Vector3(0f, -22f, 0f));

            // Arms (they swing from the shoulder): a raincoat sleeve and a round water hand.
            wArmL = Pivot("ArmL", wBody, new Vector3(-0.17f, 0.2f, 0f));
            wArmR = Pivot("ArmR", wBody, new Vector3(0.17f, 0.2f, 0f));
            foreach (var arm in new[] { wArmL, wArmR })
            {
                Part(PrimitiveType.Cylinder, arm, new Vector3(0f, -0.075f, 0f), new Vector3(0.095f, 0.075f, 0.095f), coat);
                Part(PrimitiveType.Cylinder, arm, new Vector3(0f, -0.15f, 0f), new Vector3(0.105f, 0.012f, 0.105f), coatDark);
                Part(PrimitiveType.Sphere, arm, new Vector3(0f, -0.195f, 0f), Vector3.one * 0.095f, skin);
            }

            // The head: a big drop of water. A see-through shell over a paler middle gives it
            // some depth, like jelly, and a curl of droplets stands up on top like a quiff.
            Vector3 headCenter = new Vector3(0f, 0.2f, 0f);          // relative to the head pivot
            const float headRadius = 0.225f;
            wHead = Pivot("Head", wBody, new Vector3(0f, 0.2f, 0f));
            Part(PrimitiveType.Sphere, wHead, headCenter + new Vector3(0f, -0.005f, -0.01f), Vector3.one * (headRadius * 2f - 0.07f), skin);
            Part(PrimitiveType.Sphere, wHead, headCenter, new Vector3(headRadius * 2f, headRadius * 1.94f, headRadius * 2f), shell);
            Part(PrimitiveType.Sphere, wHead, headCenter + new Vector3(0.01f, 0.215f, -0.01f), new Vector3(0.15f, 0.17f, 0.15f), skin);
            Part(PrimitiveType.Sphere, wHead, headCenter + new Vector3(0.035f, 0.305f, -0.015f), new Vector3(0.095f, 0.11f, 0.095f), skin, new Vector3(0f, 0f, -25f));
            Part(PrimitiveType.Sphere, wHead, headCenter + new Vector3(0.075f, 0.36f, -0.02f), Vector3.one * 0.055f, skin);
            // Shiny highlights.
            Part(PrimitiveType.Sphere, wHead, headCenter + new Vector3(-0.1f, 0.12f, 0.15f), new Vector3(0.085f, 0.05f, 0.03f), shine, new Vector3(0f, -25f, 30f));
            Part(PrimitiveType.Sphere, wHead, headCenter + new Vector3(-0.145f, 0.07f, 0.14f), Vector3.one * 0.025f, shine);

            // Big friendly eyes sitting ON the surface of the head so they stay crisp. They blink.
            const float eyeHeight = 0.035f;
            bubbleEyes = Pivot("Eyes", wHead, headCenter + Vector3.up * eyeHeight);
            bubbleEyesBase = bubbleEyes.localPosition;
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 n = new Vector3(side * 0.4f, eyeHeight / headRadius, 1f).normalized;
                Vector3 local = n * (headRadius - 0.012f) - Vector3.up * eyeHeight;
                Vector3 euler = Quaternion.LookRotation(n, Vector3.up).eulerAngles;
                Part(PrimitiveType.Sphere, bubbleEyes, local, new Vector3(0.105f, 0.135f, 0.05f), white, euler);
                Vector3 inward = new Vector3(-side * 0.008f, -0.006f, 0f);
                Part(PrimitiveType.Sphere, bubbleEyes, local + n * 0.016f + inward, new Vector3(0.068f, 0.092f, 0.036f), black, euler);
                Part(PrimitiveType.Sphere, bubbleEyes, local + n * 0.031f + new Vector3(0.016f, 0.022f, 0f), new Vector3(0.026f, 0.026f, 0.012f), shine, euler);
                // A rosy cheek under each eye.
                Vector3 cn = new Vector3(side * 0.62f, -0.3f, 1f).normalized;
                Part(PrimitiveType.Sphere, wHead, headCenter + cn * (headRadius - 0.004f), new Vector3(0.06f, 0.035f, 0.02f), PatchworkAssets.Felt(new Color(1f, 0.62f, 0.72f)), Quaternion.LookRotation(cn, Vector3.up).eulerAngles);
            }

            // Mouths (and brows for sad / angry): little stitches on the surface of the head.
            // Happy: smile. Sad: frown with brows tilted up in the middle. Angry: flatter frown, brows down.
            var happy = Pivot("Face_Happy", wHead, Vector3.zero);
            WaterMouth(happy, headCenter, headRadius, -0.36f, -1f, black);
            var sad = Pivot("Face_Sad", wHead, Vector3.zero);
            WaterMouth(sad, headCenter, headRadius, -0.46f, 1f, black);
            WaterBrows(sad, headCenter, headRadius, -1f, black);
            var angry = Pivot("Face_Angry", wHead, Vector3.zero);
            WaterMouth(angry, headCenter, headRadius, -0.42f, 0.5f, black);
            WaterBrows(angry, headCenter, headRadius, 1f, black);
            waterFaces[0] = happy.gameObject;
            waterFaces[1] = sad.gameObject;
            waterFaces[2] = angry.gameObject;

            var tiny = PatchworkAssets.Bubble(new Color(0.8f, 0.92f, 1f, 0.5f));
            for (int i = 0; i < miniBubbles.Length; i++)
                miniBubbles[i] = Part(PrimitiveType.Sphere, water.Root, Vector3.zero, Vector3.one * (0.07f + i * 0.03f), tiny);
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
            var happy = Pivot("Face_Happy", cloud, Vector3.zero);
            AddMouth(happy, new Vector3(0f, 0.57f, 0.4f), 0.1f, black, 0.03f, 1f);
            var sad = Pivot("Face_Sad", cloud, Vector3.zero);
            AddMouth(sad, new Vector3(0f, 0.53f, 0.4f), 0.09f, black, 0.03f, -1f);
            CloudBrows(sad, -1f, black);
            var angry = Pivot("Face_Angry", cloud, Vector3.zero);
            AddMouth(angry, new Vector3(0f, 0.55f, 0.4f), 0.1f, black, 0.03f, -0.5f);
            CloudBrows(angry, 1f, black);
            windFaces[0] = happy.gameObject;
            windFaces[1] = sad.gameObject;
            windFaces[2] = angry.gameObject;

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
            AddMouth(parent, center, width, mat, dot, 1f);
        }

        // A curved line of dots. bend 1 = smile, -1 = frown, in between = flatter.
        void AddMouth(Transform parent, Vector3 center, float width, Material mat, float dot, float bend)
        {
            for (int i = 0; i < 5; i++)
            {
                float t = (i / 4f) * 2f - 1f;          // -1..1
                float x = t * width;
                float y = -bend * (1f - t * t) * width * 0.45f;
                Part(PrimitiveType.Sphere, parent, center + new Vector3(x, y, 0f), Vector3.one * dot, mat);
            }
        }

        // Mouth dots placed on the bubble's round surface. curve -1 = smile, +1 = frown.
        void WaterMouth(Transform parent, Vector3 center, float radius, float baseY, float curve, Material mat)
        {
            for (int i = 0; i < 5; i++)
            {
                float t = (i / 4f) * 2f - 1f;
                Vector3 n = new Vector3(t * 0.2f, baseY + curve * (1f - t * t) * 0.1f, 1f).normalized;
                Part(PrimitiveType.Sphere, parent, center + n * (radius - 0.003f), Vector3.one * radius * 0.09f, mat);
            }
        }

        // Eyebrows over the water doll's eyes. slope 1 = angry (inner ends down), -1 = sad (inner ends up).
        void WaterBrows(Transform parent, Vector3 center, float radius, float slope, Material mat)
        {
            for (int side = -1; side <= 1; side += 2)
                for (int i = 0; i < 4; i++)
                {
                    float t = i / 3f;   // 0 = inner end, 1 = outer end
                    Vector3 n = new Vector3(side * (0.2f + t * 0.36f), 0.62f + slope * (t - 0.5f) * 0.18f, 1f).normalized;
                    Part(PrimitiveType.Sphere, parent, center + n * (radius - 0.002f), Vector3.one * radius * 0.095f, mat);
                }
        }

        // Eyebrows over the cloud's eyes. slope 1 = angry, -1 = sad.
        void CloudBrows(Transform parent, float slope, Material mat)
        {
            for (int side = -1; side <= 1; side += 2)
                for (int i = 0; i < 4; i++)
                {
                    float t = i / 3f;
                    var p = new Vector3(side * (0.07f + t * 0.16f), 0.84f + slope * (t - 0.5f) * 0.07f, 0.34f);
                    Part(PrimitiveType.Sphere, parent, p, new Vector3(0.035f, 0.035f, 0.03f), mat);
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

            // Golden crown: a felt-lined gold band with five points and a pink gem.
            var crown = Pivot("Hat_GoldenCrown", accessoryRoot, Vector3.zero);
            var gold = PatchworkAssets.Glow(new Color(1f, 0.8f, 0.25f), 0.25f);
            Prim(PrimitiveType.Cylinder, crown, new Vector3(0f, 0.0f, 0f), new Vector3(0.42f, 0.08f, 0.42f), gold);
            Prim(PrimitiveType.Cylinder, crown, new Vector3(0f, -0.06f, 0f), new Vector3(0.44f, 0.02f, 0.44f), PatchworkAssets.Felt(new Color(0.75f, 0.2f, 0.3f)));
            for (int i = 0; i < 5; i++)
            {
                float a = i * Mathf.PI * 2f / 5f;
                Prim(PrimitiveType.Sphere, crown, new Vector3(Mathf.Sin(a) * 0.19f, 0.12f, Mathf.Cos(a) * 0.19f), new Vector3(0.09f, 0.18f, 0.09f), gold);
                Prim(PrimitiveType.Sphere, crown, new Vector3(Mathf.Sin(a) * 0.19f, 0.22f, Mathf.Cos(a) * 0.19f), Vector3.one * 0.06f, gold);
            }
            Prim(PrimitiveType.Sphere, crown, new Vector3(0f, 0.02f, 0.21f), new Vector3(0.08f, 0.08f, 0.04f), PatchworkAssets.Glow(new Color(1f, 0.4f, 0.6f), 0.6f));
            hats[4] = crown.gameObject;

            // Party hat: a striped cone of stacked felt rings with a pompom on top.
            var party = Pivot("Hat_Party", accessoryRoot, Vector3.zero);
            party.localRotation = Quaternion.Euler(0f, 0f, -12f);
            var stripeA = PatchworkAssets.Felt(new Color(0.95f, 0.4f, 0.55f));
            var stripeB = PatchworkAssets.Felt(new Color(1f, 0.85f, 0.35f));
            for (int i = 0; i < 7; i++)
            {
                float k = i / 7f;
                float r = Mathf.Lerp(0.34f, 0.05f, k);
                Prim(PrimitiveType.Cylinder, party, new Vector3(0f, -0.04f + i * 0.065f, 0f), new Vector3(r, 0.034f, r), i % 2 == 0 ? stripeA : stripeB);
            }
            Prim(PrimitiveType.Sphere, party, new Vector3(0f, 0.43f, 0f), Vector3.one * 0.13f, PatchworkAssets.Felt(new Color(0.55f, 0.8f, 1f)));
            hats[5] = party.gameObject;

            // Pirate hat: a black felt tricorn with a little white skull-and-button badge.
            var pirate = Pivot("Hat_Pirate", accessoryRoot, Vector3.zero);
            var black = PatchworkAssets.Felt(new Color(0.16f, 0.14f, 0.18f));
            Prim(PrimitiveType.Sphere, pirate, new Vector3(0f, 0.02f, 0f), new Vector3(0.46f, 0.26f, 0.46f), black);
            for (int i = 0; i < 3; i++)
            {
                float a = i * 120f;
                var flap = Prim(PrimitiveType.Cube, pirate, Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 0.04f, 0.24f), new Vector3(0.42f, 0.16f, 0.03f), black, new Vector3(-25f, a, 0f));
                flap.name = "Flap";
            }
            Prim(PrimitiveType.Cylinder, pirate, Quaternion.Euler(0f, 0f, 0f) * new Vector3(0f, 0.07f, 0.262f), new Vector3(0.11f, 0.01f, 0.11f), PatchworkAssets.Felt(Color.white), new Vector3(65f, 0f, 0f));
            Prim(PrimitiveType.Cube, pirate, new Vector3(0f, -0.07f, 0f), new Vector3(0.5f, 0.03f, 0.5f), PatchworkAssets.Felt(new Color(0.75f, 0.2f, 0.25f)), new Vector3(0f, 45f, 0f));
            hats[6] = pirate.gameObject;

            // Propeller cap: a quartered cap with a little brim and a spinning propeller.
            var prop = Pivot("Hat_Propeller", accessoryRoot, Vector3.zero);
            Color[] quarters = { new Color(0.95f, 0.35f, 0.35f), new Color(0.35f, 0.6f, 0.95f), new Color(1f, 0.85f, 0.3f), new Color(0.45f, 0.75f, 0.45f) };
            Prim(PrimitiveType.Sphere, prop, new Vector3(0f, -0.04f, 0f), new Vector3(0.5f, 0.32f, 0.5f), PatchworkAssets.Felt(quarters[0]));
            for (int i = 1; i < 4; i++)
                Prim(PrimitiveType.Sphere, prop, new Vector3(0f, -0.04f, 0f), new Vector3(0.505f, 0.325f, 0.505f) - new Vector3(0f, 0f, 0.4f), PatchworkAssets.Felt(quarters[i]), new Vector3(0f, i * 45f, 0f));
            Prim(PrimitiveType.Cylinder, prop, new Vector3(0f, -0.09f, 0.2f), new Vector3(0.3f, 0.012f, 0.2f), PatchworkAssets.Felt(quarters[1]));
            Prim(PrimitiveType.Cylinder, prop, new Vector3(0f, 0.15f, 0f), new Vector3(0.03f, 0.05f, 0.03f), PatchworkAssets.Plain(new Color(0.6f, 0.6f, 0.65f), 0.7f));
            var blades = Pivot("Propeller", prop, new Vector3(0f, 0.2f, 0f));
            Prim(PrimitiveType.Cube, blades, Vector3.zero, new Vector3(0.42f, 0.015f, 0.07f), PatchworkAssets.Felt(quarters[2]), new Vector3(8f, 0f, 0f));
            Prim(PrimitiveType.Cube, blades, Vector3.zero, new Vector3(0.07f, 0.015f, 0.42f), PatchworkAssets.Felt(quarters[0]), new Vector3(0f, 0f, 8f));
            var spin = blades.gameObject.AddComponent<Spinner>();
            spin.Speed = 540f;
            hats[7] = prop.gameObject;

            // Starry wizard hat: a tall, floppy midnight-blue felt cone with a wide brim, stitched
            // stars, a little moon and a bobble that twinkles.
            var wiz = Pivot("Hat_Wizard", accessoryRoot, Vector3.zero);
            var midnight = PatchworkAssets.Felt(new Color(0.2f, 0.22f, 0.5f));
            var midnight2 = PatchworkAssets.Felt(new Color(0.24f, 0.27f, 0.58f));
            Prim(PrimitiveType.Cylinder, wiz, new Vector3(0f, -0.07f, 0f), new Vector3(0.78f, 0.02f, 0.78f), midnight);
            Prim(PrimitiveType.Cylinder, wiz, new Vector3(0f, -0.04f, 0f), new Vector3(0.48f, 0.04f, 0.48f), PatchworkAssets.Felt(new Color(0.95f, 0.8f, 0.3f)));
            Vector3 at = new Vector3(0f, -0.02f, 0f);
            for (int i = 0; i < 9; i++)
            {
                float k = i / 8f;
                float r = Mathf.Lerp(0.46f, 0.07f, k);
                // The top third flops over to one side.
                Vector3 step = k < 0.6f ? new Vector3(0f, 0.08f, 0f) : new Vector3(0.04f + (k - 0.6f) * 0.12f, 0.05f, -0.01f);
                at += step;
                Prim(PrimitiveType.Cylinder, wiz, at, new Vector3(r, 0.045f, r), i % 2 == 0 ? midnight : midnight2, new Vector3(0f, 0f, k < 0.6f ? 0f : -(k - 0.6f) * 110f));
            }
            Prim(PrimitiveType.Sphere, wiz, at + new Vector3(0.05f, 0.03f, 0f), Vector3.one * 0.09f, PatchworkAssets.Glow(new Color(1f, 0.9f, 0.5f), 1.6f));
            var starMat = PatchworkAssets.Glow(new Color(1f, 0.88f, 0.4f), 1.2f);
            Vector3[] stars = { new Vector3(0.2f, 0.1f, 0.15f), new Vector3(-0.17f, 0.22f, 0.14f), new Vector3(0.05f, 0.38f, 0.12f), new Vector3(-0.08f, 0.08f, -0.22f) };
            foreach (var s in stars)
            {
                Prim(PrimitiveType.Cube, wiz, s, new Vector3(0.06f, 0.06f, 0.015f), starMat, new Vector3(0f, Mathf.Atan2(s.x, s.z) * Mathf.Rad2Deg, 45f));
                Prim(PrimitiveType.Cube, wiz, s, new Vector3(0.06f, 0.06f, 0.015f), starMat, new Vector3(0f, Mathf.Atan2(s.x, s.z) * Mathf.Rad2Deg, 0f));
            }
            Prim(PrimitiveType.Sphere, wiz, new Vector3(0.22f, 0.24f, -0.02f), new Vector3(0.1f, 0.1f, 0.02f), PatchworkAssets.Glow(new Color(0.95f, 0.95f, 1f), 1.1f), new Vector3(0f, 90f, 0f));
            hats[8] = wiz.gameObject;
        }

        // Visual-only primitive (collider removed, player layer).
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

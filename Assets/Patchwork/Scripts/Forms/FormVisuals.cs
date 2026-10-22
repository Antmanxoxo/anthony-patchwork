using UnityEngine;

namespace Patchwork
{
    /// <summary>
    /// Builds and animates the three handmade bodies (yarn ball, water droplet, wind puff) and the
    /// cosmetic accessory slot. Cosmetics live on their own transform, so changing form never
    /// removes or resets them.
    /// </summary>
    public class FormVisuals
    {
        public static readonly string[] HatNames = { "No Hat", "Red Beanie", "Felt Top Hat", "Daisy Clip" };

        readonly Transform root;        // squash/stretch pivot (at the feet)
        readonly Transform yarnBody, waterBody, windBody;
        readonly Transform yarnBall;
        readonly Transform[] windWisps = new Transform[3];
        readonly Transform accessoryRoot;
        readonly GameObject[] hats = new GameObject[4];

        FormType form;
        Vector3 scale = Vector3.one;
        Vector3 targetBaseScale = Vector3.one;
        float time;
        float headHeight = 1.38f;
        bool gliding;

        public FormVisuals(Transform player)
        {
            root = new GameObject("Visuals").transform;
            root.SetParent(player, false);

            yarnBody = BuildYarn(out yarnBall);
            waterBody = BuildWater();
            windBody = BuildWind();
            accessoryRoot = new GameObject("Accessories").transform;
            accessoryRoot.SetParent(root, false);
            BuildHats();
            SetHat(GameState.HatIndex);
        }

        // ------------------------------------------------------------------ API

        public void SetForm(FormType newForm, bool punch)
        {
            form = newForm;
            yarnBody.gameObject.SetActive(form == FormType.Yarn);
            waterBody.gameObject.SetActive(form == FormType.Water);
            windBody.gameObject.SetActive(form == FormType.Wind);
            headHeight = FormLibrary.Get(form).HeadHeight;
            if (punch) PunchScale(new Vector3(0.4f, 1.6f, 0.4f));
        }

        public void SetHat(int index)
        {
            index = ((index % hats.Length) + hats.Length) % hats.Length;
            for (int i = 0; i < hats.Length; i++)
                if (hats[i] != null) hats[i].SetActive(i == index);
        }

        public void SetGliding(bool on)
        {
            gliding = on;
        }

        public void PunchScale(Vector3 s)
        {
            scale = s;
        }

        public void Tick(PlayerMotor motor, float dt)
        {
            time += dt;
            scale = Vector3.Lerp(scale, Vector3.one, 1f - Mathf.Exp(-9f * dt));

            targetBaseScale = Vector3.one;
            if (form == FormType.Wind && gliding) targetBaseScale = new Vector3(1.3f, 0.75f, 1.3f);

            Vector3 s = Vector3.Scale(scale, targetBaseScale);
            float bob = motor.Grounded ? Mathf.Sin(time * 3f) * 0.02f : 0f;

            if (form == FormType.Water)
            {
                float w = Mathf.Sin(time * 9f) * 0.05f;
                s = Vector3.Scale(s, new Vector3(1f + w, 1f - w, 1f + w));
            }

            root.localScale = s;
            root.localPosition = new Vector3(0f, bob, 0f);

            // Yarn ball rolls with ground speed.
            if (form == FormType.Yarn && yarnBall != null)
            {
                Vector3 local = motor.transform.InverseTransformDirection(motor.Velocity);
                float spin = local.z / 0.55f * Mathf.Rad2Deg * dt;
                yarnBall.Rotate(Vector3.right, spin, Space.Self);
            }

            // Wind wisps orbit.
            if (form == FormType.Wind)
            {
                for (int i = 0; i < windWisps.Length; i++)
                {
                    float a = time * (gliding ? 6f : 2.5f) + i * Mathf.PI * 2f / 3f;
                    windWisps[i].localPosition = new Vector3(Mathf.Cos(a) * 0.75f, 0.45f + i * 0.25f + Mathf.Sin(time * 2f + i) * 0.05f, Mathf.Sin(a) * 0.75f);
                }
            }

            Vector3 ap = accessoryRoot.localPosition;
            ap.y = Mathf.Lerp(ap.y, headHeight, 1f - Mathf.Exp(-12f * dt));
            accessoryRoot.localPosition = ap;
        }

        // ------------------------------------------------------------------ Builders

        Transform BuildYarn(out Transform ball)
        {
            var def = FormLibrary.Get(FormType.Yarn);
            var body = new GameObject("YarnForm").transform;
            body.SetParent(root, false);

            ball = Prim(PrimitiveType.Sphere, body, new Vector3(0f, 0.62f, 0f), Vector3.one * 1.12f, PatchworkAssets.Yarn(def.PrimaryColor));
            // Stitched fabric patch on the belly, with a cross-stitch.
            Prim(PrimitiveType.Cube, body, new Vector3(0.18f, 0.48f, 0.5f), new Vector3(0.32f, 0.3f, 0.06f), PatchworkAssets.Fabric(new Color(1f, 0.85f, 0.55f)), new Vector3(0f, 18f, 8f));
            Prim(PrimitiveType.Cube, body, new Vector3(-0.3f, 0.95f, 0.38f), new Vector3(0.2f, 0.035f, 0.03f), PatchworkAssets.Felt(new Color(0.45f, 0.15f, 0.25f)), new Vector3(0f, -35f, 45f));
            Prim(PrimitiveType.Cube, body, new Vector3(-0.3f, 0.95f, 0.38f), new Vector3(0.2f, 0.035f, 0.03f), PatchworkAssets.Felt(new Color(0.45f, 0.15f, 0.25f)), new Vector3(0f, -35f, -45f));
            // Loose thread tail curling off the back.
            Prim(PrimitiveType.Cylinder, body, new Vector3(0f, 0.25f, -0.62f), new Vector3(0.05f, 0.22f, 0.05f), PatchworkAssets.Yarn(def.PrimaryColor), new Vector3(60f, 0f, 0f));
            Prim(PrimitiveType.Cylinder, body, new Vector3(0.1f, 0.08f, -0.88f), new Vector3(0.05f, 0.14f, 0.05f), PatchworkAssets.Yarn(def.PrimaryColor), new Vector3(90f, 30f, 0f));
            // Felt feet.
            Prim(PrimitiveType.Sphere, body, new Vector3(-0.24f, 0.07f, 0.1f), new Vector3(0.3f, 0.16f, 0.4f), PatchworkAssets.Felt(def.SecondaryColor));
            Prim(PrimitiveType.Sphere, body, new Vector3(0.24f, 0.07f, 0.1f), new Vector3(0.3f, 0.16f, 0.4f), PatchworkAssets.Felt(def.SecondaryColor));
            AddFace(body, 0.82f, 0.52f, 0.17f, 0.15f);
            return body;
        }

        Transform BuildWater()
        {
            var def = FormLibrary.Get(FormType.Water);
            var body = new GameObject("WaterForm").transform;
            body.SetParent(root, false);
            var mat = PatchworkAssets.Water(def.PrimaryColor);
            Prim(PrimitiveType.Sphere, body, new Vector3(0f, 0.34f, 0f), new Vector3(0.74f, 0.66f, 0.74f), mat);
            Prim(PrimitiveType.Sphere, body, new Vector3(0f, 0.62f, 0f), new Vector3(0.42f, 0.42f, 0.42f), mat);
            Prim(PrimitiveType.Sphere, body, new Vector3(0f, 0.8f, -0.02f), new Vector3(0.2f, 0.3f, 0.2f), mat);
            // Bright highlight.
            Prim(PrimitiveType.Sphere, body, new Vector3(-0.17f, 0.52f, 0.24f), new Vector3(0.1f, 0.14f, 0.06f), PatchworkAssets.Glow(Color.white, 0.6f));
            // Little splash ring at the base.
            Prim(PrimitiveType.Cylinder, body, new Vector3(0f, 0.02f, 0f), new Vector3(0.95f, 0.01f, 0.95f), PatchworkAssets.Water(def.SecondaryColor));
            AddFace(body, 0.44f, 0.36f, 0.11f, 0.1f);
            return body;
        }

        Transform BuildWind()
        {
            var def = FormLibrary.Get(FormType.Wind);
            var body = new GameObject("WindForm").transform;
            body.SetParent(root, false);
            var mat = PatchworkAssets.Wind(def.PrimaryColor);
            Prim(PrimitiveType.Sphere, body, new Vector3(0f, 0.72f, 0f), Vector3.one * 0.95f, mat);
            Prim(PrimitiveType.Sphere, body, new Vector3(-0.45f, 0.58f, -0.05f), Vector3.one * 0.6f, mat);
            Prim(PrimitiveType.Sphere, body, new Vector3(0.45f, 0.6f, -0.05f), Vector3.one * 0.62f, mat);
            Prim(PrimitiveType.Sphere, body, new Vector3(0.12f, 1.08f, -0.08f), Vector3.one * 0.55f, mat);
            Prim(PrimitiveType.Sphere, body, new Vector3(0f, 0.3f, 0f), new Vector3(0.6f, 0.35f, 0.6f), PatchworkAssets.Wind(def.SecondaryColor));
            var wispMat = PatchworkAssets.Glow(def.SecondaryColor, 0.5f);
            for (int i = 0; i < windWisps.Length; i++)
                windWisps[i] = Prim(PrimitiveType.Sphere, body, Vector3.zero, new Vector3(0.16f, 0.1f, 0.16f), wispMat);
            AddFace(body, 0.82f, 0.45f, 0.15f, 0.13f);
            return body;
        }

        /// <summary>Two sewn-on button eyes and rosy felt cheeks, facing +Z.</summary>
        void AddFace(Transform body, float eyeY, float faceZ, float spacing, float eyeSize)
        {
            var eyeMat = PatchworkAssets.Plain(new Color(0.08f, 0.06f, 0.1f), 0.7f);
            var holeMat = PatchworkAssets.Plain(new Color(0.3f, 0.28f, 0.35f), 0.3f);
            var cheekMat = PatchworkAssets.Felt(new Color(1f, 0.6f, 0.65f));
            for (int side = -1; side <= 1; side += 2)
            {
                var eye = Prim(PrimitiveType.Cylinder, body, new Vector3(side * spacing, eyeY, faceZ), new Vector3(eyeSize, 0.02f, eyeSize), eyeMat, new Vector3(90f, 0f, 0f));
                // Thread cross over each button eye.
                Prim(PrimitiveType.Cube, body, new Vector3(side * spacing, eyeY, faceZ + 0.025f), new Vector3(eyeSize * 0.5f, 0.012f, 0.01f), holeMat, new Vector3(0f, 0f, 45f));
                Prim(PrimitiveType.Cube, body, new Vector3(side * spacing, eyeY, faceZ + 0.025f), new Vector3(eyeSize * 0.5f, 0.012f, 0.01f), holeMat, new Vector3(0f, 0f, -45f));
                Prim(PrimitiveType.Sphere, body, new Vector3(side * spacing * 1.7f, eyeY - eyeSize * 0.9f, faceZ - 0.04f), new Vector3(eyeSize * 0.9f, eyeSize * 0.5f, 0.05f), cheekMat);
                eye.name = "ButtonEye";
            }
        }

        void BuildHats()
        {
            accessoryRoot.localPosition = new Vector3(0f, headHeight, 0f);
            hats[0] = new GameObject("Hat_None");
            hats[0].transform.SetParent(accessoryRoot, false);

            var beanie = new GameObject("Hat_RedBeanie").transform;
            beanie.SetParent(accessoryRoot, false);
            var red = PatchworkAssets.Yarn(new Color(0.85f, 0.25f, 0.25f));
            Prim(PrimitiveType.Sphere, beanie, new Vector3(0f, -0.02f, -0.04f), new Vector3(0.6f, 0.38f, 0.6f), red);
            Prim(PrimitiveType.Cylinder, beanie, new Vector3(0f, -0.1f, -0.04f), new Vector3(0.62f, 0.05f, 0.62f), PatchworkAssets.Yarn(new Color(0.95f, 0.9f, 0.8f)));
            Prim(PrimitiveType.Sphere, beanie, new Vector3(0f, 0.2f, -0.04f), Vector3.one * 0.18f, PatchworkAssets.Yarn(new Color(0.95f, 0.9f, 0.8f)));
            hats[1] = beanie.gameObject;

            var topHat = new GameObject("Hat_FeltTopHat").transform;
            topHat.SetParent(accessoryRoot, false);
            var green = PatchworkAssets.Felt(new Color(0.25f, 0.5f, 0.35f));
            Prim(PrimitiveType.Cylinder, topHat, new Vector3(0f, -0.06f, 0f), new Vector3(0.62f, 0.02f, 0.62f), green);
            Prim(PrimitiveType.Cylinder, topHat, new Vector3(0f, 0.12f, 0f), new Vector3(0.36f, 0.17f, 0.36f), green);
            Prim(PrimitiveType.Cylinder, topHat, new Vector3(0f, 0.02f, 0f), new Vector3(0.37f, 0.03f, 0.37f), PatchworkAssets.Felt(new Color(0.95f, 0.8f, 0.3f)));
            hats[2] = topHat.gameObject;

            var daisy = new GameObject("Hat_Daisy").transform;
            daisy.SetParent(accessoryRoot, false);
            var petal = PatchworkAssets.Felt(Color.white);
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI / 3f;
                Prim(PrimitiveType.Sphere, daisy, new Vector3(0.22f + Mathf.Cos(a) * 0.12f, -0.05f + Mathf.Sin(a) * 0.12f, 0.15f), new Vector3(0.12f, 0.12f, 0.05f), petal);
            }
            Prim(PrimitiveType.Sphere, daisy, new Vector3(0.22f, -0.05f, 0.17f), new Vector3(0.11f, 0.11f, 0.06f), PatchworkAssets.Felt(new Color(1f, 0.8f, 0.2f)));
            hats[3] = daisy.gameObject;
        }

        /// <summary>Visual-only primitive (collider removed, player layer, no shadows from tiny bits).</summary>
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

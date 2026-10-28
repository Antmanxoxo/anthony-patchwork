using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Patchwork
{
    // The Blender-made Patch used for the Yarn form (Resources/Characters/Patch.fbx).
    // Gives him his baked textures, picks and blends his animations from how he's moving,
    // swaps between his three faces, and points the yarn arm at the anchor while swinging.
    // If the model is missing, FormVisuals falls back to the old block-built toy.
    public class PatchModel : MonoBehaviour
    {
        public enum Face { Happy = 0, Sad = 1, Angry = 2 }
        enum Anim { Idle, Walk, Run, Jump, Fall, Land, Swing, Wave, Celebrate }
        static readonly string[] AnimNames = { "Idle", "Walk", "Run", "Jump", "Fall", "Land", "Swing", "Wave", "Celebrate" };
        static readonly bool[] Loops = { true, true, true, false, true, false, true, false, true };

        const string ModelPath = "Characters/Patch";
        // Where the top of his head should be (hats sit just under this).
        const float TargetHeadTop = 1.45f;
        // Measured in Blender: body mesh height, top of the yarn-ball head, head bone to head top.
        const float BlenderBodyHeight = 1.306f;
        const float BlenderHeadTop = 1.215f;
        const float BlenderHeadBoneToTop = 0.509f;
        const float BlendTime = 0.14f;

        // Remembered between scenes so the face you picked sticks.
        public static Face CurrentFace = Face.Happy;

        public readonly List<Renderer> Renderers = new List<Renderer>();
        public Transform HandR { get; private set; }

        readonly Renderer[] faces = new Renderer[3];
        Transform modelRoot, head, armR;
        // World units per Blender metre once the model is sized.
        float worldPerBlenderMetre = 1f;

        PlayableGraph graph;
        AnimationMixerPlayable mixer;
        readonly AnimationClipPlayable[] clips = new AnimationClipPlayable[9];
        readonly bool[] hasClip = new bool[9];
        readonly float[] weights = new float[9];
        Anim current = Anim.Idle;
        float oneShotLeft;
        readonly float[] targets = new float[9];

        // Lean into speeding up, slowing down and turning (springy, visual only).
        Quaternion modelBaseRotation;
        bool haveBaseRotation;
        Vector3 lastVelocity;
        float leanPitch, leanRoll, leanPitchVel, leanRollVel;
        float wantLeanPitch, wantLeanRoll;
        const float MaxLeanRoll = 12f, MaxLeanPitch = 8f;
        float airTime;
        bool wasGrounded = true;
        float idleTime;
        bool tethered;
        Vector3 tetherTarget;

        public static PatchModel Create(Transform parent)
        {
            var prefab = Resources.Load<GameObject>(ModelPath);
            if (prefab == null) return null;
            var holder = new GameObject("PatchModel");
            holder.transform.SetParent(parent, false);
            var pm = holder.AddComponent<PatchModel>();
            if (!pm.Setup(Instantiate(prefab, holder.transform, false)))
            {
                Destroy(holder);
                return null;
            }
            return pm;
        }

        // Height of the hat spot above the feet, following the head as it bobs.
        public float HatHeight(Transform space)
        {
            if (head == null) return TargetHeadTop - 0.05f;
            Vector3 top = head.position + transform.up * (BlenderHeadBoneToTop * worldPerBlenderMetre * transform.lossyScale.y);
            return space.InverseTransformPoint(top).y - 0.05f;
        }

        public void SetFace(Face f)
        {
            CurrentFace = f;
            for (int i = 0; i < faces.Length; i++)
                if (faces[i] != null) faces[i].enabled = i == (int)f;
        }

        public void SetTether(bool on, Vector3 target)
        {
            tethered = on;
            tetherTarget = target;
        }

        bool Setup(GameObject model)
        {
            modelRoot = model.transform;
            SetLayer(model, 2);

            var skinned = model.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (skinned.Length == 0) return false;

            // One material for everything: the baked colour and stitch/knit bumps.
            var albedo = LoadTexture("Patch_Albedo");
            var normal = LoadTexture("Patch_Normal");
            var template = Resources.Load<Material>(PatchworkAssets.LitMaterialResource);
            var mat = template != null ? new Material(template) : new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.name = "Patch";
            if (albedo != null)
            {
                mat.SetTexture("_BaseMap", albedo);
                mat.SetTexture("_MainTex", albedo);
                mat.mainTexture = albedo;
            }
            mat.SetColor("_BaseColor", Color.white);
            mat.SetColor("_Color", Color.white);
            if (normal != null)
            {
                mat.SetTexture("_BumpMap", normal);
                mat.SetFloat("_BumpScale", 1f);
                mat.EnableKeyword("_NORMALMAP");
            }
            mat.SetFloat("_Smoothness", 0.12f);
            mat.SetFloat("_Metallic", 0f);

            SkinnedMeshRenderer body = null;
            patchMaterial = mat;
            foreach (var r in skinned)
            {
                var mats = new Material[Mathf.Max(1, r.sharedMesh != null ? r.sharedMesh.subMeshCount : 1)];
                for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                r.sharedMaterials = mats;
                r.updateWhenOffscreen = true;
                Renderers.Add(r);
                string n = r.name;
                if (n.Contains("Face_Happy")) faces[0] = r;
                else if (n.Contains("Face_Sad")) faces[1] = r;
                else if (n.Contains("Face_Angry")) faces[2] = r;
                else if (body == null || n.Contains("Body")) body = r;
            }
            if (body == null) return false;

            HandR = FindDeep(modelRoot, "hand.R");
            armR = FindDeep(modelRoot, "arm.R");
            head = FindDeep(modelRoot, "head");

            // Face forward. In Blender he faces -Y with his rope arm ("arm.R") on +X. Importing into
            // Unity mirrors the handedness, so his front is up x (arm.R - arm.L) here.
            var armL = FindDeep(modelRoot, "arm.L");
            float yaw = 0f;
            if (armR != null && armL != null)
            {
                Vector3 side = transform.InverseTransformDirection(armR.position - armL.position);
                Vector3 front = Vector3.Cross(Vector3.up, side);
                front.y = 0f;
                if (front.sqrMagnitude > 1e-8f)
                {
                    yaw = -Mathf.Atan2(front.x, front.z) * Mathf.Rad2Deg;
                    modelRoot.localRotation = Quaternion.Euler(0f, yaw, 0f) * modelRoot.localRotation;
                }
            }

            // Size: whatever units the file came in, make the head top land at TargetHeadTop.
            float h = body.bounds.size.y / Mathf.Max(0.0001f, transform.lossyScale.y);
            float unitScale = h / BlenderBodyHeight;
            float k = TargetHeadTop / (BlenderHeadTop * unitScale);
            modelRoot.localScale = modelRoot.localScale * k;
            worldPerBlenderMetre = unitScale * k;
            // Feet on the floor.
            float minY = transform.InverseTransformPoint(body.bounds.min).y;
            modelRoot.localPosition += new Vector3(0f, -minY, 0f);

            SetFace(CurrentFace);
            BuildGraph(model);

            Debug.Log("[PatchModel] renderers=" + skinned.Length +
                      " albedo=" + (albedo != null ? albedo.width + "x" + albedo.height : "MISSING") +
                      " normal=" + (normal != null ? normal.width + "x" + normal.height : "MISSING") +
                      " shader=" + mat.shader.name + " baseMap=" + (mat.GetTexture("_BaseMap") != null) +
                      " submeshes=" + (body.sharedMesh != null ? body.sharedMesh.subMeshCount : -1) +
                      " yaw=" + yaw.ToString("0") + " scale=" + modelRoot.localScale.y.ToString("0.###") +
                      " clips=" + ClipCount());
            return true;
        }

        void BuildGraph(GameObject model)
        {
            var animator = model.GetComponent<Animator>();
            if (animator == null) animator = model.AddComponent<Animator>();
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            var all = Resources.LoadAll<AnimationClip>(ModelPath);
            graph = PlayableGraph.Create("PatchAnimation");
            graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            var output = AnimationPlayableOutput.Create(graph, "Patch", animator);
            mixer = AnimationMixerPlayable.Create(graph, AnimNames.Length);
            output.SetSourcePlayable(mixer);

            for (int i = 0; i < AnimNames.Length; i++)
            {
                AnimationClip clip = null;
                foreach (var c in all)
                {
                    if (c == null || c.name.StartsWith("__preview__")) continue;
                    if (c.name == AnimNames[i] || c.name.EndsWith("|" + AnimNames[i])) { clip = c; break; }
                }
                if (clip == null) continue;
                clips[i] = AnimationClipPlayable.Create(graph, clip);
                clips[i].SetApplyFootIK(false);
                graph.Connect(clips[i], 0, mixer, i);
                hasClip[i] = true;
            }
            weights[(int)Anim.Idle] = 1f;
            for (int i = 0; i < weights.Length; i++) mixer.SetInputWeight(i, weights[i]);
            graph.Play();
        }

        Material patchMaterial;
        bool reportedSwap;

        int ClipCount()
        {
            int n = 0;
            for (int i = 0; i < hasClip.Length; i++) if (hasClip[i]) n++;
            return n;
        }

        static Texture2D LoadTexture(string name)
        {
            var t = Resources.Load<Texture2D>("Characters/" + name);
            if (t != null) return t;
            foreach (var c in Resources.LoadAll<Texture2D>("Characters"))
                if (c != null && c.name == name) return c;
            return null;
        }

        // Called every frame while Yarn is the active form.
        public void Tick(PlayerMotor motor, float dt)
        {
            if (!graph.IsValid()) return;
            Vector3 v = motor.Velocity;
            float speed = new Vector3(v.x, 0f, v.z).magnitude;
            float maxSpeed = motor.Params != null ? motor.Params.MoveSpeed : 6.5f;
            bool grounded = motor.Grounded;
            bool levelDone = PatchworkGame.Instance != null && PatchworkGame.Instance.Mode == UIMode.Complete;

            if (oneShotLeft > 0f) oneShotLeft -= dt;

            Anim want;
            if (levelDone) want = Anim.Celebrate;
            else if (tethered) want = Anim.Swing;
            else if (!grounded)
            {
                airTime += dt;
                if (motor.JustJumped) Play(Anim.Jump);
                if (current == Anim.Jump && oneShotLeft > 0f && v.y > -2f) want = Anim.Jump;
                else want = airTime > 0.12f || v.y < -1f ? Anim.Fall : current;
            }
            else
            {
                if (!wasGrounded && airTime > 0.3f && speed < maxSpeed * 0.5f) Play(Anim.Land);
                airTime = 0f;
                if (current == Anim.Land && oneShotLeft > 0f && speed < maxSpeed * 0.5f) want = Anim.Land;
                else if (current == Anim.Wave && oneShotLeft > 0f && speed < 0.3f) want = Anim.Wave;
                else if (speed < 0.3f) want = Anim.Idle;
                else if (speed < maxSpeed * 0.62f) want = Anim.Walk;
                else want = Anim.Run;
            }
            wasGrounded = grounded;

            // A friendly wave if he's left standing around for a while.
            if (want == Anim.Idle) idleTime += dt; else idleTime = 0f;
            if (idleTime > 9f && hasClip[(int)Anim.Wave]) { idleTime = 0f; Play(Anim.Wave); want = Anim.Wave; }

            if (!hasClip[(int)want]) want = hasClip[(int)Anim.Idle] ? Anim.Idle : want;
            if (want != current)
            {
                if (Loops[(int)want]) current = want;
                else Play(want);
            }

            // Step lengths match his speed so his feet don't slide.
            if (hasClip[(int)Anim.Walk]) clips[(int)Anim.Walk].SetSpeed(Mathf.Clamp(speed / (maxSpeed * 0.42f), 0.6f, 1.5f));
            if (hasClip[(int)Anim.Run]) clips[(int)Anim.Run].SetSpeed(Mathf.Clamp(speed / (maxSpeed * 0.85f), 0.8f, 1.4f));

            // What each clip should be weighted towards this frame.
            for (int i = 0; i < targets.Length; i++) targets[i] = 0f;
            bool locomotion = current == Anim.Idle || current == Anim.Walk || current == Anim.Run;
            bool airPose = current == Anim.Jump || current == Anim.Fall;
            if (locomotion)
            {
                // Idle, walk and run blend smoothly by speed instead of snapping between them.
                float moving = Mathf.Clamp01(speed / 1.5f);
                float run = Mathf.Clamp01((speed - 3.5f) / Mathf.Max(0.5f, maxSpeed * 0.92f - 3.5f));
                targets[(int)Anim.Idle] = 1f - moving;
                targets[(int)Anim.Walk] = moving * (1f - run);
                targets[(int)Anim.Run] = moving * run;
                if (!hasClip[(int)Anim.Run]) { targets[(int)Anim.Walk] += targets[(int)Anim.Run]; targets[(int)Anim.Run] = 0f; }
                SyncFeet();
            }
            else if (airPose && hasClip[(int)Anim.Jump] && hasClip[(int)Anim.Fall])
            {
                // In the air the pose follows how fast he's going up or down: rising looks like the
                // jump, the top is a mix, and falling turns into the fall pose.
                float fall = Mathf.Clamp01((2f - v.y) / 4f);
                targets[(int)Anim.Jump] = 1f - fall;
                targets[(int)Anim.Fall] = fall;
            }
            else targets[(int)current] = 1f;

            UpdateLean(v, grounded, dt);

            float step = dt / BlendTime;
            for (int i = 0; i < weights.Length; i++)
            {
                if (!hasClip[i]) continue;
                weights[i] = Mathf.MoveTowards(weights[i], targets[i], step);
                mixer.SetInputWeight(i, weights[i]);
                // Keep looping clips looping even if the importer didn't mark them as loops.
                var len = clips[i].GetAnimationClip().length;
                double t = clips[i].GetTime();
                if (Loops[i] && t > len) clips[i].SetTime(t % len);
                else if (!Loops[i] && t > len) clips[i].SetTime(len);
            }
        }

        // Keep walk and run on the same step so blending between them doesn't scramble his feet.
        void SyncFeet()
        {
            int w = (int)Anim.Walk, r = (int)Anim.Run;
            if (!hasClip[w] || !hasClip[r]) return;
            float wl = clips[w].GetAnimationClip().length, rl = clips[r].GetAnimationClip().length;
            if (wl <= 0f || rl <= 0f) return;
            if (weights[r] > weights[w]) clips[w].SetTime((clips[r].GetTime() % rl) / rl * wl);
            else clips[r].SetTime((clips[w].GetTime() % wl) / wl * rl);
        }

        void UpdateLean(Vector3 v, bool grounded, float dt)
        {
            Vector3 accel = dt > 0f ? (v - lastVelocity) / dt : Vector3.zero;
            lastVelocity = v;
            if (tethered || !grounded) { wantLeanPitch = 0f; wantLeanRoll = 0f; }
            else
            {
                Vector3 local = transform.InverseTransformDirection(accel);
                wantLeanPitch = Mathf.Clamp(local.z * 0.35f, -MaxLeanPitch, MaxLeanPitch);
                wantLeanRoll = Mathf.Clamp(-local.x * 0.5f, -MaxLeanRoll, MaxLeanRoll);
            }
            // Eased so it settles smoothly instead of snapping.
            leanPitch = Mathf.SmoothDamp(leanPitch, wantLeanPitch, ref leanPitchVel, 0.12f, Mathf.Infinity, dt);
            leanRoll = Mathf.SmoothDamp(leanRoll, wantLeanRoll, ref leanRollVel, 0.12f, Mathf.Infinity, dt);
        }

        void Play(Anim a)
        {
            int i = (int)a;
            if (!hasClip[i]) return;
            clips[i].SetTime(0);
            clips[i].SetSpeed(1);
            current = a;
            oneShotLeft = clips[i].GetAnimationClip().length;
        }

        void LateUpdate()
        {
            // If anything swapped his material out, put it back (and say so once in the Console).
            if (patchMaterial != null)
            {
                foreach (var r in Renderers)
                {
                    if (r == null || r.sharedMaterial == patchMaterial) continue;
                    if (!reportedSwap) { reportedSwap = true; Debug.LogWarning("[PatchModel] material was replaced by " + (r.sharedMaterial != null ? r.sharedMaterial.name : "null") + "; restoring it."); }
                    var mats = r.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++) mats[i] = patchMaterial;
                    r.sharedMaterials = mats;
                }
            }

            // Lean the whole body (around his feet) into acceleration and turns.
            if (modelRoot != null)
            {
                if (!haveBaseRotation) { modelBaseRotation = modelRoot.localRotation; haveBaseRotation = true; }
                modelRoot.localRotation = Quaternion.Euler(leanPitch, 0f, leanRoll) * modelBaseRotation;
            }

            // After the animation: aim the yarn arm at the anchor so the rope lines up.
            if (tethered && armR != null && HandR != null)
            {
                Vector3 cur = HandR.position - armR.position;
                Vector3 want = tetherTarget - armR.position;
                if (cur.sqrMagnitude > 1e-6f && want.sqrMagnitude > 1e-6f)
                    armR.rotation = Quaternion.FromToRotation(cur, want) * armR.rotation;
            }

            // The hand is in its final spot now, so anything attached to it (the rope) can follow.
            if (PoseApplied != null) PoseApplied();
        }

        // Called at the end of every LateUpdate, once the pose and arm aim are final.
        public System.Action PoseApplied;

        void OnDestroy()
        {
            if (graph.IsValid()) graph.Destroy();
        }

        static Transform FindDeep(Transform t, string name)
        {
            if (t.name == name) return t;
            for (int i = 0; i < t.childCount; i++)
            {
                var r = FindDeep(t.GetChild(i), name);
                if (r != null) return r;
            }
            return null;
        }

        static void SetLayer(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform c in go.transform) SetLayer(c.gameObject, layer);
        }
    }
}

using UnityEngine;

namespace Patchwork
{
    // Patch sews the world as he goes. Running leaves a line of running stitches on the ground
    // behind him in his form's colour; taking off leaves a little ring of dashes, like the
    // action lines in a comic, and a hard landing sews a cross stitch on the spot and throws up
    // a few yarn pompoms. Every stitch pops in, sits for a few seconds, then is pulled out.
    // They're a fixed pool of tiny flat shapes that get reused, so it costs next to nothing.
    public class StitchTrail : MonoBehaviour
    {
        const int Count = 72;
        const float Life = 5f, PopIn = 0.12f, PullOut = 0.9f, Spacing = 0.55f;

        PatchworkPlayer player;
        Transform root;
        Transform[] stitch;
        MeshRenderer[] rend;
        Vector3[] size;
        float[] born;
        int next;
        float travelled, airTime;
        bool side;
        FormType matForm = (FormType)(-1);
        Material thread, accent;
        int groundMask;

        public static StitchTrail Create(PatchworkPlayer player)
        {
            var trail = player.gameObject.AddComponent<StitchTrail>();
            trail.player = player;
            return trail;
        }

        void Start()
        {
            root = new GameObject("StitchTrail").transform;
            stitch = new Transform[Count];
            rend = new MeshRenderer[Count];
            size = new Vector3[Count];
            born = new float[Count];
            groundMask = ~((1 << 2) | (1 << RenderOptimizer.GrassLayer));
            for (int i = 0; i < Count; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(go.GetComponent<Collider>());
                go.name = "Stitch";
                go.transform.SetParent(root, false);
                rend[i] = go.GetComponent<MeshRenderer>();
                rend[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                rend[i].receiveShadows = false;
                go.SetActive(false);
                stitch[i] = go.transform;
                born[i] = -100f;
            }
        }

        void OnDestroy()
        {
            if (root != null) Destroy(root.gameObject);
        }

        // Called by the player once a frame, straight after he's moved.
        public void Tick(float dt)
        {
            if (player == null || stitch == null) return;
            var motor = player.Motor;
            float now = Time.time;

            var form = player.Forms.Current;
            if (form != matForm)
            {
                matForm = form;
                var def = FormLibrary.Get(form);
                thread = PatchworkAssets.Yarn(Color.Lerp(def.PrimaryColor, Color.white, 0.45f));
                accent = PatchworkAssets.Yarn(Color.Lerp(def.SecondaryColor, Color.white, 0.15f));
            }

            bool sewing = motor.Grounded && !motor.ExternalControl && !motor.WasSwimming;
            if (motor.JustJumped && !motor.WasSwimming) TakeOff();
            if (motor.JustLanded && airTime > 0.45f && !motor.WasSwimming) Touchdown();
            airTime = motor.Grounded ? 0f : airTime + dt;

            if (sewing)
            {
                Vector3 v = motor.Velocity;
                v.y = 0f;
                float speed = v.magnitude;
                if (speed > 1.2f)
                {
                    travelled += speed * dt;
                    if (travelled >= Spacing)
                    {
                        travelled = 0f;
                        side = !side;
                        Vector3 dir = v / speed;
                        Vector3 across = new Vector3(dir.z, 0f, -dir.x);
                        // Two neat rows, one from each foot.
                        Place(transform.position - dir * 0.25f + across * (side ? 0.13f : -0.13f), dir, 0.3f, 0.055f, thread);
                    }
                }
                else travelled = Spacing * 0.6f;
            }

            // Pop in, sit, get pulled out.
            for (int i = 0; i < Count; i++)
            {
                float age = now - born[i];
                if (age > Life) { if (stitch[i].gameObject.activeSelf) stitch[i].gameObject.SetActive(false); continue; }
                float k = age < PopIn ? Mathf.SmoothStep(0f, 1.15f, age / PopIn)
                        : age > Life - PullOut ? 1f - Mathf.SmoothStep(0f, 1f, (age - (Life - PullOut)) / PullOut) : 1f;
                stitch[i].localScale = new Vector3(size[i].x, size[i].y, size[i].z * k);
            }
        }

        // A ring of short dashes pointing outwards from where he pushed off.
        void TakeOff()
        {
            Vector3 c = transform.position;
            for (int i = 0; i < 6; i++)
            {
                float a = (i / 6f) * Mathf.PI * 2f + 0.3f;
                Vector3 d = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                Place(c + d * 0.62f, d, 0.24f, 0.05f, thread);
            }
        }

        // A cross stitch where he came down, and pompoms bouncing away.
        void Touchdown()
        {
            Vector3 c = transform.position;
            Vector3 f = transform.forward;
            f.y = 0f;
            if (f.sqrMagnitude < 0.01f) f = Vector3.forward;
            f.Normalize();
            Vector3 r = new Vector3(f.z, 0f, -f.x);
            Place(c, (f + r).normalized, 0.62f, 0.075f, accent);
            Place(c, (f - r).normalized, 0.62f, 0.075f, accent);
            var def = FormLibrary.Get(player.Forms.Current);
            FxBurst.Spawn(FxBurst.Kind.Pompoms, c + Vector3.up * 0.15f, Color.Lerp(def.PrimaryColor, Color.white, 0.25f));
        }

        void Place(Vector3 at, Vector3 dir, float length, float width, Material mat)
        {
            RaycastHit hit;
            if (!Physics.Raycast(at + Vector3.up * 0.6f, Vector3.down, out hit, 1.3f, groundMask, QueryTriggerInteraction.Ignore)) return;
            if (hit.normal.y < 0.6f || hit.rigidbody != null) return;      // walls, and things that roll away
            // Nothing sewn onto things that move (the stitch would be left hanging in the air).
            if (hit.collider.GetComponentInParent<Carrier>() != null || hit.collider.name == "Tape") return;
            int i = next;
            next = (next + 1) % Count;
            Vector3 along = Vector3.ProjectOnPlane(dir, hit.normal).normalized;
            if (along.sqrMagnitude < 0.01f) return;
            stitch[i].SetPositionAndRotation(hit.point + hit.normal * 0.012f, Quaternion.LookRotation(along, hit.normal));
            size[i] = new Vector3(width, 0.014f, length);
            stitch[i].localScale = new Vector3(width, 0.014f, 0.001f);
            rend[i].sharedMaterial = mat;
            born[i] = Time.time;
            stitch[i].gameObject.SetActive(true);
        }
    }
}

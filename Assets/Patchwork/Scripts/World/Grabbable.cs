using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // A physics toy Patch can hold onto (Grab) and push or pull around: crates, building blocks,
    // beach balls. Light ones also get kicked along when you run into them, and Wind's gust blows
    // them. If one falls out of the world it pops back where it started.
    [RequireComponent(typeof(Rigidbody))]
    public class Grabbable : MonoBehaviour
    {
        public static readonly List<Grabbable> All = new List<Grabbable>();

        public Rigidbody Body { get; private set; }
        public float SpeedFactor = 0.6f;    // how fast Patch moves while holding it
        public bool KickOnWalk;             // balls: running into it knocks it along
        public bool Rolls;                  // pushed along smoothly instead of hopping (the Tape Ball)
        public bool NoGrab;                 // can't be picked up
        public bool Held { get; set; }
        Vector3 home;
        Quaternion homeRot;

        void Awake()
        {
            Body = GetComponent<Rigidbody>();
            home = transform.position;
            homeRot = transform.rotation;
        }

        void OnEnable() { All.Add(this); }
        void OnDisable() { All.Remove(this); }

        void Update()
        {
            if (transform.position.y < -12f) ResetHome();
        }

        public void ResetHome()
        {
            Body.linearVelocity = Vector3.zero;
            Body.angularVelocity = Vector3.zero;
            Body.position = home + Vector3.up * 0.5f;
            Body.rotation = homeRot;
            transform.SetPositionAndRotation(home + Vector3.up * 0.5f, homeRot);
        }

        // ----- Builders -----

        // A heavy wooden crate (stays upright, slides when pushed).
        public static Grabbable Crate(LevelKit kit, Vector3 feet, float size, Color col, string name = "Crate")
        {
            var t = kit.Block(feet + Vector3.up * size * 0.5f, Vector3.one * size, col, name, PatchworkAssets.Planks(col));
            var dark = PatchworkAssets.Wood(Color.Lerp(col, Color.black, 0.35f));
            // Cross-braces on each side.
            for (int s = 0; s < 4; s++)
            {
                var face = Quaternion.Euler(0f, s * 90f, 0f);
                var brace = kit.Deco(PrimitiveType.Cube, Vector3.zero, Vector3.one, dark, default(Vector3), t);
                brace.localPosition = face * new Vector3(0f, 0f, 0.51f);
                brace.localRotation = face * Quaternion.Euler(0f, 0f, 45f);
                brace.localScale = new Vector3(1.3f, 0.12f, 0.03f);
                var rim = kit.Deco(PrimitiveType.Cube, Vector3.zero, Vector3.one, dark, default(Vector3), t);
                rim.localPosition = face * new Vector3(0f, 0.44f, 0.51f);
                rim.localRotation = face;
                rim.localScale = new Vector3(1.02f, 0.12f, 0.03f);
            }
            var rb = t.gameObject.AddComponent<Rigidbody>();
            rb.mass = 30f * size;
            rb.linearDamping = 1.5f;
            rb.angularDamping = 4f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            var g = t.gameObject.AddComponent<Grabbable>();
            g.SpeedFactor = 0.55f;
            return g;
        }

        // A light wooden building block that tumbles.
        public static Grabbable ToyBlock(LevelKit kit, Vector3 feet, float size, Color col, float yaw)
        {
            var t = kit.Block(feet + Vector3.up * size * 0.5f, Vector3.one * size, col, "ToyBlock", PatchworkAssets.Wood(col));
            t.rotation = Quaternion.Euler(0f, yaw, 0f);
            var rb = t.gameObject.AddComponent<Rigidbody>();
            rb.mass = 4f;
            rb.linearDamping = 0.4f;
            rb.angularDamping = 0.8f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            var g = t.gameObject.AddComponent<Grabbable>();
            g.SpeedFactor = 0.85f;
            g.KickOnWalk = true;
            return g;
        }

        // A bouncy striped beach ball.
        public static Grabbable BeachBall(LevelKit kit, Vector3 feet, float radius)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "BeachBall";
            var t = go.transform;
            t.SetParent(kit.Root, false);
            t.position = feet + Vector3.up * radius;
            t.localScale = Vector3.one * radius * 2f;
            go.GetComponent<Renderer>().sharedMaterial = PatchworkAssets.Plain(Color.white, 0.75f);
            Color[] stripes = { new Color(0.95f, 0.3f, 0.35f), new Color(1f, 0.8f, 0.25f), new Color(0.3f, 0.6f, 0.95f) };
            for (int i = 0; i < 3; i++)
            {
                var band = kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one, PatchworkAssets.Plain(stripes[i], 0.75f), default(Vector3), t);
                band.localRotation = Quaternion.Euler(0f, i * 60f, 0f);
                band.localScale = new Vector3(1.01f, 1.01f, 0.32f);
            }
            var cap = kit.Deco(PrimitiveType.Sphere, Vector3.zero, Vector3.one, PatchworkAssets.Plain(Color.white, 0.8f), default(Vector3), t);
            cap.localPosition = new Vector3(0f, 0.47f, 0f);
            cap.localScale = new Vector3(0.22f, 0.1f, 0.22f);
            var col = go.GetComponent<SphereCollider>();
            col.material = new PhysicsMaterial("Bouncy") { bounciness = 0.7f, bounceCombine = PhysicsMaterialCombine.Maximum, dynamicFriction = 0.4f, staticFriction = 0.4f };
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 1.2f;
            rb.linearDamping = 0.25f;
            rb.angularDamping = 0.3f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            var g = go.AddComponent<Grabbable>();
            g.SpeedFactor = 0.9f;
            g.KickOnWalk = true;
            return g;
        }
    }
}

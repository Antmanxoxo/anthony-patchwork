using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    // A toy who lives in Toybox Town: tin soldiers, rag dolls, a teddy-bear mayor, a nutcracker,
    // a jack-in-the-box... Each one is built from felt, yarn, tin and wood, sways and watches Patch
    // go by, can carry an instrument, and talks in its own little voice.
    public class ToyNpc : Npc
    {
        public enum Kind { Teddy, RagDoll, TinSoldier, Nutcracker, JackInBox, Bunny, Bird }
        public enum Instrument { None, Lute, Trumpet, Pipe, Drum }

        public string Name = "Toy";
        public Color Tag = new Color(0.45f, 0.35f, 0.6f);
        public float VoicePitch = 1f;
        public System.Func<string[]> Lines;
        public System.Action TalkEnded;
        public bool Playing;            // performing (bobs to the beat, plays the instrument)
        public bool LookAtPatch = true;

        float height = 1.6f;
        Transform body, head, instrumentRoot, arm;
        readonly List<Renderer> fixedParts = new List<Renderer>();   // only shown once the instrument is mended
        readonly List<Renderer> tarnishParts = new List<Renderer>();
        Material shiny, tarnished;
        AudioSource voice;
        float t;
        float baseYaw, yaw;
        float talkPulse;
        float greetCooldown;
        Instrument instrument;

        public override string DisplayNameText { get { return Name; } }
        public override Color NameColor { get { return Tag; } }
        public override Vector3 HeadPosition { get { return transform.position + Vector3.up * (height + 0.2f); } }

        public static ToyNpc Create(LevelKit kit, Vector3 feet, float faceYaw, Kind kind, string name, Color main, Color accent, Instrument inst = Instrument.None)
        {
            var go = new GameObject("Toy_" + name);
            go.transform.SetParent(kit.Root, false);
            go.transform.position = feet;
            go.transform.rotation = Quaternion.Euler(0f, faceYaw, 0f);
            var npc = go.AddComponent<ToyNpc>();
            npc.Name = name;
            npc.baseYaw = faceYaw;
            npc.yaw = faceYaw;
            npc.instrument = inst;
            npc.t = Random.value * 10f;
            npc.Build(kind, main, accent);
            npc.BuildInstrument(inst);
            var col = go.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, npc.height * 0.5f, 0f);
            col.radius = 0.45f;
            col.height = npc.height;
            npc.voice = go.AddComponent<AudioSource>();
            npc.voice.playOnAwake = false;
            npc.voice.spatialBlend = 0.8f;
            npc.voice.rolloffMode = AudioRolloffMode.Linear;
            npc.voice.minDistance = 3f;
            npc.voice.maxDistance = 22f;
            npc.voice.dopplerLevel = 0f;
            return npc;
        }

        static Transform Part(PrimitiveType type, Transform parent, Vector3 p, Vector3 s, Material m, Vector3 e = default(Vector3))
        {
            var go = GameObject.CreatePrimitive(type);
            Object.Destroy(go.GetComponent<Collider>());
            go.layer = 2;
            var tr = go.transform;
            tr.SetParent(parent, false);
            tr.localPosition = p;
            tr.localRotation = Quaternion.Euler(e);
            tr.localScale = s;
            go.GetComponent<Renderer>().sharedMaterial = m;
            return tr;
        }

        void Eyes(Transform h, float r, float y, float z, bool buttons)
        {
            var dark = PatchworkAssets.Plain(new Color(0.1f, 0.08f, 0.1f), 0.8f);
            if (buttons)
            {
                var b = PatchworkAssets.Felt(new Color(0.15f, 0.12f, 0.15f));
                Part(PrimitiveType.Cylinder, h, new Vector3(-r, y, z), new Vector3(0.12f, 0.02f, 0.12f), b, new Vector3(80f, 0f, 0f));
                Part(PrimitiveType.Cylinder, h, new Vector3(r, y, z), new Vector3(0.12f, 0.02f, 0.12f), b, new Vector3(80f, 0f, 0f));
            }
            else
            {
                Part(PrimitiveType.Sphere, h, new Vector3(-r, y, z), Vector3.one * 0.09f, dark);
                Part(PrimitiveType.Sphere, h, new Vector3(r, y, z), Vector3.one * 0.09f, dark);
            }
        }

        void Build(Kind kind, Color main, Color accent)
        {
            body = new GameObject("Body").transform;
            body.SetParent(transform, false);
            head = new GameObject("Head").transform;
            var felt = PatchworkAssets.Felt(main);
            var acc = PatchworkAssets.Felt(accent);
            var cream = PatchworkAssets.Felt(new Color(0.97f, 0.93f, 0.85f));
            var gold = PatchworkAssets.Plain(LevelKit.Palette.Gold, 0.75f);
            var black = PatchworkAssets.Felt(new Color(0.15f, 0.12f, 0.16f));
            var cheek = PatchworkAssets.Felt(new Color(1f, 0.6f, 0.62f));

            switch (kind)
            {
                case Kind.Teddy:
                {
                    // A round teddy bear in a frock coat and top hat (the Mayor).
                    var fur = PatchworkAssets.Knit(main);
                    height = 1.9f;
                    Part(PrimitiveType.Capsule, body, new Vector3(-0.22f, 0.3f, 0f), new Vector3(0.32f, 0.3f, 0.32f), fur);
                    Part(PrimitiveType.Capsule, body, new Vector3(0.22f, 0.3f, 0f), new Vector3(0.32f, 0.3f, 0.32f), fur);
                    Part(PrimitiveType.Sphere, body, new Vector3(0f, 0.85f, 0f), new Vector3(0.95f, 0.9f, 0.85f), fur);
                    Part(PrimitiveType.Cylinder, body, new Vector3(0f, 0.85f, -0.04f), new Vector3(1f, 0.38f, 0.86f), acc);       // coat
                    Part(PrimitiveType.Cube, body, new Vector3(0f, 0.95f, 0.4f), new Vector3(0.3f, 0.5f, 0.06f), cream);          // shirt front
                    Part(PrimitiveType.Cube, body, new Vector3(0f, 1.18f, 0.43f), new Vector3(0.28f, 0.1f, 0.04f), PatchworkAssets.Felt(new Color(0.8f, 0.2f, 0.25f)), new Vector3(0f, 0f, 0f)); // bow tie
                    for (int i = 0; i < 3; i++) Part(PrimitiveType.Sphere, body, new Vector3(0.12f, 0.95f - i * 0.15f, 0.44f), Vector3.one * 0.06f, gold);
                    Part(PrimitiveType.Cylinder, body, new Vector3(0.25f, 0.8f, 0.42f), new Vector3(0.02f, 0.18f, 0.02f), gold, new Vector3(0f, 0f, 50f));   // watch chain
                    head.SetParent(body, false);
                    head.localPosition = new Vector3(0f, 1.5f, 0f);
                    Part(PrimitiveType.Sphere, head, Vector3.zero, new Vector3(0.75f, 0.68f, 0.68f), fur);
                    Part(PrimitiveType.Sphere, head, new Vector3(-0.3f, 0.3f, 0f), new Vector3(0.26f, 0.26f, 0.12f), fur);
                    Part(PrimitiveType.Sphere, head, new Vector3(0.3f, 0.3f, 0f), new Vector3(0.26f, 0.26f, 0.12f), fur);
                    Part(PrimitiveType.Sphere, head, new Vector3(0f, -0.1f, 0.3f), new Vector3(0.32f, 0.24f, 0.2f), cream);
                    Part(PrimitiveType.Sphere, head, new Vector3(0f, -0.04f, 0.4f), new Vector3(0.12f, 0.08f, 0.08f), black);
                    Eyes(head, 0.14f, 0.08f, 0.3f, true);
                    // Monocle.
                    Part(PrimitiveType.Cylinder, head, new Vector3(0.14f, 0.08f, 0.34f), new Vector3(0.17f, 0.01f, 0.17f), gold, new Vector3(80f, 0f, 0f));
                    // Top hat.
                    Part(PrimitiveType.Cylinder, head, new Vector3(0f, 0.36f, 0f), new Vector3(0.7f, 0.025f, 0.7f), black);
                    Part(PrimitiveType.Cylinder, head, new Vector3(0f, 0.6f, 0f), new Vector3(0.44f, 0.24f, 0.44f), black);
                    Part(PrimitiveType.Cylinder, head, new Vector3(0f, 0.43f, 0f), new Vector3(0.45f, 0.05f, 0.45f), PatchworkAssets.Felt(new Color(0.7f, 0.2f, 0.3f)));
                    arm = Part(PrimitiveType.Capsule, body, new Vector3(0.52f, 0.95f, 0.05f), new Vector3(0.24f, 0.3f, 0.24f), acc, new Vector3(0f, 0f, 25f));
                    Part(PrimitiveType.Capsule, body, new Vector3(-0.52f, 0.95f, 0.05f), new Vector3(0.24f, 0.3f, 0.24f), acc, new Vector3(0f, 0f, -25f));
                    break;
                }
                case Kind.RagDoll:
                {
                    // A rag doll with yarn hair, button eyes and a long dress.
                    height = 1.7f;
                    Part(PrimitiveType.Cylinder, body, new Vector3(0f, 0.45f, 0f), new Vector3(0.9f, 0.45f, 0.9f), PatchworkAssets.Damask(main));       // skirt
                    Part(PrimitiveType.Sphere, body, new Vector3(0f, 0.92f, 0f), new Vector3(0.55f, 0.55f, 0.45f), felt);
                    Part(PrimitiveType.Cylinder, body, new Vector3(0f, 0.12f, 0f), new Vector3(0.95f, 0.06f, 0.95f), acc);
                    head.SetParent(body, false);
                    head.localPosition = new Vector3(0f, 1.38f, 0f);
                    Part(PrimitiveType.Sphere, head, Vector3.zero, new Vector3(0.6f, 0.62f, 0.56f), cream);
                    Eyes(head, 0.12f, 0.05f, 0.27f, true);
                    Part(PrimitiveType.Sphere, head, new Vector3(-0.2f, -0.08f, 0.22f), Vector3.one * 0.1f, cheek);
                    Part(PrimitiveType.Sphere, head, new Vector3(0.2f, -0.08f, 0.22f), Vector3.one * 0.1f, cheek);
                    Part(PrimitiveType.Cube, head, new Vector3(0f, -0.14f, 0.28f), new Vector3(0.14f, 0.025f, 0.02f), PatchworkAssets.Felt(new Color(0.8f, 0.25f, 0.3f)));
                    var hair = PatchworkAssets.Yarn(accent);
                    for (int i = 0; i < 9; i++)
                    {
                        float a = Mathf.Lerp(-150f, 150f, i / 8f);
                        var strand = Part(PrimitiveType.Capsule, head, Quaternion.Euler(0f, a, 0f) * new Vector3(0f, -0.05f, -0.28f), new Vector3(0.1f, 0.3f, 0.1f), hair);
                        strand.localRotation = Quaternion.Euler(0f, a, 0f) * Quaternion.Euler(-12f, 0f, 0f);
                    }
                    Part(PrimitiveType.Sphere, head, new Vector3(0f, 0.2f, -0.02f), new Vector3(0.62f, 0.32f, 0.58f), hair);
                    arm = Part(PrimitiveType.Capsule, body, new Vector3(0.36f, 0.9f, 0.05f), new Vector3(0.14f, 0.26f, 0.14f), felt, new Vector3(0f, 0f, 20f));
                    Part(PrimitiveType.Capsule, body, new Vector3(-0.36f, 0.9f, 0.05f), new Vector3(0.14f, 0.26f, 0.14f), felt, new Vector3(0f, 0f, -20f));
                    break;
                }
                case Kind.TinSoldier:
                case Kind.Nutcracker:
                {
                    // Tall and upright: red coat, gold buttons, white cross belts, black boots and
                    // a tall busby (a tin soldier) or a crowned helmet and a big jaw (a nutcracker).
                    bool nut = kind == Kind.Nutcracker;
                    height = nut ? 2f : 1.9f;
                    var tin = PatchworkAssets.Plain(main, 0.6f);
                    Part(PrimitiveType.Cylinder, body, new Vector3(-0.15f, 0.32f, 0f), new Vector3(0.24f, 0.32f, 0.24f), PatchworkAssets.Plain(new Color(0.2f, 0.25f, 0.45f), 0.5f));
                    Part(PrimitiveType.Cylinder, body, new Vector3(0.15f, 0.32f, 0f), new Vector3(0.24f, 0.32f, 0.24f), PatchworkAssets.Plain(new Color(0.2f, 0.25f, 0.45f), 0.5f));
                    Part(PrimitiveType.Cylinder, body, new Vector3(-0.15f, 0.08f, 0.04f), new Vector3(0.27f, 0.08f, 0.32f), PatchworkAssets.Plain(new Color(0.1f, 0.08f, 0.1f), 0.8f));
                    Part(PrimitiveType.Cylinder, body, new Vector3(0.15f, 0.08f, 0.04f), new Vector3(0.27f, 0.08f, 0.32f), PatchworkAssets.Plain(new Color(0.1f, 0.08f, 0.1f), 0.8f));
                    Part(PrimitiveType.Cylinder, body, new Vector3(0f, 0.95f, 0f), new Vector3(0.6f, 0.32f, 0.5f), tin);
                    var belt = PatchworkAssets.Plain(new Color(0.96f, 0.95f, 0.92f), 0.4f);
                    Part(PrimitiveType.Cube, body, new Vector3(0f, 0.95f, 0.25f), new Vector3(0.08f, 0.62f, 0.03f), belt, new Vector3(0f, 0f, 32f));
                    Part(PrimitiveType.Cube, body, new Vector3(0f, 0.95f, 0.25f), new Vector3(0.08f, 0.62f, 0.03f), belt, new Vector3(0f, 0f, -32f));
                    for (int i = 0; i < 3; i++) Part(PrimitiveType.Sphere, body, new Vector3(0f, 1.15f - i * 0.17f, 0.26f), Vector3.one * 0.05f, gold);
                    Part(PrimitiveType.Cylinder, body, new Vector3(0f, 0.65f, 0f), new Vector3(0.62f, 0.05f, 0.52f), PatchworkAssets.Plain(new Color(0.1f, 0.08f, 0.1f), 0.8f));
                    head.SetParent(body, false);
                    head.localPosition = new Vector3(0f, 1.42f, 0f);
                    var face = PatchworkAssets.Plain(new Color(0.98f, 0.85f, 0.75f), 0.55f);
                    Part(PrimitiveType.Cylinder, head, Vector3.zero, new Vector3(0.42f, 0.2f, 0.42f), face);
                    Eyes(head, 0.09f, 0.06f, 0.2f, false);
                    Part(PrimitiveType.Sphere, head, new Vector3(-0.14f, -0.06f, 0.18f), Vector3.one * 0.08f, cheek);
                    Part(PrimitiveType.Sphere, head, new Vector3(0.14f, -0.06f, 0.18f), Vector3.one * 0.08f, cheek);
                    if (nut)
                    {
                        Part(PrimitiveType.Cube, head, new Vector3(0f, -0.2f, 0.12f), new Vector3(0.36f, 0.12f, 0.24f), PatchworkAssets.Wood(new Color(0.95f, 0.9f, 0.85f)));   // jaw / beard
                        Part(PrimitiveType.Cylinder, head, new Vector3(0f, 0.3f, 0f), new Vector3(0.46f, 0.16f, 0.46f), tin);
                        for (int i = 0; i < 5; i++)
                        {
                            float a = i / 5f * Mathf.PI * 2f;
                            Part(PrimitiveType.Cube, head, new Vector3(Mathf.Sin(a) * 0.2f, 0.5f, Mathf.Cos(a) * 0.2f), new Vector3(0.08f, 0.14f, 0.08f), gold);
                        }
                    }
                    else
                    {
                        Part(PrimitiveType.Cylinder, head, new Vector3(0f, 0.42f, 0f), new Vector3(0.44f, 0.3f, 0.44f), PatchworkAssets.Knit(new Color(0.12f, 0.1f, 0.12f)));
                        Part(PrimitiveType.Cube, head, new Vector3(0f, 0.36f, 0.2f), new Vector3(0.06f, 0.4f, 0.04f), gold);
                        Part(PrimitiveType.Sphere, head, new Vector3(0.18f, 0.6f, 0.06f), new Vector3(0.1f, 0.18f, 0.1f), PatchworkAssets.Felt(new Color(0.95f, 0.95f, 0.95f)));
                    }
                    arm = Part(PrimitiveType.Capsule, body, new Vector3(0.36f, 0.95f, 0.06f), new Vector3(0.16f, 0.28f, 0.16f), tin, new Vector3(0f, 0f, 12f));
                    Part(PrimitiveType.Capsule, body, new Vector3(-0.36f, 0.95f, 0.06f), new Vector3(0.16f, 0.28f, 0.16f), tin, new Vector3(0f, 0f, -12f));
                    break;
                }
                case Kind.JackInBox:
                {
                    // A painted box with a jester on a spring popping out of it.
                    height = 1.8f;
                    var box = PatchworkAssets.Planks(main);
                    Part(PrimitiveType.Cube, body, new Vector3(0f, 0.4f, 0f), new Vector3(0.8f, 0.8f, 0.8f), box);
                    for (int i = 0; i < 4; i++)
                        Part(PrimitiveType.Cube, body, Quaternion.Euler(0f, i * 90f, 0f) * new Vector3(0f, 0.4f, 0.41f), new Vector3(0.4f, 0.4f, 0.02f), acc, new Vector3(0f, i * 90f, 45f));
                    var spring = PatchworkAssets.Plain(new Color(0.75f, 0.76f, 0.8f), 0.85f);
                    for (int i = 0; i < 5; i++) Part(PrimitiveType.Cylinder, body, new Vector3(0f, 0.85f + i * 0.08f, 0f), new Vector3(0.3f, 0.015f, 0.3f), spring);
                    head.SetParent(body, false);
                    head.localPosition = new Vector3(0f, 1.42f, 0f);
                    Part(PrimitiveType.Sphere, head, new Vector3(0f, -0.25f, 0f), new Vector3(0.5f, 0.3f, 0.4f), acc);   // collar
                    Part(PrimitiveType.Sphere, head, Vector3.zero, new Vector3(0.48f, 0.5f, 0.46f), cream);
                    Eyes(head, 0.1f, 0.06f, 0.21f, false);
                    Part(PrimitiveType.Sphere, head, new Vector3(0f, -0.04f, 0.24f), Vector3.one * 0.12f, PatchworkAssets.Felt(new Color(0.9f, 0.2f, 0.25f)));
                    Part(PrimitiveType.Capsule, head, new Vector3(-0.15f, 0.32f, 0f), new Vector3(0.14f, 0.24f, 0.14f), felt, new Vector3(0f, 0f, 35f));
                    Part(PrimitiveType.Capsule, head, new Vector3(0.15f, 0.32f, 0f), new Vector3(0.14f, 0.24f, 0.14f), acc, new Vector3(0f, 0f, -35f));
                    Part(PrimitiveType.Sphere, head, new Vector3(-0.32f, 0.48f, 0f), Vector3.one * 0.1f, gold);
                    Part(PrimitiveType.Sphere, head, new Vector3(0.32f, 0.48f, 0f), Vector3.one * 0.1f, gold);
                    break;
                }
                case Kind.Bunny:
                {
                    height = 1.5f;
                    var plush = PatchworkAssets.Knit(main);
                    Part(PrimitiveType.Sphere, body, new Vector3(0f, 0.5f, 0f), new Vector3(0.75f, 0.8f, 0.7f), plush);
                    Part(PrimitiveType.Sphere, body, new Vector3(0f, 0.45f, 0.3f), new Vector3(0.45f, 0.5f, 0.2f), cream);
                    Part(PrimitiveType.Sphere, body, new Vector3(0f, 0.3f, -0.35f), Vector3.one * 0.22f, cream);
                    head.SetParent(body, false);
                    head.localPosition = new Vector3(0f, 1.05f, 0f);
                    Part(PrimitiveType.Sphere, head, Vector3.zero, new Vector3(0.6f, 0.55f, 0.55f), plush);
                    Part(PrimitiveType.Capsule, head, new Vector3(-0.14f, 0.45f, -0.05f), new Vector3(0.16f, 0.35f, 0.08f), plush, new Vector3(0f, 0f, 10f));
                    Part(PrimitiveType.Capsule, head, new Vector3(0.14f, 0.45f, -0.05f), new Vector3(0.16f, 0.35f, 0.08f), plush, new Vector3(0f, 0f, -14f));
                    Part(PrimitiveType.Capsule, head, new Vector3(-0.14f, 0.45f, -0.01f), new Vector3(0.08f, 0.28f, 0.04f), cheek, new Vector3(0f, 0f, 10f));
                    Part(PrimitiveType.Capsule, head, new Vector3(0.14f, 0.45f, -0.01f), new Vector3(0.08f, 0.28f, 0.04f), cheek, new Vector3(0f, 0f, -14f));
                    Eyes(head, 0.12f, 0.04f, 0.25f, true);
                    Part(PrimitiveType.Sphere, head, new Vector3(0f, -0.06f, 0.27f), Vector3.one * 0.07f, cheek);
                    arm = Part(PrimitiveType.Capsule, body, new Vector3(0.36f, 0.62f, 0.12f), new Vector3(0.16f, 0.2f, 0.16f), plush, new Vector3(0f, 0f, 30f));
                    Part(PrimitiveType.Capsule, body, new Vector3(-0.36f, 0.62f, 0.12f), new Vector3(0.16f, 0.2f, 0.16f), plush, new Vector3(0f, 0f, -30f));
                    break;
                }
                default:
                {
                    // A plump felt songbird with a little beret (the town's best piper).
                    height = 1.4f;
                    Part(PrimitiveType.Sphere, body, new Vector3(0f, 0.6f, 0f), new Vector3(0.85f, 0.95f, 0.8f), PatchworkAssets.Yarn(main));
                    Part(PrimitiveType.Sphere, body, new Vector3(0f, 0.55f, 0.25f), new Vector3(0.5f, 0.6f, 0.3f), cream);
                    Part(PrimitiveType.Capsule, body, new Vector3(-0.42f, 0.65f, -0.05f), new Vector3(0.12f, 0.35f, 0.3f), acc, new Vector3(0f, 0f, 15f));
                    Part(PrimitiveType.Capsule, body, new Vector3(0.42f, 0.65f, -0.05f), new Vector3(0.12f, 0.35f, 0.3f), acc, new Vector3(0f, 0f, -15f));
                    Part(PrimitiveType.Cube, body, new Vector3(0f, 0.45f, -0.42f), new Vector3(0.35f, 0.08f, 0.3f), acc, new Vector3(-30f, 0f, 0f));
                    Part(PrimitiveType.Capsule, body, new Vector3(-0.15f, 0.08f, 0.05f), new Vector3(0.07f, 0.1f, 0.07f), PatchworkAssets.Felt(new Color(1f, 0.7f, 0.25f)));
                    Part(PrimitiveType.Capsule, body, new Vector3(0.15f, 0.08f, 0.05f), new Vector3(0.07f, 0.1f, 0.07f), PatchworkAssets.Felt(new Color(1f, 0.7f, 0.25f)));
                    head.SetParent(body, false);
                    head.localPosition = new Vector3(0f, 1.12f, 0.05f);
                    Part(PrimitiveType.Sphere, head, Vector3.zero, Vector3.one * 0.5f, PatchworkAssets.Yarn(main));
                    Part(PrimitiveType.Cube, head, new Vector3(0f, -0.04f, 0.27f), new Vector3(0.1f, 0.1f, 0.16f), PatchworkAssets.Felt(new Color(1f, 0.7f, 0.25f)), new Vector3(0f, 0f, 45f));
                    Eyes(head, 0.11f, 0.06f, 0.2f, false);
                    Part(PrimitiveType.Cylinder, head, new Vector3(0.03f, 0.24f, -0.02f), new Vector3(0.5f, 0.06f, 0.5f), PatchworkAssets.Felt(accent), new Vector3(-8f, 0f, 10f));
                    Part(PrimitiveType.Sphere, head, new Vector3(0.05f, 0.3f, -0.02f), Vector3.one * 0.08f, PatchworkAssets.Felt(accent));
                    break;
                }
            }
        }

        void BuildInstrument(Instrument inst)
        {
            if (inst == Instrument.None) return;
            instrumentRoot = new GameObject("Instrument").transform;
            instrumentRoot.SetParent(body, false);
            var wood = PatchworkAssets.Wood(new Color(0.75f, 0.5f, 0.3f));
            shiny = PatchworkAssets.Plain(new Color(1f, 0.78f, 0.3f), 0.85f);
            tarnished = PatchworkAssets.Plain(new Color(0.42f, 0.4f, 0.38f), 0.2f);
            switch (inst)
            {
                case Instrument.Lute:
                {
                    instrumentRoot.localPosition = new Vector3(0.05f, height * 0.5f, 0.42f);
                    instrumentRoot.localRotation = Quaternion.Euler(0f, 0f, -35f);
                    Part(PrimitiveType.Sphere, instrumentRoot, Vector3.zero, new Vector3(0.5f, 0.6f, 0.22f), wood);
                    Part(PrimitiveType.Cylinder, instrumentRoot, new Vector3(0f, 0f, 0.1f), new Vector3(0.14f, 0.005f, 0.14f), PatchworkAssets.Plain(new Color(0.2f, 0.12f, 0.08f), 0.3f), new Vector3(90f, 0f, 0f));
                    Part(PrimitiveType.Cube, instrumentRoot, new Vector3(0f, 0.55f, 0.02f), new Vector3(0.1f, 0.6f, 0.06f), wood);
                    Part(PrimitiveType.Cube, instrumentRoot, new Vector3(0f, 0.9f, -0.04f), new Vector3(0.14f, 0.16f, 0.06f), wood, new Vector3(-30f, 0f, 0f));
                    // The strings (golden thread) only appear once the lute string is found.
                    for (int i = -1; i <= 1; i++)
                    {
                        var s = Part(PrimitiveType.Cylinder, instrumentRoot, new Vector3(i * 0.025f, 0.35f, 0.12f), new Vector3(0.008f, 0.5f, 0.008f), PatchworkAssets.Glow(LevelKit.Palette.Gold, 0.6f));
                        fixedParts.Add(s.GetComponent<Renderer>());
                    }
                    break;
                }
                case Instrument.Trumpet:
                {
                    instrumentRoot.localPosition = new Vector3(0.25f, height * 0.62f, 0.38f);
                    var tube = Part(PrimitiveType.Cylinder, instrumentRoot, Vector3.zero, new Vector3(0.06f, 0.3f, 0.06f), tarnished, new Vector3(80f, 0f, 0f));
                    var bell = Part(PrimitiveType.Cylinder, instrumentRoot, new Vector3(0f, -0.05f, 0.32f), new Vector3(0.24f, 0.05f, 0.24f), tarnished, new Vector3(80f, 0f, 0f));
                    var valves = Part(PrimitiveType.Cube, instrumentRoot, new Vector3(0f, 0.06f, -0.05f), new Vector3(0.08f, 0.12f, 0.16f), tarnished);
                    tarnishParts.Add(tube.GetComponent<Renderer>());
                    tarnishParts.Add(bell.GetComponent<Renderer>());
                    tarnishParts.Add(valves.GetComponent<Renderer>());
                    break;
                }
                case Instrument.Pipe:
                {
                    instrumentRoot.localPosition = new Vector3(0f, height * 0.7f, 0.42f);
                    instrumentRoot.localRotation = Quaternion.Euler(60f, 0f, 0f);
                    Part(PrimitiveType.Cylinder, instrumentRoot, Vector3.zero, new Vector3(0.07f, 0.28f, 0.07f), wood);
                    for (int i = 0; i < 4; i++) Part(PrimitiveType.Sphere, instrumentRoot, new Vector3(0f, -0.18f + i * 0.09f, 0.035f), Vector3.one * 0.025f, PatchworkAssets.Plain(new Color(0.15f, 0.1f, 0.08f), 0.3f));
                    break;
                }
                case Instrument.Drum:
                {
                    instrumentRoot.localPosition = new Vector3(0f, height * 0.38f, 0.5f);
                    Part(PrimitiveType.Cylinder, instrumentRoot, Vector3.zero, new Vector3(0.6f, 0.2f, 0.6f), PatchworkAssets.Felt(new Color(0.8f, 0.2f, 0.25f)));
                    Part(PrimitiveType.Cylinder, instrumentRoot, new Vector3(0f, 0.2f, 0f), new Vector3(0.62f, 0.01f, 0.62f), PatchworkAssets.Felt(new Color(0.97f, 0.95f, 0.88f)));
                    Part(PrimitiveType.Cylinder, instrumentRoot, new Vector3(0f, 0.12f, 0f), new Vector3(0.63f, 0.02f, 0.63f), shiny);
                    Part(PrimitiveType.Cylinder, instrumentRoot, new Vector3(0f, -0.12f, 0f), new Vector3(0.63f, 0.02f, 0.63f), shiny);
                    break;
                }
            }
            SetInstrumentFixed(inst == Instrument.Drum || inst == Instrument.Pipe);
        }

        // Mended instruments: the lute gets its strings, the trumpet shines.
        public void SetInstrumentFixed(bool ok)
        {
            foreach (var r in fixedParts) if (r != null) r.enabled = ok;
            foreach (var r in tarnishParts) if (r != null) r.sharedMaterial = ok ? shiny : tarnished;
            if (ok && instrumentRoot != null) FxBurst.Spawn(FxBurst.Kind.Sparkle, instrumentRoot.position, new Color(1f, 0.85f, 0.5f));
        }

        void Update()
        {
            float dt = Time.deltaTime;
            t += dt;
            greetCooldown -= dt;
            talkPulse = Mathf.MoveTowards(talkPulse, 0f, dt * 6f);

            float want = baseYaw;
            var game = PatchworkGame.Instance;
            if (LookAtPatch && !Playing && game != null && game.Player != null)
            {
                Vector3 to = game.Player.transform.position - transform.position;
                to.y = 0f;
                if (to.sqrMagnitude < 64f && to.sqrMagnitude > 0.01f) want = Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg;
            }
            yaw = Mathf.MoveTowardsAngle(yaw, want, dt * 140f);
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            float beat = t * MusicPlayer.ToyBpm / 60f;
            float bob, sway;
            if (Playing)
            {
                bob = Mathf.Abs(Mathf.Sin(beat * Mathf.PI)) * 0.08f;
                sway = Mathf.Sin(beat * Mathf.PI / 3f) * 6f;
            }
            else
            {
                bob = Mathf.Abs(Mathf.Sin(t * 1.3f)) * 0.025f + talkPulse * 0.05f;
                sway = Mathf.Sin(t * 0.9f) * 2.5f;
            }
            body.localPosition = new Vector3(0f, bob, 0f);
            body.localRotation = Quaternion.Euler(-talkPulse * 4f, 0f, sway);
            if (head != null) head.localRotation = Quaternion.Euler(Playing ? Mathf.Sin(beat * Mathf.PI) * 6f : 0f, Mathf.Sin(t * 0.7f) * 10f, 0f);
            if (arm != null && Playing) arm.localRotation = Quaternion.Euler(Mathf.Sin(beat * Mathf.PI * 2f) * 25f, 0f, 20f);
            if (instrumentRoot != null && Playing && instrument == Instrument.Trumpet)
                instrumentRoot.localRotation = Quaternion.Euler(-10f - Mathf.Abs(Mathf.Sin(beat * Mathf.PI)) * 10f, 0f, 0f);
        }

        public override string[] NextLines()
        {
            if (Lines != null) return Lines();
            return new[] { "Hello there!" };
        }

        public override void Blip()
        {
            if (voice == null) return;
            talkPulse = 1f;
            voice.pitch = VoicePitch * Random.Range(0.92f, 1.08f);
            var clip = Sfx.Clip(Random.value < 0.75f ? SfxId.VoiceHup : SfxId.VoiceYay);
            if (clip != null) voice.PlayOneShot(clip, 0.3f * Sfx.Volume / 10f);
        }

        public override void Greet()
        {
            if (greetCooldown > 0f || voice == null) return;
            greetCooldown = 15f;
            talkPulse = 1f;
            voice.pitch = VoicePitch;
            var clip = Sfx.Clip(SfxId.VoiceYay);
            if (clip != null) voice.PlayOneShot(clip, 0.3f * Sfx.Volume / 10f);
        }

        public override void OnTalkEnd()
        {
            if (TalkEnded != null) TalkEnded();
        }
    }
}

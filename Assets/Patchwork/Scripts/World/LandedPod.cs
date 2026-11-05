using UnityEngine;

namespace Patchwork
{
    // Your Pod, landed outside a level: the same egg-shaped cardboard pod you live in, standing
    // on three little legs with its round window glowing, a hatch door at the front and its
    // antenna blinking. You hop out of the door when you arrive.
    public class LandedPod
    {
        public Transform Root, Door;
        public Vector3 Inside, DoorStep;

        public static LandedPod Build(LevelKit kit, Vector3 feet, float yaw)
        {
            var pod = new LandedPod();
            var g = kit.Group("LandedPod", feet);
            g.rotation = Quaternion.Euler(0f, yaw, 0f);
            pod.Root = g;
            Color kraft = new Color(0.8f, 0.62f, 0.42f), dark = new Color(0.62f, 0.45f, 0.3f);
            var shell = PatchworkAssets.Cardboard(kraft);
            var ribMat = PatchworkAssets.Cardboard(dark);
            const float cy = 3.9f;
            kit.Deco(PrimitiveType.Sphere, new Vector3(0f, cy, 0f), new Vector3(5f, 6.2f, 5f), shell, default(Vector3), g);
            // Cardboard ribs round the egg (none down the front, where the door is).
            foreach (float r in new[] { 0f, 30f, 60f, 120f, 150f })
                kit.Deco(PrimitiveType.Sphere, new Vector3(0f, cy, 0f), new Vector3(5.12f, 6.3f, 0.12f), ribMat, new Vector3(0f, r, 0f), g);
            // A zigzag band round its middle, like the box art.
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, cy, 0f), new Vector3(5.1f, 0.22f, 5.1f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Zigzag, new Color(0.2f, 0.25f, 0.55f), new Color(1f, 0.8f, 0.28f)), default(Vector3), g);
            // The round window, glowing from inside.
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 4.9f, 2.32f), new Vector3(1.5f, 0.06f, 1.5f), PatchworkAssets.Plain(new Color(1f, 0.8f, 0.3f), 0.8f), new Vector3(90f, 0f, 0f), g);
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 4.9f, 2.36f), new Vector3(1.2f, 0.05f, 1.2f), PatchworkAssets.Glow(new Color(1f, 0.85f, 0.55f), 1.4f), new Vector3(90f, 0f, 0f), g);
            // The doorway and its hatch (hinged on the left).
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 2.05f, 1.92f), new Vector3(1.5f, 2.3f, 0.12f), PatchworkAssets.Plain(new Color(0.12f, 0.09f, 0.14f), 0.2f), default(Vector3), g);
            var hinge = new GameObject("DoorHinge").transform;
            hinge.SetParent(g, false);
            hinge.localPosition = new Vector3(-0.8f, 2.05f, 2.02f);
            kit.Deco(PrimitiveType.Cube, new Vector3(0.8f, 0f, 0f), new Vector3(1.6f, 2.4f, 0.14f), PatchworkAssets.Print(PatchworkAssets.PrintKind.Polka, new Color(0.9f, 0.3f, 0.35f), new Color(0.99f, 0.95f, 0.88f)), default(Vector3), hinge);
            kit.Deco(PrimitiveType.Sphere, new Vector3(1.35f, 0f, 0.1f), Vector3.one * 0.2f, PatchworkAssets.Plain(new Color(1f, 0.8f, 0.3f), 0.8f), default(Vector3), hinge);
            pod.Door = hinge;
            // A little felt step.
            kit.Deco(PrimitiveType.Cube, new Vector3(0f, 0.4f, 2.5f), new Vector3(1.6f, 0.8f, 0.8f), PatchworkAssets.Felt(new Color(1f, 0.8f, 0.3f)), default(Vector3), g);
            // Three legs with round felt feet.
            var leg = PatchworkAssets.Plain(new Color(0.85f, 0.86f, 0.9f), 0.7f);
            for (int k = 0; k < 3; k++)
            {
                float a = (k * 120f + 60f) * Mathf.Deg2Rad;
                Vector3 top = new Vector3(Mathf.Sin(a) * 1.9f, 1.6f, Mathf.Cos(a) * 1.9f);
                Vector3 bottom = new Vector3(Mathf.Sin(a) * 2.9f, 0.1f, Mathf.Cos(a) * 2.9f);
                var l = kit.Deco(PrimitiveType.Cylinder, (top + bottom) * 0.5f, new Vector3(0.22f, (top - bottom).magnitude * 0.5f, 0.22f), leg, default(Vector3), g);
                l.localRotation = Quaternion.FromToRotation(Vector3.up, (top - bottom).normalized);
                kit.Deco(PrimitiveType.Cylinder, bottom, new Vector3(0.9f, 0.1f, 0.9f), PatchworkAssets.Felt(new Color(0.9f, 0.3f, 0.35f)), default(Vector3), g);
            }
            // An antenna with a blinking light, and a little chimney.
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0.5f, 7.4f, 0f), new Vector3(0.06f, 0.6f, 0.06f), leg, new Vector3(0f, 0f, -10f), g);
            var blink = kit.Deco(PrimitiveType.Sphere, new Vector3(0.6f, 8.05f, 0f), Vector3.one * 0.26f, PatchworkAssets.Glow(new Color(1f, 0.35f, 0.4f), 2f), default(Vector3), g);
            blink.gameObject.AddComponent<Blinker>();
            kit.Deco(PrimitiveType.Cylinder, new Vector3(-0.6f, 7.1f, -0.3f), new Vector3(0.6f, 0.4f, 0.6f), shell, default(Vector3), g);
            // The grass it flattened as it landed.
            kit.Deco(PrimitiveType.Cylinder, new Vector3(0f, 0.015f, 0f), new Vector3(8f, 0.01f, 8f), PatchworkAssets.Felt(new Color(0.42f, 0.6f, 0.34f)), default(Vector3), g);
            // Solid (but not in front of the door, where Patch waits).
            var solid = kit.Block(feet + g.rotation * new Vector3(0f, 3.6f, -0.5f), new Vector3(4f, 5.2f, 3.4f), Color.white, "PodSolid");
            solid.rotation = g.rotation;
            solid.GetComponent<Renderer>().enabled = false;
            pod.Inside = feet + g.rotation * new Vector3(0f, 0.9f, 1.55f);
            pod.DoorStep = feet + g.rotation * new Vector3(0f, 0.85f, 2.7f);
            return pod;
        }
    }

    // Blinks something on and off (the Pod's antenna light).
    public class Blinker : MonoBehaviour
    {
        public float Period = 1.2f;
        Vector3 scale;
        void Start() { scale = transform.localScale; }
        void Update() { transform.localScale = Mathf.Repeat(Time.time, Period) < Period * 0.5f ? scale : scale * 0.4f; }
    }
}

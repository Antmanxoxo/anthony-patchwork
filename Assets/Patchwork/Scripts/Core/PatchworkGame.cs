using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Patchwork
{
    public enum UIMode
    {
        Gameplay,
        FormWheel,
        Satchel,
        Paused,
        Complete,
        Warping,
        Loading
    }

    /// <summary>
    /// One per scene. Builds the level, camera, light and player, then drives everything from a
    /// single Update so the order is always: input → menus → player → camera.
    /// Only one menu can be open at a time, so the form wheel can never trigger pause or the satchel.
    /// </summary>
    public class PatchworkGame : MonoBehaviour
    {
        public static PatchworkGame Instance { get; private set; }

        public PatchworkInput Input { get; private set; }
        public PatchworkPlayer Player { get; private set; }
        public PlayerCamera CameraRig { get; private set; }
        public LevelInfo Level { get; private set; }
        public UIMode Mode { get; private set; }
        public PatchworkHUD Hud { get; private set; }

        public FormSelector Selector { get; private set; }
        public SatchelMenu Satchel { get; private set; }
        public PauseMenu Pause { get; private set; }

        // Messages
        public string ToastText { get; private set; }
        public float ToastTimer { get; private set; }
        public HintZone ActiveHint { get; private set; }
        /// <summary>How long a tutorial card stays up before sliding away.</summary>
        public const float HintDuration = 7f;
        public float HintAge { get; private set; }
        public float TitleTimer { get; private set; }
        public float Fade { get; private set; }
        public Color FadeColor { get; private set; }

        static readonly Color DefaultFadeColor = new Color(0.16f, 0.1f, 0.2f);
        static Color carriedFadeColor = new Color(0.16f, 0.1f, 0.2f);

        Portal warpPortal;
        float warpTime;
        Vector3 warpStart;

        Checkpoint activeCheckpoint;
        Vector3 respawnPoint;
        float respawnYaw;
        string pendingScene;
        bool modeChangedThisFrame;

        public Vector3 CameraForwardFlat
        {
            get { return Quaternion.Euler(0f, CameraRig != null ? CameraRig.Yaw : 0f, 0f) * Vector3.forward; }
        }

        // ------------------------------------------------------------------ Boot

        public static void Boot(LevelId level)
        {
            PatchworkAssets.EnsureInit();
            MusicPlayer.EnsurePlaying();
            var go = new GameObject("Patchwork Game");
            var game = go.AddComponent<PatchworkGame>();
            game.Init(level);
        }

        void Init(LevelId levelId)
        {
            Instance = this;
            Time.timeScale = 1f;
            Input = new PatchworkInput();

            // Camera + audio
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 600f;
            camGo.AddComponent<AudioListener>();
            var urpData = cam.GetUniversalAdditionalCameraData();
            if (urpData != null)
            {
                urpData.renderPostProcessing = true;
                urpData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            }
            CameraRig = camGo.AddComponent<PlayerCamera>();
            Sfx.Init(camGo);

            // Level
            var kit = new LevelKit(levelId == LevelId.Hub ? "Hub" : "Level1");
            Level = levelId == LevelId.Hub ? HubBuilder.Build(kit) : Level1Builder.Build(kit);
            SetupEnvironment(cam);

            // Player
            Player = PatchworkPlayer.Create(this, Level.Spawn, Level.SpawnYaw);
            respawnPoint = Level.Spawn;
            respawnYaw = Level.SpawnYaw;
            CameraRig.Target = Player.transform;
            CameraRig.SnapBehind(Level.SpawnYaw);

            Selector = new FormSelector(this);
            Satchel = new SatchelMenu(this);
            Pause = new PauseMenu(this);

            Mode = UIMode.Gameplay;
            Fade = 1f;
            FadeColor = carriedFadeColor;
            carriedFadeColor = DefaultFadeColor;
            TitleTimer = 4.5f;

            Hud = gameObject.AddComponent<PatchworkHUD>();
            Hud.Game = this;
        }

        void SetupEnvironment(Camera cam)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Level.Sky;

            var sunGo = new GameObject("Sun");
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.95f, 0.86f);
            sun.intensity = 1.5f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.75f;
            sunGo.transform.rotation = Quaternion.Euler(52f, -35f, 0f);
            RenderSettings.sun = sun;

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Color.Lerp(Level.Sky, Color.white, 0.2f) * 0.85f;
            RenderSettings.ambientEquatorColor = new Color(0.72f, 0.68f, 0.7f);
            RenderSettings.ambientGroundColor = new Color(0.45f, 0.4f, 0.42f);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = Level.Sky;
            RenderSettings.fogStartDistance = 55f;
            RenderSettings.fogEndDistance = 190f;
            DynamicGI.UpdateEnvironment();
        }

        void OnDestroy()
        {
            if (Input != null) Input.Dispose();
            Time.timeScale = 1f;
            if (Instance == this) Instance = null;
        }

        // ------------------------------------------------------------------ Frame

        void Update()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            float udt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            modeChangedThisFrame = false;
            Input.UpdateDeviceKind();

            switch (Mode)
            {
                case UIMode.Gameplay:
                    if (Input.Pause.WasPressedThisFrame()) OpenPause();
                    else if (Input.FormWheel.WasPressedThisFrame()) OpenWheel();
                    else if (Input.Satchel.WasPressedThisFrame()) OpenSatchel();
                    break;
                case UIMode.FormWheel:
                    Selector.Update(udt);
                    break;
                case UIMode.Satchel:
                    Satchel.Update(udt);
                    break;
                case UIMode.Paused:
                    Pause.Update(udt);
                    break;
                case UIMode.Complete:
                    if (Input.Confirm.WasPressedThisFrame()) LoadScene(HubBuilder.SceneName);
                    break;
                case UIMode.Warping:
                    UpdateWarp(udt);
                    break;
            }

            if (Mode == UIMode.Gameplay || Mode == UIMode.FormWheel)
            {
                PlayerInputFrame frame;
                if (Mode == UIMode.Gameplay && !modeChangedThisFrame)
                {
                    frame = ReadFrame();
                }
                else
                {
                    // Menus open: no new actions, but keep "held" states so an active swing/glide isn't cut off.
                    frame = PlayerInputFrame.Empty;
                    frame.AbilityHeld = Input.Ability.IsPressed();
                    frame.JumpHeld = Input.Jump.IsPressed();
                }
                MovingPlatform.TickAll(dt);
                Player.Tick(frame, dt);

                if (Player.transform.position.y < Level.KillY) Respawn(true);
            }

            Sfx.TickLoops(udt);
            if (ActiveHint != null)
            {
                HintAge += udt;
                if (HintAge >= HintDuration) ActiveHint = null;
            }
            if (ToastTimer > 0f) ToastTimer -= udt;
            if (TitleTimer > 0f) TitleTimer -= udt;
            UpdateFade(udt);
            UpdateCursor();
        }

        void LateUpdate()
        {
            float udt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            Vector2 stick = Vector2.zero, mouse = Vector2.zero;
            if (Mode == UIMode.Gameplay)
            {
                stick = Input.LookStick.ReadValue<Vector2>();
                if (Cursor.lockState == CursorLockMode.Locked) mouse = Input.LookMouse.ReadValue<Vector2>();
            }
            if (Mode != UIMode.Paused && Mode != UIMode.Satchel)
            {
                // Lower the camera focus for small forms so it fits through tunnels.
                float wantHeight = Player.Forms.Definition.HeadHeight * 0.75f + 0.2f;
                CameraRig.TargetHeight = Mathf.MoveTowards(CameraRig.TargetHeight, wantHeight, udt * 2.5f);
                CameraRig.Tick(stick, mouse, udt);
            }
        }

        PlayerInputFrame ReadFrame()
        {
            var f = new PlayerInputFrame();
            f.Move = Vector2.ClampMagnitude(Input.Move.ReadValue<Vector2>(), 1f);
            f.MoveWorld = CameraRig.ToWorld(f.Move);
            f.JumpPressed = Input.Jump.WasPressedThisFrame();
            f.JumpHeld = Input.Jump.IsPressed();
            f.AbilityPressed = Input.Ability.WasPressedThisFrame();
            f.AbilityHeld = Input.Ability.IsPressed();
            return f;
        }

        void UpdateCursor()
        {
            bool lockIt = Mode == UIMode.Gameplay && Input.UsingKeyboard && Application.isFocused;
            Cursor.lockState = lockIt ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !lockIt;
        }

        // ------------------------------------------------------------------ Menus

        void SetMode(UIMode mode, float timeScale)
        {
            Mode = mode;
            Time.timeScale = timeScale;
            modeChangedThisFrame = true;
        }

        void OpenWheel()
        {
            Selector.Open();
            SetMode(UIMode.FormWheel, 0.2f);
            Sfx.Play(SfxId.MenuOpen, 0.5f);
        }

        /// <summary>Closes the form wheel; if confirmed, asks the form controller to transform.</summary>
        public void CloseWheel(bool confirm, FormType choice)
        {
            SetMode(UIMode.Gameplay, 1f);
            if (confirm)
            {
                if (choice == Player.Forms.Current && !Player.Forms.HasPending)
                    ShowToast("Already in " + FormLibrary.Get(choice).DisplayName + ".");
                else
                    Player.Forms.Request(choice);
            }
            else
            {
                Sfx.Play(SfxId.MenuCancel, 0.4f);
            }
        }

        void OpenSatchel()
        {
            Satchel.Open();
            SetMode(UIMode.Satchel, 0f);
            Sfx.Play(SfxId.MenuOpen, 0.5f);
        }

        public void CloseSatchel()
        {
            SetMode(UIMode.Gameplay, 1f);
            if (Hud != null) Hud.PokeBadge();
            Sfx.Play(SfxId.MenuCancel, 0.4f);
        }

        void OpenPause()
        {
            Pause.Open();
            SetMode(UIMode.Paused, 0f);
            Sfx.Play(SfxId.MenuOpen, 0.5f);
        }

        public void ClosePause()
        {
            SetMode(UIMode.Gameplay, 1f);
            Sfx.Play(SfxId.MenuCancel, 0.4f);
        }

        // ------------------------------------------------------------------ Gameplay events

        public void ShowToast(string message)
        {
            ToastText = message;
            ToastTimer = 2.6f;
        }

        public void ShowHint(HintZone zone)
        {
            if (ActiveHint == zone && HintAge < HintDuration) return;
            ActiveHint = zone;
            HintAge = 0f;
            switch (zone.Theme)
            {
                case HintTheme.Water: Sfx.Play(SfxId.HintWater, 0.55f); break;
                case HintTheme.Yarn: Sfx.Play(SfxId.HintYarn, 0.6f); break;
                case HintTheme.Wind: Sfx.Play(SfxId.HintWind, 0.55f); break;
                default: Sfx.Play(SfxId.HintNeutral, 0.45f); break;
            }
        }

        /// <summary>Leaving the zone makes the card slide away early.</summary>
        public void ClearHint(HintZone zone)
        {
            if (ActiveHint == zone && HintAge < HintDuration - 0.45f) HintAge = HintDuration - 0.45f;
        }

        /// <summary>Thread spools picked up this visit.</summary>
        public int ThreadsCollected { get; private set; }

        public void CollectThread()
        {
            ThreadsCollected++;
            if (Hud != null) Hud.PokeThreads();
        }

        public void SetCheckpoint(Checkpoint cp)
        {
            if (activeCheckpoint == cp) return;
            if (activeCheckpoint != null) activeCheckpoint.SetActiveVisual(false);
            bool first = activeCheckpoint == null && Vector3.Distance(cp.SpawnPoint, Level.Spawn) < 1f;
            activeCheckpoint = cp;
            cp.SetActiveVisual(true);
            respawnPoint = cp.SpawnPoint;
            respawnYaw = cp.SpawnYaw;
            if (!first)
            {
                Sfx.Play(SfxId.Checkpoint, 0.6f);
                ShowToast("Checkpoint stitched!");
            }
        }

        public void Respawn(bool fell)
        {
            Player.Respawn(respawnPoint, respawnYaw);
            CameraRig.SnapBehind(respawnYaw);
            Sfx.Play(SfxId.Respawn, 0.5f);
            TransformBurst.Spawn(respawnPoint + Vector3.up, new Color(1f, 1f, 1f), FormLibrary.Get(Player.Forms.Current).PrimaryColor, 12, 0.7f);
            if (fell) ShowToast("Oops! Back to the last checkpoint.");
        }

        public void CompleteLevel()
        {
            if (Mode != UIMode.Gameplay && Mode != UIMode.FormWheel) return;
            GameState.SetLevelComplete(Level.Id);
            SetMode(UIMode.Complete, 1f);
            Player.Forms.CancelAll();
            Sfx.Play(SfxId.Complete, 0.8f);
            TransformBurst.Spawn(Player.Motor.Center + Vector3.up, new Color(1f, 0.8f, 0.25f), Color.white, 30, 1.4f);
        }

        /// <summary>Starts the portal warp: Patch is pulled into the vortex, spins, shrinks, then the scene loads.</summary>
        public void EnterPortal(Portal portal)
        {
            if (Mode != UIMode.Gameplay && Mode != UIMode.FormWheel) return;
            Player.Forms.CancelAll();
            warpPortal = portal;
            warpTime = 0f;
            warpStart = Player.transform.position;
            SetMode(UIMode.Warping, 1f);
            Player.Motor.Controller.enabled = false;
            portal.Excite();
            Sfx.Play(SfxId.PortalWarp, 0.8f);
        }

        void UpdateWarp(float udt)
        {
            const float Duration = 1.1f;
            warpTime += udt;
            float k = Mathf.Clamp01(warpTime / Duration);
            float ease = k * k * (3f - 2f * k);
            Vector3 target = warpPortal != null ? warpPortal.Center - Vector3.up * 0.5f : Player.transform.position;
            Player.transform.position = Vector3.Lerp(warpStart, target, ease);
            Player.Visuals.SetWarp(1f - ease);
            Player.Visuals.Tick(Player.Motor, udt);
            if (k > 0.55f && Mathf.Repeat(warpTime, 0.12f) < udt) FxBurst.Spawn(FxBurst.Kind.Sparkle, target, new Color(0.85f, 0.7f, 1f));
            if (k >= 1f && warpPortal != null)
            {
                string scene = warpPortal.TargetScene;
                warpPortal = null;
                FadeColor = new Color(0.97f, 0.93f, 1f);
                carriedFadeColor = FadeColor;
                LoadScene(scene);
            }
        }

        public void LoadScene(string sceneName)
        {
            if (Mode == UIMode.Loading) return;
            if (carriedFadeColor == DefaultFadeColor) FadeColor = DefaultFadeColor;
            GameState.Save();
            pendingScene = sceneName;
            SetMode(UIMode.Loading, 1f);
            Player.Forms.CancelAll();
            Sfx.StopLoops();
        }

        public void QuitGame()
        {
            GameState.Save();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void UpdateFade(float udt)
        {
            if (Mode == UIMode.Loading)
            {
                Fade = Mathf.MoveTowards(Fade, 1f, udt * 3f);
                if (Fade >= 1f && !string.IsNullOrEmpty(pendingScene))
                {
                    string s = pendingScene;
                    pendingScene = null;
                    Time.timeScale = 1f;
                    SceneManager.LoadScene(s);
                }
            }
            else
            {
                Fade = Mathf.MoveTowards(Fade, 0f, udt * 2f);
            }
        }
    }
}

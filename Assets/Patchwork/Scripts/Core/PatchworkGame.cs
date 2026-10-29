using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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
        Loading,
        Dance,      // the D-pad easter egg: Patch dances, the camera turns it into a little cutscene
        Intro,      // studio logo and title screen (first time the Skiff loads)
        LevelSelect, // at the Skiff's helm, turning the globe
        Talk         // chatting with an NPC (Captain Yarny)
    }

    // Main game object, one per scene. Builds the level, camera, light and player,
    // then runs everything from one Update: input, then menus, then player, then camera.
    // Only one menu can be open at once so the wheel/pause/satchel never clash.
    public class PatchworkGame : MonoBehaviour
    {
        public static PatchworkGame Instance { get; private set; }

        public PatchworkInput Input { get; private set; }
        public PatchworkPlayer Player { get; private set; }
        public PlayerCamera CameraRig { get; private set; }
        public LevelInfo Level { get; private set; }
        public UIMode Mode { get; private set; }
        public PatchworkHUD Hud { get; private set; }
        public DanceParty Dance { get; private set; }
        // Only in the Test Gym: measures jump distance and height.
        public JumpMeter Meter { get; private set; }

        public FormSelector Selector { get; private set; }
        public SatchelMenu Satchel { get; private set; }
        public PauseMenu Pause { get; private set; }
        public TitleSequence Title { get; private set; }
        public LevelSelectMenu LevelSelect { get; private set; }
        // The helm you're standing at on the Skiff (null when you're not at it).
        public HelmStation Helm { get; private set; }
        // The NPC you're close enough to talk to (null if none).
        public TalkZone Talker { get; private set; }
        public DialogueBox Dialogue { get; private set; }
        // The ears: sit between the camera and Patch, so sounds get louder as Patch walks up to them.
        Transform listener;

        // Messages
        public string ToastText { get; private set; }
        public float ToastTimer { get; private set; }
        public HintZone ActiveHint { get; private set; }
        // How long a tutorial card stays up before sliding away.
        public const float HintDuration = 7f;
        public float HintAge { get; private set; }
        bool hintPlayerInside;
        public bool HintHeld { get { return hintPlayerInside; } }
        // After walking out of a hint zone, the card stays this long before sliding away.
        const float HintLinger = 1.5f;
        const float HintSlideOut = 0.45f;
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
        bool modeChangedThisFrame;

        public Vector3 CameraForwardFlat
        {
            get { return Quaternion.Euler(0f, CameraRig != null ? CameraRig.Yaw : 0f, 0f) * Vector3.forward; }
        }

        // ------------------------------------------------------------------ Boot

        public static void ResetStatics()
        {
            Instance = null;
            carriedFadeColor = DefaultFadeColor;
        }

        public static void Boot(LevelId level)
        {
            PatchworkAssets.EnsureInit();
            MusicPlayer.EnsurePlaying();
            var go = new GameObject("Patchwork Game");
            var game = go.AddComponent<PatchworkGame>();
            game.Init(level);
        }

        LevelId currentLevel;

        void Init(LevelId levelId)
        {
            Instance = this;
            currentLevel = levelId;
            Time.timeScale = 1f;
            // VSync is on in Quality settings; this cap only matters if a graphics driver forces it off,
            // so the game never runs at hundreds of frames per second and heats up people's computers.
            Application.targetFrameRate = 120;
            GameSettings.ApplyVSync();
            Input = new PatchworkInput();

            // Camera + audio
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 600f;
            var ears = new GameObject("Listener");
            ears.AddComponent<AudioListener>();
            listener = ears.transform;
            var urpData = cam.GetUniversalAdditionalCameraData();
            if (urpData != null)
            {
                urpData.renderPostProcessing = true;
                urpData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            }
            CameraRig = camGo.AddComponent<PlayerCamera>();
            Sfx.Init(camGo);
            Dance = new DanceParty(camGo);

            // Level
            var kit = new LevelKit(levelId.ToString());
            switch (levelId)
            {
                case LevelId.Level1: Level = Level1Builder.Build(kit); break;
                case LevelId.Gym: Level = GymBuilder.Build(kit); Meter = new JumpMeter(); break;
                default: Level = HubBuilder.Build(kit); break;
            }
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

            Title = new TitleSequence(this);
            LevelSelect = new LevelSelectMenu(this);
            Dialogue = new DialogueBox(this);

            Mode = UIMode.Gameplay;
            // Coming from the loading screen, it's still covering everything, so skip our own fade.
            Fade = LoadingScreen.Busy ? 0f : 1f;
            FadeColor = carriedFadeColor;
            carriedFadeColor = DefaultFadeColor;
            TitleTimer = LoadingScreen.Busy ? 5.2f : 4.5f;

            Hud = gameObject.AddComponent<PatchworkHUD>();
            Hud.Game = this;

            // First time on the Skiff since the game started: studio logo, then the title screen.
            if (Level.IsHub && TitleSequence.ShouldPlay() && !LoadingScreen.Busy)
            {
                Mode = UIMode.Intro;
                Fade = 0f;
                TitleTimer = 0f;
                Title.Start();
            }
            ApplyAudioMode();
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
            // The scenes still point at Unity's default blue sky; it doesn't match the pastel rooms.
            RenderSettings.skybox = null;
            DynamicGI.UpdateEnvironment();
            AddReflectionProbe();
        }

        // One snapshot of the room for shiny things (thimbles, needles, the bubble) to reflect,
        // so they show the room's colours instead of looking like grey plastic. Taken once at load.
        void AddReflectionProbe()
        {
            Bounds area = Level.Area;
            if (area.size.sqrMagnitude < 1f) area = new Bounds(new Vector3(0f, 8f, 0f), new Vector3(60f, 30f, 60f));
            // The walls reach far below the floor; the probe only needs the playable height.
            float bottom = Mathf.Max(area.min.y, -10f);
            var box = new Bounds();
            box.SetMinMax(new Vector3(area.min.x, bottom, area.min.z), area.max);

            var go = new GameObject("Room Reflections");
            go.transform.position = new Vector3(box.center.x, Mathf.Min(6f, box.max.y - 1f), box.center.z);
            var probe = go.AddComponent<ReflectionProbe>();
            probe.mode = ReflectionProbeMode.Realtime;
            probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
            probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;
            probe.boxProjection = true;
            probe.size = box.size;
            probe.center = box.center - go.transform.position;
            probe.resolution = 128;
            probe.hdr = true;
            probe.clearFlags = ReflectionProbeClearFlags.SolidColor;
            probe.backgroundColor = Level.Sky;
            probe.cullingMask = ~(1 << 2); // leave Patch out of his own reflections
            probe.shadowDistance = 0f;
            probe.importance = 1;
            probe.RenderProbe();
        }

        void OnApplicationQuit()
        {
            GameState.Save();
        }

        void OnDestroy()
        {
            if (Input != null) Input.Dispose();
            Haptics.StopAll();
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
                    else if (Helm != null && Input.Ability.WasPressedThisFrame() && Player.Motor.Grounded) OpenLevelSelect();
                    else if (Talker != null && Talker.Npc != null && Input.Ability.WasPressedThisFrame() && Player.Motor.Grounded) StartTalk();
                    else if (FacePressed())
                    {
                        SetPatchFace(pressedFace, true);
                        if (Dance.RegisterTap() && CanDance()) StartDance();
                    }
                    break;
                case UIMode.Dance:
                    if (Input.Pause.WasPressedThisFrame() || Input.Cancel.WasPressedThisFrame()) EndDance();
                    else
                    {
                        // Keep tapping to keep dancing; each tap also swaps his face (quietly).
                        if (FacePressed()) { Dance.RegisterTap(); SetPatchFace(pressedFace, false); }
                        if (Dance.TimedOut) EndDance();
                    }
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
                case UIMode.Intro:
                    if (Title.Update(udt))
                    {
                        SetMode(UIMode.Gameplay, 1f);
                        TitleTimer = 4.5f;
                    }
                    break;
                case UIMode.LevelSelect:
                    LevelSelect.Update(udt);
                    break;
                case UIMode.Talk:
                    Dialogue.Update(udt);
                    break;
            }

            if (Mode == UIMode.Gameplay || Mode == UIMode.FormWheel || Mode == UIMode.Dance || Mode == UIMode.Intro || Mode == UIMode.LevelSelect || Mode == UIMode.Talk)
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
                if (Meter != null) Meter.Tick(Player.Motor);

                if (Player.transform.position.y < Level.KillY || (Player.Motor.Center.y < Level.FailY && !Swinging())) Respawn(true);
            }

            if (Dance.Update(udt) && Mode == UIMode.Dance) Haptics.Pulse(0.15f, 0.05f, 0.08f);
            Player.Visuals.DanceBeat = Dance.Beat;
            // Background ambience for the level (muffled with everything else while paused).
            if (currentLevel == LevelId.Hub) Sfx.RequestLoop(LoopId.Skiff);
            else Sfx.RequestLoop(LoopId.Meadow);
            Sfx.TickLoops(udt);
            MusicPlayer.SetForm(Player.Forms.Current);
            Haptics.Tick(udt);
            if (ActiveHint != null)
            {
                HintAge += udt;
                // The card waits while you're standing in its zone, so slow readers can finish it.
                if (hintPlayerInside) HintAge = Mathf.Min(HintAge, HintDuration - HintSlideOut - 0.01f);
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
                CameraRig.TargetGrounded = Player.Motor.Grounded;
                CameraRig.TargetVelocity = Player.Motor.Velocity;
                var yarn = Player.Forms.Ability as YarnAbility;
                CameraRig.Swinging = yarn != null && yarn.Attached;
                if (CameraRig.Swinging) CameraRig.SwingAnchor = yarn.AnchorPosition;
                var wind = Player.Forms.Ability as WindAbility;
                CameraRig.Gliding = wind != null && wind.Gliding;
                CameraRig.Tick(stick, mouse, udt);
            }
            if (listener != null && CameraRig != null && Player != null)
            {
                Transform cam = CameraRig.transform;
                listener.position = Vector3.Lerp(cam.position, Player.Motor.Center + Vector3.up * 0.3f, 0.7f);
                listener.rotation = cam.rotation;
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
            bool lockIt = (Mode == UIMode.Gameplay || Mode == UIMode.Dance) && Input.UsingKeyboard && Application.isFocused;
            Cursor.lockState = lockIt ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !lockIt;
        }

        // ------------------------------------------------------------------ Menus

        void SetMode(UIMode mode, float timeScale)
        {
            Mode = mode;
            Time.timeScale = timeScale;
            modeChangedThisFrame = true;
            ApplyAudioMode();
        }

        // Pause and Satchel sound "behind glass" (quieter, muffled music); the Form Wheel muffles
        // music and loops so time feels like it thickens. Rumble stops in all three.
        void ApplyAudioMode()
        {
            switch (Mode)
            {
                case UIMode.Paused:
                case UIMode.Satchel:
                    MusicPlayer.SetMuffle(900f, 0.5f);
                    Sfx.SetLoopCutoff(900f);
                    Haptics.SetSilenced(true);
                    break;
                case UIMode.Dance:
                    // The dance song takes over from the background music.
                    MusicPlayer.SetMuffle(22000f, 0f);
                    Sfx.SetLoopCutoff(22000f);
                    Haptics.SetSilenced(false);
                    break;
                case UIMode.FormWheel:
                    MusicPlayer.SetMuffle(1500f, 1f);
                    Sfx.SetLoopCutoff(1500f);
                    Haptics.SetSilenced(true);
                    break;
                case UIMode.Intro:
                    // The title sequence sets the music itself (quiet under the studio logo).
                    Sfx.SetLoopCutoff(22000f);
                    Haptics.SetSilenced(true);
                    break;
                case UIMode.Talk:
                    MusicPlayer.SetMuffle(22000f, 0.6f);
                    Sfx.SetLoopCutoff(22000f);
                    Haptics.SetSilenced(true);
                    break;
                case UIMode.LevelSelect:
                    MusicPlayer.SetMuffle(5000f, 0.85f);
                    Sfx.SetLoopCutoff(3000f);
                    Haptics.SetSilenced(true);
                    break;
                default:
                    MusicPlayer.SetMuffle(22000f, 1f);
                    Sfx.SetLoopCutoff(22000f);
                    Haptics.SetSilenced(false);
                    break;
            }
        }

        void OpenWheel()
        {
            Selector.Open();
            SetMode(UIMode.FormWheel, 0.2f);
            Sfx.Play(SfxId.MenuOpen, 0.5f);
        }

        // Closes the form wheel; if confirmed, asks the form controller to transform.
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

        // ------------------------------------------------------------------ The Skiff's helm

        public void SetHelm(HelmStation station) { Helm = station; }

        public void ClearHelm(HelmStation station) { if (Helm == station) Helm = null; }

        public void SetTalker(TalkZone zone) { Talker = zone; }

        public void ClearTalker(TalkZone zone) { if (Talker == zone) Talker = null; }

        void StartTalk()
        {
            Player.Forms.CancelAll();
            Player.Motor.Velocity = Vector3.zero;
            Dialogue.Open(Talker.Npc);
            SetMode(UIMode.Talk, 1f);
            ToastTimer = 0f;
            if (ActiveHint != null) HintAge = HintDuration;
        }

        public void EndTalk()
        {
            Dialogue.Close();
            SetMode(UIMode.Gameplay, 1f);
            Sfx.Play(SfxId.MenuCancel, 0.3f);
        }

        void OpenLevelSelect()
        {
            Player.Forms.CancelAll();
            Player.Motor.Velocity = Vector3.zero;
            LevelSelect.Open();
            SetMode(UIMode.LevelSelect, 1f);
            ToastTimer = 0f;
            if (ActiveHint != null) HintAge = HintDuration;
            Sfx.Play(SfxId.MenuOpen, 0.5f);
        }

        public void CloseLevelSelect()
        {
            LevelSelect.Close();
            SetMode(UIMode.Gameplay, 1f);
            Sfx.Play(SfxId.MenuCancel, 0.4f);
        }

        // Chosen a level on the globe: off we go.
        public void SetSail(string scene)
        {
            if (string.IsNullOrEmpty(scene)) return;
            LoadScene(scene);
        }

        // ------------------------------------------------------------------ Gameplay events

        public void ShowToast(string message)
        {
            ToastText = message;
            ToastTimer = 2.6f;
        }

        public void ShowHint(HintZone zone)
        {
            if (ActiveHint == zone && HintAge < HintDuration)
            {
                // Walked back in before it slid away: keep it up.
                hintPlayerInside = true;
                HintAge = Mathf.Min(HintAge, HintDuration - HintSlideOut - 0.01f);
                return;
            }
            // Level cards only show the first time; the hub's practice cards always show.
            if (!Level.IsHub)
            {
                string id = Level.Id + "_" + zone.Title;
                if (GameState.IsHintSeen(id)) return;
                GameState.SetHintSeen(id);
            }
            ActiveHint = zone;
            HintAge = 0f;
            hintPlayerInside = true;
            switch (zone.Theme)
            {
                case HintTheme.Water: Sfx.Play(SfxId.HintWater, 0.55f); break;
                case HintTheme.Yarn: Sfx.Play(SfxId.HintYarn, 0.6f); break;
                case HintTheme.Wind: Sfx.Play(SfxId.HintWind, 0.55f); break;
                default: Sfx.Play(SfxId.HintNeutral, 0.45f); break;
            }
        }

        // Leaving the zone makes the card slide away early.
        public void ClearHint(HintZone zone)
        {
            if (ActiveHint != zone) return;
            hintPlayerInside = false;
            HintAge = Mathf.Max(HintAge, HintDuration - HintSlideOut - HintLinger);
        }

        // Thread spools picked up this visit.
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

        bool Swinging()
        {
            var yarn = Player.Forms.Ability as YarnAbility;
            return yarn != null && yarn.Attached;
        }

        public void Respawn(bool fell)
        {
            if (fell)
            {
                // Quick dip to dark so the jump back to the checkpoint isn't a jarring cut.
                FadeColor = DefaultFadeColor;
                Fade = Mathf.Max(Fade, 0.65f);
            }
            Player.Respawn(respawnPoint, respawnYaw);
            CameraRig.SnapBehind(respawnYaw);
            Sfx.Play(SfxId.Respawn, 0.5f);
            if (fell) Sfx.Voice(SfxId.VoiceAww, Player.Forms.Current, 0.45f);
            FxBurst.Spawn(FxBurst.Kind.Scraps, respawnPoint + Vector3.up, FormLibrary.Get(Player.Forms.Current).PrimaryColor);
            if (fell) ShowToast("Oops! Back to the last checkpoint.");
        }

        PatchModel.Face pressedFace;

        bool FacePressed()
        {
            if (Input.FaceHappy.WasPressedThisFrame()) { pressedFace = PatchModel.Face.Happy; return true; }
            if (Input.FaceSad.WasPressedThisFrame()) { pressedFace = PatchModel.Face.Sad; return true; }
            if (Input.FaceAngry.WasPressedThisFrame()) { pressedFace = PatchModel.Face.Angry; return true; }
            return false;
        }

        // Swap Patch's face (every form has happy / sad / angry). A little pop and sound so it's obvious.
        void SetPatchFace(PatchModel.Face face, bool withSound)
        {
            Player.Visuals.SetFace(face);
            Player.Visuals.PunchScale(new Vector3(1.08f, 0.94f, 1.08f));
            if (withSound) Sfx.Play(SfxId.HintYarn, 0.3f);
        }

        // ------------------------------------------------------------------ Dance easter egg

        bool CanDance()
        {
            var yarn = Player.Forms.Ability as YarnAbility;
            return Mode == UIMode.Gameplay && Player.Motor.Grounded && !Player.Forms.HasPending
                   && (yarn == null || !yarn.Attached);
        }

        void StartDance()
        {
            Player.Forms.CancelAll();
            Player.Motor.Velocity = Vector3.zero;
            SetMode(UIMode.Dance, 1f);
            Dance.Start(Player.Forms.Current);
            Player.Visuals.SetDancing(true);
            CameraRig.Cutscene = true;
            ToastTimer = 0f;
            if (ActiveHint != null) HintAge = HintDuration;
            FxBurst.Spawn(FxBurst.Kind.Sparkle, Player.Motor.Center + Vector3.up * 0.5f, new Color(1f, 0.85f, 0.4f));
        }

        void EndDance()
        {
            Dance.Stop();
            Player.Visuals.SetDancing(false);
            CameraRig.Cutscene = false;
            SetMode(UIMode.Gameplay, 1f);
        }

        public void CompleteLevel()
        {
            if (Mode != UIMode.Gameplay && Mode != UIMode.FormWheel) return;
            GameState.SetLevelComplete(Level.Id);
            // Best thread count unlocks hats.
            bool topHat = GameState.IsHatUnlocked(2), daisy = GameState.IsHatUnlocked(3);
            bool newBest = GameState.RecordThreads(Level.Id, ThreadsCollected, Level.ThreadTotal);
            if (!daisy && GameState.IsHatUnlocked(3)) ShowToast("Every thread! New hat: Daisy Clip (see the Satchel).");
            else if (!topHat && GameState.IsHatUnlocked(2)) ShowToast("Half the threads! New hat: Felt Top Hat (see the Satchel).");
            else if (newBest && Level.ThreadTotal > 0) ShowToast("New best: " + ThreadsCollected + " threads!");
            SetMode(UIMode.Complete, 1f);
            Player.Forms.CancelAll();
            Sfx.Play(SfxId.Complete, 0.8f);
            FxBurst.Spawn(FxBurst.Kind.Scraps, Player.Motor.Center + Vector3.up, new Color(1f, 0.8f, 0.25f));
            FxBurst.Spawn(FxBurst.Kind.Sparkle, Player.Motor.Center + Vector3.up, Color.white);
        }

        // Starts the portal warp: Patch is pulled into the vortex, spins, shrinks, then the scene loads.
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

        // Every scene change goes through the loading screen (quilt patches, the level card, the
        // yarn-ball loading bar).
        public void LoadScene(string sceneName)
        {
            if (Mode == UIMode.Loading || LoadingScreen.Busy) return;
            GameState.Save();
            SetMode(UIMode.Loading, 1f);
            Player.Forms.CancelAll();
            Sfx.StopLoops();
            Haptics.StopAll();
            LoadingScreen.Begin(sceneName);
        }

        public void QuitGame()
        {
            GameState.Save();
            Haptics.StopAll();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void UpdateFade(float udt)
        {
            Fade = Mathf.MoveTowards(Fade, 0f, udt * 2f);
        }
    }
}

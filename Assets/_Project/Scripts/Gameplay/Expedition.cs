using UnityEngine;
using UnityEngine.SceneManagement;

namespace DesertRush
{
    public enum RunState { Menu, Running, Paused, Finished }

    public sealed class Expedition : MonoBehaviour
    {
        public ExpeditionCatalog catalog;
        public DuneCamera chase;
        public DesertAtmosphere atmosphere;
        public ExpeditionUI interfacePrefab;
        public bool startImmediately;

        public RelicEffects Effects { get; } = new();
        public RunnerMotor Motor { get; private set; }
        public RunState State { get; private set; }
        public float Clock { get; private set; }
        public float Distance { get; private set; }
        public float Speed { get; private set; }
        public int Coins { get; private set; }
        public int Score => Mathf.FloorToInt(Distance) + Coins * 15 + bonus;

        private EndlessRoute route;
        private ExpeditionAudio audioRig;
        private ExpeditionUI ui;
        private ExplorerPose pose;
        private GameObject board;
        private IRunnerInput input = new DesktopInput();
        private float runTime, stepTimer, grace, initialBest;
        private int bonus;

        private void Start()
        {
            Application.targetFrameRate = 120;
            ExpeditionSave.ApplySettings();
            Motor = Instantiate(catalog.explorer);
            pose = Motor.GetComponent<ExplorerPose>();
            var holder = new GameObject("Streaming route");
            route = new EndlessRoute(holder.transform, catalog);
            audioRig = gameObject.AddComponent<ExpeditionAudio>();
            audioRig.Bind(catalog.sounds);
            board = Instantiate(catalog.glider, Motor.transform);
            atmosphere.Bind(Motor.transform, chase.transform);
            ui = Instantiate(interfacePrefab);
            ui.Bind(this);
            if (startImmediately) Begin();
            else
            {
                ResetRun(17551);
                Show(RunState.Menu);
            }
        }

        public void UseInput(IRunnerInput provider) => input = provider ?? new DesktopInput();

        public void Begin()
        {
            if (State == RunState.Menu && !startImmediately)
            {
                ui.ShowLoading();
                SceneManager.LoadSceneAsync("Gameplay");
                return;
            }

            ResetRun(unchecked(System.Environment.TickCount * 397));
            initialBest = ExpeditionSave.Best;
            Show(RunState.Running);
        }

        public void Menu()
        {
            if (startImmediately)
            {
                ui.ShowLoading();
                SceneManager.LoadSceneAsync("MainMenu");
            }
        }

        public void Dress() => pose.Dress(catalog.outfits[ExpeditionSave.Outfit]);

        public void TogglePause()
        {
            if (State == RunState.Running) Show(RunState.Paused);
            else if (State == RunState.Paused) Show(RunState.Running);
        }

        private void ResetRun(int seed)
        {
            Distance = runTime = Speed = stepTimer = grace = 0f;
            Coins = bonus = 0;
            Effects.Clear();
            Motor.ResetMotor();
            pose.ResetPose();
            Dress();
            route.Reset(seed);
            atmosphere.ClearTrails();
            board.SetActive(false);
            chase.transform.position = new Vector3(0, 3.3f, -7f);
        }

        private void Update()
        {
            float dt = Mathf.Min(Time.unscaledDeltaTime, .035f);
            if (dt <= 0 || Motor == null) return;
            RunnerInput command = input.Read();
            if (command.Pause) TogglePause();
            if (State == RunState.Paused)
            {
                audioRig.Mix(0f, true);
                return;
            }

            Clock += dt;
            if (State == RunState.Running) Run(command, dt);
            bool running = State == RunState.Running;
            bool gliding = running && Effects.Active(PickupKind.Glider);
            board.SetActive(gliding);
            if (gliding)
            {
                board.transform.localPosition = Vector3.up * (.76f + Mathf.Sin(Clock * 8f) * .07f);
                board.transform.localRotation = Quaternion.Euler(0, 0, -Motor.LateralVelocity * 1.4f);
            }

            pose.Pose(Motor, running ? Speed : 0f, Clock, dt, State == RunState.Finished, gliding);
            chase.Track(Motor.transform.position, running ? Speed : 0f, dt, State == RunState.Menu, Clock);
            audioRig.Mix(atmosphere.Tick(runTime, Clock, running, Motor.Grounded,
                Effects.Active(PickupKind.Boost)), false);
        }

        private void Run(RunnerInput command, float dt)
        {
            Effects.Tick(dt);
            grace = Mathf.Max(0f, grace - dt);
            runTime += dt;
            float targetSpeed = Mathf.Min(27f, 11.5f + runTime * .185f)
                + (Effects.Active(PickupKind.Boost) ? 7f : 0f);
            Speed = Mathf.MoveTowards(Speed, targetSpeed, dt * (Speed < 12f ? 18f : 9f));
            Vector3 from = Motor.transform.position;
            Motor.Tick(command, Speed, dt);
            Vector3 to = Motor.transform.position;
            Distance += to.z - from.z;
            route.Interact(this, from, to, dt);
            if (State != RunState.Running) return;
            route.Stream(to.z);

            if (to.z > 650f)
            {
                const float offset = 570f;
                route.Rebase(offset);
                Motor.Rebase(offset);
                chase.Rebase(offset);
                atmosphere.ClearTrails();
            }

            if (Motor.Jumped) audioRig.Play(1);
            if (Motor.StartedSlide) audioRig.Play(2);
            if (Motor.Landed) audioRig.Play(0, .78f);
            if (Motor.Grounded && !Motor.Sliding && !Effects.Active(PickupKind.Glider))
            {
                stepTimer -= dt;
                if (stepTimer <= 0f)
                {
                    audioRig.Play(0, .9f + Mathf.Sin(Clock * 18f) * .13f);
                    stepTimer = Mathf.Lerp(.32f, .22f, Speed / RoutePlanner.MaximumSpeed);
                }
            }
            ui.UpdateHud();
        }

        public void Collect(PickupKind kind, Vector3 position)
        {
            Coins += Effects.Collect(kind);
            if (kind == PickupKind.Gem) bonus += 70;
            if (kind == PickupKind.Idol) bonus += 200;
            audioRig.Play(kind == PickupKind.Coin ? 3 : 4, kind == PickupKind.Coin ? 1f + (Coins % 5) * .065f : 1f);
            atmosphere.Burst(position, false);
        }

        public bool Hit(Vector3 position)
        {
            if (grace > 0f) return true;
            chase.Impact();
            atmosphere.Burst(position, true);
            audioRig.Play(5);
            if (Effects.AbsorbImpact())
            {
                grace = .38f;
                return true;
            }

            ExpeditionSave.Record(Score, (int)Distance, Coins);
            audioRig.Play(6);
            Show(RunState.Finished);
            return false;
        }

        public bool NewBest => Score > initialBest;

        private void Show(RunState state)
        {
            State = state;
            ui.Show(state);
        }
    }
}

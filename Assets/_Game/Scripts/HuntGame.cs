using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Wildlands
{
    public sealed class HuntGame : MonoBehaviour
    {
        public Transform Player { get; private set; }
        private CharacterController controller;
        private Camera view;
        private Transform weapon;
        private TouchControls movement, look;
        private PerformanceSettings performance;
        private Text status, notice, settingsLabel;
        private GameObject settings;
        private float pitch, fallSpeed, nextShot;
        private bool aiming, reloading;
        private int gun, ammo = 5, harvested;
        private readonly int[] capacity = { 5, 2 };
        private readonly string[] gunNames = { "BOLT RIFLE", "SHOTGUN" };
        private Material bark, foliage, fur, dark, metal;
        private Font font;

        private void Awake()
        {
            Random.InitState(7319);
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            BuildWorld();
            BuildPlayer();
            performance = gameObject.AddComponent<PerformanceSettings>();
            BuildUI();
            ShowNotice("Find wildlife near the meadow. Harvest 3 animals.");
        }

        private Material Material(Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            return material;
        }
        private GameObject Shape(string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material, Transform parent = null, bool solid = true)
        {
            var obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position;
            obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().sharedMaterial = material;
            if (!solid) Destroy(obj.GetComponent<Collider>());
            return obj;
        }
        private void BuildWorld()
        {
            RenderSettings.ambientLight = new Color(0.58f, 0.66f, 0.73f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.62f, 0.73f, 0.76f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.004f;
            var sun = new GameObject("Morning sunlight").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.7f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(38, -32, 0);
            bark = Material(new Color(0.24f, 0.15f, 0.09f));
            foliage = Material(new Color(0.17f, 0.32f, 0.14f));
            fur = Material(new Color(0.43f, 0.27f, 0.14f));
            dark = Material(new Color(0.12f, 0.14f, 0.13f));
            metal = Material(new Color(0.22f, 0.25f, 0.26f));
            Shape("Meadow", PrimitiveType.Cube, new Vector3(0, -0.3f, 0), new Vector3(200, 0.6f, 200), Material(new Color(0.37f, 0.43f, 0.2f)));
            var stone = Material(new Color(0.42f, 0.45f, 0.42f));
            // Keep the center meadow open; forest occupies the outer ring.
            for (int i = 0; i < 180; i++)
            {
                Vector2 p = Random.insideUnitCircle * 96;
                if (p.magnitude < 29) continue;
                float height = Random.Range(5f, 10f);
                var tree = new GameObject("Forest tree").transform;
                tree.position = new Vector3(p.x, 0, p.y);
                Shape("Trunk", PrimitiveType.Cylinder, Vector3.up * height * 0.38f, new Vector3(0.65f, height * 0.38f, 0.65f), bark, tree);
                Shape("Canopy", PrimitiveType.Sphere, Vector3.up * height, new Vector3(5, height * 0.75f, 5), foliage, tree, false);
            }
            for (int i = 0; i < 35; i++)
            {
                Vector2 p = Random.insideUnitCircle * 95;
                if (p.magnitude < 18) continue;
                Shape("Rock", PrimitiveType.Sphere, new Vector3(p.x, 0.3f, p.y), new Vector3(3, 1.8f, 2), stone);
            }
            // Invisible perimeter keeps the prototype traversal inside its playable area.
            Shape("North boundary", PrimitiveType.Cube, new Vector3(0, 4, 100), new Vector3(205, 10, 1), stone).GetComponent<Renderer>().enabled = false;
            Shape("South boundary", PrimitiveType.Cube, new Vector3(0, 4, -100), new Vector3(205, 10, 1), stone).GetComponent<Renderer>().enabled = false;
            Shape("East boundary", PrimitiveType.Cube, new Vector3(100, 4, 0), new Vector3(1, 10, 205), stone).GetComponent<Renderer>().enabled = false;
            Shape("West boundary", PrimitiveType.Cube, new Vector3(-100, 4, 0), new Vector3(1, 10, 205), stone).GetComponent<Renderer>().enabled = false;
            for (int i = 0; i < 12; i++) SpawnAnimal(i);
        }
        private void SpawnAnimal(int index)
        {
            var root = new GameObject("Wildlife").transform;
            float angle = index * 137.5f * Mathf.Deg2Rad;
            root.position = new Vector3(Mathf.Sin(angle) * Random.Range(18, 55), 0, Mathf.Cos(angle) * Random.Range(18, 55));
            bool boar = index % 3 == 2;
            Shape("Body", PrimitiveType.Sphere, new Vector3(0, 1, 0), new Vector3(boar ? 1 : 0.7f, 0.8f, 1.6f), fur, root);
            Shape("Neck", PrimitiveType.Capsule, new Vector3(0, 1.35f, 0.65f), new Vector3(0.35f, 0.5f, 0.4f), fur, root);
            Shape("Head", PrimitiveType.Sphere, new Vector3(0, 1.75f, 0.9f), new Vector3(0.45f, 0.4f, 0.65f), fur, root);
            var legs = new Transform[4];
            for (int l = 0; l < 4; l++)
                legs[l] = Shape("Leg", PrimitiveType.Cylinder, new Vector3(l < 2 ? -0.24f : 0.24f, 0.45f, l % 2 == 0 ? -0.5f : 0.5f), new Vector3(0.13f, 0.45f, 0.13f), fur, root).transform;
            if (!boar)
                for (int a = -1; a <= 1; a += 2)
                    Shape("Antler", PrimitiveType.Cylinder, new Vector3(a * 0.18f, 2.1f, 0.8f), new Vector3(0.07f, 0.3f, 0.07f), bark, root);
            root.gameObject.AddComponent<Wildlife>().Initialize(this, boar ? "Boar" : index % 3 == 0 ? "Deer" : "Gazelle", boar ? 120 : 90, legs);
        }
        private void BuildPlayer()
        {
            Player = new GameObject("Hunter").transform;
            Player.position = new Vector3(0, 0.1f, -12);
            controller = Player.gameObject.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = Vector3.up * 0.9f;
            view = new GameObject("Hunter camera").AddComponent<Camera>();
            view.tag = "MainCamera";
            view.transform.SetParent(Player, false);
            view.transform.localPosition = Vector3.up * 1.65f;
            view.nearClipPlane = 0.05f;
            view.farClipPlane = 220;
            view.clearFlags = CameraClearFlags.SolidColor;
            view.backgroundColor = new Color(0.62f, 0.73f, 0.79f);
            view.gameObject.AddComponent<AudioListener>();
            weapon = new GameObject("Weapon placeholder").transform;
            weapon.SetParent(view.transform, false);
            weapon.localPosition = new Vector3(0.23f, -0.23f, 0.45f);
            Shape("Stock", PrimitiveType.Cube, Vector3.zero, new Vector3(0.09f, 0.12f, 0.4f), bark, weapon, false);
            var barrel = Shape("Barrel", PrimitiveType.Cylinder, new Vector3(0, 0.04f, 0.3f), new Vector3(0.035f, 0.3f, 0.035f), metal, weapon, false);
            barrel.transform.localRotation = Quaternion.Euler(90, 0, 0);
            Shape("Scope", PrimitiveType.Cube, new Vector3(0, 0.12f, 0.05f), new Vector3(0.06f, 0.06f, 0.2f), dark, weapon, false);
        }
        private void Update()
        {
            if (settings.activeSelf) return;
            Vector2 input = movement.Move;
            if (!Application.isMobilePlatform)
            {
                input += new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                if (Input.GetMouseButton(1)) Rotate(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * 10);
                if (Input.GetKeyDown(KeyCode.Space)) Fire();
                if (Input.GetKeyDown(KeyCode.R)) Reload();
                if (Input.GetKeyDown(KeyCode.Q)) SwitchGun();
            }
            Rotate(look.ConsumeLook());
            Vector3 direction = Player.right * input.x + Player.forward * input.y;
            direction = Vector3.ClampMagnitude(direction, 1) * (aiming ? 2 : 4);
            fallSpeed = controller.isGrounded ? -2 : fallSpeed + Physics.gravity.y * Time.deltaTime;
            direction.y = fallSpeed;
            controller.Move(direction * Time.deltaTime);
            view.fieldOfView = Mathf.Lerp(view.fieldOfView, aiming ? (gun == 0 ? 27 : 45) : 65, Time.deltaTime * 10);
            weapon.localPosition = Vector3.Lerp(weapon.localPosition, aiming ? new Vector3(0, -0.15f, 0.4f) : new Vector3(0.23f, -0.23f, 0.45f), Time.deltaTime * 12);
            status.text = $"WILDLANDS  /  FIELD TEST\n{gunNames[gun]}  {ammo}/{capacity[gun]}{(reloading ? "  RELOADING" : "")}\nHARVEST {harvested}/3  •  {performance.Quality}  •  {performance.MeasuredFps:0} FPS";
        }
        private void Rotate(Vector2 delta)
        {
            float sensitivity = aiming ? 0.045f : 0.12f;
            Player.Rotate(0, delta.x * sensitivity, 0);
            pitch = Mathf.Clamp(pitch - delta.y * sensitivity, -75, 75);
            view.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
        }
        public void Fire()
        {
            if (settings.activeSelf || reloading || Time.time < nextShot) return;
            if (ammo == 0) { Reload(); return; }
            ammo--;
            nextShot = Time.time + (gun == 0 ? 0.85f : 1.1f);
            bool hitAnimal = false;
            int pellets = gun == 0 ? 1 : 8;
            for (int i = 0; i < pellets; i++)
            {
                Vector3 direction = view.transform.forward;
                if (gun == 1) direction = (direction + view.transform.right * Random.Range(-0.045f, 0.045f) + view.transform.up * Random.Range(-0.045f, 0.045f)).normalized;
                if (Physics.Raycast(view.transform.position, direction, out RaycastHit hit, gun == 0 ? 180 : 55))
                {
                    var animal = hit.collider.GetComponentInParent<Wildlife>();
                    if (animal) { animal.Hit(gun == 0 ? 100 : 22); hitAnimal = true; }
                }
            }
            foreach (var animal in FindObjectsByType<Wildlife>(FindObjectsSortMode.None)) animal.HearShot(Player.position);
            pitch = Mathf.Clamp(pitch - (gun == 0 ? 2 : 4), -75, 75);
            ShowNotice(hitAnimal ? "Hit confirmed" : "Shot fired — wildlife alerted");
        }
        private void Reload() { if (!reloading && ammo < capacity[gun]) StartCoroutine(ReloadRoutine()); }
        private IEnumerator ReloadRoutine() { reloading = true; yield return new WaitForSeconds(2); ammo = capacity[gun]; reloading = false; }
        private void SwitchGun() { if (reloading) return; gun = 1 - gun; ammo = capacity[gun]; aiming = false; }
        public void RecordHarvest(string species)
        {
            harvested++;
            ShowNotice(harvested >= 3 ? "MISSION COMPLETE — 3 harvests recorded. Continue exploring." : $"{species} harvested — {harvested}/3");
        }
        private void ShowNotice(string message) { notice.text = message; }
        private void OnDestroy() { Time.timeScale = 1; }

        private RectTransform Panel(string name, Transform parent, Vector2 anchor, Vector2 size, Vector2 offset, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = (RectTransform)obj.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = offset;
            obj.GetComponent<Image>().color = color;
            return rect;
        }
        private Text Label(Transform parent, string value, int size, TextAnchor alignment)
        {
            var obj = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            var rect = (RectTransform)obj.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(8, 4); rect.offsetMax = new Vector2(-8, -4);
            var text = obj.GetComponent<Text>();
            text.font = font; text.fontSize = size; text.color = Color.white;
            text.text = value; text.alignment = alignment; text.raycastTarget = false;
            return text;
        }
        private void Button(Transform parent, string title, Vector2 anchor, Vector2 size, Vector2 offset, UnityEngine.Events.UnityAction action)
        {
            var rect = Panel(title, parent, anchor, size, offset, new Color(0.08f, 0.14f, 0.12f, 0.82f));
            rect.gameObject.AddComponent<Button>().onClick.AddListener(action);
            Label(rect, title, 19, TextAnchor.MiddleCenter);
        }
        private void BuildUI()
        {
            if (!FindFirstObjectByType<EventSystem>()) new GameObject("Event system", typeof(EventSystem), typeof(StandaloneInputModule));
            var canvas = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
            var safe = new GameObject("Safe area", typeof(RectTransform)).GetComponent<RectTransform>();
            safe.SetParent(canvas.transform, false);
            Rect area = Screen.safeArea;
            safe.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
            safe.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
            safe.offsetMin = safe.offsetMax = Vector2.zero;
            var lookRect = Panel("Look area", safe, new Vector2(0.75f, 0.5f), new Vector2(640, 720), Vector2.zero, Color.clear);
            look = lookRect.gameObject.AddComponent<TouchControls>(); look.LookPad = true;
            var moveRect = Panel("Move", safe, Vector2.zero, new Vector2(175, 175), new Vector2(125, 125), new Color(1, 1, 1, 0.12f));
            movement = moveRect.gameObject.AddComponent<TouchControls>();
            var knob = Panel("Knob", moveRect, Vector2.one * 0.5f, new Vector2(60, 60), Vector2.zero, new Color(1, 1, 1, 0.3f));
            knob.GetComponent<Image>().raycastTarget = false; movement.SetKnob(knob);
            var header = Panel("Status", safe, new Vector2(0, 1), new Vector2(430, 110), new Vector2(225, -65), new Color(0, 0, 0, 0.45f));
            header.GetComponent<Image>().raycastTarget = false;
            status = Label(header, "", 20, TextAnchor.MiddleLeft);
            var message = Panel("Notice", safe, new Vector2(0.5f, 1), new Vector2(600, 50), new Vector2(0, -150), Color.clear);
            message.GetComponent<Image>().raycastTarget = false;
            notice = Label(message, "", 20, TextAnchor.MiddleCenter);
            var crosshair = Panel("Reticle", safe, Vector2.one * 0.5f, new Vector2(40, 40), Vector2.zero, Color.clear);
            crosshair.GetComponent<Image>().raycastTarget = false;
            Label(crosshair, "+", 30, TextAnchor.MiddleCenter);
            Button(safe, "FIRE", new Vector2(1, 0), new Vector2(120, 100), new Vector2(-90, 150), Fire);
            Button(safe, "AIM", new Vector2(1, 0), new Vector2(100, 65), new Vector2(-220, 110), () => aiming = !aiming);
            Button(safe, "RELOAD", new Vector2(1, 0), new Vector2(110, 60), new Vector2(-95, 45), Reload);
            Button(safe, "SWITCH", new Vector2(1, 0), new Vector2(110, 60), new Vector2(-220, 40), SwitchGun);
            Button(safe, "SETTINGS", Vector2.one, new Vector2(150, 55), new Vector2(-90, -40), () => SetSettings(true));
            settings = Panel("Settings", safe, Vector2.one * 0.5f, new Vector2(660, 480), Vector2.zero, new Color(0.035f, 0.06f, 0.05f, 0.98f)).gameObject;
            var title = Panel("Settings text", settings.transform, new Vector2(0.5f, 1), new Vector2(620, 170), new Vector2(0, -90), Color.clear);
            settingsLabel = Label(title, "", 24, TextAnchor.MiddleCenter);
            for (int i = 0; i < 3; i++)
            {
                var quality = (PerformanceSettings.Detail)i;
                Button(settings.transform, quality.ToString(), new Vector2(0.5f, 0.5f), new Vector2(180, 55), new Vector2((i - 1) * 195, 30), () => { performance.Configure(quality, performance.FrameCap, false); RefreshSettings(); });
            }
            Button(settings.transform, "30 FPS", Vector2.one * 0.5f, new Vector2(180, 55), new Vector2(-195, -45), () => SetCap(30));
            Button(settings.transform, "60 FPS", Vector2.one * 0.5f, new Vector2(180, 55), new Vector2(0, -45), () => SetCap(60));
            Button(settings.transform, "AUTO", Vector2.one * 0.5f, new Vector2(180, 55), new Vector2(195, -45), () => { performance.Configure(performance.Quality, performance.FrameCap, true); RefreshSettings(); });
            Button(settings.transform, "RESUME", new Vector2(0.5f, 0), new Vector2(250, 60), new Vector2(0, 65), () => SetSettings(false));
            settings.SetActive(false);
        }
        private void SetCap(int cap) { performance.Configure(performance.Quality, cap, performance.Automatic); RefreshSettings(); }
        private void RefreshSettings() { settingsLabel.text = $"GRAPHICS & PERFORMANCE\n{performance.Quality}  /  {performance.FrameCap} FPS target  /  {(performance.Automatic ? "AUTO" : "MANUAL")}\nFrame rate is a target, not a guarantee."; }
        private void SetSettings(bool open) { settings.SetActive(open); Time.timeScale = open ? 0 : 1; if (open) { look.ConsumeLook(); RefreshSettings(); } }
    }
}

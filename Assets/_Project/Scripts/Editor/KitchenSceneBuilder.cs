using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using YesChef.Core;
using YesChef.Core.Channels;
using YesChef.Data;
using YesChef.DevTools;
using YesChef.Gameplay;

namespace YesChef.Editor
{
    public static class KitchenSceneBuilder
    {
        private const string ScenePath    = "Assets/_Project/Scenes/Kitchen.unity";
        private const string PrefabPath   = "Assets/_Project/Prefabs/Player.prefab";
        private const string PlayerMatPath = "Assets/_Project/Art/Materials/Player.mat";
        private const string FloorMatPath  = "Assets/_Project/Art/Materials/Floor.mat";
        private const string WallMatPath   = "Assets/_Project/Art/Materials/Wall.mat";

        [MenuItem("YesChef/Build Kitchen Scene")]
        public static void BuildKitchenScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            // ── Folders ───────────────────────────────────────────────────────
            EnsureFolder("Assets/_Project/Art");
            EnsureFolder("Assets/_Project/Art/Materials");
            EnsureFolder("Assets/_Project/Prefabs");
            EnsureFolder("Assets/_Project/Scenes");

            // ── Scene ─────────────────────────────────────────────────────────
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ── Materials ─────────────────────────────────────────────────────
            var floorMat  = GetOrCreateMat(FloorMatPath,  new Color(0.82f, 0.72f, 0.58f));
            var wallMat   = GetOrCreateMat(WallMatPath,   new Color(0.72f, 0.72f, 0.72f));
            var playerMat = GetOrCreateMat(PlayerMatPath, new Color(0.18f, 0.52f, 0.98f));

            // ── Lighting ──────────────────────────────────────────────────────
            var lightGO = new GameObject("Directional Light");
            var light   = lightGO.AddComponent<Light>();
            light.type      = LightType.Directional;
            light.intensity = 1.1f;
            light.color     = new Color(1f, 0.96f, 0.84f);
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Flat ambient — ambientLight drives the colour directly (no intensity knob in Flat mode)
            RenderSettings.ambientMode  = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.60f);

            // ── Camera ────────────────────────────────────────────────────────
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.fieldOfView   = 60f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane  = 100f;
            cam.clearFlags    = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
            camGO.transform.SetPositionAndRotation(
                new Vector3(0f, 16f, -4f),
                Quaternion.Euler(72f, 0f, 0f));
            camGO.AddComponent<AudioListener>();

            // ── Floor (20 × 12 world units) ───────────────────────────────────
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.localScale = new Vector3(2f, 1f, 1.2f);
            floor.GetComponent<Renderer>().sharedMaterial = floorMat;
            floor.isStatic = true;

            // ── Walls (floor spans X[-10,10] Z[-6,6]) ─────────────────────────
            MakeWall("Wall_North", new Vector3(0f,      1.5f,  6.25f), new Vector3(21f, 3f, 0.5f), wallMat);
            MakeWall("Wall_South", new Vector3(0f,      1.5f, -6.25f), new Vector3(21f, 3f, 0.5f), wallMat);
            MakeWall("Wall_East",  new Vector3( 10.25f, 1.5f,  0f),    new Vector3(0.5f, 3f, 13f),  wallMat);
            MakeWall("Wall_West",  new Vector3(-10.25f, 1.5f,  0f),    new Vector3(0.5f, 3f, 13f),  wallMat);

            // ── Station markers ───────────────────────────────────────────────
            // Fridge — back-right
            var fridge = Marker("RefrigeratorSlots", new Vector3(7f, 0f, 4.5f), null);
            Marker("VegetableSlot", new Vector3(-1.5f, 0f, 0f), fridge.transform);
            Marker("CheeseSlot",    new Vector3( 0f,   0f, 0f), fridge.transform);
            Marker("MeatSlot",      new Vector3( 1.5f, 0f, 0f), fridge.transform);

            // Table — centre
            Marker("Table", new Vector3(2f, 0f, 0f), null);

            // Stove — left of centre
            var stove = Marker("Stove", new Vector3(-4f, 0f, 0f), null);
            Marker("Slot0", new Vector3(-0.75f, 0f, 0f), stove.transform);
            Marker("Slot1", new Vector3( 0.75f, 0f, 0f), stove.transform);

            // Customer windows — along left wall
            var windows = Marker("CustomerWindows", new Vector3(-9f, 0f, 0f), null);
            Marker("Window1", new Vector3(0f, 0f, -4f),   windows.transform);
            Marker("Window2", new Vector3(0f, 0f, -1.5f), windows.transform);
            Marker("Window3", new Vector3(0f, 0f,  1.5f), windows.transform);
            Marker("Window4", new Vector3(0f, 0f,  4f),   windows.transform);

            // Trash — far corner
            Marker("Trash", new Vector3(8.5f, 0f, -4.5f), null);

            // ── Player ────────────────────────────────────────────────────────
            var player = BuildPlayer(playerMat);
            player.transform.position = new Vector3(0f, 1f, 0f);

            // ── GameManager ───────────────────────────────────────────────────
            var gmGO = new GameObject("GameManager");
            var gm   = gmGO.AddComponent<GameManager>();
            var dbg  = gmGO.AddComponent<DebugGameControls>();

            var settings     = Load<GameSettings>    ("Assets/_Project/Data/Settings/DefaultGameSettings.asset");
            var stateCh      = Load<GameStateChannel>("Assets/_Project/Data/Channels/GameStateChannel.asset");
            var floatCh      = Load<FloatChannel>    ("Assets/_Project/Data/Channels/FloatChannel.asset");

            SetField(gm,  "_settings",              settings);
            SetField(gm,  "_stateChannel",          stateCh);
            SetField(gm,  "_timeRemainingChannel",  floatCh);
            SetField(dbg, "_gameManager",           gm);

            // ── PlayerMovement wiring ─────────────────────────────────────────
            var pm = player.GetComponent<PlayerMovement>();
            var rb = player.GetComponent<Rigidbody>();

            SetField(pm, "_rb",           rb);
            SetField(pm, "_gameSettings", settings);
            SetField(pm, "_stateChannel", stateCh);

            var inputAsset = Load<InputActionAsset>("Assets/_Project/Input/PlayerInput.inputactions");
            if (inputAsset != null)
            {
                var moveAction = inputAsset.FindAction("Gameplay/Move");
                if (moveAction != null)
                {
                    var moveRef = InputActionReference.Create(moveAction);
                    SetField(pm, "_moveAction", moveRef);
                }
            }

            // ── Save Player prefab ────────────────────────────────────────────
            PrefabUtility.SaveAsPrefabAssetAndConnect(
                player, PrefabPath, InteractionMode.AutomatedAction);

            // ── Save scene ────────────────────────────────────────────────────
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Debug.Log("[KitchenSceneBuilder] Done → " + ScenePath);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        static GameObject BuildPlayer(Material mat)
        {
            var root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            root.name = "Player";
            root.GetComponent<Renderer>().sharedMaterial = mat;

            var rb = root.AddComponent<Rigidbody>();
            rb.useGravity     = true;
            rb.interpolation  = RigidbodyInterpolation.Interpolate;
            rb.constraints    = RigidbodyConstraints.FreezeRotationX
                              | RigidbodyConstraints.FreezeRotationY
                              | RigidbodyConstraints.FreezeRotationZ;

            root.AddComponent<PlayerMovement>();

            // HandSocket — front, waist height
            var hand = new GameObject("HandSocket");
            hand.transform.SetParent(root.transform, false);
            hand.transform.localPosition = new Vector3(0f, -0.1f, 0.6f);

            // InteractionRange — sphere trigger
            var rangeGO = new GameObject("InteractionRange");
            rangeGO.transform.SetParent(root.transform, false);
            var sphere   = rangeGO.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius    = 1.0f;

            return root;
        }

        static void MakeWall(string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position   = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            go.isStatic = true;
        }

        static GameObject Marker(string name, Vector3 localPos, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            return go;
        }

        static Material GetOrCreateMat(string path, Color color)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) return mat;
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        static T Load<T>(string path) where T : Object =>
            AssetDatabase.LoadAssetAtPath<T>(path);

        static void SetField(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop != null) { prop.objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
            else Debug.LogWarning($"[KitchenSceneBuilder] Field '{field}' not found on {target.GetType().Name}");
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parts  = path.Split('/');
            var parent = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var full = parent + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(full))
                    AssetDatabase.CreateFolder(parent, parts[i]);
                parent = full;
            }
        }
    }
}

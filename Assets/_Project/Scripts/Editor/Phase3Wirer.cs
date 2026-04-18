using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using YesChef.Data;
using YesChef.Gameplay.Interactions;

namespace YesChef.Editor
{
    /// <summary>
    /// Wires Phase 3 interaction components into the Kitchen scene.
    /// Run once after the Kitchen scene is open: YesChef → Wire Phase 3 Interactions.
    /// Safe to re-run — skips components that already exist.
    /// </summary>
    public static class Phase3Wirer
    {
        private const string HighlightMatPath = "Assets/_Project/Art/Materials/Highlight.mat";
        private const string FridgeMatPath    = "Assets/_Project/Art/Materials/Fridge.mat";
        private const string TrashMatPath     = "Assets/_Project/Art/Materials/TrashCan.mat";

        [MenuItem("YesChef/Wire Phase 3 Interactions")]
        public static void WirePhase3()
        {
            // ── Materials ─────────────────────────────────────────────────────
            var highlightMat = GetOrCreateEmissiveMat(HighlightMatPath, new Color(1f, 0.9f, 0.1f), 3f);
            var fridgeMat    = GetOrCreateMat(FridgeMatPath, new Color(0.35f, 0.55f, 0.95f));
            var trashMat     = GetOrCreateMat(TrashMatPath,  new Color(0.95f, 0.45f, 0.1f));

            // ── Fridge slots ──────────────────────────────────────────────────
            WireFridgeSlot("VegetableSlot", "Assets/_Project/Data/Ingredients/Vegetable.asset", fridgeMat, highlightMat);
            WireFridgeSlot("CheeseSlot",    "Assets/_Project/Data/Ingredients/Cheese.asset",    fridgeMat, highlightMat);
            WireFridgeSlot("MeatSlot",      "Assets/_Project/Data/Ingredients/Meat.asset",      fridgeMat, highlightMat);

            // ── Trash ─────────────────────────────────────────────────────────
            WireTrash("Trash", trashMat, highlightMat);

            // ── Player ────────────────────────────────────────────────────────
            WirePlayer();

            // ── Save ─────────────────────────────────────────────────────────
            EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.Refresh();

            Debug.Log("[Phase3Wirer] Done.");
        }

        // ── Per-station wiring ────────────────────────────────────────────────

        static void WireFridgeSlot(string goName, string ingredientPath, Material visMat, Material hlMat)
        {
            var go = GameObject.Find(goName);
            if (go == null) { Debug.LogError($"[Phase3Wirer] '{goName}' not found in scene."); return; }

            // Visual cube child
            EnsureVisualCube(go, "Visual", visMat, new Vector3(0.8f, 1.4f, 0.8f), Vector3.zero);

            // Highlight cube child (disabled by default, slightly larger)
            var hl = EnsureHighlightCube(go, "Highlight", hlMat, new Vector3(0.88f, 1.48f, 0.88f));

            // BoxCollider on root
            var col = go.GetComponent<BoxCollider>();
            if (col == null) col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size   = new Vector3(1f, 1.6f, 1f);
            col.center = new Vector3(0f, 0.7f, 0f);

            // RefrigeratorSlot component
            var slot = go.GetComponent<RefrigeratorSlot>();
            if (slot == null) slot = go.AddComponent<RefrigeratorSlot>();

            var so = new SerializedObject(slot);
            var ingredient = AssetDatabase.LoadAssetAtPath<IngredientDefinition>(ingredientPath);
            so.FindProperty("_ingredient").objectReferenceValue = ingredient;
            so.FindProperty("_highlight").objectReferenceValue  = hl;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void WireTrash(string goName, Material visMat, Material hlMat)
        {
            var go = GameObject.Find(goName);
            if (go == null) { Debug.LogError($"[Phase3Wirer] '{goName}' not found in scene."); return; }

            EnsureVisualCube(go, "Visual", visMat, new Vector3(0.8f, 0.8f, 0.8f), new Vector3(0f, 0.4f, 0f));
            var hl = EnsureHighlightCube(go, "Highlight", hlMat, new Vector3(0.88f, 0.88f, 0.88f));

            var col = go.GetComponent<BoxCollider>();
            if (col == null) col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size   = new Vector3(1f, 1f, 1f);
            col.center = new Vector3(0f, 0.4f, 0f);

            var trash = go.GetComponent<TrashCan>();
            if (trash == null) trash = go.AddComponent<TrashCan>();

            var so = new SerializedObject(trash);
            so.FindProperty("_highlight").objectReferenceValue = hl;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void WirePlayer()
        {
            var player = GameObject.Find("Player");
            if (player == null) { Debug.LogError("[Phase3Wirer] 'Player' not found in scene."); return; }

            // PlayerHand on root
            var hand = player.GetComponent<PlayerHand>();
            if (hand == null) hand = player.AddComponent<PlayerHand>();

            var handSocket = FindChildRecursive(player.transform, "HandSocket");
            if (handSocket == null) { Debug.LogError("[Phase3Wirer] 'HandSocket' child not found on Player."); }
            else
            {
                var so = new SerializedObject(hand);
                so.FindProperty("_handSocket").objectReferenceValue = handSocket;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // PlayerInteractor on InteractionRange child
            var rangeGO = FindChildRecursive(player.transform, "InteractionRange");
            if (rangeGO == null) { Debug.LogError("[Phase3Wirer] 'InteractionRange' child not found on Player."); return; }

            if (rangeGO.GetComponent<PlayerInteractor>() == null)
                rangeGO.gameObject.AddComponent<PlayerInteractor>();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        static GameObject EnsureVisualCube(GameObject parent, string name, Material mat, Vector3 scale, Vector3 localPos)
        {
            var existing = FindChildRecursive(parent.transform, name);
            if (existing != null) return existing.gameObject;

            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            Object.DestroyImmediate(cube.GetComponent<BoxCollider>()); // no extra colliders on visual
            cube.transform.SetParent(parent.transform, false);
            cube.transform.localScale    = scale;
            cube.transform.localPosition = localPos;
            cube.GetComponent<Renderer>().sharedMaterial = mat;
            return cube;
        }

        static GameObject EnsureHighlightCube(GameObject parent, string name, Material mat, Vector3 scale)
        {
            var existing = FindChildRecursive(parent.transform, name);
            GameObject hl = existing != null ? existing.gameObject : null;

            if (hl == null)
            {
                hl = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hl.name = name;
                Object.DestroyImmediate(hl.GetComponent<BoxCollider>());
                hl.transform.SetParent(parent.transform, false);
                hl.transform.localScale    = scale;
                hl.transform.localPosition = new Vector3(0f, scale.y * 0.5f - 0.5f, 0f);
                hl.GetComponent<Renderer>().sharedMaterial = mat;
            }
            hl.SetActive(false);
            return hl;
        }

        static Material GetOrCreateMat(string path, Color color)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) return mat;
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        static Material GetOrCreateEmissiveMat(string path, Color color, float emissiveMultiplier)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) return mat;
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * emissiveMultiplier);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        static Transform FindChildRecursive(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                var found = FindChildRecursive(child, name);
                if (found != null) return found;
            }
            return null;
        }
    }
}

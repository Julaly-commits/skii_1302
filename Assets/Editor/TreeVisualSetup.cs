using UnityEditor;
using UnityEngine;

/// <summary>
/// Swaps the placeholder capsule on Tree.prefab for the Fir_Tree model. The
/// prefab keeps its transform, CapsuleCollider and Tree script, so collisions
/// and damage behave exactly as before - only the visible mesh changes.
/// The placeholder renderer is removed outright: Tree.cs now finds its renderer
/// with GetComponentInChildren, and a leftover renderer on the root would be
/// found first and get tinted instead of the tree.
/// </summary>
static class TreeVisualSetup
{
    const string TreePrefabPath = "Assets/Prefabs/Tree.prefab";
    const string ModelPrefabPath = "Assets/Darth_Artisan/Free_Trees/Prefabs/Fir_Tree.prefab";
    const string MeshChildName = "FirTreeMesh";
    const string RunOnceKey = "TreeVisualSetup.applied.v3";

    [MenuItem("Tools/Ski/Put Fir Tree On The Tree Prefab")]
    public static void Run()
    {
        Apply();
    }

    [InitializeOnLoadMethod]
    static void AutoRunOnce()
    {
        if (EditorPrefs.GetBool(RunOnceKey, false))
            return;

        EditorApplication.delayCall += () =>
        {
            if (EditorPrefs.GetBool(RunOnceKey, false))
                return;

            EditorPrefs.SetBool(RunOnceKey, true);
            Apply();
        };
    }

    static void Apply()
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPrefabPath);
        if (model == null)
        {
            Debug.LogError($"[TreeVisualSetup] Model prefab not found at {ModelPrefabPath}");
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<GameObject>(TreePrefabPath) == null)
        {
            Debug.LogError($"[TreeVisualSetup] Prefab not found at {TreePrefabPath}");
            return;
        }

        var root = PrefabUtility.LoadPrefabContents(TreePrefabPath);
        try
        {
            var capsule = root.GetComponent<CapsuleCollider>();
            if (capsule == null)
            {
                Debug.LogError("[TreeVisualSetup] Tree.prefab has no CapsuleCollider to size the model against.");
                return;
            }

            // Rebuild from scratch so re-running never stacks two trees.
            var existing = root.transform.Find(MeshChildName);
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);

            // Drop the placeholder capsule entirely so the only renderer left on
            // the prefab is the tree itself.
            var placeholder = root.GetComponent<MeshRenderer>();
            if (placeholder != null)
                Object.DestroyImmediate(placeholder);

            var placeholderFilter = root.GetComponent<MeshFilter>();
            if (placeholderFilter != null)
                Object.DestroyImmediate(placeholderFilter);

            // Measure the model on its own, away from the prefab. Measuring it
            // after parenting reported a height already multiplied by the root's
            // 6x vertical scale, which made the tree come out six times short.
            float modelHeight = UnscaledBounds(model).size.y;
            if (modelHeight <= Mathf.Epsilon)
            {
                Debug.LogError("[TreeVisualSetup] Fir_Tree bounds are degenerate.");
                return;
            }

            var child = (GameObject)PrefabUtility.InstantiatePrefab(model, root.transform);
            child.name = MeshChildName;
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;

            // A tree is defined by its height, so match the collider along its
            // own axis rather than by overall bounding size.
            var lossy = root.transform.lossyScale;
            float targetHeight = capsule.height * Mathf.Abs(lossy.y);

            // The prefab root is scaled 1,6,1. Divide that back out so the tree
            // comes out evenly proportioned instead of stretched upwards.
            float uniform = targetHeight / modelHeight;
            child.transform.localScale = new Vector3(
                uniform / SafeScale(lossy.x),
                uniform / SafeScale(lossy.y),
                uniform / SafeScale(lossy.z));

            // Measure in place now that it is scaled, then stand the trunk on the
            // base of the collider rather than centring it.
            if (!TryGetWorldBounds(child, out var bounds))
            {
                Debug.LogError("[TreeVisualSetup] Fir_Tree has no renderers to measure.");
                return;
            }

            var colliderBounds = capsule.bounds;
            child.transform.position += new Vector3(
                colliderBounds.center.x - bounds.center.x,
                colliderBounds.min.y - bounds.min.y,
                colliderBounds.center.z - bounds.center.z);

            PrefabUtility.SaveAsPrefabAsset(root, TreePrefabPath);
            Debug.Log($"[TreeVisualSetup] Fir_Tree put on {TreePrefabPath}: " +
                      $"model height {modelHeight:0.###}, collider height {targetHeight:0.###}, " +
                      $"world scale {uniform:0.####}.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static float SafeScale(float v)
    {
        return Mathf.Approximately(v, 0f) ? 1f : v;
    }

    /// <summary>
    /// Size of the model relative to its own root, measured unparented,
    /// unrotated and unscaled from a throwaway instance.
    /// </summary>
    static Bounds UnscaledBounds(GameObject prefab)
    {
        var probe = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        try
        {
            probe.transform.position = Vector3.zero;
            probe.transform.rotation = Quaternion.identity;
            probe.transform.localScale = Vector3.one;

            return TryGetWorldBounds(probe, out var bounds)
                ? bounds
                : new Bounds(Vector3.zero, Vector3.zero);
        }
        finally
        {
            Object.DestroyImmediate(probe);
        }
    }

    static bool TryGetWorldBounds(GameObject go, out Bounds bounds)
    {
        bounds = default;
        var renderers = go.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return false;

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return true;
    }
}

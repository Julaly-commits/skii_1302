using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Swaps the player's placeholder primitive for the CakeRabbit model. The
/// player GameObject keeps its transform, BoxCollider, Rigidbody and Player
/// script, so movement and collisions behave exactly as before - only the
/// visible mesh changes.
/// </summary>
static class PlayerVisualSetup
{
    const string TargetSceneName = "Scene01";
    const string ModelPath = "Assets/KamiChan/3D Cutie Desserts/Models/CakeRabbit/CakeRabbit.fbx";
    const string MeshChildName = "CakeRabbitMesh";
    const string RunOnceKey = "PlayerVisualSetup.applied.v2";

    /// <summary>
    /// Turn the model on the spot if it ends up facing the wrong way down the
    /// slope. The player travels along +Z.
    /// </summary>
    const float YawDegrees = 0f;

    [MenuItem("Tools/Ski/Put CakeRabbit On The Player")]
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
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != TargetSceneName)
        {
            Debug.LogWarning($"[PlayerVisualSetup] Active scene is '{scene.name}', expected " +
                             $"'{TargetSceneName}'. Open it and run " +
                             "Tools > Ski > Put CakeRabbit On The Player.");
            return;
        }

        var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
        if (model == null)
        {
            Debug.LogError($"[PlayerVisualSetup] Model not found at {ModelPath}");
            return;
        }

        var player = Object.FindAnyObjectByType<Player>();
        if (player == null)
        {
            Debug.LogError("[PlayerVisualSetup] No Player component in the scene.");
            return;
        }

        var root = player.gameObject;
        var box = root.GetComponent<BoxCollider>();
        if (box == null)
        {
            Debug.LogError("[PlayerVisualSetup] The player has no BoxCollider to size the model against.");
            return;
        }

        // Rebuild from scratch so re-running never stacks two rabbits.
        var previous = root.transform.Find(MeshChildName);
        if (previous != null)
            Undo.DestroyObjectImmediate(previous.gameObject);

        var renderer = root.GetComponent<MeshRenderer>();
        if (renderer != null)
            Undo.DestroyObjectImmediate(renderer);

        var filter = root.GetComponent<MeshFilter>();
        if (filter != null)
            Undo.DestroyObjectImmediate(filter);

        var bounds = LocalBounds(model);
        if (bounds.size.y <= Mathf.Epsilon)
        {
            Debug.LogError("[PlayerVisualSetup] CakeRabbit.fbx bounds are degenerate.");
            return;
        }

        var child = (GameObject)PrefabUtility.InstantiatePrefab(model, root.transform);
        child.name = MeshChildName;
        Undo.RegisterCreatedObjectUndo(child, "Put CakeRabbit On The Player");

        // Local rotation stays flat, so the model inherits the player's 45 degree
        // pitch and stands square to the slope like the placeholder box did.
        var rotation = Quaternion.Euler(0f, YawDegrees, 0f);
        child.transform.localRotation = rotation;

        // Everything below is in the player's local space. Both the collider and
        // the child are scaled by the same parent, so the parent scale cancels
        // out and the 45 degree pitch never enters the maths.
        float scale = box.size.y / bounds.size.y;
        child.transform.localScale = Vector3.one * scale;

        var scaledCenter = rotation * (bounds.center * scale);
        float scaledHeight = bounds.size.y * scale;

        // Feet on the floor of the collider, centred on its other two axes.
        var target = new Vector3(
            box.center.x,
            box.center.y - box.size.y * 0.5f + scaledHeight * 0.5f,
            box.center.z);

        child.transform.localPosition = target - scaledCenter;

        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = root;

        Debug.Log($"[PlayerVisualSetup] CakeRabbit put on '{root.name}': model " +
                  $"{bounds.size.x:0.###} x {bounds.size.y:0.###} x {bounds.size.z:0.###}, " +
                  $"collider {box.size.x:0.###} x {box.size.y:0.###} x {box.size.z:0.###}, " +
                  $"local scale {scale:0.####}. " +
                  "Scene is dirty - save it if you want to keep the change.");
    }

    /// <summary>
    /// Size of the model relative to its own root, measured unrotated and
    /// unscaled from a throwaway instance.
    /// </summary>
    static Bounds LocalBounds(GameObject prefab)
    {
        var probe = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        try
        {
            probe.transform.position = Vector3.zero;
            probe.transform.rotation = Quaternion.identity;
            probe.transform.localScale = Vector3.one;

            var renderers = probe.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                return new Bounds(Vector3.zero, Vector3.zero);

            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            return bounds;
        }
        finally
        {
            Object.DestroyImmediate(probe);
        }
    }
}

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Puts the Paw picture on the main menu Panel in place of the plain built-in
/// sprite. The Panel's own transparency is read off the existing colour and put
/// back, so the plate stays exactly as see-through as it was.
/// </summary>
static class PanelImageSetup
{
    const string TargetSceneName = "MainMemu";
    const string SpritePath = "Assets/Textures/Paw.jpg";
    const string PanelPath = "Canvas/Panel";
    const string RunOnceKey = "PanelImageSetup.applied.v1";

    [MenuItem("Tools/Ski/Put Paw Image On The Panel")]
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
            Debug.LogWarning($"[PanelImageSetup] Active scene is '{scene.name}', expected " +
                             $"'{TargetSceneName}'. Open it and run " +
                             "Tools > Ski > Put Paw Image On The Panel.");
            return;
        }

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        if (sprite == null)
        {
            Debug.LogError($"[PanelImageSetup] No sprite at {SpritePath}. " +
                           "Check that its Texture Type is set to Sprite (2D and UI).");
            return;
        }

        var panelObject = GameObject.Find(PanelPath);
        if (panelObject == null)
        {
            Debug.LogError($"[PanelImageSetup] '{PanelPath}' not found in the scene.");
            return;
        }

        var image = panelObject.GetComponent<Image>();
        if (image == null)
        {
            Debug.LogError($"[PanelImageSetup] '{PanelPath}' has no Image component.");
            return;
        }

        Undo.RecordObject(image, "Put Paw Image On The Panel");

        // Keep whatever alpha the panel already had rather than assuming a value.
        float alpha = image.color.a;

        image.sprite = sprite;

        // The old sprite was a 9-sliced UI plate. A photo has no border data, so
        // Sliced would just warn and behave like Simple anyway.
        image.type = Image.Type.Simple;
        image.color = new Color(1f, 1f, 1f, alpha);

        EditorUtility.SetDirty(image);
        EditorSceneManager.MarkSceneDirty(scene);

        Debug.Log($"[PanelImageSetup] Paw put on '{PanelPath}', alpha kept at {alpha:0.###}. " +
                  "Scene is dirty - save it if you want to keep the change.");
    }
}

using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

/// <summary>
/// Builds a TextMeshPro font asset from the Cooper Black face bundled under
/// Assets/Fonts and puts it on the main menu title and on the Start and Exit
/// button labels.
/// </summary>
static class TitleFontSetup
{
    const string TargetSceneName = "MainMemu";
    const string SourceFontPath = "Assets/Fonts/CooperBlack.ttf";
    const string FontAssetPath = "Assets/Fonts/CooperBlack SDF.asset";
    const string RunOnceKey = "TitleFontSetup.applied.v2";

    /// <summary>
    /// Each label to restyle, as a scene path with the text it should contain.
    /// The text is a sanity check in case the objects get renamed or reordered.
    /// </summary>
    static readonly (string Path, string Text)[] Targets =
    {
        ("Canvas/Text (TMP)", "MY SKIIII"),
        ("Canvas/Panel/ButtonStart/Text (TMP)", "Start"),
        ("Canvas/Panel/ButtonExit/Text (TMP)", "Exit"),
    };

    // A display face at 126pt needs a generous sampling size to stay crisp.
    const int SamplingPointSize = 90;
    const int AtlasPadding = 9;
    const int AtlasSize = 1024;

    [MenuItem("Tools/Ski/Set Main Menu Title Font")]
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
            Debug.LogWarning($"[TitleFontSetup] Active scene is '{scene.name}', expected " +
                             $"'{TargetSceneName}'. Open it and run " +
                             "Tools > Ski > Set Main Menu Title Font.");
            return;
        }

        var fontAsset = GetOrCreateFontAsset();
        if (fontAsset == null)
            return;

        int done = 0;
        foreach (var target in Targets)
        {
            var label = FindLabel(target.Path, target.Text);
            if (label == null)
            {
                Debug.LogWarning($"[TitleFontSetup] Could not find '{target.Path}' " +
                                 $"(expected the text \"{target.Text}\").");
                continue;
            }

            Undo.RecordObject(label, "Set Menu Font");
            label.font = fontAsset;

            // The material comes from the font asset, so replace any override
            // left behind by the previous font or the text renders with the old
            // atlas and shows nothing.
            label.fontSharedMaterial = fontAsset.material;
            EditorUtility.SetDirty(label);
            done++;
        }

        if (done == 0)
        {
            Debug.LogError("[TitleFontSetup] No menu labels were found, nothing changed.");
            return;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"[TitleFontSetup] {done} of {Targets.Length} menu labels now use {fontAsset.name}. " +
                  "Scene is dirty - save it if you want to keep the change.");
    }

    static TMP_FontAsset GetOrCreateFontAsset()
    {
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (existing != null)
            return existing;

        var font = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
        if (font == null)
        {
            Debug.LogError($"[TitleFontSetup] Font file not found at {SourceFontPath}");
            return null;
        }

        // Dynamic population keeps the atlas small and fills in glyphs as they
        // are used, with the imported .ttf staying the source of truth.
        var fontAsset = TMP_FontAsset.CreateFontAsset(
            font,
            SamplingPointSize,
            AtlasPadding,
            GlyphRenderMode.SDFAA,
            AtlasSize,
            AtlasSize,
            AtlasPopulationMode.Dynamic,
            true);

        if (fontAsset == null)
        {
            Debug.LogError("[TitleFontSetup] TMP could not build a font asset from " + SourceFontPath);
            return null;
        }

        fontAsset.name = System.IO.Path.GetFileNameWithoutExtension(FontAssetPath);
        AssetDatabase.CreateAsset(fontAsset, FontAssetPath);

        // The atlas texture and material have to live inside the asset, the way
        // the Font Asset Creator window saves them.
        if (fontAsset.atlasTextures != null && fontAsset.atlasTextures.Length > 0)
        {
            fontAsset.atlasTextures[0].name = fontAsset.name + " Atlas";
            AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);
        }

        if (fontAsset.material != null)
        {
            fontAsset.material.name = fontAsset.name + " Atlas Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(FontAssetPath);

        Debug.Log($"[TitleFontSetup] Created {FontAssetPath}");
        return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
    }

    static TextMeshProUGUI FindLabel(string path, string expectedText)
    {
        var byPath = GameObject.Find(path);
        if (byPath != null)
        {
            var component = byPath.GetComponent<TextMeshProUGUI>();
            if (component != null)
                return component;
        }

        // Fall back to matching on the text itself, in case the objects were
        // renamed or moved in the hierarchy.
        foreach (var text in Object.FindObjectsByType<TextMeshProUGUI>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (text.text == expectedText)
                return text;
        }

        return null;
    }
}

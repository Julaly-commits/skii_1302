using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Puts ski2 behind the main menu as a full-screen Canvas image, and retires
/// the old world-space sprite that used to stand in for it. The art is
/// portrait, so it is cropped to cover the screen rather than squashed: a
/// clipping frame fills the canvas and the picture is sized to cover it.
/// </summary>
static class MainMenuBackground
{
    const string TargetSceneName = "MainMemu";
    const string SpritePath = "Assets/Textures/ski2.png";
    const string CanvasName = "Canvas";
    const string FrameName = "Background";
    const string ImageName = "BackgroundImage";
    const string LegacySpriteName = "skii";
    const string RunOnceKey = "MainMenuBackground.applied.v2";

    [MenuItem("Tools/Ski/Set Main Menu Background")]
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
            Debug.LogWarning($"[MainMenuBackground] Active scene is '{scene.name}', expected " +
                             $"'{TargetSceneName}'. Open it and run " +
                             "Tools > Ski > Set Main Menu Background.");
            return;
        }

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        if (sprite == null)
        {
            Debug.LogError($"[MainMenuBackground] No sprite at {SpritePath}. " +
                           "Check that its Texture Type is set to Sprite (2D and UI).");
            return;
        }

        var canvasObject = GameObject.Find(CanvasName);
        if (canvasObject == null)
        {
            Debug.LogError($"[MainMenuBackground] No '{CanvasName}' in the scene.");
            return;
        }

        var frame = FindOrCreate(canvasObject.transform, FrameName);

        Undo.RecordObject(frame, "Set Main Menu Background");
        Stretch(frame);

        // First child draws first, so the menu panel and title stay on top.
        frame.SetSiblingIndex(0);

        // An earlier version drew straight onto the frame. That image would be
        // stretched rather than cropped, so take it off.
        var strayImage = frame.GetComponent<Image>();
        if (strayImage != null)
            Undo.DestroyObjectImmediate(strayImage);

        if (frame.GetComponent<RectMask2D>() == null)
            Undo.AddComponent<RectMask2D>(frame.gameObject);

        var picture = FindOrCreate(frame, ImageName);
        Undo.RecordObject(picture, "Set Main Menu Background");
        picture.anchorMin = new Vector2(0.5f, 0.5f);
        picture.anchorMax = new Vector2(0.5f, 0.5f);
        picture.pivot = new Vector2(0.5f, 0.5f);
        picture.anchoredPosition = Vector2.zero;
        picture.localScale = Vector3.one;

        var image = picture.GetComponent<Image>();
        if (image == null)
            image = Undo.AddComponent<Image>(picture.gameObject);

        Undo.RecordObject(image, "Set Main Menu Background");
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.color = Color.white;

        // The image is decoration, so it must not swallow clicks meant for the buttons.
        image.raycastTarget = false;

        if (picture.GetComponent<BackgroundCover>() == null)
            Undo.AddComponent<BackgroundCover>(picture.gameObject);

        var legacy = GameObject.Find(LegacySpriteName);
        if (legacy != null && legacy.GetComponent<SpriteRenderer>() != null && legacy.activeSelf)
        {
            Undo.RecordObject(legacy, "Set Main Menu Background");
            legacy.SetActive(false);
            Debug.Log($"[MainMenuBackground] Turned off the old world-space '{LegacySpriteName}' sprite.");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"[MainMenuBackground] {SpritePath} set as the menu background " +
                  $"({sprite.rect.width:0} x {sprite.rect.height:0}, cropped to cover). " +
                  "Scene is dirty - save it if you want to keep the change.");
    }

    static RectTransform FindOrCreate(Transform parent, string name)
    {
        var existing = parent.Find(name);
        if (existing != null)
            return (RectTransform)existing;

        var created = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(created, "Set Main Menu Background");
        created.transform.SetParent(parent, false);
        return (RectTransform)created.transform;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }
}

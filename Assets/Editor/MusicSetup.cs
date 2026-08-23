using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Adds the looping background track to both scenes. The menu is the normal way
/// in, and Scene01 gets its own copy so it can be played on its own during
/// testing - MusicPlayer makes sure only one of them survives.
/// </summary>
static class MusicSetup
{
    const string MusicPath = "Assets/Audio/tawipop-treasure-hunt-343358.ogg";
    const string MusicObjectName = "Music";
    const string RunOnceKey = "MusicSetup.applied.v1";

    static readonly string[] ScenePaths =
    {
        "Assets/Scenes/MainMemu.unity",
        "Assets/Scenes/Scene01.unity",
    };

    [MenuItem("Tools/Ski/Add Background Music To Both Scenes")]
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
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(MusicPath);
        if (clip == null)
        {
            Debug.LogError($"[MusicSetup] No AudioClip at {MusicPath}");
            return;
        }

        int done = 0;
        foreach (var path in ScenePaths)
        {
            if (AddMusicTo(path, clip))
                done++;
        }

        Debug.Log($"[MusicSetup] Music set up in {done} of {ScenePaths.Length} scenes. " +
                  "The scenes were saved directly, nothing left dirty.");
    }

    static bool AddMusicTo(string scenePath, AudioClip clip)
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
        {
            Debug.LogWarning($"[MusicSetup] Scene not found: {scenePath}");
            return false;
        }

        // Open additively so whatever the user has open, saved or not, is left
        // exactly as it is.
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

        try
        {
            GameObject music = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == MusicObjectName)
                {
                    music = root;
                    break;
                }
            }

            if (music == null)
            {
                music = new GameObject(MusicObjectName);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(music, scene);
            }

            var source = music.GetComponent<AudioSource>();
            if (source == null)
                source = music.AddComponent<AudioSource>();

            source.clip = clip;
            source.loop = true;
            source.playOnAwake = true;
            source.volume = 0.5f;

            // Menu and run music are not coming from a place in the world.
            source.spatialBlend = 0f;

            if (music.GetComponent<MusicPlayer>() == null)
                music.AddComponent<MusicPlayer>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[MusicSetup] '{MusicObjectName}' ready in {scenePath}");
            return true;
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }
}

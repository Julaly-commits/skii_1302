using UnityEngine;

/// <summary>
/// Background music that survives a scene change. Only one is ever alive, so
/// going from the menu into the run does not restart or double up the track.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Keep playing when another scene loads. Turn this off to have the " +
             "music stop when the menu goes away.")]
    private bool keepPlayingAcrossScenes = true;

    [SerializeField]
    [Range(0f, 1f)]
    private float volume = 0.5f;

    private static MusicPlayer current;

    /// <summary>The music player that is currently alive, if there is one.</summary>
    public static MusicPlayer Current => current;

    private AudioSource source;

    void Awake()
    {
        // Scene01 carries its own player so it can be tested on its own. When it
        // is reached from the menu, that one bows out instead of stacking.
        if (keepPlayingAcrossScenes && current != null && current != this)
        {
            Destroy(gameObject);
            return;
        }

        current = this;
        source = GetComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = true;
        source.volume = volume;

        if (keepPlayingAcrossScenes)
            DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (source == null)
            return;

        if (source.clip == null)
        {
            Debug.LogWarning("[MusicPlayer] No AudioClip assigned - nothing to play.", this);
            return;
        }

        if (!source.isPlaying)
            source.Play();
    }

    void OnDestroy()
    {
        if (current == this)
            current = null;
    }

    /// <summary>Sets the level directly. 0 is silent, 1 is full.</summary>
    public void SetVolume(float value)
    {
        volume = Mathf.Clamp01(value);

        if (source != null)
            source.volume = volume;
    }

    public void StopMusic()
    {
        if (source != null)
            source.Stop();
    }
}

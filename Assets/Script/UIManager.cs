using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text notiText;

    [SerializeField]
    private GameObject restartButton;

    public static UIManager Instance;

    void Awake()
    {
        Instance = this;
    }

    public void ShowNotiText(string s)
    {
        notiText.text = s;
    }

    /// <summary>
    /// Loads the run again from scratch. Moving the player by hand left every
    /// flag and tree that had already been collected or hit gone for good, so
    /// reload the scene and let the whole course rebuild itself.
    /// </summary>
    public void RestartGame()
    {
        // Time has to run again before the reload, or the fresh scene starts
        // frozen and nothing moves.
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ShowHideRestartButton(bool flag)
    {
        restartButton.SetActive(flag);
    }

}

using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text notiText;

    [SerializeField]
    private GameObject restartButton;

    [SerializeField]
    private Player player;

    public static UIManager Instance;

    void Awake()
    {
        Instance = this;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowNotiText(string s)
    {
        notiText.text = s;
    }

    public void RestartGame()
    {
        player.transform.position = new Vector3(0f, 88f, -86f);
        player.HP = 100;
        ShowNotiText("Restart kub auan");
        Time.timeScale = 1f;
        ShowHideRestartButton(false);
    }

    public void ShowHideRestartButton(bool flag)
    {
        restartButton.SetActive(flag);
    }

}

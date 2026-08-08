using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMemu : MonoBehaviour
{
    public void Startgame()
    {
        SceneManager.LoadScene("Scene01");
    }

    public void Exit()
    {
        Application.Quit();
    }
}

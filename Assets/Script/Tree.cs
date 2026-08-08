using UnityEngine;

public class Tree : MonoBehaviour
{
    private MeshRenderer rd;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rd = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        rd.material.color = Color.red;

       Player player = collision.gameObject.GetComponent<Player>();

       if (player == null)
            return;
        player.HP -= 15;
        UIManager.Instance.ShowNotiText($"Hurt -15\nHP: {player.HP}");

        if (player.HP <= 0)
        {
            player.HP = 0;
            UIManager.Instance.ShowNotiText($"You are dead!!!\nPoint: {player.HP}");
            Time.timeScale = 0f;
            UIManager.Instance.ShowHideRestartButton(true);
        }
           
    }

    private void OnCollisionExit(Collision collision)
    {
        rd.material.color = new Color32(212, 109, 29, 255);
    }
}

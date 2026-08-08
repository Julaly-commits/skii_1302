using UnityEngine;

public class Flag : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Player player = other.gameObject.GetComponent<Player>();

        if (player == null)
            return;

        player.Point += 10;

        UIManager.Instance.ShowNotiText($"+10 point\nPoint: {player.Point}");
        Destroy(gameObject);
       
     
    }
}

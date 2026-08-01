using UnityEngine;

public class Finish : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Player p = other.GetComponent<Player>();

        if (p == null)
            return;

        UIManager.Instance.ShowNotiText($"You Win!\nPoints: {p.Point}");
    }
}

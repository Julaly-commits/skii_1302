using UnityEngine;

public class Tree : MonoBehaviour
{
    private MeshRenderer rd;
    private Color normalColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // The visible mesh is the Fir_Tree child, not this object, so search
        // downwards instead of only on the root.
        rd = GetComponentInChildren<MeshRenderer>();

        if (rd == null)
        {
            Debug.LogWarning("[Tree] No MeshRenderer found - the hit colour is disabled.", this);
            return;
        }

        // Remember the colour the material was authored with. Reading it from
        // sharedMaterial avoids cloning a material for every tree up front.
        normalColor = rd.sharedMaterial != null ? rd.sharedMaterial.color : Color.white;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (rd != null)
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
        // Back to whatever the material started as. The old hardcoded orange was
        // the placeholder capsule's colour and would have stained the fir tree.
        if (rd != null)
            rd.material.color = normalColor;
    }
}

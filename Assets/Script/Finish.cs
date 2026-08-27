using UnityEngine;

public class Finish : MonoBehaviour
{
    // A wide finish trigger fires again as the player slides through it, so the
    // win is only allowed to happen once.
    private bool finished;

    private void OnTriggerEnter(Collider other)
    {
        Player p = other.GetComponent<Player>();

        if (p == null)
            return;

        if (finished)
            return;

        finished = true;

        UIManager.Instance.ShowNotiText($"You Win!\nPoints: {p.Point}");

        // Stop the slope so the player is not still sliding away while trying
        // to click, the same way hitting zero HP already does.
        Time.timeScale = 0f;
        UIManager.Instance.ShowHideRestartButton(true);
    }
}

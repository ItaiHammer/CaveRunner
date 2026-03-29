using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class LevelExit : MonoBehaviour
{
    void Awake()
    {
        Collider2D c = GetComponent<Collider2D>();
        if (c != null && !c.isTrigger)
            Debug.LogWarning($"{nameof(LevelExit)} on {name}: set Collider2D Is Trigger = true.", this);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other))
            return;

        GameManager gm = GameManager.Instance;
        if (gm == null)
            return;

        gm.TriggerVictory();
    }

    static bool IsPlayer(Collider2D other)
    {
        if (other.CompareTag("Player"))
            return true;
        return other.GetComponentInParent<PlayerMovement>() != null;
    }
}

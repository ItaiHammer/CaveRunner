using UnityEngine;

public class CoreFragment : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other))
            return;

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterCoreFragmentCollected();

        Destroy(gameObject);
    }

    static bool IsPlayer(Collider2D other)
    {
        if (other.CompareTag("Player"))
            return true;
        return other.GetComponentInParent<PlayerMovement>() != null;
    }
}

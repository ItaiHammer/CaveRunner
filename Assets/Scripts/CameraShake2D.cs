using UnityEngine;

[DefaultExecutionOrder(100)]
public class CameraShake2D : MonoBehaviour
{
    float timeLeft;
    float duration;
    float magnitude;

    public void AddShake(float seconds, float worldAmplitude)
    {
        if (seconds <= 0f || worldAmplitude <= 0f)
            return;

        timeLeft = seconds;
        duration = seconds;
        magnitude = worldAmplitude;
    }

    void LateUpdate()
    {
        if (timeLeft <= 0f)
            return;

        timeLeft -= Time.deltaTime;
        float envelope = duration > 0.0001f ? Mathf.Clamp01(timeLeft / duration) : 0f;
        Vector2 j = Random.insideUnitCircle * (magnitude * envelope);
        transform.position += new Vector3(j.x, j.y, 0f);
    }
}

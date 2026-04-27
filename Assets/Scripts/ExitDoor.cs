using System.Collections;
using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    [Header("Unlock")]
    [Tooltip("How far the door moves up (local Y) when open.")]
    public float openDistance = 3f;
    [Tooltip("Seconds to slide up after the shake.")]
    public float openDuration = 0.55f;
    public SpriteRenderer doorOpen;
    public SpriteRenderer doorClose;

    [Header("Shake (door)")]
    [Tooltip("Seconds of rumble before the door lifts.")]
    public float shakeDuration = 0.32f;
    [Tooltip("World-space jitter (scaled down over time). Try 0.12–0.35 for a visible rumble.")]
    public float doorShakeStrength = 0.22f;

    [Header("Shake (camera)")]
    [Tooltip("Add CameraShake2D to the same GameObject as the camera / CameraFollow2D.")]
    public CameraShake2D cameraShake;
    [Tooltip("How long the view shakes (seconds).")]
    public float cameraShakeDuration = 0.4f;
    [Tooltip("World units — orthographic games often use ~0.1–0.35.")]
    public float cameraShakeMagnitude = 0.22f;

    [Header("Audio")]
    public AudioClip doorOpenSfx;
    [Tooltip("Optional override. If null, uses AudioSource on child \"Door Audio\", then on this object.")]
    public AudioSource doorAudioSource;

    Vector3 closedLocalPosition;
    bool unlockStarted;

    void Awake()
    {
        doorOpen.enabled = false;
        doorClose.enabled = true;

        closedLocalPosition = transform.localPosition;
        if (doorAudioSource == null)
        {
            Transform doorAudioChild = transform.Find("Door Audio");
            if (doorAudioChild != null)
                doorAudioSource = doorAudioChild.GetComponent<AudioSource>();
        }

        if (doorAudioSource == null)
            doorAudioSource = GetComponent<AudioSource>();

        if (doorOpenSfx != null && doorAudioSource == null)
            doorAudioSource = gameObject.AddComponent<AudioSource>();

        if (cameraShake == null && Camera.main != null)
            cameraShake = Camera.main.GetComponent<CameraShake2D>();
    }

    void Update()
    {
        if (unlockStarted)
            return;

        if (GameManager.Instance == null)
            return;

        if (!GameManager.Instance.HasMetFragmentRequirement)
            return;

        unlockStarted = true;
        StartCoroutine(UnlockRoutine());
    }

    IEnumerator UnlockRoutine()
    {
        if (doorOpenSfx != null && doorAudioSource != null)
            doorAudioSource.PlayOneShot(doorOpenSfx);

        cameraShake?.AddShake(cameraShakeDuration, cameraShakeMagnitude);

        Vector3 closedWorldPos = transform.position;
        float t = 0f;
        while (t < shakeDuration)
        {
            t += Time.deltaTime;
            float damp = 1f - t / shakeDuration;
            Vector2 j = Random.insideUnitCircle * (doorShakeStrength * damp);
            transform.position = closedWorldPos + new Vector3(j.x, j.y, 0f);
            yield return null;
        }

        transform.localPosition = closedLocalPosition;

        Vector3 start = closedLocalPosition;
        Vector3 end = closedLocalPosition + Vector3.up * openDistance;
        t = 0f;
        while (t < openDuration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / openDuration);
            u = u * u * (3f - 2f * u);
            transform.localPosition = Vector3.Lerp(start, end, u);
            doorOpen.enabled = true;
            doorClose.enabled = false;
            yield return null;
        }

        transform.localPosition = end;
    }
}

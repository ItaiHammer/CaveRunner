using UnityEngine;

/// <summary>
/// Smooth 2D follow with horizontal "lead" (camera sits slightly behind motion so you see more ahead).
/// Optional world-space map clamp for orthographic cameras.
/// </summary>
public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    [Tooltip("Fixed offset from the player (e.g. small Y lift).")]
    public Vector2 focusOffset;

    [Header("Lead (not dead-center)")]
    [Tooltip("If true, camera stays 'behind' motion (player toward the front of the frame). If false, camera shifts with motion (more tunnel ahead — common for speedrunners).")]
    public bool cameraBehindMotion = true;
    [Tooltip("How far the camera shifts horizontally (world units).")]
    public float horizontalLead = 2f;
    [Tooltip("How fast lead catches up when you change direction.")]
    public float leadResponsiveness = 5f;
    [Tooltip("Below this horizontal speed, lead eases back toward 0.")]
    public float leadVelocityDeadzone = 0.35f;

    [Header("Smoothing")]
    [Tooltip("Lower = snappier follow, higher = floatier.")]
    public float followSmoothTime = 0.12f;

    [Header("Map bounds (orthographic)")]
    public bool clampToMap = true;
    [Tooltip("World position at the bottom-left of the playable/map area.")]
    public Vector2 mapWorldMin;
    [Tooltip("World position at the top-right of the playable/map area.")]
    public Vector2 mapWorldMax;

    Camera cam;
    Rigidbody2D targetRb;
    Vector3 smoothVelocity;
    float currentLeadX;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (target != null)
            targetRb = target.GetComponent<Rigidbody2D>();
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        float vx = 0f;
        if (targetRb != null)
            vx = targetRb.velocity.x;
        else
            vx = Input.GetAxisRaw("Horizontal") * 10f;

        float leadGoal = 0f;
        if (Mathf.Abs(vx) > leadVelocityDeadzone)
        {
            float sign = Mathf.Sign(vx);
            leadGoal = (cameraBehindMotion ? -sign : sign) * horizontalLead;
        }

        currentLeadX = Mathf.Lerp(currentLeadX, leadGoal, 1f - Mathf.Exp(-leadResponsiveness * Time.deltaTime));

        Vector3 desired = target.position + (Vector3)focusOffset;
        desired.x += currentLeadX;
        desired.z = transform.position.z;

        Vector3 nextPos = Vector3.SmoothDamp(transform.position, desired, ref smoothVelocity, followSmoothTime);
        nextPos.z = transform.position.z;

        if (clampToMap && cam != null && cam.orthographic)
        {
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;

            float minX = mapWorldMin.x + halfW;
            float maxX = mapWorldMax.x - halfW;
            float minY = mapWorldMin.y + halfH;
            float maxY = mapWorldMax.y - halfH;

            if (minX > maxX)
                nextPos.x = (mapWorldMin.x + mapWorldMax.x) * 0.5f;
            else
                nextPos.x = Mathf.Clamp(nextPos.x, minX, maxX);

            if (minY > maxY)
                nextPos.y = (mapWorldMin.y + mapWorldMax.y) * 0.5f;
            else
                nextPos.y = Mathf.Clamp(nextPos.y, minY, maxY);
        }

        transform.position = nextPos;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!clampToMap)
            return;
        Gizmos.color = Color.cyan;
        Vector3 a = new Vector3(mapWorldMin.x, mapWorldMin.y, 0f);
        Vector3 b = new Vector3(mapWorldMax.x, mapWorldMax.y, 0f);
        Vector3 c1 = new Vector3(mapWorldMin.x, mapWorldMax.y, 0f);
        Vector3 c2 = new Vector3(mapWorldMax.x, mapWorldMin.y, 0f);
        Gizmos.DrawLine(a, c1);
        Gizmos.DrawLine(c1, b);
        Gizmos.DrawLine(b, c2);
        Gizmos.DrawLine(c2, a);
    }
#endif
}

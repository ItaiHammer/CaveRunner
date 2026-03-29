using UnityEngine;

[DefaultExecutionOrder(0)]
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
    Vector3 smoothedFollowPosition;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (target != null)
            targetRb = target.GetComponent<Rigidbody2D>();
        smoothedFollowPosition = transform.position;
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

        smoothedFollowPosition = Vector3.SmoothDamp(smoothedFollowPosition, desired, ref smoothVelocity, followSmoothTime);
        smoothedFollowPosition.z = transform.position.z;

        if (clampToMap && cam != null && cam.orthographic)
        {
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;

            float minX = mapWorldMin.x + halfW;
            float maxX = mapWorldMax.x - halfW;
            float minY = mapWorldMin.y + halfH;
            float maxY = mapWorldMax.y - halfH;

            if (minX > maxX)
                smoothedFollowPosition.x = (mapWorldMin.x + mapWorldMax.x) * 0.5f;
            else
                smoothedFollowPosition.x = Mathf.Clamp(smoothedFollowPosition.x, minX, maxX);

            if (minY > maxY)
                smoothedFollowPosition.y = (mapWorldMin.y + mapWorldMax.y) * 0.5f;
            else
                smoothedFollowPosition.y = Mathf.Clamp(smoothedFollowPosition.y, minY, maxY);
        }

        transform.position = smoothedFollowPosition;
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

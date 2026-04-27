using UnityEngine;

public class CoreFragment : MonoBehaviour
{
    [Header("Light Pulse")]
    [SerializeField] float minIntensityMultiplier = 0.7f;
    [SerializeField] float maxIntensityMultiplier = 1.25f;
    [SerializeField] float pulseSpeed = 2f;
    [SerializeField] float randomSpeedOffset = 0.8f;

    Transform lightTransform;
    Light pointLight;
    Component genericLightComponent;
    System.Reflection.PropertyInfo intensityProperty;
    float baseIntensity = 1f;
    float perInstanceSeed;
    float perInstanceSpeed;

    void Awake()
    {
        CacheLightComponent();
        perInstanceSeed = Random.Range(0f, 1000f);
        perInstanceSpeed = Mathf.Max(0.01f, pulseSpeed + Random.Range(-randomSpeedOffset, randomSpeedOffset));
    }

    void Update()
    {
        if (pointLight == null && intensityProperty == null)
            return;

        float noise = Mathf.PerlinNoise(perInstanceSeed, Time.time * perInstanceSpeed);
        float intensityMultiplier = Mathf.Lerp(minIntensityMultiplier, maxIntensityMultiplier, noise);
        SetLightIntensity(baseIntensity * intensityMultiplier);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other))
            return;

        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (player != null)
            player.PlayItemPickupSfx();

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

    void CacheLightComponent()
    {
        lightTransform = transform.Find("CoreFragment Light");
        if (lightTransform == null)
            return;

        pointLight = lightTransform.GetComponent<Light>();
        if (pointLight != null)
        {
            baseIntensity = pointLight.intensity;
            return;
        }

        genericLightComponent = lightTransform.GetComponent("Light2D");
        if (genericLightComponent == null)
            return;

        intensityProperty = genericLightComponent.GetType().GetProperty("intensity");
        if (intensityProperty == null)
        {
            genericLightComponent = null;
            return;
        }

        object intensityValue = intensityProperty.GetValue(genericLightComponent, null);
        if (intensityValue is float floatIntensity)
            baseIntensity = floatIntensity;
    }

    void SetLightIntensity(float newIntensity)
    {
        if (pointLight != null)
        {
            pointLight.intensity = newIntensity;
            return;
        }

        if (genericLightComponent != null && intensityProperty != null)
            intensityProperty.SetValue(genericLightComponent, newIntensity, null);
    }
}

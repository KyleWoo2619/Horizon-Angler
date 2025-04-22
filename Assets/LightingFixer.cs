using UnityEngine;
using UnityEngine.Rendering;

public class LightingFixer : MonoBehaviour
{
    void Start()
    {
        // Re-apply baked lighting setup
        DynamicGI.UpdateEnvironment();

        // Manually force the current lighting asset settings to apply
        RenderSettings.ambientIntensity = 1.0f;
        RenderSettings.reflectionIntensity = 1.0f;

        // (Optional) Reset fog or skybox if they break
        // RenderSettings.skybox = yourSkyboxMaterial;
        // RenderSettings.fog = true;
    }
}

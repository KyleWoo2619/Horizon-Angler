using UnityEngine;

[ExecuteAlways]
public class OceanAdvancedURP : MonoBehaviour
{
    public Material ocean;
    public Light sun;

    [Header("Wave 1 Settings")]
    public float waveLength = 10.0f;
    public float intensity = 4.0f;
    public float period = 1.0f;
    public float waveRotation = 0.0f;

    [Header("Wave 2 Settings")]
    public bool useSecondWave = false;
    public float waveLength2 = 5.0f;
    public float intensity2 = 2.0f;
    public float period2 = 0.5f;
    public float waveRotation2 = 45.0f;

    void Update()
    {
        if (ocean && sun)
        {
            ocean.SetVector("_WorldLightDir", -sun.transform.forward);
            ocean.SetVector("_SpecularColor", sun.color);

            ApplyWaveParams();
        }
    }

    void ApplyWaveParams()
    {
        if (ocean)
        {
            ocean.SetFloat("_WaveLengthInverse", 1.0f / Mathf.Max(waveLength, 0.0001f));
            ocean.SetFloat("_Intensity", intensity);
            ocean.SetFloat("_Periode", period);
            ocean.SetFloat("_WaveRotation", waveRotation);

            ocean.SetFloat("_UseSecondWave", useSecondWave ? 1.0f : 0.0f);
            ocean.SetFloat("_WaveLengthInverse2", 1.0f / Mathf.Max(waveLength2, 0.0001f));
            ocean.SetFloat("_Intensity2", intensity2);
            ocean.SetFloat("_Periode2", period2);
            ocean.SetFloat("_WaveRotation2", waveRotation2);
        }
    }

    public float GetWaterHeight(Vector3 worldPos)
    {
        float time = Time.time; // You could even use Time.timeSinceLevelLoad for closer match
        Vector2 pos = new Vector2(worldPos.x, worldPos.z);

        float angle1 = waveRotation * Mathf.Deg2Rad;
        Vector2 dir1 = new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1));
        float wave1 = Mathf.Sin(Vector2.Dot(pos, dir1 * (1.0f / waveLength)) + time * period) * intensity;

        float height = wave1;

        if (useSecondWave)
        {
            float angle2 = waveRotation2 * Mathf.Deg2Rad;
            Vector2 dir2 = new Vector2(Mathf.Cos(angle2), Mathf.Sin(angle2));
            float wave2 = Mathf.Sin(Vector2.Dot(pos, dir2 * (1.0f / waveLength2)) + time * period2) * intensity2;
            height += wave2;
        }

        return height;
    }

}

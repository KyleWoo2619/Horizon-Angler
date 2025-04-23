using System.Collections;
using UnityEngine;

public class Set6ObstacleWaveManager : MonoBehaviour
{
    private Test2Script gameManager;

    [Header("Obstacle Settings")]
    public GameObject[] obstaclePrefabs;
    public Transform leftWarning;
    public Transform rightWarning;

    [Header("Spawn Ranges")]
    public Transform leftSpawnMin;
    public Transform leftSpawnMax;
    public Transform rightSpawnMin;
    public Transform rightSpawnMax;

    public float warningTime = 1.5f;
    public float timeBetweenWaves = 4f;
    public int obstaclesPerWave = 1;
    public float obstacleFallSpeed = 250f;
    private bool isRunning = false;

    private Coroutine waveRoutine;
    private Coroutine monitorRoutine;

    void Start()
    {
        gameManager = FindObjectOfType<Test2Script>();

        leftWarning.gameObject.SetActive(false);
        rightWarning.gameObject.SetActive(false);

        if (monitorRoutine == null)
            monitorRoutine = StartCoroutine(MonitorSet6State());
    }

    void OnEnable()
    {
        if (monitorRoutine == null)
            monitorRoutine = StartCoroutine(MonitorSet6State());
    }


    void OnDisable()
    {
        if (monitorRoutine != null)
        {
            StopCoroutine(monitorRoutine);
            monitorRoutine = null;
        }
    }

    IEnumerator MonitorSet6State()
    {
        while (true)
        {
            bool shouldBeRunning = gameManager != null &&
                                   gameManager.microgamesActive &&
                                   gameManager.Sets.ContainsKey("Set6") &&
                                   gameManager.Sets["Set6"];

            if (shouldBeRunning && !isRunning)
            {
                if (waveRoutine != null)
                {
                    StopCoroutine(waveRoutine);
                    waveRoutine = null;
                }

                isRunning = true;
                waveRoutine = StartCoroutine(WaveRoutine());
                Debug.Log("[WaveManager] Started Set6 waves");
            }
            else if (!shouldBeRunning && isRunning)
            {
                if (waveRoutine != null)
                {
                    StopCoroutine(waveRoutine);
                    waveRoutine = null;
                }

                isRunning = false;
                leftWarning.gameObject.SetActive(false);
                rightWarning.gameObject.SetActive(false);

                Debug.Log("[WaveManager] Stopped Set6 waves");
            }


            yield return null;
        }
    }


    IEnumerator WaveRoutine()
    {
        Debug.Log("[WaveManager] Entered WaveRoutine loop");

        while (true)
        {
            if (!isRunning)
            {
                Debug.Log("[WaveManager] WaveRoutine canceled immediately — isRunning false at start");
                yield break;
            }
            leftWarning.gameObject.SetActive(false);
            rightWarning.gameObject.SetActive(false);
            bool isLeft = Random.value < 0.5f;
            Transform warn = isLeft ? leftWarning : rightWarning;
            Transform spawnMin = isLeft ? leftSpawnMin : rightSpawnMin;
            Transform spawnMax = isLeft ? leftSpawnMax : rightSpawnMax;

            // Flash warning
            warn.gameObject.SetActive(true);
            yield return new WaitForSeconds(warningTime);
            warn.gameObject.SetActive(false);

            for (int i = 0; i < obstaclesPerWave; i++)
            {
                Vector2 min = spawnMin.GetComponent<RectTransform>().anchoredPosition;
                Vector2 max = spawnMax.GetComponent<RectTransform>().anchoredPosition;
                Vector2 spawnPos = new Vector2(
                    Random.Range(min.x, max.x),
                    min.y
                );

                // Randomly choose one of the prefab tags
                GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
                string tag = prefab.name;

                GameObject pooledObstacle = ObstaclePooler.Instance.SpawnFromPool(tag, spawnPos, ObstaclePooler.Instance.transform);
                pooledObstacle.transform.localScale = Vector3.one;

                RectTransform rt = pooledObstacle.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = spawnPos;
                    rt.sizeDelta = new Vector2(25f, 50f); // Or whatever your intended size is
                }


                // Ensure it has a mover and configure speed
                ObstacleMover mover = pooledObstacle.GetComponent<ObstacleMover>();
                if (mover == null)
                {
                    mover = pooledObstacle.AddComponent<ObstacleMover>();
                }
                mover.moveSpeed = obstacleFallSpeed;

                Debug.Log($"[WaveManager] Spawned {tag} at {spawnPos}");
            }

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    public void ResetObstaclesAndState()
    {
        if (waveRoutine != null)
        {
            StopCoroutine(waveRoutine);
            waveRoutine = null;
        }

        isRunning = false; // <<< CRITICAL
        leftWarning.gameObject.SetActive(false);
        rightWarning.gameObject.SetActive(false);

        Debug.Log("[WaveManager] ResetObstaclesAndState completed.");
    }


}

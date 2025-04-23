using UnityEngine;
using UnityEngine.AI;

public class FishBossAI : MonoBehaviour
{
    public Transform[] patrolPoints;
    public Transform bossLocation;
    public float checkInterval = 1.5f;
    public float switchThreshold = 5f;
    public float idleTimeAtPoint = 2f;
    public float wanderRadius = 3f;

    private NavMeshAgent agent;
    private Transform player;
    private int currentTargetIndex = -1;
    private float idleTimer = 0f;
    private float wanderTimer = 0f;
    private Vector3 targetWanderPoint;
    private bool chasingBossSpot = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        InvokeRepeating(nameof(UpdateDestination), 0f, checkInterval);
    }

    void UpdateDestination()
    {
        if (player == null || patrolPoints.Length == 0 || agent == null || !agent.isOnNavMesh)
            return;

        if (chasingBossSpot)
        {
            agent.SetDestination(bossLocation.position);
            return;
        }

        // Find closest waypoint to player
        Transform closest = patrolPoints[0];
        float minDistance = Vector3.Distance(player.position, closest.position);

        for (int i = 1; i < patrolPoints.Length; i++)
        {
            float distance = Vector3.Distance(player.position, patrolPoints[i].position);
            if (distance < minDistance)
            {
                closest = patrolPoints[i];
                minDistance = distance;
            }
        }

        if (agent.remainingDistance <= switchThreshold)
        {
            idleTimer += checkInterval;
            wanderTimer += checkInterval;

            if (idleTimer >= idleTimeAtPoint)
            {
                Vector3 randomOffset = Random.insideUnitSphere * wanderRadius;
                randomOffset.y = 0f;
                targetWanderPoint = closest.position + randomOffset;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(targetWanderPoint, out hit, 1.5f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }

                idleTimer = 0f;
            }
        }
    }

    public void GoToBossLocation()
    {
        Debug.Log("[FishBossAI] Triggered boss catch behavior. Moving to boss location.");
        chasingBossSpot = true;
        agent.SetDestination(bossLocation.position);
    }

    public void DisableIfBossCaught()
    {
        var save = GameManager.Instance?.currentSaveData;
        if (save == null)
        {
            Debug.LogWarning("[FishBossAI] Save data not found.");
            return;
        }

        bool alreadyCaught = save.currentLevel switch
        {
            "Pond" => save.hasCaughtPondBoss,
            "River" => save.hasCaughtRiverBoss,
            "Ocean" => save.hasCaughtOceanBoss,
            _ => false
        };

        if (alreadyCaught)
        {
            Debug.Log("[FishBossAI] Boss has already been caught in this level. Disabling fish AI.");
            if (agent != null) agent.enabled = false;
            gameObject.SetActive(false);
        }
    }
}

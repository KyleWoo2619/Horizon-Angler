using UnityEngine;

public class BoundaryTrigger : MonoBehaviour
{
    private ForbiddenDirectionWarning warningManager;

    private void Start()
    {
        warningManager = FindObjectOfType<ForbiddenDirectionWarning>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.CompareTag("Player"))
        {
            warningManager?.OnBoundaryEntered();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.root.CompareTag("Player"))
        {
            warningManager?.OnBoundaryExited();
        }
    }
}

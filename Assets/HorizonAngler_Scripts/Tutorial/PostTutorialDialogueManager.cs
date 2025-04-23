using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PostTutorialDialogueManager : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public string[] lines;
    public float textSpeed = 0.05f;

    private int index = 0;
    private bool dialogueEnded = false;

    [Header("Dialogue UI")]
    public GameObject dialogueUI;
    private bool dialogueInteractionEnabled = true;

    [Header("Input")]
    public InputAction continueDialogueAction;

    [Header("Camera Settings")]
    public Transform cameraTarget;
    public float cameraMoveSpeed = 2f;
    private Camera mainCamera;
    private bool isMovingCamera = false;
    private bool hasCameraMoved = false;

    [Header("Cutscene Settings")]
    public PostTutorialCutsceneManager cutsceneManager;
    private bool cutsceneTriggered = false;

    void Awake()
    {
        if (continueDialogueAction != null)
            continueDialogueAction.Enable();
    }

    void OnEnable()
    {
        if (continueDialogueAction != null)
            continueDialogueAction.performed += OnContinueDialogueAction;
    }

    void OnDisable()
    {
        if (continueDialogueAction != null)
        {
            continueDialogueAction.performed -= OnContinueDialogueAction;
            continueDialogueAction.Disable();
        }
    }

    void Start()
    {
        mainCamera = Camera.main;

        if (dialogueUI != null)
            dialogueUI.SetActive(true);

        textComponent.text = string.Empty;
        StartDialogue();
    }

    // void Update()
    // {
    //     // Legacy mouse input for compatibility - can be removed if using only InputSystem
    //     if (dialogueInteractionEnabled && !isMovingCamera && Input.GetMouseButtonDown(0) && !dialogueEnded && !cutsceneTriggered)
    //     {
    //         ProgressDialogue();
    //     }
    // }

    public void OnContinueDialogueAction(InputAction.CallbackContext context)
    {
        if (dialogueInteractionEnabled && !isMovingCamera && !dialogueEnded && !cutsceneTriggered)
        {
            ProgressDialogue();
        }
    }

    private void ProgressDialogue()
    {
        if (textComponent.text == lines[index])
        {
            NextLine();
        }
        else
        {
            if (index != lines.Length -1)
            {
                StopAllCoroutines();
                textComponent.text = lines[index];
            }
        }
    }

    public void SetDialogueInteractionEnabled(bool enabled)
    {
        dialogueInteractionEnabled = enabled;
    }

    void StartDialogue()
    {
        index = 0;
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        textComponent.text = string.Empty;
        foreach (char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        // Safely trigger camera move
        if (index == lines.Length - 2 && !hasCameraMoved)
        {
            StartCoroutine(MoveCameraToTarget());
        }

        // Cutscene trigger ONLY after text has printed AND index is at last
        if (index == lines.Length - 1 && !cutsceneTriggered)
        {
            cutsceneTriggered = true;
            dialogueEnded = true;

            // Disable input before delay to avoid skip
            SetDialogueInteractionEnabled(false);

            yield return new WaitForSeconds(1.2f);  // Delay to linger on final dialogue
            Debug.Log("Triggering post-tutorial cutscene play...");

            if (cutsceneManager != null)
            {
                cutsceneManager.PlayCutscene();
                dialogueUI.SetActive(false); // Hide dialogue UI after cutscene starts
            }
            else
            {
                Debug.LogError("PostTutorialCutsceneManager is null! Please assign in inspector.");
            }
        }
    }


    void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;

            // If this is the second to last line and camera hasn't moved yet
            if (index == lines.Length - 2 && !hasCameraMoved)
            {
                StartCoroutine(MoveCameraToTarget());
                StartCoroutine(DelayedTypeLine(1.0f));
            }
            else
            {
                StartCoroutine(TypeLine());
            }
        }
    }

    IEnumerator DelayedTypeLine(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(TypeLine());
    }

    IEnumerator MoveCameraToTarget()
    {
        if (cameraTarget == null)
        {
            Debug.LogError("Camera target is null! Please assign in inspector.");
            yield break;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Main camera not found!");
                yield break;
            }
        }

        // Prevent multiple camera movements
        if (hasCameraMoved)
        {
            Debug.Log("Camera has already moved, skipping movement.");
            yield break;
        }

        isMovingCamera = true;
        hasCameraMoved = true;
        SetDialogueInteractionEnabled(false);

        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        Vector3 endPos = cameraTarget.position;
        Quaternion endRot = cameraTarget.rotation;

        float elapsed = 0f;
        float duration = 2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        mainCamera.transform.position = endPos;
        mainCamera.transform.rotation = endRot;

        SetDialogueInteractionEnabled(true);
        isMovingCamera = false;

        Debug.Log("Camera movement complete!");
    }

    IEnumerator DelayedPlayCutscene(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (cutsceneManager != null && !cutsceneTriggered)
        {
            Debug.Log("Triggering post-tutorial cutscene play...");
            cutsceneManager.PlayCutscene();
        }
        else if (cutsceneManager == null)
        {
            Debug.LogError("PostTutorialCutsceneManager is null! Please assign in inspector.");
        }
    }
}
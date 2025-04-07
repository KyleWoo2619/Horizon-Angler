using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// Put this script on your player model
public class PlayerController : MonoBehaviour
{
    [Header("Drag in Components")]
    public CharacterController controller;
    public Transform cam;
    public Transform player;
    public Transform rotator;

    [Header("Inputs")]
    public string moveX = "Horizontal";
    public string moveY = "Vertical";
    public string lookX = "Look X";
    public string lookY = "Look Y";

    // Input Variables
    [HideInInspector] public float inputMoveX = 0.0f;
    [HideInInspector] public float inputMoveY = 0.0f;
    [HideInInspector] public float inputLookX = 0.0f;
    [HideInInspector] public float inputLookY = 0.0f;

    [Header("Variable Settings")]
    public float speed = 10f;

    private float qMoveX, qMoveY;
    private Vector3 calc, lookAtPos;
    private bool inFZone;

    void Start()
    {

    }

    void Update()
    {
        ProcessInputs();
        ProcessMovement();
        ProcessLookAt();
        ProcessRotation();
    }

    void ProcessInputs()
    {
        inputMoveX = Input.GetAxis(moveX);
        inputMoveY = Input.GetAxis(moveY);
        inputLookX = Input.GetAxis(lookX);
        inputLookY = Input.GetAxis(lookY);
    }

    void ProcessMovement()
    {
        qMoveX = inputMoveX;
        qMoveY = inputMoveY;
        Vector3 Movement = cam.transform.right * qMoveX + cam.transform.forward * qMoveY;
        Movement.y = 0f;
        Movement = Movement.normalized;

        //Debug.Log(Movement);

        calc = Movement * speed * Time.deltaTime;
        controller.Move(calc);

        rotator.position = transform.position;
    }

    // Added ProcessRotation to make the player model smoothly rotate regardless of how snappy rotator is rotating
    // Added rotator so that when the player is in a fishing zone, rotator will snap to looking at the correct spot
    void ProcessRotation()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, rotator.rotation, 6 * Time.deltaTime); // If you want the player model to turn quicker or slower change the value multipled by Time.deltaTime
    }

    void ProcessLookAt()
    {
        // The below code is for turning the player towards where they are moving
        if (!inFZone)
        {
            if (calc.magnitude != 0f)
            {
                Vector3 newPosition = new Vector3(qMoveX, 0.0f, qMoveY);

                Quaternion CamRotation = cam.rotation;
                CamRotation.x = 0f;
                CamRotation.z = 0f;

                rotator.rotation = Quaternion.Lerp(rotator.rotation, Quaternion.LookRotation(newPosition) * CamRotation, 60 * Time.deltaTime);
                //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(newPosition) * CamRotation, 6 * Time.deltaTime); 

                // The below code works for strafing (player model always looks where the camera is facing when moving)
                /*
                transform.rotation = Quaternion.Lerp(transform.rotation, CamRotation, 3 * Time.deltaTime);
                */
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FishZone"))
        {
            Transform lookAt = other.transform.GetChild(0);
            lookAtPos = lookAt.position;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("FishZone"))
        {
            inFZone = true;
            float fishAngle = 10;
            rotator.LookAt(lookAtPos); 
            if (Vector3.Angle(transform.forward, lookAtPos - transform.position) < fishAngle)
            {
                // Prompt with click to cast

            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("FishZone"))
        {
            inFZone = false;
        }
    }
}

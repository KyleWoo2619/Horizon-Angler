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

    void Start()
    {
        
    }

    void Update()
    {
        ProcessInputs();
        ProcessMovement();
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
        float moveX = inputMoveX;
        float moveY = inputMoveY;
        Vector3 calc;
        Vector3 Movement = cam.transform.right * moveX + cam.transform.forward * moveY;
        Movement.y = 0f;
        Movement = Movement.normalized;

        //Debug.Log(Movement);

        calc = Movement * speed * Time.deltaTime;
        controller.Move(calc);
        
        // The below code is for turning the player towards where they are moving
        if (calc.magnitude != 0f)
        {
            Vector3 newPosition = new Vector3(moveX, 0.0f, moveY);

            Quaternion CamRotation = cam.rotation;
            CamRotation.x = 0f;
            CamRotation.z = 0f;

            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(newPosition) * CamRotation, 3 * Time.deltaTime); // If you want the player model to turn quicker or slower change the value multipled by Time.deltaTime

            // The below code works for strafing (player model always looks where the camera is facing when moving)
            /*
            transform.rotation = Quaternion.Lerp(transform.rotation, CamRotation, 3 * Time.deltaTime);
            */
        }
    }
}

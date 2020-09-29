using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Look Sensitivity")]
    public float sensX;
    public float sensY;

    [Header("Clamping")]
    public float minY;
    public float maxY;

    [Header("Spectator")]
    public float spectatorMoveSpeed;

    private float rotX;
    private float rotY;


    private bool isSpectator;



    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// LateUpdate is called every frame, if the Behaviour is enabled.
    /// It is called after all Update functions have been called.
    /// </summary>
    void LateUpdate()
    {
        rotX += Input.GetAxis("Mouse X") * sensX;
        rotY += Input.GetAxis("Mouse Y") * sensY;

        rotY = Mathf.Clamp(rotY, minY, maxY);


        if (isSpectator)
        {
            //Rot cam vertically
            transform.rotation = Quaternion.Euler(-rotY, rotX, 0);

            //Movement
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            float y = 0;

            if (Input.GetKey(KeyCode.E))
            {
                y = 1;
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                y = -1;
            }

            Vector3 dir = transform.right * x + transform.up * y + transform.forward * z;
            transform.position += dir * spectatorMoveSpeed * Time.deltaTime;
        }
        else
        {
            //rot cam vert
            transform.localRotation = Quaternion.Euler(-rotY, 0, 0);

            //rot player horiz
            transform.parent.rotation = Quaternion.Euler(transform.rotation.x, rotX, 0);
        }
    }


    public void SetAsSpectator()
    {
        isSpectator = true;
        transform.parent = null;
    }
}

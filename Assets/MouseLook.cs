using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public enum RotationAxis
    {
        MouseXandY,
        MouseX,
        MouseY
    }

    public Camera playerCamera;

    public RotationAxis axes = RotationAxis.MouseXandY;

    public float sensitivityHor = 8f;
    public float sensitivityVert = 6f;

    public float minimumVertRot = -45f;
    public float maximumVertRot = 45f;

    private float verticalRot = 0f;

    private void Start()
    {
        if(!playerCamera)
        {
            playerCamera = Camera.current;
        }

        Rigidbody body = GetComponent<Rigidbody>();
        if(body)
        {
            body.freezeRotation = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (axes)
        {
            case RotationAxis.MouseX:
                transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityHor, 0);
                break;

            case RotationAxis.MouseY:
                verticalRot -= Input.GetAxis("Mouse Y") * sensitivityVert;
                verticalRot = Mathf.Clamp(verticalRot, minimumVertRot, maximumVertRot);
                playerCamera.transform.localEulerAngles = new Vector3(verticalRot, transform.localEulerAngles.y, 0f);
                break;

            case RotationAxis.MouseXandY:
                verticalRot -= Input.GetAxis("Mouse Y") * sensitivityVert;
                verticalRot = Mathf.Clamp(verticalRot, minimumVertRot, maximumVertRot);

                float delta = Input.GetAxis("Mouse X") * sensitivityHor;

                playerCamera.transform.localEulerAngles = new Vector3(verticalRot, 0f, 0f);
                transform.localEulerAngles = new Vector3(0f, transform.localEulerAngles.y + delta, 0f);
                break;
        }
    }
}

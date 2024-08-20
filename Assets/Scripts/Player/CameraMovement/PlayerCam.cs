using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
//using DG.Tweening;

public class PlayerCam : MonoBehaviour
{
    [Tooltip("Input Controls from gamepad or KBM")]
    public InputControls input = null;

    [Tooltip("Used for the horizontal (X) and vertical (Y) camera movement sensitivity")]
    public float sensX;
    public float sensY;

    [Tooltip("The orientation of where the player is looking currently")]
    public Transform orientation;
    //public Transform camHolder;

    [Tooltip("Used for the horizontal (X) and vertical (Y) rotation movement")]
    float xRotation;
    float yRotation;

    private void Start()
    {
        //Cursor is locked to the middle of the screen and not visible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
    }

    private void Update()
    {
        //get device input
        float deviceX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX; //Might need more tweaking for controller input
        float deviceY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += deviceX;
        xRotation -= deviceY;
        
        //Clamping X roatation to 90
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Rotate Camera & Orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    //public void DoFov(float endValue)
    //{
    //    //GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    //}

    //public void DoTilt(float zTilt)
    //{
    //    //transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    //}
}

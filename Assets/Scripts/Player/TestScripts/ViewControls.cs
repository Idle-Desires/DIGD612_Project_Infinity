using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;//Input system use

public class ViewControls : MonoBehaviour
{
    //public GameObject player;

    //public Vector2 mousemovement;

    public float xRotation;
    public float yRotation;

    //public float cameraOffset;
    public float sensitivity = 20f;

    // Start is called before the first frame update
    void Start()
    {
        //cameraOffset = transform.position.y.player.transform.position.y;

        ////Cursor is locked to the middle of the screen and not visible
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        //Time.timeScale = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        //mousemovement = mousemovement.current.delta.ReadValue();
        //xRotation += mousemovement.y * Time.deltaTime * sensitivity;

        //xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        //yRotation += mousemovement.x * Time.deltaTime * sensitivity;

        ////Rotate Camera & Orientation
        //transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        //player.transform.rotation = Quaternion.Euler(0, yRotation, 0);

        //Vector3 pos = player.transform.position;
        //pos.y += cameraOffset;
        //transform.position = pos;
    }
}

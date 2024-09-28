using UnityEngine;
using Photon.Pun;

public class WeaponHolderCamera : MonoBehaviour
{
    public Transform cameraTransform; // Reference to the player's camera
    private PhotonView photonView;
    private bool isLocalPlayer;

    void Start()
    {
        // Get the PhotonView component to check if this is the local player
        photonView = GetComponentInParent<PhotonView>();

        // If this is the local player, assign the cameraTransform to the local camera
        if (photonView.IsMine)
        {
            isLocalPlayer = true;
            cameraTransform = GetComponentInParent<PlayerController>().playerCam.transform; // Get the player camera's transform
        }
    }

    void Update()
    {
        // If this is not the local player, keep the Weapon Holder synced to the camera's position and rotation
        if (!isLocalPlayer && cameraTransform != null)
        {
            // Sync the Weapon Holder's position with the camera
            transform.position = cameraTransform.position;

            // Sync the Weapon Holder's rotation with the camera's rotation (including vertical rotation)
            transform.rotation = cameraTransform.rotation;
        }
    }
}

using UnityEngine;
using Photon.Pun;

public class Projectile : MonoBehaviourPun
{
    public int damage = 10;
    private int shooterViewID;

    private void Start()
    {
        // Optional: Destroy the projectile after a certain time to prevent clutter
        Destroy(gameObject, 5f);
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    // Ignore collision with the shooter
    //    if (collision.gameObject.GetPhotonView() != null && collision.gameObject.GetPhotonView().ViewID == shooterViewID)
    //    {
    //        return;
    //    }

    //    PhotonView hitPhotonView = collision.collider.GetComponent<PhotonView>();
    //    if (hitPhotonView != null && !hitPhotonView.IsMine)
    //    {
    //        PlayerVariables hitPlayerVariables = hitPhotonView.GetComponent<PlayerVariables>();
    //        if (hitPlayerVariables != null)
    //        {
    //            //hitPlayerVariables.photonView.RPC("RPC_TakeDamage", RpcTarget.All, damage, photonView.ViewID); //use to be TakeDamage
    //            // Call TakeDamage on the hit player
    //            hitPlayerVariables.TakeDamage(damage, photonView);
    //            Debug.Log($"Projectile hit {hitPlayerVariables.playerName} for {damage} damage.");
    //        }
    //    }

    //    // Destroy the projectile upon collision
    //    PhotonNetwork.Destroy(gameObject);
    //}

    // Method to set the shooter’s PhotonView ID
    public void SetShooter(int viewID)
    {
        shooterViewID = viewID;
    }
}

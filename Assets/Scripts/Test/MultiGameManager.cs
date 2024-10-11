using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class MultiGameManager : MonoBehaviourPunCallbacks
{
    public static MultiGameManager Instance;

    [Header("Respawn Settings")]
    public Transform[] respawnPoints;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Respawns a player at a random respawn point.
    /// </summary>
    /// <param name="playerView">PhotonView of the player to respawn.</param>
    public void RespawnPlayer(PhotonView playerView)
    {
        if (respawnPoints.Length == 0)
        {
            Debug.LogError("No respawn points set in MultiGameManager.");
            return;
        }

        Transform respawnPoint = respawnPoints[Random.Range(0, respawnPoints.Length)];
        playerView.RPC("RPC_RespawnAtPosition", RpcTarget.AllBuffered, respawnPoint.position);
    }

    /// <summary>
    /// Update the scoreboard UI.
    /// </summary>
    public void UpdateScoreboard()
    {
        // Implement your scoreboard UI update logic here.
    }
}

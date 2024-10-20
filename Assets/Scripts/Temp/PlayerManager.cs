using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using System.IO;

public class PlayerManager : MonoBehaviour
{
    internal object PV;
    PhotonView photonView;

    GameObject controller;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

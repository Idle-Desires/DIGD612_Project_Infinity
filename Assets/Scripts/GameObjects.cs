using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjects : MonoBehaviour
{
    //Setting variables
    [SerializeField] GameObject gunObj;
    [SerializeField] GameObject gunHolder;
    
    // Start is called before the first frame update
    void Start()
    {
        gunObj.SetActive(true);
        gunHolder.SetActive(true);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class physics_simu : MonoBehaviour
{

    private GameObject player;
    public bool enemy_hit = false;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerStay(Collider col)
    {
        if (col.tag == "Enemy")
        {
            enemy_hit = true;
        }
    }
    void OnTriggerExit(Collider col)
    {
        if (col.tag == "Enemy")
        {

            enemy_hit = false;
        }
    }
    

}

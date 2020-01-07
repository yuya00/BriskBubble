using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageSetPos : MonoBehaviour
{

    private Transform camera_pos;
    private Transform my_transform;
    public GameObject camera;

    // Start is called before the first frame update
    void Start()
    {
        my_transform = GetComponent<Transform>();

        my_transform.position = GameObject.Find("Signboard").transform.position;

        camera = GameObject.Find("Camera");



    }

    // Update is called once per frame
    void Update()
    {
        my_transform.LookAt(camera.transform);
    }
}

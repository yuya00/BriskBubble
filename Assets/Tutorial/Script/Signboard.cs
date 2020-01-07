using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Signboard : MonoBehaviour
{

    public GameObject signboard_object = null;
    public Image signboard_image;
    // Start is called before the first frame update
    void Start()
    {
        signboard_object = GameObject.Find("test_image");
        signboard_image = signboard_object.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {

        signboard_image.enabled = false;
    }
}

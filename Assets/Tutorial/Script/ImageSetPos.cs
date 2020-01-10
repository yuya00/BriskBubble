using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageSetPos : MonoBehaviour
{

    private Transform my_transform;
    private GameObject camera;
    private GameObject read_button;
    private GameObject player;
    // Start is called before the first frame update
    void Start()
    {

        my_transform = GetComponent<Transform>();

        camera = GameObject.Find("Camera");

        read_button = GameObject.Find("buttonB");

        read_button.SetActive(false);

        //プレイヤーのゲームオブジェクトを取得
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        my_transform.LookAt(camera.transform);

        float distans = Vector3.Distance(player.transform.position, this.transform.position);

        if (distans > 10.0f)
        {
            read_button.SetActive(false);
        }
    }

    public void SetReadButton(Vector3 target_pos)
    {
        my_transform.position = target_pos;
        read_button.SetActive(true);
    }
    public void RemoveReadButton()
    {
        read_button.SetActive(false);
    }
}

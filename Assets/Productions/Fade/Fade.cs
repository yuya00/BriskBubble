using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Fade : FadeManager
{
    public string scene_name = "1st";
    public float interval_time = 2.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // シーンの名前と暗転するまでの時間設定
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FadeLoadScene(scene_name, interval_time);
        }

        /*
            base.load_scene(scene_name);
            これを書いたらシーンが切り替わる(引数にシーンの名前を入れる)
        */

    }
}

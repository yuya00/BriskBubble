using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Fade_Test : MonoBehaviour
{
    public Fade fade;
    private float interval_time = 0.0f;
    public string scene_name;
    public float interval_time_max;
    private bool fade_fg = false;

    void Start()
    {
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            fade.FadeIn(0.5f, () => print("フェードイン完了"));
            fade_fg = true;
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            fade.FadeOut(0.5f, () => print("フェードアウト完了"));
            //TransScene(scene_name, interval_time_max);
        }
        if (fade_fg) TransScene(scene_name, interval_time_max);
    }

    // シーン遷移用コルーチン .
    void TransScene(string scene_name, float interval_time_max)
    {
        interval_time += Time.deltaTime;
        if (interval_time > interval_time_max)
        {
            //シーン切替
            SceneManager.LoadScene(scene_name);
            fade_fg = false;
        }
    }

}

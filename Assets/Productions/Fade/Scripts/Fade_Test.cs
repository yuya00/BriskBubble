using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Fade_Test : MonoBehaviour
{
    public  Fade    fade;
    public  string  scene_name;
    private bool    fade_fg = false;
    private float   interval_time = 0.0f;
    public  float   interval_time_max;

    public float fade_in_time = 0.5f;
    public float fade_out_time = 1.0f;


    void Start()
    {
        fade.FadeOut(fade_out_time);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            fade.FadeIn(fade_in_time);
            //fade_fg = true;
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            fade.FadeOut(fade_out_time);
        }

        if (fade_fg) TransScene(scene_name, interval_time_max);
    }

    // シーン遷移用コルーチン .
    void TransScene(string scene_name, float interval_time_max)
    {
        interval_time += Time.deltaTime;
        if (interval_time > interval_time_max)
        {
            scene_change(scene_name);
            fade_fg = false;
        }
    }

    //シーン切替
    void scene_change(string scene_name)
    {
        SceneManager.LoadScene(scene_name);
    }

}

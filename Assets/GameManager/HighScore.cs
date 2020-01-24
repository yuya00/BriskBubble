using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
public class HighScore : MonoBehaviour
{
    //ハイスコア保存用変数
    static private int stage1_score=3;
    static private int stage2_score=2;
    static private int stage3_score=1;

    //開始時の時間保存用
    private int temp_time;

    private LimitTimer limit_timer;
    // Start is called before the first frame update
    void Start()
    {
        limit_timer = GetComponent<LimitTimer>();

        temp_time += limit_timer.min * 60;
        temp_time += (int)limit_timer.sec;
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().name == "stage_select")
        {
            switch(EventSystem.current.currentSelectedGameObject.name)
            {
                case "stage_1":
                    Debug.Log(stage1_score);
                    break;
                case "stage_2":
                    Debug.Log(stage2_score);
                    break;
                case "stage_3":
                    Debug.Log(stage3_score);
                    break;
            }
        }
    }

    public void SetScore()
    {
        switch(SceneManager.GetActiveScene().name)
        {
            case "stage_1":
                if (stage1_score > temp_time - (limit_timer.min * 60 + (int)limit_timer.sec))
                {
                    stage1_score = limit_timer.min * 60 + (int)limit_timer.sec;
                }
                break;
            case "stage_2":
                if (stage2_score > temp_time - (limit_timer.min * 60 + (int)limit_timer.sec))
                {
                    stage2_score = limit_timer.min * 60 + (int)limit_timer.sec;
                }
                break;
            case "stage_3":
                if (stage3_score > temp_time - (limit_timer.min * 60 + (int)limit_timer.sec))
                {
                    stage3_score = limit_timer.min * 60 + (int)limit_timer.sec;
                }
                break;
        }
    }

}

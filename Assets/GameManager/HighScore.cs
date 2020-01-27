using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class HighScore : MonoBehaviour
{
    //ハイスコア保存用変数
    static private int stage1_score=151;
    static private int stage2_score=76;
    static private int stage3_score=53;

    //開始時の時間保存用
    private int temp_time=0;

    //ベストタイム表示用
    public Text min;
    public Text sec;

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
            switch (EventSystem.current.currentSelectedGameObject.name)
            {
                case "stage_1":
                    min.text = stage1_score/60+"分";
                    sec.text = "" + stage1_score%60 + "秒";
                     break;
                case "stage_2":
                    min.text = "" + stage2_score/60 + "分";
                    sec.text = "" + stage2_score% 60 + "秒";
                    break;
                case "stage_3":
                    min.text = "" + stage3_score/60 + "分";
                    sec.text = "" + stage3_score% 60 + "秒";
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
                    stage1_score = temp_time - (limit_timer.min * 60 + (int)limit_timer.sec);
                }
                break;
            case "stage_2":
                if (stage2_score > temp_time - (limit_timer.min * 60 + (int)limit_timer.sec))
                {
                    stage2_score = temp_time - (limit_timer.min * 60 + (int)limit_timer.sec);
                }
                break;
            case "stage_3":
                if (stage3_score > temp_time - (limit_timer.min * 60 + (int)limit_timer.sec))
                {
                    stage3_score = temp_time - (limit_timer.min * 60 + (int)limit_timer.sec);
                }
                break;
        }
    }

}

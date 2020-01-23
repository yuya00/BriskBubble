using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pixeye.Unity;


public sealed class LimitTimer : MonoBehaviour
{
    private GameObject game_manager;

    //public Text time_text;

    public  int min;    // 分
    public float sec;   // 秒

    private int state;

    private bool timer_stop;

    private const int START = 0;
    private const int GAME = 1;

    private GameObject timer;

    // Start is called before the first frame update
    void Start()
    {
        game_manager = GameObject.FindGameObjectWithTag("GameManager");
        state = START;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch (state)
        {
            case START:
                if (game_manager.GetComponent<Scene>().StartFg()) state = GAME;
                break;
            case GAME:
                // 制限時間計算
                LimitTime();
                // 文字設定
                //if (time_text) SetText();
                break;
        }
    }

    void LimitTime()
    {
        // 一秒づつ減らす
        if (!timer_stop) sec -= Time.deltaTime;

        // 時間をとめる条件
        if (min <= 0 && sec <= 0)
        {
            timer_stop = true;
            sec = 0;
        }

        // 秒が減ったら分減らす
        if (sec <= 0)
        {
            if (!timer_stop) sec = 60;
            if (min > 0) min--;
        }
    }


    // 文字設定
    void SetText()
    {
        //time_text.text = min + "分" + (int)sec + "秒";
    }

    public bool TimerStop
    {
        get { return timer_stop; }
    }

    public int Min
    {
        get { return min; }
    }

    public float Sec
    {
        get { return sec; }
    }
}

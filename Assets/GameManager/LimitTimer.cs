using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class LimitTimer : MonoBehaviour
{
    public Text time_text;

    public int min;     // 分

    public float sec;   // 秒

    private bool timer_stop;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // 制限時間計算
        limit_time();

        // 文字設定
        set_text();
    }

    void limit_time()
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
    void set_text()
    {
        time_text.text = min + "分" + (int)sec + "秒";
    }

    public bool Timer_stop
    {
        get { return timer_stop; }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerSetSec : MonoBehaviour
{
    private GameObject obj;
    private Image image;
    private Sprite sprite;

    public int dig = 1;

    void Start()
    {
        obj = GameObject.FindGameObjectWithTag("GameManager");
    }

    void Update()
    {
        float sec = obj.GetComponent<LimitTimer>().Sec;
        Timer((int)sec);
    }

    // 画像を数字によって変える
    void Timer(int sec)
    {
        sprite = Resources.Load<Sprite>("blue/" + (sec / dig) % 10);
        image = this.GetComponent<Image>();
        image.sprite = sprite;

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerSetMin : MonoBehaviour
{
    private GameObject obj;
    private Image image;
    private Sprite sprite;

    void Start()
    {
        obj = GameObject.FindGameObjectWithTag("GameManager");
    }

    void Update()
    {
        int min = obj.GetComponent<LimitTimer>().Min;
        Timer(min);
    }

    // 画像を数字によって変える
    void Timer(int min)
    {
        sprite = Resources.Load<Sprite>("blue/" + min);
        image = this.GetComponent<Image>();
        image.sprite = sprite;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartTimer : MonoBehaviour
{
    private GameObject obj;
    private Image image;
    private Sprite sprite;

    public string path = "red";

    void Start()
    {
        obj = GameObject.FindGameObjectWithTag("GameManager");
    }

    void Update()
    {
        // 表示する数字を取得
        int sec = obj.GetComponent<Scene>().Send_buf_no;

        // 描画
        if (sec < 4)
        {
            Timer(sec);
            Alpha(1.0f);
        }
        else Alpha(0.0f);

        // いらなくなったら削除
        if (sec <= 0) Destroy(gameObject);
    }

    // 透明度調整
    void Alpha(float alp)
    {
        Color col = transform.GetComponent<Image>().color;
        transform.GetComponent<Image>().color = new Color(col.a, col.g, col.b, alp);
    }

    // 画像を数字によって変える
    void Timer(int sec)
    {
        sprite = Resources.Load<Sprite>(path + "/" + sec);
        image = this.GetComponent<Image>();
        image.sprite = sprite;
    }
}

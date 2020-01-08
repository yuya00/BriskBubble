using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyNum : MonoBehaviour
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
        int num = obj.GetComponent<EnemyKillCount>().EnemyNumMax;

        // 敵の数で画像変更
        Set(num);
    }


    // 画像を数字によって変える
    void Set(int sec)
    {
        sprite = Resources.Load<Sprite>(path + "/" + sec);
        image = this.GetComponent<Image>();
        image.sprite = sprite;
    }
}

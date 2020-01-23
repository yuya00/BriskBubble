using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteText : MonoBehaviour
{
    private Color col;
    private CanvasRenderer canvas;
    private float alpha = 1.0f;
    public float alpha_spd = -2;

    // 待機時間
    private float timer = 0;
    public float wait_timer = 2.5f;

    // Start is called before the first frame update
    void Start()
    {
        // モデルの色
        canvas = GetComponent<CanvasRenderer>();
        col = canvas.GetColor();
        alpha = 1.0f;

        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Wait()) Alpha();
    }

    // 時間待つ
    bool Wait()
    {
        timer += Time.deltaTime;
        if (timer >= wait_timer)
        {
            timer = wait_timer;
            return true;
        }
        return false;
    }

    // 消す
    void Alpha()
    {
        // 透明度操作
        alpha += alpha_spd * Time.deltaTime;

        // 透明なったら消す
        if (alpha <= 0)
        {
            gameObject.SetActive(false);
        }

        // 変更した透明度を画像にセット
        canvas.SetAlpha(alpha);
    }

}

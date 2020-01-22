using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIProduction : MonoBehaviour
{
    // 点滅用変数
    private Color col;
    private CanvasRenderer canvas;

    private float timer = 0;
    private float timer_max = 0.3f;

    private float alpha = 0.0f;
    public float alpha_max = 0.4f;

    public float alpha_spd = 3;

    // 位置を固定
    private Vector3 pos = new Vector3(224.5f, 100.5f, 0);

    // Start is called before the first frame update
    void Start()
    {
        // モデルの色
        canvas = GetComponent<CanvasRenderer>();
        col = canvas.GetColor();
        timer = 0;
        //alpha_max = alpha;
        transform.position = pos;
    }

    // Update is called once per frame
    void Update()
    {
        Flash();
    }

    // 点滅
    void Flash()
    {
        // 透明度操作
        alpha += alpha_spd * Time.deltaTime;

        // 透明度をあげるか下げるか
        if (alpha < 0)
        {
            alpha = 0;
            alpha_spd *= -1;
        }
        if (alpha > alpha_max)
        {
            alpha = alpha_max;
            alpha_spd *= -1;
        }

        // 変更した透明度を画像にセット
        canvas.SetAlpha(alpha);
    }
}

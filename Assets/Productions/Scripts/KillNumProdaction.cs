using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillNumProdaction : MonoBehaviour
{
#if false
    /*
        敵倒したときに文字を大きくする,
        
    */
#endif

    // 変数宣言
    private GameObject obj;             // 敵倒したのを取得
    private Vector3 init_pos;           // 修正位置
    private Vector3 init_scale;
    public float scale_spd = 1.0f;      // 大きくなる速さ
    public float scale_max = 3.0f;      // 最大サイズ
    public float move_spd = 5.0f;       // 移動する速さ
    private float adjust = 1.4f;        // 調整
    private bool prodaction_fg = false; // 演出するとき

    private int state = 0;

    // Start is called before the first frame update
    void Start()
    {
        obj = GameObject.FindGameObjectWithTag("GameManager");
        init_pos = transform.position;
        init_scale = transform.localScale;
        state = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // 演出on
        if (obj.GetComponent<EnemyKillCount>().KillFg) prodaction_fg = true;

        // 敵のカウントが減った時に演出させる
        Prodaction();
    }

    // 演出まとめ
    void Prodaction()
    {
        // 待機、大きく、小さく、待機
        switch (state)
        {
            case 0:
                // 位置を固定して待機
                transform.position = init_pos;
                if (prodaction_fg) state++;
                break;
            case 1:
                ScaleChange(scale_spd);     // サイズ変更
                PosChange(-move_spd);       // 位置変更
                break;
            case 2:
                ScaleChange(-scale_spd);    // サイズ変更
                PosChange(move_spd);        // 位置変更

                if (transform.localScale.x < init_scale.x)
                {
                    // 大きさ修正
                    transform.localScale = init_scale;
                    state = 0;
                }

                break;
        }
    }

    // サイズ変更(終了を戻り値で決定)
    void ScaleChange(float spd)
    {
        // サイズ変更
        transform.localScale += new Vector3(spd * Time.deltaTime, spd * Time.deltaTime, spd * Time.deltaTime);

        // 大きさチェック
        if (transform.localScale.x > scale_max)
        {
            // 大きさ修正
            transform.localScale = new Vector3(scale_max, scale_max, scale_max);
            state++;
        }

        // 動いてるときは初期化
        prodaction_fg = false;
    }

    // 大きさにあわせて位置変更
    void PosChange(float spd)
    {
        transform.position += new Vector3((spd * Time.deltaTime) * adjust, (-spd * Time.deltaTime) * adjust, 0);
    }

}

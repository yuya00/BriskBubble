using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixeye.Unity;

public sealed partial class Player : CharaBase
{
    [Foldout("ShotParameter", true)]
    public GameObject[] shot_object;        // ショットのobj
    public float shot_interval_time_max;    // ショットを撃つまでの間隔
    public float stop_time_max;             // どれだけ動けないか
    public float back_spd = 0.5f;           // 後ろ方向に進む速度
    [Foldout("ShotParameter", false)]

    private const float SHOT_POSITION = 2.8f;   // ショットを出す正面方向の位置補正
    private int shot_state;                 // debugでpublicにしてる
    private float charge_time;              // チャージ時間
    private float shot_interval_time;       // ショットの間隔
    private bool back_player;               // ショット3を撃った後にプレイヤーを後ろに飛ばす
    private float stop_time;                // 動けない時間
    private float init_back_spd;            // 初期速度保存用

    //---------------------------------------------//
    //                  ショット                   //
    //---------------------------------------------//
    void Shot()
    {
        // 次ショットまでの時間加算
        shot_interval();

        // 撃てるとき
        if (shot_interval_check())
        {
            // ショットの時間を固定
            shot_interval_time = shot_interval_time_max;

            // ショットのチャージが終わる前に発射したら、飛べるやつを配置
            if (Input.GetButton("Shot_L"))
            {
                // ショットのチャージ
                shot_charge();

                // ショットをチャージしてるときにショットの選択
                if (Input.GetButton("Shot_R")) shot_state = 1;
            }

            // 最終ショット発射
            if (Input.GetButtonUp("Shot_R"))
            {
                // ショットをstateの値で選択
                shot_select(shot_object[shot_state]);
            }
        }

    }

    // ショットの間隔
    void shot_interval()
    {
        // ショットを撃った後
        if (!shot_interval_check())
        {
            shot_interval_time += Time.deltaTime;
        }
    }

    // ショットのチャージ
    void shot_charge()
    {
        // ショットのチャージ
        charge_time += Time.deltaTime;

        // ショットをstateで管理
        if (charge_time > 2) shot_state = 2;
    }

    // ショットの設定リセット
    void riset()
    {
        // ショット、チャージリセット
        charge_time = 0;
        shot_state = 0;

        // ショット間隔の時間リセット
        shot_interval_time = 0;
    }


    // ショットの選択
    void shot_select(GameObject obj)
    {
        // ショットのオブジェクトを設定
        GameObject shot = Instantiate(shot_object[shot_state], transform.position + (transform.forward * SHOT_POSITION), Quaternion.identity);

        switch (shot_state)
        {
            case 0:
                // 1段階目
                shot.GetComponent<Shot01>().SetCharacterObject(gameObject);
                break;
            case 1:
                // 2段階目
                shot.GetComponent<Shot02>().SetCharacterObject(gameObject);
                break;
            case 2:
                // 3段階目
                shot.GetComponent<Shot03>().SetCharacterObject(gameObject);
                back_spd = init_back_spd;   // 初期化
                back_player = true;
                break;
        }
        // ショットが出たら値リセット
        riset();
    }

    // ショット3を撃った後、プレイヤーを後ろに飛ばす
    void back_move()
    {
        // 徐々に遅く
        if (back_spd > 0) back_spd -= 1.0f;

        // バックの速度をもとに後退
        velocity.x = -transform.forward.x * back_spd;
        velocity.z = -transform.forward.z * back_spd;

        // 待機時間が過ぎたらプレイヤーが動ける
        stop_time += Time.deltaTime;
        if (stop_time > stop_time_max)
        {
            // プレイヤーが操作可能になる
            back_player = false;
            stop_time = 0;
        }
    }

    // ショットが撃てるか
    bool shot_interval_check()
    {
        // ショットを撃った後、時間がたったら再度発射可能
        if (shot_interval_time >= shot_interval_time_max) return true;
        return false;
    }
    //---------------------------------------------//

    //---------------------------------------------//
    //           ショットに乗ったとき              //
    //---------------------------------------------//
    // ショットに乗った判定
    bool down_hit_shot()
    {
        // ショットのレイヤーを指定
        LayerMask layer = 1 << 8;

        // 落下中判定
        if (fall())
        {
            // きっちり足元判定
            for (int i = 0; i < 9; ++i)
            {
                //下レイが当たっていたら着地
                if (Physics.Linecast(
                    chara_ray.position + ofset_layer_pos[i],
                    chara_ray.position + ofset_layer_pos[i] + Vector3.down * (chara_ray_length), layer))
                {
                    return true;
                }
            }
        }
        return false;
    }

}

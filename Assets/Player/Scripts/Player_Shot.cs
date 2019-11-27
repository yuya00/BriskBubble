﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixeye.Unity;

public sealed partial class Player : CharaBase
{

    //---------------------------------------------//
    //                  ショット                   //
    //---------------------------------------------//
    void Shot()
    {
        // アニメーション
        shot_anime();

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
            else
            {
                charge_time = 0;
                shot_state = 0;
            }

            // 最終ショット発射
            if (Input.GetButtonUp("Shot_R"))
            {
                // ショットをstateの値で選択
                shot_select(shot_object[shot_state]);
            }
        }

    }

    // ショットのアニメーション
    void shot_anime()
    {
        // ショットのアニメーションがtrueのとき速度を上げる
        if (animator.GetBool("Shot"))
        {
            animator.speed = SHOT_ANIME_SPD;
        }

        // ショットが撃てるとき
        if (shot_interval_check())
        {
            // ショットを撃った
            if (Input.GetButtonUp("Shot_R"))
            {
                animator.SetBool("Shot", true);
                shot_anime_timer = 0;
            }
        }

        // ショットアニメの初期化
        if(shot_anime_timer++ > shot_anime_timer_max)
        {
            animator.SetBool("Shot", false);
            shot_anime_timer = 0;
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

        //animator.speed = init_anim_spd;
        //animator.SetBool("Shot", false);
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
    public bool down_hit_shot()
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
                    shot_jump_fg = true;
                    return true;
                }
            }
        }
        return false;
    }

    public bool Shot_jump_fg
    {
        get { return shot_jump_fg; }
    }


}

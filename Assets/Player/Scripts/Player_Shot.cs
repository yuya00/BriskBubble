using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixeye.Unity;

public sealed partial class Player : CharaBase
{

    // ショット-------------------------------------------------//
    void Shot()
    {
        // アニメーション
        ShotAnime();

        // 次ショットまでの時間加算
        ShotInterval();

        // 撃てるとき
        if (ShotIntervalCheck())
        {
            // ショットの時間を固定
            shot_interval_time = shot_interval_time_max;

            // ショットのチャージが終わる前に発射したら、飛べるやつを配置
            if (Input.GetButton("Shot_L"))
            {             
                // ショットをチャージしてるときにショットの選択
                if (Input.GetButton("Shot_R")) shot_state = 1;
            }
            else
            {
                charge_time = 0;
                shot_state = 0;
            }

            //ショットのチャージ
            if (Input.GetButton("Shot_R"))
            {
                ShotCharge();
            }

            // 最終ショット発射
            if (Input.GetButtonUp("Shot_R"))
            {
                // ショットをstateの値で選択
                ShotSelect(shot_object[shot_state]);
            }
        }

    }

    // ショットのアニメーション
    void ShotAnime()
    {
        // ショットのアニメーションがtrueのとき速度を上げる
        if (animator.GetBool("Shot"))
        {
            animator.speed = SHOT_ANIME_SPD;
        }

        // ショットが撃てるとき
        if (ShotIntervalCheck())
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
    void ShotInterval()
    {
        // ショットを撃った後
        if (!ShotIntervalCheck())
        {
            shot_interval_time += Time.deltaTime;
        }
    }

    // ショットのチャージ
    void ShotCharge()
    {
        // ショットのチャージ
        shot_charge_vol += shot_charge_speed;
        if (shot_charge_vol > max_charge_vol) shot_charge_vol = max_charge_vol;

        //チャージ中はプレイヤーを減速
        velocity.x *= charge_slow_down;
        velocity.z *= charge_slow_down;
    }

    // ショットの設定リセット
    void Riset()
    {
        // ショット、チャージリセット
        charge_time = 0;
        shot_state = 0;

        // ショット間隔の時間リセット
        shot_interval_time = 0;

        //ショットのチャージリセット
        shot_charge_vol = 0;

        //animator.speed = init_anim_spd;
        //animator.SetBool("Shot", false);
    }


    // ショットの選択
    void ShotSelect(GameObject obj)
    {
        // ショットのオブジェクトを設定


        GameObject shot = Instantiate(shot_object[shot_state], transform.position + (transform.forward * SHOT_POSITION), Quaternion.identity);

        switch (shot_state)
        {
            case 0:
                // 1段階目(チャージショット)

                //shotの大きさによって位置を補正
                shot.transform.position += (transform.forward * ((SHOT_POSITION/2) * (shot_charge_vol / shot.transform.localScale.x)));//前方向

                //shotの大きさを加算
                shot.transform.localScale = new Vector3(shot.transform.localScale.x + shot_charge_vol,
                                                        shot.transform.localScale.x + shot_charge_vol,
                                                        shot.transform.localScale.x + shot_charge_vol
                                                        );

                shot.GetComponent<Shot01>().SetCharacterObject(gameObject);
                break;
            case 1:
                // 2段階目
                shot.GetComponent<Shot02>().SetCharacterObject(gameObject);
                break;
            case 2:
                // 3段階目
                shot.GetComponent<Shot03>().SetCharacterObject(gameObject);
                back_speed = init_back_speed;   // 初期化
                back_player = true;
                break;
        }
        // ショットが出たら値リセット
        Riset();
    }

    // ショット3を撃った後、プレイヤーを後ろに飛ばす
    void BackMove()
    {
        // 徐々に遅く
        if (back_speed > 0) back_speed -= 1.0f;

        // バックの速度をもとに後退
        velocity.x = -transform.forward.x * back_speed;
        velocity.z = -transform.forward.z * back_speed;

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
    bool ShotIntervalCheck()
    {
        // ショットを撃った後、時間がたったら再度発射可能
        if (shot_interval_time >= shot_interval_time_max) return true;
        return false;
    }

    // ショットに乗った判定
    public bool DownHitShot()
    {
        // ショットのレイヤーを指定
        LayerMask layer = 1 << 8;

        // 落下中判定
        if (Falling())
        {
            // きっちり足元判定
            for (int i = 0; i < 9; ++i)
            {
				////下レイが当たっていたら着地
				//if (Physics.Linecast(
				//    chara_ray.position + ofset_layer_pos[i],
				//    chara_ray.position + ofset_layer_pos[i] + Vector3.down * (chara_ray_length), layer))
				//{
				//    shot_jump_fg = true;
				//    return true;
				//}
				//下レイが当たっていたら着地
				if (Physics.Linecast(
					ground_ray_pos + ofset_layer_pos[i],
					ground_ray_pos + ofset_layer_pos[i] + Vector3.down * (ground_ray_length), layer)) {
					shot_jump_fg = true;
					return true;
				}

			}
		}
        return false;
    }

    public bool ShotJumpFg
    {
        get { return shot_jump_fg; }
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public sealed partial class Camera_Script : MonoBehaviour
{
    #region 上みるカメラ追加ver
#if true
    // 初期化---------------------------------------------
    void Start()
    {
        // カメラの位置を設定
        transform.position = init_pos;

        // カメラの位置をプレイヤーの後ろにする為の方向ベクトル
        direction = -player.transform.forward.normalized;

        init_up_pos = player.transform.position.y;

        //camera_state = SCENE;
        camera_state = 0;

        // 演出初期化
        approach_state = 0;
        approach_timer = 0;
        init_zoom_out_spd = zoom_out_spd;
        enemy_hit_flg = false;
        enm_id = 0;
        clear_end = false;
        scene = GameObject.FindGameObjectWithTag("GameManager");

        // 敵を検索
        obj = GameObject.FindGameObjectsWithTag("Enemy");
        // 敵の数を初めに保存しておかないとLengthをそのまま使ったら
        // 4とか、初期値のままで処理されるからバグ出るかもしらん
        // 敵倒して時とかに保存しといた変数を-1とかしてやったらいけるかも
    }

    // 更新-----------------------------------------------
    void Update()
    {
        camera_enemy_approach();
    }

    // 処理が終わってから呼び出される---------------------
    void FixedUpdate()
    {
        camera_update();
    }

    // カメラまとめ---------------------------------------
    void camera_update()
    {
        switch (camera_state)
        {
            case NONE: camera_none(); break;            // 演出なし
            case ENM_HIT: enemy_hit_camera(); break;    // 敵に近づく
            case SCENE: scene_camera(); break;          // シーン開始カメラ
            case CLEAR: clear_camera(); break;
        }

        // カメラが何を見るかまとめ
        look();

        // クリア演出にはいる
        if (scene.GetComponent<Scene>().Clear_fg())
        {
            camera_state = CLEAR;
        }

    }

    // カメラが何を見るかまとめ---------------------------
    void look()
    {
        // 最終的に見る方向初期化
        Vector3 look_pos = Vector3.zero;

        // 演出時と通常時の注視点の切り替え
        switch (camera_state)
        {
            // 演出無いとき
            case NONE: look_pos = player_target(); break;
            case ENM_HIT: look_pos = enemy_target(look_pos); break;
            case SCENE: look_pos = world_target(); break;
            case CLEAR: look_pos = clear_target(); break;
        }

        // 注視点の方に向く
        look_lerp(transform.position, look_pos, LOOK_SPD);
    }

    // NONE-----------------------------------------------
    #region プレイヤー追従カメラ(通常時)
    void camera_none()
    {
        // カメラ位置
        Vector3 cam_pos = new Vector3(player.transform.position.x, player.transform.position.y + init_up_pos + UP, player.transform.position.z);

        // パッド情報を取得
        pad_rx = -Input.GetAxis("R_Stick_H");
        pad_ry = Input.GetAxis("R_Stick_V");
        pad_lx = Input.GetAxis("L_Stick_H");

        // カメラの位置変更
        x_rotate(cam_pos, pad_rx);
        y_rotate(cam_pos, pad_ry, pad_rx);

        // 左スティックで入力してる時に条件付でカメラ追従
        if (pad_lx_check(pad_lx)) follow_camera();

        // 位置が戻ってきたらここに来て初期化
        clear_enemy_hit_camera();
    }

    // 右スティックでカメラ移動
    void x_rotate(Vector3 cam_pos, float pad_rx)
    {
        // 正規化に使う平方根
        float x_len = Mathf.Sqrt(pad_rx * pad_rx);

        // 入力軸正規化
        if (x_len > 1.0f) pad_rx /= x_len;

        // アフィン変換
        float s = Mathf.Sin((TURN + TURN) * pad_rx);
        float c = Mathf.Cos((TURN + TURN) * pad_rx);
        float x = direction.x * c - direction.z * s;
        float z = direction.x * s + direction.z * c;
        direction.x = x;
        direction.z = z;

        // 正規化
        direction.Normalize();

        // pad_xで入力したらカメラの位置変更
        transform.position = cam_pos + (direction * DIST);
    }

    void y_rotate(Vector3 cam_pos, float pad_ry, float pad_rx)
    {
        // スティックの入力が無いときは0に向かう
        if (pad_ry == 0f && pad_rx == 0)
        {
            direction.y += (0f - direction.y) * (Y_CLEAR_SPD * Time.deltaTime);
        }

        // pad_xで入力したらカメラの位置変更
        if (direction.y > 0.5f) direction.y = 0.5f;
        if (direction.y < -0.5f) direction.y = -0.5f;

        direction.y += pad_ry * TURN;

        // 正規化
        direction.Normalize();
    }

    // カメラの追従
    void follow_camera()
    {
        state_check(player.transform.right.normalized - player.transform.forward.normalized, pad_lx);
    }

    // カメラとプレイヤーの角度によって追従を変更
    void state_check(Vector3 vec, float pad_lx)
    {
        switch (follow_state)
        {
            case 0:
                // 内積で角度がありすぎたらカメラ追跡
                if (angle_check()) follow_state = 1;
                break;
            case 1:
                // ベクトルを徐々にその方向に持っていく
                //direction = Vector3.Lerp(direction, vec, 1 * Time.deltaTime);
                direction.x += vec.x * (FOLLOW_SPD * Time.deltaTime);
                direction.z += vec.z * (FOLLOW_SPD * Time.deltaTime);

                // 入力をやめたらカメラの追跡をやめる
                if (Mathf.Abs(pad_lx) <= STATE_CHECK) follow_state = 0;
                break;
        }
    }

    // プレイヤーとカメラの角度チェック
    bool angle_check()
    {
        // 角度がANGLEとANGLE_MAXの間やったら、trueを返す、それを左右やってる
        if ((int)rotate_angle() > ANGLE && (int)rotate_angle() < ANGLE_MAX) return true;
        if ((int)rotate_angle() < -ANGLE && (int)rotate_angle() > -ANGLE_MAX) return true;

        // 角度が範囲外
        return false;
    }

    // 左スティックのパッド操作をしているか
    bool pad_lx_check(float pad_lx)
    {
        // 入力してるか
        if (pad_lx == 0) return false;
        return true;
    }

    // カメラの位置とプレイヤーの正面で回転角を取得
    float rotate_angle()
    {
        // 方向取得
        Vector3 vec = transform.position - player.transform.position;

        // 外積で横ベクトルをだす
        Vector3 axis = Vector3.Cross(-player.transform.forward, vec);

        // ＋とーの角度計算
        float angle = Vector3.Angle(-player.transform.forward, vec) * (axis.y < 0 ? -1 : 1);

        return angle;
    }

    #endregion

    // ENM_HIT--------------------------------------------
    #region エネミー演出カメラ(近づく)
    // カメラが敵に近づく演出するための処理---------------
    void camera_enemy_approach()
    {
        // 敵を全て探す
        for (int i = 0; i < obj.Length; i++)
        {
            // そのオブジェクトが存在していたら
            if (obj[i])
            {
                // 敵の当たり判定がtrueになった敵の位置を取得
                if (obj[i].GetComponent<Enemy>().Shot_touch_flg)
                {
                    // 当たった敵の番号を保存
                    enm_id = i;

                    // 位置を保存
                    enm_pos = obj[i].GetComponent<Enemy>().Transform_position;

                    // 演出させる
                    enemy_hit_flg = true;
                }
            }
        }

        // 演出処理する
        if (enemy_hit_flg) camera_state = ENM_HIT;
    }

    void enemy_hit_camera()
    {
        // どこまで近づいてどこまで遠ざかる(下がる)処理
        approach(enm_pos, save_pos, zoom_len, zoom_in_spd, zoom_out_spd);
    }

    // 近づいて遠ざかる処理
    void approach(Vector3 near_pos, Vector3 back_pos, float len, float zoom_in_spd, float zoom_out_spd)
    {
        // 初期化
        Vector3 vec = Vector3.zero;

        switch (approach_state)
        {
            case 0:
                // 送られてきた位置(敵位置)とのベクトル取得
                vec = near_pos - transform.position;

                // 近づける(Lerpなしで)
                transform.position += vec.normalized * (zoom_in_spd * Time.deltaTime);

                // 近づいたら次のステート
                if (vec.magnitude < len) approach_state++;
                break;
            case 1:
                // 戻る位置とのベクトル取得
                vec = back_pos - transform.position;

                // 時間経ったら遠ざける
                if (timer_check_enemy_hit_camera(approach_timer_max))
                {
                    transform.position += vec.normalized * (zoom_out_spd * Time.deltaTime);

                    // 遠ざかったらカメラステート変更（approach_stateを初期化するのはNONEで）
                    if (vec.magnitude < zoom_out_spd * Time.deltaTime)
                    {
                        // 近づいた敵の判定を初期化する
                        if (obj[enm_id]) obj[enm_id].GetComponent<Enemy>().Shot_touch_flg_false();

                        // 演出用の判定を初期化
                        enemy_hit_flg = false;

                        // カメラ状態をプレイヤー追従に
                        camera_state = NONE;
                    }
                }
                break;
        }
    }

    // 1回演出が終わったら初期化する
    void clear_enemy_hit_camera()
    {
        // ここで位置を保存してここの位置に戻らせる
        save_pos = transform.position;

        // ステートの初期化
        approach_state = 0;

        // 遠ざける速度の初期化
        zoom_out_spd = init_zoom_out_spd;

        approach_timer = 0;
    }

    #endregion

    // SCENE----------------------------------------------
    #region シーン始まったときのカメラ
    // シーンを見下ろすカメラ
    void scene_camera()
    {
        Vector3[] scene_camera_pos = { init_pos, new Vector3(init_pos.x, init_pos.y, init_pos.z * -1) };

        float len = (scene_camera_pos[scene_pos_no] - transform.position).magnitude;

        if (len < SCENE_LEN) scene_pos_no++;
        if (scene_pos_no > SCENE_POS_MAX)
        {
            scene_pos_no = 0;
            camera_state = NONE;
        }

        Vector3 pos = scene_camera_pos[scene_pos_no] - transform.position;

        // 移動方法
        transform.position += pos.normalized * (scene_move_spd * Time.deltaTime);

    }

    #endregion

    // CLEAR----------------------------------------------
    #region クリア演出
    // 敵を全滅させたらここに来る
    void clear_camera()
    {
        // プレイヤーの前右斜め下(に向かっていく)
        Vector3 pos = player.transform.position + (player.transform.forward.normalized * 4) + (player.transform.right.normalized * 2) - (player.transform.up * 0.5f);
        transform.position = Vector3.Lerp(transform.position, pos, clear_cam_spd * Time.deltaTime);

        clear_wait_timer += Time.deltaTime;
        // 待機時間後にシーン移行
        if (clear_wait_timer > clear_wait_timer_max)
        {
            // 処理が全部終わったら
            clear_end = true;
            clear_wait_timer = 0;
        }
    }
    #endregion

    // カメラ関連カメラ-----------------------------------
    #region カメラ便利関数
    // 徐々に注視点の方を向くようにする
    void look_lerp(Vector3 cam_pos, Vector3 target, float spd)
    {
        // 注視点 - カメラ位置で注視点の方向を取得
        Vector3 vec = target - cam_pos;

        // 最終的に見る方向を取得
        Vector3 target_pos = cam_pos + vec.normalized;

        // 徐々に方向を変える
        Vector3 target_look = Vector3.Lerp(cam_pos + transform.forward.normalized, target_pos, spd * Time.deltaTime);

        // ここに設定したら角度が変わる
        set_target(target_look);
    }

    // セットしたほうを向く
    void set_target(Vector3 vec) { transform.LookAt(vec); }

    // 時間経過の判定
    bool timer_check_enemy_hit_camera(float timer_max)
    {
        approach_timer += Time.deltaTime;
        if (approach_timer > timer_max) return true;
        return false;
    }

    // 注視点設定
    Vector3 player_target() { return new Vector3(player.transform.position.x, player.transform.position.y + UP_TARGET, player.transform.position.z); }

    Vector3 enemy_target(Vector3 look_pos) { return Vector3.Lerp(transform.position + look_pos, enm_pos, LOOK_SPD * Time.deltaTime); }

    Vector3 world_target() { return Vector3.zero; }

    Vector3 clear_target() { return player.transform.position; }

    #endregion

    // クリア演出終了
    public bool Clear_end()
    {
        return clear_end;
    }

    void OnGUI()
    {
        if (gui_on)
        {
            GUILayout.BeginVertical("box");

            //uGUIスクロールビュー用
            Vector2 leftScrollPos = Vector2.zero;

            // スクロールビュー
            leftScrollPos = GUILayout.BeginScrollView(leftScrollPos, GUILayout.Width(200), GUILayout.Height(400));
            GUILayout.Box("Camera");


            #region ここに追加
            GUILayout.TextArea("clear_end\n" + clear_end);
            GUILayout.TextArea("scene.Clear_fg()\n" + scene.GetComponent<Scene>().Clear_fg());
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);     

            // スペース
            GUILayout.Space(200);
            GUILayout.Space(10);
            #endregion


            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }
#endif
    #endregion

}

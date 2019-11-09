using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraTest : MonoBehaviour
{
    #region 最新カメラ
#if true
    public GameObject player;               // プレイヤー
    private Vector3 direction;              // 方向ベクトル

    public float UP = 4.0f;                 // カメラの高さ
    public float UP_TARGET = 4.0f;          // 注視点の高さ
    public float TURN = 0.035f;             // 手動カメラ移動の速さ
    public float DIST = 10.0f;              // プレイヤーからどれだけ離れてるか
    public float ANGLE = 75.0f;             // カメラを追従させるまでのプレイヤーとの角度
    public float ANGLE_MAX = 145.0f;        // 角度の最大量

    private float init_up_pos;              // 初期プレイヤーのＹ位置
    private float pad_rx;                   // スティック情報の値
    private float pad_lx;                   // 左スティック

    //--------------------------------------------
    // 演出                          
    //--------------------------------------------           
    public GameObject enemy;                // 敵のオブジェクトを取得
    private Vector3 save_pos;               // 演出前の位置を保存

    public float zoom_in_spd = 30.0f;       // 近づく早さ
    public float zoom_out_spd = 50.0f;      // 遠ざかる速さ
    public float zoom_len = 8.0f;           // どこまで近づくか
    public float approach_timer_max = 1.0f; // 近づいてから何秒とめるか
    public float scene_move_spd = 1.0f;
    public float LOOK_SPD = 15.0f;          // 徐々に向かせる回転の速さ

    private float init_zoom_out_spd;        // 遠ざかる速さ(初期化用)
    private float approach_timer;

    private int camera_state = 0;           // 通常時と演出時を分ける
    private int approach_state;             // 演出時のステート
    private int scene_pos_no;

    private const int NONE = 0;             // 何も演出なし
    private const int ENM_HIT = 1;          // 敵倒すとき
    private const int SCENE = 2;            // シーン始まったとき
    //--------------------------------------------           

    // 初期化
    void Start()
    {
        //// カメラの位置をプレイヤーの位置に設定
        //transform.position = player.transform.position;
        float x = 60;
        float y = 20;
        float z = -80;
        transform.position = new Vector3(x, y, z);

        // カメラの位置をプレイヤーの後ろにする為の方向ベクトル
        direction = -player.transform.forward.normalized;

        // ジャンプ中は追従させないように位置を代入
        init_up_pos = player.transform.position.y;

        //camera_state = SCENE;
        camera_state = 0;

        // 演出初期化
        approach_state = 0;
        approach_timer = 0;
        init_zoom_out_spd = zoom_out_spd;
    }

    void Update()
    {
        // コンポーネント取得
        //enemy = GetComponent<Enemy>().gameObject;
    }

    // 処理が終わってから呼び出される
    void FixedUpdate()
    {
        camera_update();
    }

    //--------------------------------------------
    // カメラまとめ                         
    //--------------------------------------------           
    void camera_update()
    {
        switch (camera_state)
        {
            // 演出無いとき
            case NONE: camera_none(); break;
            case ENM_HIT: enemy_hit_camera(enemy.transform.position); break;
            case SCENE: scene_camera(); break;
        }

        // debug
        // ステート切り替え(ショットが当たった判定になったら)
        if (Input.GetKeyDown(KeyCode.V)) camera_state++;
        if (camera_state > 2) camera_state = 0;

        // カメラが何を見るかまとめ
        look();
    }
    //--------------------------------------------

    //--------------------------------------------
    // カメラが何を見るかまとめ
    //--------------------------------------------           
    void look()
    {
        // 最終的に見る方向初期化
        Vector3 look_pos = Vector3.zero;

        // 演出時と通常時の注視点の切り替え
        switch (camera_state)
        {
            // 演出無いとき
            case NONE:      look_pos = player_target();         break;
            case ENM_HIT:   look_pos = enemy_target(look_pos);  break;
            case SCENE:     look_pos = world_target();          break;
        }

        // 注視点の方に向く
        look_lerp(transform.position, look_pos, LOOK_SPD);
    }

    //--------------------------------------------

    // NONE
    #region プレイヤー追従カメラ(通常時)
    //--------------------------------------------
    // 通常時まとめ
    //--------------------------------------------           
    void camera_none()
    {
        // カメラ位置
        Vector3 cam_pos = new Vector3(player.transform.position.x, player.transform.position.y + init_up_pos + UP, player.transform.position.z);

        // パッド情報を取得
        pad_rx = -Input.GetAxis("R_Stick_H");
        pad_lx = Input.GetAxis("L_Stick_H");

        // カメラの位置変更
        rotate(cam_pos, pad_rx);

        // 左スティックで入力してる時に条件付でカメラ追従
        if (pad_lx_check(pad_lx)) follow_camera(player.transform.right.normalized - player.transform.forward.normalized);

        // 位置が戻ってきたらここに来て初期化
        clear_enemy_hit_camera();
    }

    // 右スティックでカメラ移動
    void rotate(Vector3 cam_pos, float pad_rx)
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

    // カメラの追従
    void follow_camera(Vector3 vec)
    {
        //Vector3 v = player.transform.right.normalized * 10 + player.transform.forward.normalized * 30;
        // 内積チェックしてカメラを移動させる
        //if (angle_check())
        //{
        //    //look_lerp(transform.position, player.transform.position + v, LOOK_SPD);
        //    direction = Vector3.Lerp(direction, vec, 1 * Time.deltaTime);
        //}
        state_check(vec, pad_lx);
    }
    private int follow_state;
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
                direction = Vector3.Lerp(direction, vec, 1 * Time.deltaTime);

                // 入力をやめたらカメラの追跡をやめる
                if (Mathf.Abs(pad_lx) <= 0.2f) follow_state = 0;
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
        // 入力していない
        if (pad_lx == 0) return false;

        // 入力あり
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

    //--------------------------------------------           
    #endregion

    // ENM_HIT
    #region エネミー演出カメラ(近づく)
    //--------------------------------------------
    // エネミーに近づく演出カメラまとめ  
    //--------------------------------------------           
    void enemy_hit_camera(Vector3 obj_pos)
    {
        // どこまで近づいてどこまで遠ざかる(下がる)処理
        approach(obj_pos, save_pos, zoom_len, zoom_in_spd, zoom_out_spd);
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
                    transform.position += vec.normalized * (zoom_out_spd * Time.deltaTime);

                // 遠ざかったらカメラステート変更（approach_stateを初期化するのはNONEで）
                if (vec.magnitude < zoom_out_spd * Time.deltaTime) camera_state = NONE;
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

    //--------------------------------------------           
    #endregion

    // SCENE
    #region シーン始まったときのカメラ
    //-----------------------------------------
    // シーン始まったときのカメラ 
    //-----------------------------------------
    void scene_camera()
    {
        float x = 60;
        float y = 20;
        float z = -80;

        Vector3[] pos = { new Vector3(x, y, z), new Vector3(x, y, z * -1) };

        float len = (pos[scene_pos_no] - transform.position).magnitude;

        if(len < 1.0f)
        {
            scene_pos_no++;
        }
        if(scene_pos_no > 1)
        {
            scene_pos_no = 0;
            camera_state = NONE;
        }

        Vector3 vec = pos[scene_pos_no] - transform.position;

        // 移動方法
        transform.position += vec.normalized * (scene_move_spd * Time.deltaTime);
        //transform.position = Vector3.Lerp(transform.position, pos[scene_pos_no], scene_move_spd * Time.deltaTime);

    }

    //-----------------------------------------
    #endregion

        
    #region カメラ便利関数
    //-----------------------------------------
    // カメラ便利関数 
    //-----------------------------------------
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

    bool timer_check_enemy_hit_camera(float timer_max)
    {
        approach_timer += Time.deltaTime;
        if (approach_timer > timer_max)
        {
            //approach_timer = 0;
            return true;
        }
        return false;

    }

    #region 注視点設定
    Vector3 player_target()                 { return new Vector3(player.transform.position.x, player.transform.position.y + UP_TARGET, player.transform.position.z); }
    Vector3 enemy_target(Vector3 look_pos)  { return Vector3.Lerp(transform.position + look_pos, enemy.transform.position, LOOK_SPD * Time.deltaTime); }
    Vector3 world_target()                  { return Vector3.zero; }

    //--------------------------------------------
    #endregion

    //-----------------------------------------
    #endregion

    void OnGUI()
    {
        /*
        GUILayout.BeginVertical("box");

        // スクロールビュー
        //leftScrollPos = GUILayout.BeginScrollView(leftScrollPos, GUILayout.Width(200), GUILayout.Height(400));

        GUILayout.TextArea("camera_state\n" + camera_state);
        GUILayout.TextArea("scene_pos_no\n" + scene_pos_no);//save_pos
        GUILayout.TextArea("pad_lx_check(pad_lx)\n" + pad_lx_check(pad_lx));//save_pos
        //GUILayout.TextArea("pos\n" + pos);//save_pos
        //GUILayout.TextArea("pos\n" + pos);//save_pos
        //GUILayout.TextArea("pos\n" + pos);//save_pos
        //GUILayout.TextArea("pos\n" + pos);//save_pos
        //GUILayout.TextArea("pos\n" + pos);//save_pos
        //GUILayout.TextArea("pos\n" + pos);//save_pos
        //GUILayout.TextArea("pos\n" + pos);//save_pos
        //GUILayout.TextArea("pos\n" + pos);//save_pos
        //GUILayout.TextArea("pos\n" + pos);//save_pos
        //GUILayout.TextArea("pos\n" + pos);//save_pos
        //GUILayout.TextArea("pos\n" + pos);//save_pos
        //GUILayout.TextArea("pos\n" + pos);//save_pos
        //GUILayout.TextArea("pos\n" + pos);//save_pos
        //GUILayout.TextArea("pos\n" + pos);//save_pos
        //GUILayout.TextArea("pos\n" + pos);//save_pos
        //GUILayout.TextArea("pos\n" + pos);//save_pos
        //GUILayout.TextArea("pos\n" + pos);//save_pos

        // スペース
        GUILayout.Space(200);
        GUILayout.Space(10);
        // スペース
        //GUILayout.EndScrollView();

        GUILayout.EndVertical();
        // */
    }

#endif
    #endregion

}

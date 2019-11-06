using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraTest : MonoBehaviour
{

    #region 今回カメラ
#if true

    public GameObject player;                 // プレイヤー
    public float UP = 4.0f;     // カメラの高さ
    public float UP_TARGET = 4.0f;     // 注視点の高さ
    public float TURN = 0.035f;   // 手動カメラ移動の速さ
    public float DIST = 10.0f;    // プレイヤーからどれだけ離れてるか

    private Vector3 direction;              // 方向ベクトル
    private float init_up_pos;            // 初期プレイヤーのＹ位置
    private float pad_rx;               // スティック情報の値
    private float pad_lx; // 左スティック

    public float spd = 1;                // カメラが動くときの速度
    public float ANGLE = 75.0f;          // カメラを追従させるまでのプレイヤーとの角度
    public float ANGLE_MAX = 145.0f;     // 角度の最大量

    private int follow_state = 0;

    // 初期化
    void Start()
    {
        // カメラの位置をプレイヤーの位置に設定
        transform.position = player.transform.position;

        // カメラの位置をプレイヤーの後ろにする為の方向ベクトル
        direction = -player.transform.forward.normalized;

        // ジャンプ中は追従させないように位置を代入
        init_up_pos = player.transform.position.y;
    }

    // 処理が終わってから呼び出される
    void FixedUpdate()
    {
        cam();
    }

    void OnGUI()
    {
        GUILayout.BeginVertical("box");

        // スクロールビュー
        //leftScrollPos = GUILayout.BeginScrollView(leftScrollPos, GUILayout.Width(200), GUILayout.Height(400));

        debug();

        // スペース
        GUILayout.Space(10);

        // スペース
        //GUILayout.EndScrollView();

        GUILayout.EndVertical();
    }

    void debug()
    {
        //GUILayout.TextArea("pad_rx\n" + pad_rx);
        //GUILayout.TextArea("angle_check()\n" + angle_check());
        //GUILayout.TextArea("direction\n" + direction);
        //GUILayout.TextArea("位置\n" + (player.transform.position - transform.position));
        //GUILayout.TextArea("stick_rx\n" + stick_rx);
        //GUILayout.TextArea("stick_rx\n" + stick_rx);
        //GUILayout.TextArea("stick_rx\n" + stick_rx);

        GUILayout.Space(10);
        GUILayout.Space(10);
        GUILayout.Space(10);
    }

    // カメラまとめ
    void cam()
    {
        // カメラ位置
        Vector3 pos = new Vector3(player.transform.position.x, player.transform.position.y + init_up_pos + UP, player.transform.position.z);

        // tps時の注視点
        Vector3 target = new Vector3(player.transform.position.x, player.transform.position.y + UP_TARGET, player.transform.position.z);

        // 注視点の方に向く
        transform.LookAt(target);

        // パッド情報を取得
        pad_rx = -Input.GetAxis("R_Stick_H");
        pad_lx = Input.GetAxis("L_Stick_H");

        // カメラの位置変更
        rotate(pos, pad_rx);

        // 左スティックで入力してる時に条件付でカメラ追従
        if (pad_lx_check(pad_lx)) follow_camera(player.transform.right.normalized - player.transform.forward.normalized);
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
        //// 内積チェック
        //if (angle_check())
        // ステートで追跡
        state_check(vec, pad_lx);
    }

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
                direction = Vector3.Lerp(direction, vec, spd * Time.deltaTime);

                // 入力をやめたらカメラの追跡をやめる
                if (Mathf.Abs(pad_lx) <= 0.1f) follow_state = 0;
                break;
        }
    }

    // プレイヤーとカメラの角度チェック
    bool angle_check()
    {
        //*************************************************************//
        //*************************************************************//
        // int型の引数にしてswitchで左右の移動させるか、
        // Lerpを自分で作るか、
        // がたつかんように急に位置変更しない
        //*************************************************************//
        //*************************************************************//

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

#endif
    #endregion

}

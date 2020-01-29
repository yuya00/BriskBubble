using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixeye.Unity;

public sealed partial class CameraScript : MonoBehaviour
{

	//[Header("カメラGUIの表示")]
	//public bool gui_on;

	[System.Serializable]
	public struct GUI {
		[Header("GUIの表示")]
		public bool on;

		[Header("全値表示")]
		public bool all_view;

		[Header("開発用値表示")]
		public bool debug_view;
	}
	[Header("GUI")]
	public GUI gui;


	[Foldout("CameraFollowPlayer", true)]
    public GameObject player;               // プレイヤー

    public float UP = 4.0f;                 // カメラの高さ
    public float UP_TARGET = 4.0f;          // 注視点の高さ
    public float TURN = 0.035f;             // 手動カメラ移動の速さ
    public float DIST = 10.0f;              // プレイヤーからどれだけ離れてるか
    public float ANGLE = 75.0f;             // カメラを追従させるまでのプレイヤーとの角度
    public float ANGLE_MAX = 145.0f;        // 角度の最大量

    [Foldout("CameraFollowPlayer", false)]

	private bool fall_can_move;             // 追従できる
	private bool front_fall_can_move;       // 追従できる
	private Vector3 cam_pos;				// カメラ位置
	private Vector3 direction;              // 方向ベクトル
    private float init_up_pos;              // 初期プレイヤーのＹ位置
    private float pad_rx;                   // スティック情報の値
    private float pad_ry;                   // スティック情報の値
    private float pad_lx;                   // 左スティック

    private int follow_state;

	private Vector3 respawn_dir;            // 方向ベクトル
	private float	respawn_init_up_pos;    // 初期プレイヤーのＹ位置

	//--------------------------------------------
	// 演出                          
	//--------------------------------------------    
	[Foldout("CameraProduction", true)]
    public Vector3 init_pos = new Vector3(60, 20, -80);               // 見渡し始める位置

    public float zoom_in_spd = 30.0f;       // 近づく早さ
    public float zoom_out_spd = 50.0f;      // 遠ざかる速さ
    public float zoom_len = 8.0f;           // どこまで近づくか
    public float approach_timer_max = 1.0f; // 近づいてから何秒とめるか
    public float LOOK_SPD = 15.0f;          // 徐々に向かせる回転の速さ
    public float Y_CLEAR_SPD = 4.0f;        // カメラY位置初期化速度

    public float clear_cam_spd = 1.0f;
    public float clear_wait_timer_max = 10.0f;
    private float clear_wait_timer = 0f;

    [Foldout("CameraProduction", false)]
    private GameObject[] obj;               // 敵のオブジェクト取得
    private GameObject scene;               // クリアフラグ取得
    private GameObject post;                // postprocess取得

    private Vector3 save_pos;               // 演出前の位置を保存
    private Vector3 enm_pos;                // 敵の位置取得
    private Vector3 adjust_pos;             // 敵に寄るときの一定値

    private float init_zoom_out_spd;        // 遠ざかる速さ(初期化用)
    private float approach_timer;

    public bool enemy_hit_flg;              // 敵の判定取得
    private bool clear_end;                 // クリア演出終わったのをゲームマネージャーに

    private int approach_state;             // 演出時のステート
    private int scene_pos_no;
    private int enm_id;                     // 敵の番号

    private const int NONE = 0;             // 何も演出なし
    private const int ENM_HIT = 1;          // 敵倒すとき
    private const int SCENE = 2;            // シーン始まったとき
    private const int CLEAR = 3;

    private const float SCENE_LEN = 1.0f;   // どこまで近づくか
    private const float FOLLOW_SPD = 1.0f;
    private const float STATE_CHECK = 0.2f;

    // シーン始まった時のカメラ
    [Foldout("SceneCameraTargetPos", true)]
    public float camera_fadein_time = 0.35f;
    public float camera_fadeout_time = 0.35f;
    public int camera_state = 0;           // 通常時と演出時を分ける
    public float scene_move_spd = 1.0f;
    public float fade_timer_max = 0.7f;

    public GameObject[] target      = new GameObject[SCENE_TARGET_MAX];
    public float[]      pos_length  = new float[SCENE_TARGET_MAX];    // 目的地からどれくらい離したところに配置するか

    [Foldout("SceneCameraTargetPos", false)]
    private int select_scene_camera = 1;// debug

    private const int SCENE_TARGET_MAX = 4;    // カメラが向かう目的地の数
    private int scene_camera_state = 0;
    private float fade_timer = 0;
    private Vector3 scene_look_pos;
    //--------------------------------------------

}

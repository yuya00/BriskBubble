using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class Player : CharaBase
{
    private Vector3 axis;                   //入力値
    private Vector3 input;                  //入力値
    public float jump_power_up;          // ショットに乗ったときにジャンプ力を何倍にするか

    private float init_spd;               // 初期速度
    private float init_fric;              // 初期慣性STOP
    public float slope = 0.3f;           // スティックの傾き具合設定用
    private float jump_anim_count = 0;
    private const float NORMALIZE = 1.0f;
    private bool ray_fg;
	private bool wall_touch_flg = false;        //壁との当たり判定

	// あにめ
	private Animator animator;
    private float COUNT;
    private float anim_spd = 3.0f;

    // 状態
    private const int WAIT = 0;
    private const int WALK = 1;
    private const int RUN = 2;


    //--- カメラ用↓ ---//
    public GameObject cam;
    public float rot_spd = 10.0f;

    // プレイヤーの足元用データ
    private Vector3[] ofset_layer_pos =
    {
        // 上
         new Vector3( -1, 0, -1 ),
         new Vector3(  0, 0, -1 ),
         new Vector3(  1, 0, -1 ),
         // 真ん中
         new Vector3( -1, 0,  0 ),
         new Vector3(  0, 0,  0 ),
         new Vector3(  1, 0,  0 ),
         // 下
         new Vector3( -1, 0,  1 ),
         new Vector3(  0, 0,  1 ),
         new Vector3(  1, 0,  1 ),
    };
}

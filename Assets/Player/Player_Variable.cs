using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixeye.Unity;

public sealed partial class Player : CharaBase
{
	[Foldout( "Base" ,true)]
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

	[Foldout( "Base" ,false)]

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


		//壁掴み判定Ray ---------------------------------------------
	[System.Serializable]
	public struct WallGrabRay {

		[Header("Gizmoの表示")]
		public bool gizmo_on;

		[SerializeField, Range(0.0f, 2.0f), Header("Rayの高さ")]
		public float height;		//1.3f

		[SerializeField, Range(0.0f, 5.0f), Header("Rayの長さ")]
		public float length;		//2.0f

		[System.NonSerialized]	//掴み準備判定
		public bool prepare_flg;

		[System.NonSerialized]	//レイ判定
		public bool ray_flg;

		[System.NonSerialized]	//掴み判定
		public bool flg;

		[SerializeField, Range(0.0f, 2.0f), Header("横移動制限")]
		public float side_length;	//1.2f;

		[Header("入力待ち")]
		public int delaytime;	//10
	}
	[Header("壁掴み判定Ray")]
	public WallGrabRay wallGrabRay;


	#region 壁掴み向き調整
	private Vector2 wall_forward;
	private Vector2 wall_back;
	private Vector2 wall_right;
	private Vector2 wall_left;
	private Vector2 player_forward;

	private float wall_forward_angle;
	private float wall_back_angle;
	private float wall_right_angle;
	private float wall_left_angle;
	#endregion

	//存在しない大きな値
	private int NOTEXIST_BIG_VALUE = 999;

}

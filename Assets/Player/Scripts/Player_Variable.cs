using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixeye.Unity;

public sealed partial class Player : CharaBase
{
	[Foldout("PlayerParameter", true)]
    private Vector3 axis;                   //入力値
    private Vector3 input;                  //入力値

    private int state;  // 状態
    private const int START = 0;
    private const int GAME = 1;
    private const int CLEAR = 2;

    private bool shot_jump_fg;

    private float init_spd;               // 初期速度
    private float init_fric;              // 初期慣性STOP

    public float slope = 0.3f;           // スティックの傾き具合設定用
    private float fall_y;
    public float fall_y_max = -100.0f;

    private Vector3 respawn_pos;

    private float jump_anim_count = 0;
    private const float NORMALIZE = 1.0f;
    private bool ray_fg;
	private bool wall_touch_flg = false;        //壁との当たり判定

    private int coin_count;     // コイン入手数

    private GameObject game_manager;
	private SphereCollider sphere_collider = null;

	// あにめ
	private Animator animator;
    private float COUNT;
    private float INIT_ANIME_SPD = 1.0f;
    private const float ANIME_SPD = 3.0f;
    private const float SHOT_ANIME_SPD = 5.0f;

    private float shot_anime_timer = 0;
    private float shot_anime_timer_max = 5;

    // 状態
    private const int WAIT = 0;
    private const int WALK = 1;
    private const int RUN = 2;


    //--- カメラ用↓ ---//
    public GameObject cam;
    public float rot_spd = 10.0f;

	[Foldout("PlayerParameter", false)]

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

    [Foldout("ShotParameter", true)]
    public GameObject[] shot_object;        // ショットのobj
    public float shot_interval_time_max;    // ショットを撃つまでの間隔
    public float stop_time_max;             // どれだけ動けないか
    public float back_spd = 0.5f;           // 後ろ方向に進む速度
    public float jump_power_up;             // ショットに乗ったときにジャンプ力を何倍にするか
    [Foldout("ShotParameter", false)]

    private const float SHOT_POSITION = 3.5f;   // ショットを出す正面方向の位置補正
    private int shot_state;                 // debugでpublicにしてる
    private float charge_time;              // チャージ時間
    private float shot_interval_time;       // ショットの間隔
    private bool back_player;               // ショット3を撃った後にプレイヤーを後ろに飛ばす
    private float stop_time;                // 動けない時間
    private float init_back_spd;            // 初期速度保存用


    //壁掴み判定Ray ---------------------------------------------
    [System.Serializable]
	public class WallGrabRay : Ray_Base {

		[SerializeField, Range(0.0f, 2.0f), Header("Rayの高さ")]
		public float height;		//1.3f

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



	#region 先行入力
	[Header("先行入力の実行")]
	public  bool		 lead_input_on;			//先行入力オンオフ
	private const int	 leadkey_num	= 3;	//保存するキー数
	private const int	 keyserve_time	= 8;	//保存時間
	private Leadkey_Kind lead_key		= 0;	//保存されたキー

	enum Leadkey_Kind{
		NONE,
		JUMP,
	}

	struct LeadInputs {
		public Leadkey_Kind pushed_key;	//押されたキー
		public int frame;				//キー保存時間
	};
	LeadInputs[] lead_inputs = new LeadInputs[leadkey_num];
	#endregion


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixeye.Unity;

public sealed partial class Player : CharaBase
{
	[Foldout("PlayerParameter", true)]
    public float slope = 0.3f;          // スティックの傾き具合設定用
    public float fall_y_max = -100.0f;  // リスポーン用

    public GameObject cam;              // カメラオブジェ
    public float rot_speed = 10.0f;     // カメラの回転速度

    [Foldout("PlayerParameter", false)]
    private Vector3 axis;                   //入力値
    private Vector3 input;                  //入力値

    private int state;  // 状態
    private const int START = 0;
    private const int GAME = 1;
    private const int CLEAR = 2;

    private bool shot_jump_fg;

    private float init_speed;               // 初期速度
    private float init_fric;              // 初期慣性STOP

    private float fall_y;

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

    //[Foldout("PlayerEffect", true)]
    //public GameObject effect_jump;
    ////public GameObject effect_run;
    ////public GameObject effect_shot;
    ////public int EFFECT_NUM = 10;

    //[Foldout("PlayerEffect", false)]
    //private float effect_down_pos = -1.0f;
    //private int effect_no = 0;
    //enum EFFECT
    //{
    //    JUMP = 0,
    //    RUN_GROUND,
    //    RUN_WATER,
    //    SHOT,
    //}
    private EffectManager effect;

    // キャラ指定
    private EffectManager.TYPE PLAYER = EffectManager.TYPE.PLAYER;

    // エフェクトの種類指定
    private EffectManager.EFFECT JUMP = EffectManager.EFFECT.JUMP;
    private EffectManager.EFFECT EFC_RUN = EffectManager.EFFECT.RUN;
    private EffectManager.EFFECT SHOT = EffectManager.EFFECT.SHOT;

    public int EFFECT_NUM = 10;         // エフェクトを出す数

    // 位置変数
    private float jump_down_pos = -1.0f;
    private float shot_down_pos = 2.0f;

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
    public float back_speed = 0.5f;           // 後ろ方向に進む速度
    public float jump_power_up;             // ショットに乗ったときにジャンプ力を何倍にするか
    [Foldout("ShotParameter", false)]

    private const float SHOT_POSITION = 3.5f;   // ショットを出す正面方向の位置補正
    private int shot_state;                 // debugでpublicにしてる
    private float charge_time;              // チャージ時間
    private float shot_interval_time;       // ショットの間隔
    private bool back_player;               // ショット3を撃った後にプレイヤーを後ろに飛ばす
    private float stop_time;                // 動けない時間
    private float init_back_speed;            // 初期速度保存用


	//気絶
	enum Enum_Faint {
		CLEAR,      //初期化
		WAIT,       //待機
		WAIT2,      //待機
		END         //終了
	}
	Enum_Faint enum_faint;


	//壁掴み判定Ray ---------------------------------------------
	[System.Serializable]
	public class WallGrabRay : RayBase
    {

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
		public int delay_time;	//10
	}
	[Header("壁掴み判定Ray")]
	public WallGrabRay wall_grab_ray;


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




	//踏みつけ判定 --------------------------------------
	[System.Serializable]
	public class TreadOn_BoxCast : BoxCastBase {
		//flgはジャンプ中、着地するまでtrue

		//半径に掛ける,XZ軸の倍率
		public const float RADIUS_MAG_XZ = 2.5f;

		//Y軸の長さ
		public const float LENGTH_Y		 = 0.3f;

		//BoxCastを飛ばす最大の長さ
		public const float MAX_DISTANCE	 = 0.2f;


		public const int FOWARD_POWER	 = 10;

		public const int JUMP_POWER		 = 30;

	}
	[Header("踏みつけ判定")]
	public TreadOn_BoxCast tread_on;


	//気絶時のノックバック --------------------------------------
	public struct KnockBack {
		public const float SPD_MAG       = 4;
		public const float JUMP_POWER    = 20;
		public const int   TIME          = 30;
		public const int   FAINT_TIME    = 80 - TIME;  //硬直時間(-ノックバック)
	}


	#region 先行入力
	[Header("先行入力の実行")]
	public  bool		 lead_input_on;			//先行入力オンオフ
	private const int	 lead_key_num	= 3;	//保存するキー数
	private const int	 key_serve_time	= 8;	//保存時間
	private LeadkeyKind lead_key		= 0;	//保存されたキー

	enum LeadkeyKind{
		NONE,
		JUMP,
	}

	struct LeadInputs
    {
		public LeadkeyKind pushed_key;	//押されたキー
		public int frame;				//キー保存時間
	};
	LeadInputs[] lead_inputs = new LeadInputs[lead_key_num];
	#endregion





}

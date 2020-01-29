using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Pixeye.Unity;

public sealed partial class Player : CharaBase
{
	[Foldout("PlayerParameter", true)]
    public float slope = 0.3f;          // スティックの傾き具合設定用
	private const float FALL_Y_CHACE_MAX	 = -60.0f;  // 移動不可,カメラ追従停止(リスポーン用)
	private const float FALL_Y_MAX		 = -100.0f; // 最大落下地点(リスポーン用)
	private bool fall_can_move;

	public GameObject cam;              // カメラオブジェ
    public float rot_speed = 10.0f;     // カメラの回転速度

	private bool speedy_flg;
	//public float SPEEDYRUN_SPEED = 20.0f;
	public float speedrun_spd = 20.0f;

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

    private float jump_anim_count = 0;
    private const float NORMALIZE = 1.0f;
    private bool ray_fg;
	private bool wall_touch_flg = false;        //壁との当たり判定

	private const int SPEDDY_TIME = 90;			 //加速までの時間

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
	private const int SPEEDYRUN = 3;

	private EffectManager effect;

    // キャラ指定
    private EffectManager.TYPE PLAYER = EffectManager.TYPE.PLAYER;

    // エフェクトの種類指定
    private EffectManager.EFFECT JUMP = EffectManager.EFFECT.JUMP;
    private EffectManager.EFFECT EFC_RUN = EffectManager.EFFECT.RUN;
    private EffectManager.EFFECT SHOT = EffectManager.EFFECT.SHOT;
    private EffectManager.EFFECT COIN = EffectManager.EFFECT.COIN;

    private SoundManager sound;

    // キャラ指定
    private SoundManager.CHARA_TYPE PLAYER_SE = SoundManager.CHARA_TYPE.PLAYER;

    // 音の種類指定
    private SoundManager.SE_TYPE JUMP_SE = SoundManager.SE_TYPE.JUMP;
    private SoundManager.SE_TYPE SHOT_SE = SoundManager.SE_TYPE.SHOT;
    private SoundManager.SE_TYPE CHANGE_SE = SoundManager.SE_TYPE.WEAPON_CHANGE;
    private SoundManager.SE_TYPE DAMAGE_SE = SoundManager.SE_TYPE.DAMAGE;


    // 位置変数
    private float jump_down_pos = -1.0f;
    private float run_down_pos = -1.5f;
    private float shot_down_pos = 2.0f;

    // 足元情報
    enum FOOT
    {
        NONE = 0,
        GROUND,
        WATER,
    }
    private int foot = 0;
    private bool fool_fg;
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
	[Tooltip("ショットの操作切り替え")]
	public bool shot_switch;

	public GameObject[] shot_object;        // ショットのobj
    public float shot_interval_time_max;    // ショットを撃つまでの間隔
    public float stop_time_max;             // どれだけ動けないか
    public float back_speed = 0.5f;         // 後ろ方向に進む速度
    public float jump_power_up;             // ショットに乗ったときにジャンプ力を何倍にするか

    public float max_charge_vol;            //ショットのチャージ上限
    public float shot_charge_time;          //チャージが最大になる時間
    [Tooltip("チャージ中のプレイヤーの減速率0～１(0.8なら20%減速)")]
    public float charge_slow_down;


    public float max_charge_length;　　　　 //やまなりショットの最大距離
    public float shot_length_charge_time;　 //やまなりショットの距離が最大になる時間
    public float simulate_interval;
    public GameObject physics_simulate_object;   //やまなりショットの軌道予測用のオブジェクト

    private const int simulate_object_num = 40;
    private List<Vector3> physics_simulate_pos = new List<Vector3>(); //やまなりショットの座標記憶用
    private List<GameObject> physics_simulate_object_clone = new List<GameObject>();//やまなりショットの軌道表示用
    public PhysicsScene physics_simulate_scene; 　　　//やまなりショット用の物理シーン
    public UnityEngine.SceneManagement.Scene scene;　 //やまなりショット用の物理シーン
    private bool enemy_hit = false;
    [Foldout("ShotParameter", false)]

    private const float SHOT_POSITION = 3.5f;   // ショットを出す正面方向の位置補正
    private int shot_state;                 // debugでpublicにしてる
    private float charge_time;              // チャージ時間
    private float shot_interval_time;       // ショットの間隔
    private bool back_player;               // ショット3を撃った後にプレイヤーを後ろに飛ばす
    private float stop_time;                // 動けない時間
    private float init_back_speed;          // 初期速度保存用
    private float shot_charge_vol;          //ショットの大きさ加算
    private float shot_charge_speed;        //ショットのチャージスピード
    private const float CHARGE_LENGTH_OFFSET = 3.0f;
    private float shot_charge_length = CHARGE_LENGTH_OFFSET;       //やまりなショットの発射距離
    private float shot_length_charge_speed; //やまなりショットのチャージスピード



    //壁掴み判定Ray ---------------------------------------------
    [System.Serializable]
	public class WallGrabRay : RayBase{
		//length 2.0f

		[SerializeField, Range(0.0f, 2.0f), Header("Rayの高さ")]
		public float height;		//0.2f

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

	//壁掴み向き調整 --------------------------------------
	private struct WallGrabAdjust {
		public Vector2 forward;
		public Vector2 back;
		public Vector2 right;
		public Vector2 left;
		public Vector2 player_forward;

		public float angle_forward;
		public float angle_back;
		public float angle_right;
		public float angle_left;

		//存在しない大きな値
		public const int BIG_VALUE = 999;
	}
	private WallGrabAdjust wall_grab_adjust;



	//踏みつけ判定 ----------------------------------------------
	[System.Serializable]
	public class TreadOn_BoxCast : BoxCastBase {
		//flgはジャンプ中、着地するまでtrue

		//半径に掛ける,XZ軸の倍率
		public const float RADIUS_MAG_XZ = 1.5f;

		//Y軸の長さ
		public const float LENGTH_Y		 = 0.6f;

		//BoxCastを飛ばす最大の長さ
		public const float MAX_DISTANCE	 = 0.2f;

		public const int FOWARD_POWER	 = 13;

		public const int JUMP_POWER		 = 30;

	}
	[Header("踏みつけ判定")]
	public TreadOn_BoxCast tread_on;

	//気絶時のノックバック ----------------------------
	private struct KnockBack {
		public const float SPD_MAG       = 4.5f;
		public const float JUMP_POWER    = 20;
		public const int   TIME          = 30;
		public const int   FAINT_TIME    = 40 - TIME;  //硬直時間(-ノックバック)
	}

	enum Enum_Faint {
		CLEAR,      //初期化
		WAIT,       //待機
		WAIT2,      //待機
		END         //終了
	}
	Enum_Faint enum_faint;





	//汎用タイマーの種類
	enum Enum_Timer {
		RUN,			//走り
		TREAD_ON,		//踏みつけ
		RESPAWN,        //リスポーン
		WALL_GRAB,      //壁掴み
	}
	Enum_Timer enum_timer;




	//先行入力 --------------------------------------------------
	[System.Serializable]
	public struct LeadInput {
		public bool			on;						//先行入力オンオフ
		public const int	NUM = 3;				//保存するキー数
		public const int    KEY_SERVE_TIME = 6;		//保存時間
	}
	[Header("先行入力")]
	public LeadInput lead_input;

	//先行入力保存 ----------------------------------------
	private struct LeadInputServe
    {
		public LeadkeyKind pushed_key;	//押されたキー
		public int frame;				//キー保存時間
	};
	private LeadInputServe[] lead_inputs = new LeadInputServe[LeadInput.NUM];

	enum LeadkeyKind {
		NONE,
		JUMP,
	}
	private LeadkeyKind lead_key;   //保存されたキー




}

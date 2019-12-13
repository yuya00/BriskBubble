using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class Enemy : CharaBase
{
	private GameObject          player_obj;
	private EnemyNear           enemy_near;
	private EnemySoundDetect    enemy_sounddetect;

	private int					wait_timer_swing;
	private bool				player_touch_flg;
	private bool				shot_touch_flg;
	private Vector3				dist_to_player;
	private const int           FAINT_TIME	 = 180;     //気絶時間



	//状態エフェクト -----------------------------------
	[System.Serializable]
	public struct ConditionEffect {
		public  GameObject warning;
		public  GameObject find;
		public  GameObject away;
		public  GameObject attack;
		public  GameObject faint;
		[System.NonSerialized]
		public  GameObject obj_attach;

		[System.NonSerialized]
		public GameObject obj_entitya;
	}
	[Header("状態エフェクト")]
	public ConditionEffect condition_effect;


	//待機 ---------------------------------------------
	[System.Serializable]
	public struct WaitAct {
		[Header("首振り前待機")]
		public int wait_time;			//240
		[Header("首振り前待機ランダム幅")]
		public int wait_random;			//0
		[Header("首振り時間")]
		public int swing_time;			//70
		[Header("首振り時間ランダム幅")]
		public int swing_random;		//0
		[Header("首振り速さ")]
		public int swing_spd;			//30
		[Header("首振り間の間隔")]
		public int swing_space_time;    //15

		//首振りの行動種類
		public enum Enum_Swing {
			SWING,  //首振り
			WAIT    //待機
		}
		[System.NonSerialized]
		public Enum_Swing enum_swing;
	}
	[Header("待機行動")]
	public WaitAct wait_act;

	//首振りランダム値設定(待機行動) -------------------
	private struct OnceRondom {
		public int   num;
		public bool  isfinish;
	}
	private OnceRondom once_random;


	//警戒 ---------------------------------------------
	[System.Serializable]
	public struct WarningAct {
		/*
		[SerializeField, Header("首振り速さ")]
		public int swing_spd;           //100
		[SerializeField, Header("首振り時間")]
		public int swing_time;          //10
		[SerializeField, Header("首振り間の間隔")]
		public int swing_space_time;    //15
		// */
	}
	[Header("警戒行動")]
	public WarningAct warning_act;


	//逃走 ---------------------------------------------
	[System.Serializable]
	public struct AwayAct {
		//音探知範囲*mag分離れたら止まる
		public const float MAG = 2.0f;

		[Header("振り向く間隔")]
		public int lookback_interval;

		[Header("振り向いている時間")]
		public int lookback_time;

		//振り向きstate
		public enum Enum_LookBack {
			NORMAL,
			LOOKBACK
		}
		[System.NonSerialized]
		public Enum_LookBack enum_lookback;

		//逃走種類 -----------------------------
		[System.Serializable]
		public struct Kind {

			//インスペクター用
			public bool normal;
			public bool curve;
			public bool jump;

			//前の状態保存
			[System.NonSerialized]
			public bool normal_front;

			[System.NonSerialized]
			public bool curve_front;

			[System.NonSerialized]
			public bool jump_front;

		}
		[Header("種類")]
		public Kind kind;

		//逃走種類に応じた変数(カーブ) ---------
		[System.Serializable]
		public struct Curve {
			//ジャンプも入れるかも

			//符号反転用
			[System.NonSerialized]
			public float        one;

			[System.NonSerialized]
			public float        timer;

			[Header("向き切り替え時間")]	//120
			public int			normal_interval;

			[Header("曲がる速さ")]			//0.02f
			public float		normal_spd;

			[Header("向き切り替え時間")]	//120
			public int			curve_interval;

			[Header("曲がる速さ")]			//0.5f
			public float		curve_spd;

		}
		[Header("調整")]
		public Curve curve;

	}
	[Header("逃走行動")]
	public AwayAct away_act;


		
	//ジャンプ判定Ray ----------------------------------
	[System.Serializable]
	public class JumpRay : BoxCastAdjustBase {
		//public float length;            //4.3f
		//public float uplimit_height;    //2.0f
		//public float downlimit_height;  //3.9f

		[System.NonSerialized]			//壁との当たり判定
		public bool flg;

		[Header("ジャンプ力")]
		public float power;				//22.0f

		[Header("事前判定の長さ")]
		public float advance_length;    //23.0f

		[System.NonSerialized]			//壁との事前当たり判定
		public bool advance_flg;
	}
	[Header("ジャンプRay")]
	public JumpRay jump_ray;


	[Header("ショットへの耐久度"),SerializeField]
	private int shot_to_defense = 3;
	//当たったショットの強さ保存
	private int shot_scale_power;





	//敵モデルの種類
	enum EnumModel {
		STILL,  //幼体
		GROWN   //成体
	}
	EnumModel enum_model;


	//状態の種類
	enum Enum_State{
		WAIT,     //待機
		WARNING,  //警戒
		FIND,     //発見
		AWAY,     //逃走
		ATTACK,   //攻撃
		FAINT,    //気絶
		WRAP,     //捕獲
		END       //消去
	}
	Enum_State enum_state;
	Enum_State old_state;


	//状態内の行動種類
	enum Enum_Act {
		CLEAR,		//初期化
		WAIT,		//待機
		SWING,		//首振り
		SWING2,		//首振り2
		SWING3,		//首振り3
		RUN,		//走る
		JUMP,       //ジャンプ
		FAINT,      //気絶
		END         //終了
	}
	Enum_Act enum_act;
	Enum_Act old_act;



	//逃走ベースの種類
	enum Enum_AwayKind {
		NORMAL,
		CURVE,
		JUMP
	}
	Enum_AwayKind enum_awaykind;




	//※要修正
	//WaitTimeの最中でWaitTimeを使用したかったので
	//もう一つ同じ用途の関数を用意した

	//指定時間になったらtrue
	bool WaitTime_Swing(int wait_time) {
		if (wait_timer_swing >= wait_time) {
			wait_timer_swing = 0;
			return true;
		}
		else {
			wait_timer_swing++;
			return false;
		}
	}
	//指定時間になったらtrue
	bool WaitTime(int wait_time) {
		if (wait_timer >= wait_time) {
			wait_timer = 0;
			return true;
		}
		else {
			wait_timer++;
			return false;
		}
	}

}
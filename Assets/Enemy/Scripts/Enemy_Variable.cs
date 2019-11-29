using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class Enemy : CharaBase
{
	private int					wait_timer_swing;
	private bool				player_touch_flg;
	private bool				shot_touch_flg;
	private Vector3				dist_to_player;
	private float				curve_spd;
	private EnemyNear			enemy_near;
	private EnemySoundDetect	enemy_sounddetect;
	private GameObject			player_obj;

	private struct OnceRondom {
		public int   num;
		public bool  isfinish;
	}
	OnceRondom once_random;


	//待機 ---------------------------------------------
	[System.Serializable]
	public struct WaitAct {
		[SerializeField, Header("首振り前待機")]
		public int wait_time;			//240
		[SerializeField, Header("首振り前待機ランダム幅")]
		public int wait_random;			//1
		[SerializeField, Header("首振り時間")]
		public int swing_time;			//70
		[SerializeField, Header("首振り時間ランダム幅")]
		public int swing_random;		//1
		[SerializeField, Header("首振り速さ")]
		public int swing_spd;			//30
		[SerializeField, Header("首振り間の間隔")]
		public int swing_space_time;	//15
	}
	[Header("待機行動")]
	public WaitAct wait_act;


	//警戒 ---------------------------------------------
	[System.Serializable]
	public struct WarningAct {
		[SerializeField, Header("首振り速さ")]
		public int swing_spd;           //60
		[SerializeField, Header("首振り時間")]
		public int swing_time;          //80
		[SerializeField, Header("首振り間の間隔")]
		public int swing_space_time;    //15
	}
	[Header("警戒行動")]
	public WarningAct warning_act;


	//逃走 ---------------------------------------------
	[System.Serializable]
	public struct AwayAct {
		[SerializeField, Header("音探知範囲*mag分離れたら止まる")]
		public float mag;               //1.5f

		//[SerializeField, Header("逃走時のランダム±角度")]
		//public float angle;				//30

		[System.NonSerialized]
		public bool lookback_flg;  //振り向き判定

		[SerializeField, Header("振り向く間隔")]
		public int lookback_interval;	//120

		[SerializeField, Header("振り向いている時間")]
		public int lookback_time;		//60
	}
	[Header("逃走行動")]
	public AwayAct away_act;


	//段差ジャンプ -------------------------------------
	[System.Serializable]
	public class JumpRay : BoxCast_Base {
		//public float length;            //4.0f
		//public float uplimit_height;    //1.7f
		//public float downlimit_height;  //1.9f

		[System.NonSerialized]			//壁との当たり判定
		public bool flg;

		[SerializeField, Range(0.0f, 40.0f), Header("ジャンプ力")]
		public float power;				//16.0f

		[SerializeField,Header("事前判定の長さ")]
		public float advance_length;    //22.0f

		[System.NonSerialized]			//壁との事前当たり判定
		public bool advance_flg;
	}
	[Header("ジャンプRay")]
	public JumpRay jump_ray;




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
		JUMP,		//ジャンプ
		END			//終了
	}
	Enum_Act enum_act;
	Enum_Act old_act;


	//首振りの行動種類
	enum Enum_SwingAct {
		SWING,	//首振り
		WAIT	//待機
	}
	Enum_SwingAct enum_swingact;




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
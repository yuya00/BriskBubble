using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class Enemy : CharaBase
{
	private bool				jump_flg;
	private int					timer;				//汎用
	//private int					wait_timer;         //汎用待機タイマー
	private int					wait_timer_swing;	//汎用待機タイマー(首振り用)
	private bool				player_touch_flg;   //プレイヤーとの当たり判定
	private bool				shot_touch_flg;   //プレイヤーとの当たり判定
	private Vector3				delection_vec;		//プレイヤーと逆方向のベクトル
	private Player				p_player; 
	private float				spd_ratio = 1.8f;	//プレイヤー速度を割る割合
	private EnemyNear			enemynear;
	private Vector2				dist;				//プレイヤーと逆方向のベクトル
	private Vector2				dist_normal_vec;    //プレイヤーと逆方向のベクトル


	private EnemySoundDetect	enemy_sound_detect;
	private struct OnceRondom {
		public int	 num;
		public bool	 isfinish;
	}
	OnceRondom					once_random;
	private bool				clear_flg;			//行動初期化判定

	private Vector2 leftScrollPos = Vector2.zero;   //uGUIスクロールビュー用


	[Header("敵GUIの表示")]
	public bool gui_on;



	//Transform wall_ray;
	//Quaternion wall_ray;
	//GameObject wall_ray;

	Vector3 old_angle;
	Vector3 new_angle;
	Vector3 dist_angle;


	[Space]
	[SerializeField]
	private GameObject player;

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
	public WaitAct waitact;


	//警戒 ---------------------------------------------
	private int swing3_spd_add	 = 20;	//首振り3回目の追加値
	private int swing3_time_add	 = 230;	//首振り3回目の追加値
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
	public WarningAct warningact;



	//逃走 ---------------------------------------------
	private bool lookback_flg = false;	//振り向き判定
	[System.Serializable]
	public struct AwayAct {
		//逃走時,音探知範囲の1.5倍
		[SerializeField, Header("音探知範囲*mag分離れたら止まる")]
		public float mag;				//1.5f

		//[SerializeField, Header("逃走時のランダム±角度")]
		//public float angle;				//30

		[SerializeField, Header("振り向く間隔")]
		public int lookback_interval;	//120

		[SerializeField, Header("振り向いている時間")]
		public int lookback_time;		//60
	}
	[Header("逃走行動")]
	public AwayAct awayact;
















	//敵モデルの種類
	enum Enum_model {
		STILL,  //幼体
		GROWN   //成体
	}
	Enum_model enum_model;


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



	////音探知範囲
	//[System.Serializable]
	//public struct SoundArea {
	//	[Range(0, 50)]
	//	public float radius;    //音探知範囲の半径(25.0f)
	//}
	//[Header("音探知範囲")]
	//public SoundArea sound_area;


	//public float getRadius {
	//	get {
	//		return sound_area.radius;
	//	}
	//}



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
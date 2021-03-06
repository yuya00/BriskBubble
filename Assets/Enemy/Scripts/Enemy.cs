﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class Enemy : CharaBase
{
    public override void Start()
    {
        base.Start();
        Clear();

		//コンポーネント取得
		enemy_near			 = GetComponentInChildren<EnemyNear>();
		enemy_sounddetect	 = GetComponentInChildren<EnemySoundDetect>();
		player_obj			 = GameObject.Find("Player");
        //chara_ray = transform.Find("CharaRay");
        sound = GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>();

        //敵のパラメーター設定
        respawn_pos = transform.position;
		respawn_angle = transform.localEulerAngles;
		wall_ray.Clear();
		hole_ray.Clear();
		player_touch_flg = false;
		shot_touch_flg		 = false;
		dist_to_player		 = Vector3.zero;
		jump_ray.flg		 = false;
		jump_ray.advance_flg = false;
		if (this.gameObject.name == "Enemy1") {
			enum_model = EnumModel.STILL;
		}
		else if (this.gameObject.name == "Enemy2") {
			enum_model = EnumModel.GROWN;
		}
        enum_state			 = Enum_State.WAIT;
        old_state			 = enum_state;

		away_act.jump.flg = true;

		//敵別の行動セット
		AwayKindSet();

		//状態エフェクト設定
		CoditionEffect_Set();
	}

	//--敵別の行動セット
	void AwayKindSet() {
		//逃走の種類決定
		if (away_act.kind.normal) {
			enum_awaykind = Enum_AwayKind.NORMAL;
		}
		else if (away_act.kind.curve) {
			enum_awaykind = Enum_AwayKind.CURVE;
		}
		else if (away_act.kind.jump) {
			enum_awaykind = Enum_AwayKind.JUMP;
		}
		else if (away_act.kind.zigzag) {
			enum_awaykind = Enum_AwayKind.ZIGZAG;
		}
		else if (away_act.kind.armar) {
			enum_awaykind = Enum_AwayKind.ARMAR;
		}
		else if (away_act.kind.shot) {
			enum_awaykind = Enum_AwayKind.SHOT;
		}
		else if (away_act.kind.spin) {
			enum_awaykind = Enum_AwayKind.SPIN;
		}
		else {
			enum_awaykind = Enum_AwayKind.NORMAL;
		}
	}

	//--状態エフェクト設定
	void CoditionEffect_Set() {

		//状態Enumの最大数
		int enum_max = System.Enum.GetNames(typeof(Enum_State)).Length;

		//インスペクターの情報を配列に代入してまとめる
		condition_eff.obj_attach2 = new GameObject[enum_max];
		condition_eff.obj_attach2[(int)Enum_State.WARNING]	 = condition_eff.warning;
		condition_eff.obj_attach2[(int)Enum_State.FIND]		 = condition_eff.find;
		condition_eff.obj_attach2[(int)Enum_State.AWAY]		 = condition_eff.away;
		condition_eff.obj_attach2[(int)Enum_State.FAINT]	 = condition_eff.faint;

		//生成、子として設定、非アクティブ
		for (int i = 0; i < enum_max; i++) {
			if (condition_eff.obj_attach2[i] == null) {
				continue;
			}
			condition_eff.obj_attach2[i] = Instantiate(condition_eff.obj_attach2[i], transform.position + transform.up * 4, transform.rotation);
			condition_eff.obj_attach2[i].transform.parent = transform;
			condition_eff.obj_attach2[i].SetActive(false);
		}

	}



	void Update()
    {
        base.Move();

		StateChange();				//プレイヤーとの当たり判定でstate変更
		DistPlayer();				//プレイヤーとの距離
		Action();					//stateに応じて個別関数に飛ぶ
		CoditionEffect_Active();    //状態エフェクトを状況に応じてアクティブにする
		Damage();                   //ショットからのダメージ
		SpinIntervalTimer();        //SPIN間の間を開けるための処理
		FallRespawn();				//リスポーン処理

		old_state = enum_state;
		DebugLog();
	}

	//プレイヤーとの距離
	void DistPlayer() {
		dist_to_player = Vector3.Scale(
			player_obj.GetComponent<Player>().TransformPosition - transform.position,
			new Vector3(1.0f, 0.0f, 1.0f));
	}

	//状態エフェクトを状況に応じてアクティブにする
	void CoditionEffect_Active() {

		//stateが切り替わっていたら進む
		if ((enum_state == old_state)) {
			return;
		}

		//ひとつ前のstateを参照して、非アクティブにする
		if ((old_state != Enum_State.WAIT) &&
			(condition_eff.obj_attach2[(int)old_state] != null)) {
			condition_eff.obj_attach2[(int)old_state].SetActive(false);
		}
		//現在stateを参照して、アクティブにする
		if ((enum_state != Enum_State.WAIT) &&
			(condition_eff.obj_attach2[(int)enum_state] != null)) {
			condition_eff.obj_attach2[(int)enum_state].SetActive(true);
		}

	}

	//ショットからのダメージ
	void Damage() {
		if (!shot_touch_flg) {
			return;
		}
		//※40fほどshotに触れている時間がある

		//攻撃受けた時、一度だけダメージ0
		if ((enum_awaykind == Enum_AwayKind.ARMAR) && (shot_scale_power > 0)) {
			shot_scale_power = 0;
			enum_awaykind = Enum_AwayKind.NORMAL;
			return;
		}

		//ショットの大きさ分耐久度軽減
		shot_to_defense -= shot_scale_power;
		shot_scale_power = 0;


		//気絶時にショットに当たったら
		if (shot_touch_flg && enum_state == Enum_State.FAINT) {
			shot_to_defense = 0;
		}

		//まだ耐えていたらダメージアニメ
		if (shot_to_defense >= 0) {
			
		}
	}

	//SPIN間の間を開けるための処理
	void SpinIntervalTimer() {
		if (enum_awaykind != Enum_AwayKind.SPIN || spin_timer <= 0) {
			return;
		}
		spin_timer--;
	}

	//リスポーン処理
	void FallRespawn() {
		// 最大限落ちたら、リスポーン
		if (transform.position.y < FALL_Y_MAX) {
			transform.position = respawn_pos;
			transform.localEulerAngles = respawn_angle;
			velocity = Vector3.zero;
			enum_state = Enum_State.WAIT;
			Clear();
		}
	}


	//デバッグログ表示 -------------------------------------------
	public override void DebugLog()
    {
        /*
		base.Debug_Log();
		Debug.Log("touch:" + player_touch_flg);
		Debug.Log("enum:" + enum_state);
		Debug.Log("enum_act:" + enum_act);
		Debug.Log(velocity);
		Debug.Log(transform.localEulerAngles);

		Debug.DrawRay(
			transform.position,
			(transform.forward + transform.forward + transform.right).normalized * -30.0f);



		new_angle = transform.eulerAngles;
		dist_angle = new_angle - old_angle;

		//斜めのベクトルを出す方法 ※要修正
		Debug.DrawRay(transform.position, (new Vector3(30 * Mathf.Deg2Rad, 0, 30 * Mathf.Deg2Rad) + dist_angle) * -wall_ray.langth);

		old_angle = new_angle;
		// */
    }

	//GUI表示 -----------------------------------------------------
	private Vector2 gui_scroll_pos = Vector2.zero;   //uGUIスクロールビュー用
	private float scroll_height = 330;
	void OnGUI()
    {
		if (!gui.on) {
			return;
		}

		//スクロール高さを変更
		//(出来ればmaximize on playがonならに変更したい)
		if (gui.all_view) {
			scroll_height = 700;
		}
		else scroll_height = 330;

		GUILayout.BeginVertical("box", GUILayout.Width(190));
		gui_scroll_pos = GUILayout.BeginScrollView(gui_scroll_pos, GUILayout.Width(180), GUILayout.Height(scroll_height));
		GUILayout.Box("Enemy");
		float spdx,spdy,spdz;

		#region ここに追加
		#region 全値
		if (gui.all_view) {
			//座標
			float posx = Mathf.Round(transform.position.x * 100.0f) / 100.0f;
			float posy = Mathf.Round(transform.position.y * 100.0f) / 100.0f;
			float posz = Mathf.Round(transform.position.z * 100.0f) / 100.0f;
			GUILayout.TextArea("座標\n (" + posx.ToString() + ", " + posy.ToString() + ", " + posz.ToString() + ")");

			//速さ
			spdx = Mathf.Round(velocity.x * 100.0f) / 100.0f;
			spdy = Mathf.Round(velocity.y * 100.0f) / 100.0f;
			spdz = Mathf.Round(velocity.z * 100.0f) / 100.0f;
			GUILayout.TextArea("速さ\n (" + spdx.ToString() + ", " + spdy.ToString() + ", " + spdz.ToString() + ")");

			//汎用タイマー配列
			GUILayout.TextArea("汎用タイマー\n"
				+ wait_timer_box[0] / 10 + "   "
				+ wait_timer_box[1] / 10 + "   "
				+ wait_timer_box[2] / 10 + "   "
				+ wait_timer_box[3] / 10 + "   "
				+ wait_timer_box[4] / 10 + "   "
				+ wait_timer_box[5] / 10);

			//状態(待機や警戒など)
			GUILayout.TextArea("状態\n enum_state：" + enum_state.ToString());

			//状態内の行動(首振りやジャンプなど)
			GUILayout.TextArea("行動\n act：" + enum_act.ToString());

			//回転
			GUILayout.TextArea("回転\n " + transform.localEulerAngles.ToString());

			//着地判定
			GUILayout.TextArea("着地判定\n" + is_ground);

			//壁判定
			GUILayout.TextArea("壁判定左右\n" + wall_ray.hit_left_flg + "  " + wall_ray.hit_right_flg);
			GUILayout.TextArea("壁判定両方左右\n" + wall_ray.cavein_left_flg + "  " + wall_ray.cavein_right_flg);
			//GUILayout.TextArea("壁判定左めり込み距離\n" + wallray.dist_left);
			//GUILayout.TextArea("壁判定右めり込み距離\n" + wallray.dist_right);

			//穴判定
			GUILayout.TextArea("穴判定左右\n" + hole_ray.hit_left_flg + "  " + hole_ray.hit_right_flg);

			//ジャンプ事前判定
			GUILayout.TextArea("ジャンプ事前判定\n" + jump_ray.advance_flg);

			//ショットに当たった判定
			GUILayout.TextArea("ショットに当たった判定\n" + shot_touch_flg);

			//気絶判定
			GUILayout.TextArea("気絶判定\n" + is_faint);

			//HP
			GUILayout.TextArea("HP\n" + shot_to_defense);

			//視界にプレイヤーが入った判定
			GUILayout.TextArea("視界にP\n" + found_player_flg);

			//プレイヤーに接触した判定
			GUILayout.TextArea("Pと接触\n" + player_touch_flg);

		}
		#endregion
		#region 開発用
		else if (gui.debug_view) {
			//速さ
			spdx = Mathf.Round(velocity.x * 100.0f) / 100.0f;
			spdy = Mathf.Round(velocity.y * 100.0f) / 100.0f;
			spdz = Mathf.Round(velocity.z * 100.0f) / 100.0f;
			GUILayout.TextArea("速さ\n (" + spdx.ToString() + ", " + spdy.ToString() + ", " + spdz.ToString() + ")");

			//壁判定
			//GUILayout.TextArea("壁判定左右\n" + wall_ray.hit_left_flg + "  " + wall_ray.hit_right_flg);
			//GUILayout.TextArea("壁判定両方左右\n" + wall_ray.cavein_left_flg + "  " + wall_ray.cavein_right_flg);
			//GUILayout.TextArea("壁判定左めり込み距離\n" + wall_ray.dist_left);
			//GUILayout.TextArea("壁判定右めり込み距離\n" + wall_ray.dist_right);

			//穴判定
			GUILayout.TextArea("穴判定左右\n" + hole_ray.hit_left_flg + "  " + hole_ray.hit_right_flg);

			//ジャンプ事前判定
			GUILayout.TextArea("ジャンプ事前判定\n" + jump_ray.advance_flg);

			//気絶判定
			GUILayout.TextArea("気絶判定\n" + is_faint);

			//逃走種類
			GUILayout.TextArea("逃走種類\n" + enum_awaykind);

			//HP
			GUILayout.TextArea("HP\n" + shot_to_defense);

			//汎用タイマー配列
			GUILayout.TextArea("汎用タイマー\n"
				+ wait_timer_box[0] / 10 + "   "
				+ wait_timer_box[1] / 10 + "   "
				+ wait_timer_box[2] / 10 + "   "
				+ wait_timer_box[3] / 10 + "   "
				+ wait_timer_box[4] / 10 + "   "
				+ wait_timer_box[5] / 10);

			//視界にプレイヤーが入った判定
			GUILayout.TextArea("視界にP\n" + found_player_flg);

			//プレイヤーに接触した判定
			GUILayout.TextArea("Pと接触\n" + player_touch_flg);


		}
		#endregion
		#endregion

		GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    //Gizmo表示 --------------------------------------------------
    void OnDrawGizmos()
    {
		//逃走行動切り替え(ここに書くのは良くないかも)
		//AwaySwitch();

		#region ※GUIの判定
		//※GUIの処理(ランタイム以外でも判定したいのでここに記述)
		if (!gui.on) {
			gui.all_view = false;
			gui.debug_view = false;
		}
		#endregion


		#region 着地判定
		if (ground_cast.gizmo_on && ground_cast.capsule_collider) {
			Gizmos.color = Color.magenta - new Color(0, 0, 0, 0.6f);
			Gizmos.DrawWireSphere(ground_cast.pos - (transform.up * ground_cast.length), GroundCast.RADIUS);
		}
		#endregion


		#region 斜めベクトル
		//new_angle = transform.eulerAngles;
		//dist_angle = new_angle - old_angle;

		////斜めのベクトルを出す方法 ※要修正
		//Gizmos.DrawRay(transform.position, (new Vector3(30 * Mathf.Deg2Rad, 0, 30 * Mathf.Deg2Rad) + dist_angle) * -wall_ray.langth);

		//old_angle = new_angle;
		#endregion


		#region 壁判定Ray
		if (wall_ray.gizmo_on)
        {
			wall_ray.BoxCastCal(transform);
			Gizmos.color = Color.green - new Color(0, 0, 0, 0.2f);

			//右ray
			Gizmos.DrawRay(transform.position + transform.up * wall_ray.up_limit,
				(transform.forward * WallRay.ANGLE_MAG + (transform.right)).normalized * wall_ray.length);	//上
			Gizmos.DrawRay(transform.position - transform.up * wall_ray.down_limit,
				(transform.forward * WallRay.ANGLE_MAG + (transform.right)).normalized * wall_ray.length);   //下
			Gizmos.DrawRay(transform.position - transform.up * wall_ray.down_limit +
				(transform.forward * WallRay.ANGLE_MAG + (transform.right)).normalized * wall_ray.length,
				transform.up * wall_ray.box_total);   //奥

			//左ray
			Gizmos.DrawRay(transform.position + transform.up * wall_ray.up_limit, 
				(transform.forward * WallRay.ANGLE_MAG + (-transform.right)).normalized * wall_ray.length);	//上
			Gizmos.DrawRay(transform.position - transform.up * wall_ray.down_limit,
				(transform.forward * WallRay.ANGLE_MAG + (-transform.right)).normalized * wall_ray.length);  //下
			Gizmos.DrawRay(transform.position - transform.up * wall_ray.down_limit +
				(transform.forward * WallRay.ANGLE_MAG + (-transform.right)).normalized * wall_ray.length,
				transform.up * wall_ray.box_total);   //奥
		}
#endregion
		
		
		#region 穴判定Ray
		if (hole_ray.gizmo_on) {
			//hole_ray.BoxCast_Cal2(transform);
			Gizmos.color = Color.green - new Color(0, 0, 0, 0.0f);
			Gizmos.color = (Color.red + Color.yellow) / 2 + new Color(0, 0, 0, 0.3f);

			//if ((!Physics.Raycast((transform.position + (transform.forward * angle_mag + (transform.right)).normalized * (hole_ray.startLength)) + (transform.right * 2)

			//Gizmos.DrawRay((transform.position + (transform.forward * angle_mag + (transform.right)).normalized * (hole_ray.startLength)) + (transform.right * 1), -transform.up * hole_ray.length);
			//Gizmos.DrawRay((transform.position + (transform.forward * angle_mag + (transform.right)).normalized * (hole_ray.startLength)) - (transform.right * 1), -transform.up * hole_ray.length);
			//Gizmos.DrawRay((transform.position + (transform.forward * angle_mag + (-transform.right)).normalized * (hole_ray.startLength)) + (transform.right * 1), -transform.up * hole_ray.length);
			//Gizmos.DrawRay((transform.position + (transform.forward * angle_mag + (-transform.right)).normalized * (hole_ray.startLength)) - (transform.right * 1), -transform.up * hole_ray.length);

			//RayCast
			Gizmos.DrawRay(transform.position + (transform.forward * WallRay.ANGLE_MAG + transform.right).normalized * hole_ray.startLength, -transform.up * hole_ray.length);
			Gizmos.DrawRay(transform.position + (transform.forward * WallRay.ANGLE_MAG + (-transform.right)).normalized * hole_ray.startLength, -transform.up * hole_ray.length);

			//BoxCast(末端のみ)
			Gizmos.DrawWireCube(transform.position + (transform.forward * WallRay.ANGLE_MAG + transform.right).normalized * hole_ray.startLength - transform.up * hole_ray.length, Vector3.one);
			Gizmos.DrawWireCube(transform.position + (transform.forward * WallRay.ANGLE_MAG + (-transform.right)).normalized * hole_ray.startLength - transform.up * hole_ray.length, Vector3.one);

		}
		#endregion


		#region ジャンプ判定Ray
		if (jump_ray.gizmo_on) 
		{
			jump_ray.BoxCastCal(transform);

			Gizmos.color = Color.blue - new Color(0, 0, 0, 0.6f);
			Gizmos.DrawRay(transform.position + transform.up * jump_ray.up_limit, transform.forward * jump_ray.advance_length);   //上

			Gizmos.color = Color.cyan - new Color(0, 0, 0, 0.0f);
			Gizmos.DrawRay(transform.position - transform.up * jump_ray.down_limit, transform.forward * jump_ray.advance_length); //下
			Gizmos.DrawRay(transform.position - transform.up * jump_ray.down_limit + transform.forward * jump_ray.length, transform.up * jump_ray.box_total);  //縦前
			Gizmos.DrawRay(transform.position - transform.up * jump_ray.down_limit + transform.forward * jump_ray.advance_length, transform.up * jump_ray.box_total);  //縦奥
		}
		#endregion


		#region　崖ジャンプRay
		if (cliffjump_ray.gizmo_on) {
			Gizmos.color = Color.red - new Color(0, 0, 0, 0.0f);
			Gizmos.DrawRay(transform.position + transform.forward * cliffjump_ray.startLength, -transform.up * cliffjump_ray.length);
		}
		#endregion
	}




	//state変更 --------------------------------------------------
	void StateChange() {
		//近くにプレイヤーがいたら(待機の時)、警戒stateに移行
		if (enemy_near.HitFlg) {
			if (enum_state == Enum_State.WAIT) {
				enum_state = Enum_State.WARNING;
			}
		}
		//音範囲内で音があったら、警戒stateに移行
		if (enemy_sounddetect.SoundCatchFlg) {
			if (enum_state == Enum_State.WAIT) {
				enum_state = Enum_State.WARNING;
			}
		}
		//(待機か,警戒,逃走のRUNの時)、
		//プレイヤーに触れたら、視界にプレイヤーが入ったら
		//発見stateに移行
		if (enum_state == Enum_State.WAIT ||
			enum_state == Enum_State.WARNING ||
			(enum_state == Enum_State.AWAY && enum_act == Enum_Act.RUN)) {
			if (player_touch_flg || found_player_flg) {
				enum_state = Enum_State.FIND;
			}
		}

		//プレイヤーに踏まれたら、気絶stateに移行
		if (is_faint && (enum_state != Enum_State.FAINT)) {
			enum_state = Enum_State.FAINT;
		}

		//耐久度が0になったら、捕獲stateに移行
		if (shot_to_defense <= 0) {
			enum_state = Enum_State.WRAP;
		}

	}

	//stateに応じて個別関数に飛ぶ ---------------------------------
	void Action()
    {
        //他のstateに行くとき初期化
        if (old_state != enum_state)
        {
            Clear();
        }

        switch (enum_state)
        {
            case Enum_State.WAIT:    //待機 ---------------
                Wait();
                break;
            case Enum_State.WARNING: //警戒 ---------------
                Warning();
                break;
            case Enum_State.FIND:    //発見 ---------------
                Find();
                break;
			case Enum_State.AWAY:    //逃走 ---------------
                Away();
                break;
			case Enum_State.BREAK:   //ショット破壊 ---------------
				BreakShot();
				break;
			case Enum_State.FAINT:   //踏まれる(気絶) -------------
				TreadBy();
				break;
			case Enum_State.WRAP:    //捕獲 ---------------
                Wrap();
                break;
            case Enum_State.END:     //消去 ---------------
                End();
                break;
        }
    }

    //--個別行動関数 ----------------------------------------------
	#region 個別行動関数

    //初期化
    void Clear()
    {
		once_random.num			 = 0;
		once_random.isfinish	 = false;

		enum_act				 = Enum_Act.CLEAR;
		wait_act.enum_swing		 = WaitAct.Enum_Swing.SWING;

		away_act.curve.one		 = 1;

		for (int i = 0; i < WAIT_BOX_NUM; i++) {
			wait_timer_box[i] = 0;
		}
	}


	//待機(定期的に正面から20度程左右に首を振る)
	void Wait()
    {
		#region 一周したら,ランダム値設定
        if (old_act != enum_act)
        {
            once_random.isfinish = false;
        }
        old_act = enum_act;
		#endregion

		//前方向確認用
		//velocity = transform.forward * 10.0f;

        switch (enum_act)
        {
            case Enum_Act.CLEAR:
                Clear();
                enum_act = Enum_Act.WAIT;
                break;
            case Enum_Act.WAIT:     //待機
                once_random.num = OnceRandom(-wait_act.wait_random, wait_act.wait_random);  //ランダム値設定
				if (WaitTimeBox((int)Enum_Timer.WAIT, wait_act.wait_time + once_random.num)) {
					enum_act = Enum_Act.SWING;
				}
                break;
            case Enum_Act.SWING:    //+首振り半分
                once_random.num = OnceRandom(-wait_act.swing_random, wait_act.swing_random);
                Swing(wait_act.swing_spd, wait_act.swing_time / 2 + once_random.num, wait_act.swing_space_time, Enum_Act.SWING2);
                break;
            case Enum_Act.SWING2:    //--首振り
                once_random.num = OnceRandom(-wait_act.swing_random, wait_act.swing_random);
                Swing(-wait_act.swing_spd, wait_act.swing_time + once_random.num, wait_act.swing_space_time, Enum_Act.SWING3);
                break;
            case Enum_Act.SWING3:    //+首振り半分
                once_random.num = OnceRandom(-wait_act.swing_random, wait_act.swing_random);
                Swing(wait_act.swing_spd, wait_act.swing_time / 2 + once_random.num, 0, Enum_Act.WAIT);
                break;
        }

		//--反撃ショットに移行
		BreakShot_Shift();
	}

	//--首振り関数(首振る速さ、首振る時間、待機時間、次のstate)
	void Swing(int spd, int time, int wait_time, Enum_Act next_state) {
		switch (wait_act.enum_swing) {
			case WaitAct.Enum_Swing.SWING:    //首振り
				transform.Rotate(0, spd * Mathf.Deg2Rad, 0);
				if (WaitTimeBox((int)Enum_Timer.WAIT_SWING, time)) {
					wait_act.enum_swing = WaitAct.Enum_Swing.WAIT;
				}
				break;
			case WaitAct.Enum_Swing.WAIT:     //首振りの間
				if (WaitTimeBox((int)Enum_Timer.WAIT_SWING, wait_time)) {
					wait_act.enum_swing = WaitAct.Enum_Swing.SWING;
					enum_act = next_state;
				}
				break;

		}
	}

	//--一度だけランダム値設定
	int OnceRandom(int min, int max) {
		if (!once_random.isfinish) {
			once_random.num = Random.Range(min, max);
			once_random.isfinish = true;
			return once_random.num;
		}
		else {
			return once_random.num;
		}
	}


	//警戒(即座に対象の方向を向く)
	void Warning()
    {
		switch (enum_act) {
			case Enum_Act.CLEAR:
				//今の角度保存

				//近くにプレイヤー
				if (enemy_near.HitFlg) {
					enum_act = Enum_Act.WARNING1;
				}
				//音範囲内で音
				if (enemy_sounddetect.SoundCatchFlg) {
					enum_act = Enum_Act.WARNING2;
				}
				break;
			case Enum_Act.WARNING1: //プレイヤーの方向を向く
				transform.LookAt(transform.position + dist_to_player);
				enum_act = Enum_Act.WAIT;
				break;
			case Enum_Act.WARNING2:  //ショットの方向を向く
				Vector3 dist = Vector3.Scale(
					enemy_sounddetect.Hitpos - transform.position,
					new Vector3(1.0f, 0.0f, 1.0f));
				transform.LookAt(transform.position + dist);
				enum_act = Enum_Act.WAIT;
				break;
			case Enum_Act.WAIT:
				if (WaitTimeBox((int)Enum_Timer.WARNIG, 30)) {
					enemy_sounddetect.SoundCatchFlg = false;
					Clear();
					enum_state = Enum_State.WAIT;
				}
				break;
		}


		#region 見回す処理
		/*
		//見つけてないとき
		if (!finder_flg)
        {
            //ゆっくり体を回して音源、プレイヤーを探す
            switch (enum_act)
            {
                case Enum_Act.CLEAR:
                    enum_act = Enum_Act.SWING;
                    break;
                case Enum_Act.SWING:    //首振り(-45度)
                    Swing(warning_act.swing_spd, warning_act.swing_time, warning_act.swing_space_time, Enum_Act.SWING2);
                    break;
                case Enum_Act.SWING2:   //首振り(+45度)
                    Swing(-warning_act.swing_spd, warning_act.swing_time + warning_act.swing_time, warning_act.swing_space_time, Enum_Act.SWING3);
                    break;
                case Enum_Act.SWING3:   //首振り(-360度)
                    Swing((warning_act.swing_spd + swing3_spd_add), (warning_act.swing_time + swing3_time_add), warning_act.swing_space_time, Enum_Act.END);
                    break;
                case Enum_Act.END:
                    enum_act = Enum_Act.SWING;
                    break;
            }

            //プレイヤーが近接範囲を出ていたら
            if (!enemy_near.HitFlg)
            {
                if (enum_act == Enum_Act.END)
                {
                    Clear();
                    enum_state = Enum_State.WAIT; //待機
                }
            }

            if (WaitTime(60 * 10))
            {
                Clear();
                enum_state = Enum_State.WAIT; //待機
            }

        }
		// */
		#endregion

	}


	//発見(ジャンプして(回転して)逃走に移行)
	void Find()
    {
        switch (enum_act)
        {
            case Enum_Act.CLEAR:
				velocity = Vector3.zero;
				found_player_flg = false;
				jump_ray.advance_flg = false;
				enum_act = Enum_Act.JUMP;
                break;
            case Enum_Act.JUMP: //その場で小さくジャンプ
                transform.LookAt(transform.position + dist_to_player); //プレイヤーの方向を向く
                Jump(jump_power);
                enum_act = Enum_Act.WAIT;
				break;
            case Enum_Act.WAIT:	//着地まで待機(もしくは15f経ったら)
                if (is_ground || WaitTimeBox((int)Enum_Timer.EACH_ACT, 15))
                {
					if (enum_awaykind == Enum_AwayKind.SPIN && spin_timer <= 0) {
						enum_act = Enum_Act.SPIN;
					}
					else {
						enum_act = Enum_Act.END;
					}
					wait_timer_box[(int)Enum_Timer.EACH_ACT] = 0;
				}
                break;
			case Enum_Act.SPIN: //回転攻撃(1割:ゆっくり、8割:早い、1割:ゆっくり)
				transform.Rotate(0, 15, 0);
				if (WaitTimeBox((int)Enum_Timer.EACH_ACT, 15)) {
					spin_timer = SPIN_INTERVAL_TIME;
					enum_act = Enum_Act.END;
				}
				transform.localScale = Vector3.one * 1.4f;
				break;
            case Enum_Act.END:
				transform.localScale = Vector3.one * 1;
				Clear();
                enum_state = Enum_State.AWAY; //警戒stateに移行
                break;
        }
    }

	//--イージング
	float Easing(float t) {
		return 1 - (t - 1) * (t - 1) * (-2.70158f * (t - 1) - 1.70158f);
	}


    //逃走(プレイヤーから逆方向に逃げ、一定距離で止まる)
    void Away()
    {
        switch (enum_act)
        {
            case Enum_Act.CLEAR:

				#region ±指定角度内でランダムにベクトル変更(保留)
				/*
				//±指定角度内でベクトル変更(-10度ではなく350度になるので注意!)
				float rand_num = Random.Range(-away_act.angle, away_act.angle);
				Vector3 localAngle = transform.localEulerAngles;
				localAngle += new Vector3(0, rand_num, 0);
				transform.localEulerAngles = localAngle;

				//float rand_num = Random.Range(-away_act.angle, away_act.angle);
				//dist_normal_vec.x += Mathf.Sin(rand_num * Mathf.Deg2Rad);
				//dist_normal_vec.y += Mathf.Cos(rand_num * Mathf.Deg2Rad);
				// */
				#endregion

				//プレイヤーと逆ベクトルの方向を見る
				transform.LookAt(transform.position - dist_to_player);

				//--穴に向かわないよう1fで向き修正
				FlashRotate();

				//前方向の速さ代入
				velocity = transform.forward * run_speed;

				goto case Enum_Act.RUN;
				//break;
            case Enum_Act.RUN:     //走る
                enum_act = Enum_Act.RUN;

				//--逃走の方法
				AwayActApproach();


				//--ジャンプ判定によるジャンプ
				JumpRayJump_Judge();

				//--壁判定による向き変更
				WallRayRotate_Judge();

                //--穴判定による向き変更
                HoleRayRotateJudge();

				//--崖ジャンプ
				CliffJump();

				//--振り向きによるspd変更
				Lookback_SpdChange();

				//--反撃ショットに移行
				BreakShot_Shift();


				//二人の距離が、音探知範囲*AwayAct.MAGより離れたら
				if ((dist_to_player.magnitude >= enemy_sounddetect.Radius * AwayAct.MAG) && (is_ground))
                {
                    enum_act = Enum_Act.END;
				}
				break;
            case Enum_Act.END:
				//プレイヤーの方向を向く
				//transform.LookAt(transform.position - velocity);
				//進行方向と逆を見る
				transform.LookAt(transform.position - transform.forward);

				Clear();
				velocity = Vector3.zero;
				enum_state = Enum_State.WAIT;
                break;
        }
    }

	//--穴に向かわないよう1fで向き修正
	void FlashRotate() {
		for (int i = 0; i < 30; i++) {
			if (!hole_ray.hit_right_flg && !hole_ray.hit_left_flg) {
				break;
			}
			HoleRayRotateJudge(); //--穴判定による向き変更
		}
	}


	//逃走行動切り替え(エディタ側のOnDrawGizmosで実行)(未実行)
	void AwaySwitch() {
		//一つ選択したら他はfalseにする処理

		//前と比べてtrueに変更されていたら切り替え
		if (!away_act.kind.normal_front && away_act.kind.normal) {
			//enum_awaykind = Enum_AwayKind.NORMAL;
			away_act.kind.curve = false;
			away_act.kind.jump = false;
		}
		if (!away_act.kind.curve_front && away_act.kind.curve) {
			//enum_awaykind = Enum_AwayKind.CURVE;
			away_act.kind.normal = false;
			away_act.kind.jump = false;
		}
		if (!away_act.kind.jump_front && away_act.kind.jump) {
			//enum_awaykind = Enum_AwayKind.JUMP;
			away_act.kind.normal = false;
			away_act.kind.curve = false;
		}
		//Debug.Log("front :" + away_act.kind.normal_front);
		//Debug.Log("normal:" + away_act.kind.normal);
		//Debug.Log("enum_awaykind:" + enum_awaykind);


		//(全てoffならnormalにする)
		//if (!away_act.kind.normal && !away_act.kind.away_act.curve && !away_act.kind.jump) {
		//	enum_awaykind = Enum_AwayKind.NORMAL;
		//	away_act.kind.normal = true;
		//	away_act.kind.away_act.curve = false;
		//	away_act.kind.jump = false;
		//}

		//前の状態保存
		away_act.kind.normal_front	 = away_act.kind.normal;
		away_act.kind.curve_front	 = away_act.kind.curve;
		away_act.kind.jump_front	 = away_act.kind.jump;
	}

	//--逃走種類
	void AwayActApproach() {
		switch (enum_awaykind) {
			case Enum_AwayKind.NORMAL:
				//----逃走時の移動
				Away_Normal();
				break;
			case Enum_AwayKind.CURVE:
				//----逃走時のカーブ移動
				Away_Curve();
				break;
			case Enum_AwayKind.JUMP:
				//----逃走時のジャンプ移動
				Away_Jump();
				break;
			case Enum_AwayKind.ZIGZAG:
				//----逃走時のジグザグ移動
				Away_Zigzag();
				break;
			default:
				Away_Normal();
				break;
		}
	}

	//----逃走時の移動
	void Away_Normal() {
		AwayActBase(away_act.normal.interval, away_act.normal.spd);
	}

	//----逃走時のカーブ移動
	void Away_Curve() {
		AwayActBase(away_act.curve.interval, away_act.curve.spd);
	}

	//------逃走時
	void AwayActBase(int interval, float spd) {
		//一定時間経つ、もしくは穴判定があれば向き切り替え
		if (hole_ray.hit_right_flg || hole_ray.hit_left_flg ||
			WaitTimeBox((int)Enum_Timer.EACH_ACT, interval)) {
			away_act.curve.one *= -1;
		}

		//向き補正
		spd *= away_act.curve.one;

		//曲がりながら走る(壁、穴判定がない時)
		if (!wall_ray.hit_right_flg && !wall_ray.hit_left_flg &&
			!hole_ray.hit_right_flg && !hole_ray.hit_left_flg) {
			transform.Rotate(0, spd, 0);
		}

	}

	//----逃走時のジャンプ移動
	void Away_Jump() {
		
		if (is_ground && away_act.jump.flg) {
			Jump(away_act.jump.power);
			away_act.jump.flg = false;
		}

		if (WaitTimeBox((int)Enum_Timer.EACH_ACT, away_act.jump.time)) {
			away_act.jump.flg = true;
		}

	}

	//----逃走時のジグザグ移動
	void Away_Zigzag() {
		//Debug.Log("ジグザグ移動");
		

	}


	//--ジャンプ判定によるジャンプ
	void JumpRayJump_Judge() {
		if (!jump_ray.judge_on) {
			return;
		}
		//----ジャンプ事前判定Ray
		JumpAdvanceRay();

		//----ジャンプ判定Ray
		JumpRay_Judge();

		//----ジャンプ
		JumpRay_Jump();
	}

	//----ジャンプ事前判定Ray
	void JumpAdvanceRay() {
		//RaycastHit hit;
		jump_ray.BoxCastCal(transform);

		#region BoxCast
		/*
		//Box:true ジャンプ上限Ray:false
		if (Physics.BoxCast(jump_ray.box_pos, new Vector3(0, jump_ray.box_total / 2, jump_ray.advance_length / 2),
			transform.forward, out hit,
			transform.rotation, jump_ray.advance_length / 2)) 
			{
			if (hit.collider.gameObject.tag == "Wall") {
				if (!Physics.Raycast(transform.position + transform.up * jump_ray.up_limit,
					transform.forward, jump_ray.advance_length)) 
					{
					jump_ray.advance_flg = true;
				}
				//else jump_ray.advance_flg = false;
			}
		}
		// */
		#endregion

		#region RayCast_Three
		//右のレイ(上,下,真ん中)
		if (JumpRay_Base(jump_ray.up_limit - 0.2f, 1, jump_ray.advance_length)) {
			if (JumpAdvanceRay_Up()) {
				jump_ray.advance_flg = true;
			}
		}
		else if (JumpRay_Base(wall_ray.down_limit, -1, jump_ray.advance_length)) {
			if (JumpAdvanceRay_Up()) {
				jump_ray.advance_flg = true;
			}
		}
		else if (JumpRay_Base(0, 0, jump_ray.advance_length)) {
			if (JumpAdvanceRay_Up()) {
				jump_ray.advance_flg = true;
			}
		}
		else {
			//jump_ray.advance_flg = false;
		}
		#endregion

	}

	//------ジャンプ出来ない判定(上側)
	bool JumpAdvanceRay_Up() {
		if (!Physics.Raycast(transform.position + transform.up * jump_ray.up_limit,
			transform.forward, jump_ray.advance_length)) {
			return true;
		}
		return false;
	}

	//----ジャンプ判定Ray
	void JumpRay_Judge() {
		//RaycastHit hit;

		#region RayCast
		if (jump_ray.advance_flg) {
			if (JumpRay_Base(wall_ray.up_limit, 1, jump_ray.length)) {
				jump_ray.flg = true;
			}
			else if (JumpRay_Base(wall_ray.down_limit, -1, jump_ray.length)) {
				jump_ray.flg = true;
			}
			else if (JumpRay_Base(0, 0, jump_ray.length)) {
				jump_ray.flg = true;
			}
		}
		#endregion

		#region BoxCast
		/*
		if (jump_ray.advance_flg) {
			if (Physics.BoxCast(jump_ray.box_pos,jump_ray.box_size,
				transform.forward, out hit,
				transform.rotation, jump_ray.length / 2)) 
				{
				if (hit.collider.gameObject.tag == "Wall") {
					jump_ray.flg = true;
				}
			}
		}
		// */
		#endregion

		#region BoxCast(事前判定無しver)
		/*
		//Box:true ジャンプ上限Ray:false
		if (Physics.BoxCast(jump_ray.pos,new Vector3(0, jump_ray.total / 2, jump_ray.length / 2),
			transform.forward,out hit,
			Quaternion.identity,jump_ray.length / 2)) 
			{
			if (hit.collider.gameObject.tag == "Wall") 
				{
				if (!Physics.Raycast(transform.position + new Vector3(0, jump_ray.up_limit, 0),
					transform.forward, out hit, jump_ray.length)) 
					{
					jump_ray.flg = true;
				}
			}
		}
		// */
		#endregion

	}

	//----ジャンプ
	void JumpRay_Jump() {
		if (jump_ray.flg) {
			Jump(jump_ray.power);
		}
	}

	//------レイ判定
	bool JumpRay_Base(float limit, int limit_one, float length) {
		RaycastHit hit;

		if (Physics.Raycast(jump_ray.box_pos + (transform.up * limit * limit_one),
			transform.forward, out hit, length)) {
			if (hit.collider.gameObject.tag == "Wall") {
				return true;
			}
		}
		return false;
	}


	//--壁判定による向き変更
	void WallRayRotate_Judge() {
		if (!wall_ray.judge_on) {
			return;
		}
		//ジャンプの準備してたら飛ばす
		if (jump_ray.advance_flg || (velocity.y != 0)) {
			return;
		}

		//----壁判定Ray当たり判定
		WallRayJudge();

		//----めり込み判定
		WallRayCavein();

		//----向き変更
		WallRayRotate();
	}

	//----壁判定Ray当たり判定
	void WallRayJudge() {
		//RaycastHit hit;
		wall_ray.BoxCastCal(transform);

		#region BoxCast
		/*
		//右のレイ
		if (Physics.BoxCast(wall_ray.box_pos, wall_ray.box_size,
				(transform.forward * angle_mag + transform.right).normalized, out hit,
				transform.rotation, wall_ray.length / 2)) 
			{
			if (hit.collider.gameObject.tag == "Wall") {
				wall_ray.dist_right = hit.distance;  //壁との距離保存
				wall_ray.hit_right_flg = true;       //壁との当たり判定
			}
		}
		else {
			wall_ray.dist_right = 0;
			wall_ray.hit_right_flg = false;
		}

		//左のレイ
		if (Physics.BoxCast(wall_ray.box_pos, wall_ray.box_size,
			(transform.forward * angle_mag + (-transform.right)).normalized, out hit,
			transform.rotation, wall_ray.length / 2)) 
			{
			if (hit.collider.gameObject.tag == "Wall") {
				wall_ray.dist_left = hit.distance;  //壁との距離保存
				wall_ray.hit_left_flg = true;       //壁との当たり判定
			}
		}
		else {
			wall_ray.dist_left = 0;
			wall_ray.hit_left_flg = false;
		}
		// */
		#endregion

		#region RayCast_Three
		//*
		//右のレイ(上,下,真ん中)
		if (WallRayRight(wall_ray.up_limit, 1, 1)) {
			wall_ray.hit_right_flg = true;
		}
		else if (WallRayRight(wall_ray.down_limit, -1, 1)) {
			wall_ray.hit_right_flg = true;
		}
		else if (WallRayRight(0, 0, 1)) {
			wall_ray.hit_right_flg = true;
		}
		else {
			wall_ray.dist_right = 0;
			wall_ray.hit_right_flg = false;
		}

		//左のレイ(上,下,真ん中)
		if (WallRayLeft(wall_ray.up_limit, 1, -1)) {
			wall_ray.hit_left_flg = true;
		}
		else if (WallRayLeft(wall_ray.down_limit, -1, -1)) {
			wall_ray.hit_left_flg = true;
		}
		else if (WallRayLeft(0, 0, -1)) {
			wall_ray.hit_left_flg = true;
		}
		else {
			wall_ray.dist_left = 0;
			wall_ray.hit_left_flg = false;
		}
		// */
		#endregion

		#region RayCast
		/*
		//右のレイ
		if (Physics.Raycast(transform.position,
			(transform.forward * angle_mag + transform.right).normalized, out hit, wall_ray.length)) {
			if (hit.collider.gameObject.tag == "Wall") {
				wall_ray.dist_right = hit.distance;  //壁との距離保存
				wall_ray.hit_right_flg = true;       //壁との当たり判定
			}
		}
		else {
			wall_ray.dist_right = 0;
			wall_ray.hit_right_flg = false;
		}

		//左のレイ
		if (Physics.Raycast(transform.position,
			(transform.forward * angle_mag + (-transform.right)).normalized, out hit, wall_ray.length)) {
			if (hit.collider.gameObject.tag == "Wall") {
				wall_ray.dist_left = hit.distance;   //壁との距離保存
				wall_ray.hit_left_flg = true;        //壁との当たり判定
			}
		}
		else {
			wall_ray.dist_left = 0;
			wall_ray.hit_left_flg = false;
		}
		// */
		#endregion
	}

	//------右レイ
	bool WallRayRight(float limit, int limit_one, int right_one) {
		RaycastHit hit;

		if (Physics.Raycast(wall_ray.box_pos + (transform.up * limit * limit_one),
			(transform.forward * WallRay.ANGLE_MAG + (transform.right * right_one)).normalized, out hit, wall_ray.length)) {
			if (hit.collider.gameObject.tag == "Wall") {
				wall_ray.dist_right = hit.distance;  //壁との距離保存
				return true;
			}
		}
		return false;
	}

	//------左レイ
	bool WallRayLeft(float limit, int limit_one, int right_one) {
		RaycastHit hit;

		if (Physics.Raycast(wall_ray.box_pos + (transform.up * limit * limit_one),
			(transform.forward * WallRay.ANGLE_MAG + (transform.right * right_one)).normalized, out hit, wall_ray.length)) {
			if (hit.collider.gameObject.tag == "Wall") {
				wall_ray.dist_left = hit.distance;  //壁との距離保存
				return true;
			}
		}
		return false;
	}

	//----めり込み判定
	void WallRayCavein() {
		//両方めり込んでいた場合、めり込みが少ないほうに曲がる
		if (wall_ray.hit_right_flg && wall_ray.hit_left_flg) {
			//めり込み少ない方を見る
			if (wall_ray.dist_right <= wall_ray.dist_left) {
				wall_ray.cavein_left_flg = true;
			}
			else {
				wall_ray.cavein_right_flg = true;
			}

			#region ATACK判定への処理
			/*
			if (wall_dist1 <= wall_dist2) {
				wall_rotate_flg2 = false;
			}
			else {
				wall_rotate_flg1 = false;
			}

            //どこで一回と区切るか
            //↓Clearで初期化
            if (!wall_ray.both_flg)
            {
                wall_ray.both_count++;
            }
            wall_ray.both_flg = true;

            //指定回数にめり込んだら、ATTACKに移行
            if (wall_ray.both_count >= 4)
            {
                //プレイヤーの方を向く
                transform.LookAt(transform.position + new Vector3(dist.x, 0, dist.y));
                enum_state = Enum_State.ATTACK;
                Clear();
            }
			// */
			#endregion
		}
		else {
			wall_ray.both_flg = false;
		}

		//上で決めた方向に曲がる
		if (wall_ray.cavein_left_flg) {
			wall_ray.hit_left_flg = true;
			wall_ray.hit_right_flg = false;
			if (wall_ray.dist_left == 0) {
				wall_ray.cavein_left_flg = false;
			}
		}
		else if (wall_ray.cavein_right_flg) {
			wall_ray.hit_right_flg = true;
			wall_ray.hit_left_flg = false;
			if (wall_ray.dist_right == 0) {
				wall_ray.cavein_right_flg = false;
			}
		}

	}

	//----向き変更
	void WallRayRotate() {
		if (wall_ray.hit_right_flg) {
			transform.Rotate(0.0f, -wall_ray.spd, 0.0f);
		}
		else if (wall_ray.hit_left_flg) {
			transform.Rotate(0.0f, wall_ray.spd, 0.0f);
		}
	}


	//--穴判定による向き変更
	public void HoleRayRotateJudge() {
		if (!hole_ray.judge_on) {
			return;
		}
		//----穴判定Ray当たり判定
		HoleRayJudge();

		//----向き変更
		HoleRayRotate();
	}

	//----穴判定Ray当たり判定
	public void HoleRayJudge() {
		//RaycastHit hit;
		LayerMask wall_layer = (1 << 14);

		//何にも当たっていなかったら
		#region BoxCast
		//右のレイ
		if (!Physics.BoxCast((transform.position + (transform.forward * WallRay.ANGLE_MAG + (transform.right)).normalized * (hole_ray.startLength)),
			Vector3.one, -transform.up, transform.rotation, hole_ray.length, wall_layer)) {
			hole_ray.hit_right_flg = true;
		}
		else {
			hole_ray.hit_right_flg = false;
		}

		//左のレイ
		if (!Physics.BoxCast((transform.position + (transform.forward * WallRay.ANGLE_MAG + (-transform.right)).normalized * (hole_ray.startLength)),
			Vector3.one, -transform.up, transform.rotation, hole_ray.length, wall_layer)) {
			hole_ray.hit_left_flg = true;
		}
		else {
			hole_ray.hit_left_flg = false;
		}
		#endregion

		#region 4RayCast 縦横
		/*
		//右のレイ
		float var = 1;
		if ((!Physics.Raycast((transform.position + (transform.forward * WallRay.ANGLE_MAG + (transform.right)).normalized * (hole_ray.startLength)) + (transform.right * var),
			-transform.up, out hit, hole_ray.length)) &&
			(!Physics.Raycast((transform.position + (transform.forward * WallRay.ANGLE_MAG + (transform.right)).normalized * (hole_ray.startLength)) - (transform.right * var),
			-transform.up, out hit, hole_ray.length)) &&
			(!Physics.Raycast((transform.position + (transform.forward * WallRay.ANGLE_MAG + (transform.right)).normalized * (hole_ray.startLength + var)),
			-transform.up, out hit, hole_ray.length)) &&
			(!Physics.Raycast((transform.position + (transform.forward * WallRay.ANGLE_MAG + (transform.right)).normalized * (hole_ray.startLength - var)),
			-transform.up, out hit, hole_ray.length)) ) {
			hole_ray.hit_right_flg = true;
		}
		else {
			hole_ray.hit_right_flg = false;
		}

		//左のレイ
		if ((!Physics.Raycast((transform.position + (transform.forward * WallRay.ANGLE_MAG + (-transform.right)).normalized * (hole_ray.startLength)) + (transform.right * var),
			-transform.up, out hit, hole_ray.length)) &&
			(!Physics.Raycast((transform.position + (transform.forward * WallRay.ANGLE_MAG + (-transform.right)).normalized * (hole_ray.startLength)) - (transform.right * var),
			-transform.up, out hit, hole_ray.length)) &&
			(!Physics.Raycast((transform.position + (transform.forward * WallRay.ANGLE_MAG + (-transform.right)).normalized * (hole_ray.startLength + var)),
			-transform.up, out hit, hole_ray.length)) &&
			(!Physics.Raycast((transform.position + (transform.forward * WallRay.ANGLE_MAG + (-transform.right)).normalized * (hole_ray.startLength - var)),
			-transform.up, out hit, hole_ray.length))) {
			hole_ray.hit_left_flg = true;
		}
		else {
			hole_ray.hit_left_flg = false;
		}
		// */
		#endregion

		#region 2RayCast 横
		/*
		//右のレイ
		float var = 1;
		if ((!Physics.Raycast((transform.position + (transform.forward * angle_mag + (transform.right)).normalized * (hole_ray.startLength)) + (transform.right*var),
			-transform.up, out hit, hole_ray.length)) &&
			(!Physics.Raycast((transform.position + (transform.forward * angle_mag + (transform.right)).normalized * (hole_ray.startLength)) - (transform.right* var),
			-transform.up, out hit, hole_ray.length))) {
			hole_ray.hit_right_flg = true;
		}
		else {
			hole_ray.hit_right_flg = false;
		}

		//左のレイ
		if ((!Physics.Raycast((transform.position + (transform.forward * angle_mag + (-transform.right)).normalized * (hole_ray.startLength)) + (transform.right* var),
			-transform.up, out hit, hole_ray.length)) &&
			(!Physics.Raycast((transform.position + (transform.forward * angle_mag + (-transform.right)).normalized * (hole_ray.startLength)) - (transform.right* var),
			-transform.up, out hit, hole_ray.length))) {
			hole_ray.hit_left_flg = true;
		}
		else {
			hole_ray.hit_left_flg = false;
		}
		// */
		#endregion

		#region 2RayCast 縦
		/*
		//右のレイ
		if ((!Physics.Raycast(transform.position + (transform.forward * angle_mag + (transform.right)).normalized * (hole_ray.startLength + 1),
			-transform.up, out hit, hole_ray.length)) &&
			(!Physics.Raycast(transform.position + (transform.forward * angle_mag + (transform.right)).normalized * (hole_ray.startLength - 1),
			-transform.up, out hit, hole_ray.length))) {
			hole_ray.hit_right_flg = true;
		}
		else {
			hole_ray.hit_right_flg = false;
		}

		//左のレイ
		if ((!Physics.Raycast(transform.position + (transform.forward * angle_mag + (-transform.right)).normalized * (hole_ray.startLength + 1),
			-transform.up, out hit, hole_ray.length)) &&
			(!Physics.Raycast(transform.position + (transform.forward * angle_mag + (-transform.right)).normalized * (hole_ray.startLength - 1),
			-transform.up, out hit, hole_ray.length))) {
			hole_ray.hit_left_flg = true;
		}
		else {
			hole_ray.hit_left_flg = false;
		}
		// */
		#endregion

		#region RayCast
		/*
		//右のレイ
		if (!Physics.Raycast(transform.position + (transform.forward * angle_mag + transform.right).normalized * wall_ray.length,
			-transform.up, out hit, hole_ray.length)) {
			hole_ray.hit_right_flg = true;
		}
		else {
			hole_ray.hit_right_flg = false;
		}

		//左のレイ
		if (!Physics.Raycast(transform.position + (transform.forward * angle_mag + (-transform.right)).normalized * wall_ray.length,
			-transform.up, out hit, hole_ray.length)) {
			hole_ray.hit_left_flg = true;
		}
		else {
			hole_ray.hit_left_flg = false;
		}
		// */
		#endregion

	}

	//----向き変更
	public void HoleRayRotate() {
		if (hole_ray.hit_right_flg) {
			transform.Rotate(0.0f, -hole_ray.speed, 0.0f);
		}
		else if (hole_ray.hit_left_flg) {
			transform.Rotate(0.0f, hole_ray.speed, 0.0f);
		}
	}



	//--崖ジャンプ
	void CliffJump() {
		if (!cliffjump_ray.judge_on) {
			return;
		}
		RaycastHit hit;
		if (is_ground && !Physics.Raycast(transform.position + transform.forward * cliffjump_ray.startLength, -transform.up, out hit, cliffjump_ray.length)){
			Jump(cliffjump_ray.power);
		}

	}


	//--振り向きによるspd変更
	void Lookback_SpdChange() {
		switch (away_act.enum_lookback) {
			case AwayAct.Enum_LookBack.NORMAL:
				//spd(通常)
				velocity.x = transform.forward.x * run_speed;
				velocity.z = transform.forward.z * run_speed;
				//120f経ったら
				if (WaitTimeBox((int)Enum_Timer.LOOKBACK, away_act.lookback_interval)) {
					//away_act.state = 1;
					away_act.enum_lookback = AwayAct.Enum_LookBack.LOOKBACK;
				}
				break;
			case AwayAct.Enum_LookBack.LOOKBACK:
				//spd(振り向き)
				velocity.x = transform.forward.x * (run_speed / 2);
				velocity.z = transform.forward.z * (run_speed / 2);
				//30f経ったら
				if (WaitTimeBox((int)Enum_Timer.LOOKBACK, away_act.lookback_time)) {
					//away_act.state = 0;
					away_act.enum_lookback = AwayAct.Enum_LookBack.NORMAL;
				}
				break;
		}

		//例外
		//穴判定で曲がっている時、spd(振り向き)
		if (hole_ray.hit_right_flg || hole_ray.hit_left_flg) {
			velocity.x = transform.forward.x * (run_speed / 2);
			velocity.z = transform.forward.z * (run_speed / 2);
		}
		//ジャンプしてる時、spd(通常)
		if (!is_ground) {
			velocity.x = transform.forward.x * run_speed;
			velocity.z = transform.forward.z * run_speed;
		}

	}


	//--反撃ショットに移行
	void BreakShot_Shift() {
		//音範囲にシャボンがあれば、ショットで破壊
		if (is_ground && enum_awaykind == Enum_AwayKind.SHOT &&
			enemy_sounddetect.FoundShotFlg) {
			//元のstateに戻りたいので、enum_stateを保存
			breakshot_act.serve_state = (int)enum_state;
			enum_state = Enum_State.BREAK;
			Clear();
		}
	}

	//反撃ショット(シャボンを破壊)
	void BreakShot() {
		//反撃敵で視界にシャボンがあれば、ショットで破壊
		switch (enum_act) {
			case Enum_Act.CLEAR:    //待機
				velocity.x = 0;
				velocity.z = 0;
				enum_act = Enum_Act.WAIT;
				break;
			case Enum_Act.WAIT:
				//ショットの方を向く(ifでうつむきすぎないよう調整)
				transform.LookAt(enemy_sounddetect.FoundHitPos);
				if (transform.localEulerAngles.x >= 12.0f && transform.localEulerAngles.x <= 90.0f) {
					transform.localEulerAngles = new Vector3(12.0f, transform.localEulerAngles.y, transform.localEulerAngles.z);
				}
				if (transform.localEulerAngles.x <= 320.0f && transform.localEulerAngles.x >= 270.0f) {
					transform.localEulerAngles = new Vector3(320.0f, transform.localEulerAngles.y, transform.localEulerAngles.z);
				}
				//待機
				if (WaitTimeBox((int)Enum_Timer.EACH_ACT, breakshot_act.front_time)) {
					enemy_sounddetect.FoundShotFlg = false;
					enum_act = Enum_Act.BREAK;
				}
				break;
			case Enum_Act.BREAK:    //ショット生成
				Instantiate(breakshot_act.obj, transform.position + (transform.forward * BreakShotAct.MAG), transform.rotation);
				breakshot_act.flg = true;
                sound.SoundSE(ENEMY_SE, SHOT_SE);
				enum_act = Enum_Act.END;
				break;
			case Enum_Act.END:      //待機(逃走のRUNに戻る)
				breakshot_act.flg = false;
				if (WaitTimeBox((int)Enum_Timer.EACH_ACT, breakshot_act.back_time)) {
					Clear();
					transform.localEulerAngles = Vector3.zero;
					//元のstateに戻る
					if (breakshot_act.serve_state == (int)Enum_State.WAIT) {
						enum_state = Enum_State.WAIT;
						enum_act = Enum_Act.CLEAR;
					}
					else if(breakshot_act.serve_state == (int)Enum_State.AWAY) {
						enum_state = Enum_State.AWAY;
						enum_act = Enum_Act.RUN;
					}
				}
				break;
		}
	}



	//踏まれる判定(気絶)
	void TreadBy() {
		//踏まれた際にプレイヤー側で変更
		//IsFaint = true;
		switch (enum_act) {
			case Enum_Act.CLEAR:
				//Debug.Log(this.name + " がプレイヤーに踏まれた");
				//大きさを元に戻す
				transform.localScale = Vector3.one * 1;
				//斜め方向
				faint_giddy.upward = transform.position + transform.up * 1 + transform.forward * 5;
				//前方向を保存しておく
				faint_giddy.forward = transform.forward * 5;

				enum_act = Enum_Act.FAINT;
				break;
			case Enum_Act.FAINT: //気絶
				//移動停止
				velocity.x = 0;
				velocity.z = 0;

				//目が回る
				transform.LookAt(faint_giddy.upward + 
					new Vector3(FaintGiddy.RADIUS_SIN * Mathf.Sin(Time.time * FaintGiddy.ROTATE_SPD), 0, 
								FaintGiddy.RADIUS_COS * Mathf.Cos(Time.time * FaintGiddy.ROTATE_SPD) ));

				//時間経ったら
				if (WaitTimeBox((int)Enum_Timer.FAINT, FAINT_TIME)) {
					enum_act = Enum_Act.END;
				}
				break;
			case Enum_Act.END:
				Clear();
				is_faint = false;
				//向きリセット
				transform.localEulerAngles = Vector3.zero;
				enum_state = Enum_State.WAIT;
				break;
		}
	}


	//捕獲
	void Wrap()
    {
		if (Mathf.Abs(velocity.z) > 0.0f) {
			velocity.z = velocity.z / 3;
		}
	}


	//消去
	void End()
    {

	}


	//ジャンプ
	void Jump(float power)
    {
        velocity.y			 = power;
		jump_ray.flg		 = false;
		jump_ray.advance_flg = false;
		rigid.useGravity	 = false;
        is_ground			 = false;
        player_touch_flg	 = false;
    }


	#endregion





	//当たり判定 ---------------------------------------------------
	private void OnCollisionEnter(Collision other)
    {
        //何かに当たったとき
        if (other.gameObject.tag == "Player")
        {
            if (!player_touch_flg)
            {
                player_touch_flg = true;
            }
        }
    }

    private void OnCollisionExit(Collision other)
    {
        //何にも当たっていないとき
        if (other.gameObject.tag == "Player")
        {
            if (player_touch_flg)
            {
                player_touch_flg = false;
            }
        }
    }

	//要修正
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Shot")
        {
            if (!shot_touch_flg)
            {
                shot_touch_flg = true;
				//ショットの大きさ(強さ)を保存
				if (other.gameObject.name == "Shot01(Clone)") {
					shot_scale_power = (int)other.gameObject.GetComponent<Shot01>().transform.localScale.x;
					//Debug.Log((int)other.gameObject.GetComponent<Shot01>().transform.localScale.x);
					//Debug.Log(other.gameObject.GetComponent<Shot01>().transform.localScale.x);
				}
				if (other.gameObject.name == "Shot02(Clone)") {
					shot_scale_power = (int)other.gameObject.GetComponent<Shot02>().transform.localScale.x;
				}
				if (other.gameObject.name == "Shot03(Clone)") {
					shot_scale_power = (int)other.gameObject.GetComponent<Shot03>().transform.localScale.x;
				}

			}
		}
    }

    private void OnTriggerExit(Collider other)
    {

	}


	//get,set ------------------------------------------------
	public bool ShotTouchFlg {
		get { return shot_touch_flg; }
		set { shot_touch_flg = value; }
	}

	public float ShotToDefense {
		get { return shot_to_defense; }
	}

	public Vector3 TransformPosition
    {
        get { return transform.position; }
    }

	public bool IsFaint {
		get { return is_faint; }
		set { is_faint = value; }
	}

	public bool IsWrap {
		get { return enum_state == Enum_State.WRAP; }
	}

	public Enum_Act EnumAct {
		get { return enum_act; }
	}


}

using System.Collections;
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

		//敵のパラメーター設定
		player_touch_flg	 = false;
		shot_touch_flg		 = false;
		dist_to_player		 = Vector3.zero;
		curve_spd			 = 0;
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

	}

	void Update()
    {
        base.Move();

		StateChange();      //プレイヤーとの当たり判定でstate変更
		DistPlayer();		//プレイヤーとの距離
		Action();           //stateに応じて個別関数に飛ぶ
		CondtionEffect();   //状態エフェクト
		old_state = enum_state;

		DebugLog();
    }

	//プレイヤーとの距離
	void DistPlayer() {
		dist_to_player = Vector3.Scale(
			player_obj.GetComponent<Player>().TransformPosition - transform.position,
			new Vector3(1.0f, 0.0f, 1.0f));
	}

	//状態エフェクト
	void CondtionEffect() {
		CondtionEffect_Create(Enum_State.WARNING);

		CondtionEffect_Create(Enum_State.FIND);

		CondtionEffect_Create(Enum_State.AWAY);

		CondtionEffect_Create(Enum_State.ATTACK);

		CondtionEffect_Create(Enum_State.FAINT);
	}

	//--エフェクト生成
	void CondtionEffect_Create(Enum_State state) {
		//切り替わった瞬間なら進む
		if (!((enum_state == state) && (old_state != state))) {
			return;
		}

		//何も入っていなかったら進む
		if (condition_effect.obj_entitya != null) {
			return;
		}

		//----各々のエフェクトを代入
		CondtionEffect_Assign(state);

		//エフェクトが入っていれば進む
		if (condition_effect.obj_attach == null) {
			return;
		}

		//生成,子としてに設定
		condition_effect.obj_entitya = Instantiate(condition_effect.obj_attach, transform.position + transform.up * 4, transform.rotation);
		condition_effect.obj_entitya.transform.parent = transform;
	}

	//----各々のエフェクトを代入
	void CondtionEffect_Assign(Enum_State state) {
		switch (state) {
			case Enum_State.WARNING:
				condition_effect.obj_attach = condition_effect.warning;
				break;
			case Enum_State.FIND:
				condition_effect.obj_attach = condition_effect.find;
				break;
			case Enum_State.AWAY:
				condition_effect.obj_attach = condition_effect.away;
				break;
			case Enum_State.ATTACK:
				condition_effect.obj_attach = condition_effect.attack;
				break;
			case Enum_State.FAINT:
				condition_effect.obj_attach = condition_effect.faint;
				break;
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

				//汎用待機タイマー
				GUILayout.TextArea("汎用待機タイマー\n wait_timer：" + (wait_timer / 10).ToString());

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
				GUILayout.TextArea("壁判定左右\n" + wall_ray.hit_left_flg + "  " + wall_ray.hit_right_flg);
				GUILayout.TextArea("壁判定両方左右\n" + wall_ray.cavein_left_flg + "  " + wall_ray.cavein_right_flg);
				GUILayout.TextArea("壁判定左めり込み距離\n" + wall_ray.dist_left);
				GUILayout.TextArea("壁判定右めり込み距離\n" + wall_ray.dist_right);

				//穴判定
				GUILayout.TextArea("穴判定左右\n" + hole_ray.hit_left_flg + "  " + hole_ray.hit_right_flg);

				//ジャンプ事前判定
				GUILayout.TextArea("ジャンプ事前判定\n" + jump_ray.advance_flg);

				//気絶判定
				GUILayout.TextArea("気絶判定\n" + is_faint);

			}
			#endregion
			#endregion

		GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    //Gizmo表示 --------------------------------------------------
    void OnDrawGizmos()
    {
		#region ※GUIの判定
		//※GUIの処理(ランタイム以外でも判定したいのでここに記述)
		if (!gui.on) {
			gui.all_view = false;
			gui.debug_view = false;
		}
		#endregion


		#region 着地判定
		if (ground_cast.capsule_collider) {
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

			//if ((!Physics.Raycast((transform.position + (transform.forward * angle_mag + (transform.right)).normalized * (hole_ray.startLength)) + (transform.right * 2)

			//Gizmos.DrawRay((transform.position + (transform.forward * angle_mag + (transform.right)).normalized * (hole_ray.startLength)) + (transform.right * 1), -transform.up * hole_ray.length);
			//Gizmos.DrawRay((transform.position + (transform.forward * angle_mag + (transform.right)).normalized * (hole_ray.startLength)) - (transform.right * 1), -transform.up * hole_ray.length);
			//Gizmos.DrawRay((transform.position + (transform.forward * angle_mag + (-transform.right)).normalized * (hole_ray.startLength)) + (transform.right * 1), -transform.up * hole_ray.length);
			//Gizmos.DrawRay((transform.position + (transform.forward * angle_mag + (-transform.right)).normalized * (hole_ray.startLength)) - (transform.right * 1), -transform.up * hole_ray.length);

			Gizmos.DrawRay(transform.position + (transform.forward * WallRay.ANGLE_MAG + transform.right).normalized * hole_ray.startLength, -transform.up * hole_ray.length);
			Gizmos.DrawRay(transform.position + (transform.forward * WallRay.ANGLE_MAG + (-transform.right)).normalized * hole_ray.startLength, -transform.up * hole_ray.length);
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

	}




	//state変更 --------------------------------------------------
	void StateChange() {
		//近くにプレイヤーがいたら(待機の時)
		if (enemy_near.HitFlg) {
			if (enum_state == Enum_State.WAIT) {
				enum_state = Enum_State.WARNING;   //警戒stateに移行
			}
		}
		//音範囲内で音があったら
		if (enemy_sounddetect.HitFlg) {
			if (enum_state == Enum_State.WAIT) {
				enum_state = Enum_State.WARNING;   //警戒stateに移行
			}
		}
		//プレイヤーに触れたら(待機か,警戒,逃走のRUNの時)
		if (player_touch_flg) {
			if (enum_state == Enum_State.WAIT ||
				enum_state == Enum_State.WARNING ||
				(enum_state == Enum_State.AWAY && enum_act == Enum_Act.RUN)) {
				enum_state = Enum_State.FIND;   //発見stateに移行
			}
		}
		//視界にプレイヤーが入ったら(待機か,警戒,逃走のRUNの時)
		if (finder_flg) {
			if (enum_state == Enum_State.WAIT ||
				enum_state == Enum_State.WARNING ||
				(enum_state == Enum_State.AWAY && enum_act == Enum_Act.RUN)) {
				enum_state = Enum_State.FIND;   //発見stateに移行
			}
		}

		//プレイヤーに踏まれたら
		if (is_faint && (enum_state != Enum_State.FAINT)) {
			enum_state = Enum_State.FAINT;   //気絶stateに移行
		}

		//ショットに当たったら
		if (shot_touch_flg) {
			enum_state = Enum_State.WRAP;   //捕獲stateに移行
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
            case Enum_State.AWAY:    //逃げる ---------------
                Away();
                break;
			case Enum_State.FAINT:   //踏まれる(気絶) ---------------
				TreadBy();
				break;
			case Enum_State.ATTACK:  //攻撃 ---------------
                Attack();
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
		wait_timer_swing		 = 0;
		once_random.num			 = 0;
		once_random.isfinish	 = false;
		away_act.lookback_flg	 = false;

		enum_act		 = Enum_Act.CLEAR;
        enum_swingact	 = Enum_SwingAct.SWING;

        wait_timer = 0;
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
                if (WaitTime(wait_act.wait_time + once_random.num))
                {
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
    }

	//--首振り関数(首振る速さ、首振る時間、待機時間、次のstate)
	void Swing(int spd, int time, int wait_time, Enum_Act next_state) {
		switch (enum_swingact) {
			case Enum_SwingAct.SWING:    //首振り
				transform.Rotate(0, spd * Mathf.Deg2Rad, 0);
				if (WaitTime_Swing(time)) {
					enum_swingact = Enum_SwingAct.WAIT;
				}
				break;
			case Enum_SwingAct.WAIT:     //首振りの間
				if (WaitTime_Swing(wait_time)) {
					enum_swingact = Enum_SwingAct.SWING;
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


	//警戒(ゆっくり首を振る)
	void Warning()
    {
		//近くにプレイヤーがいた場合なら
		if (enemy_near.HitFlg) {
			transform.LookAt(transform.position + dist_to_player); //プレイヤーの方向を向く
			Clear();
		}

		//音範囲内で音があったら
		if (enemy_sounddetect.HitFlg) {
			Vector3 dist = Vector3.Scale(enemy_sounddetect.Hitpos - transform.position,
							new Vector3(1.0f, 0.0f, 1.0f));
			transform.LookAt(transform.position + dist); //ショットの方向を向く
			Clear();
			enum_state = Enum_State.WAIT;
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


	//発見(ジャンプして逃走に移行)
	void Find()
    {
        switch (enum_act)
        {
            case Enum_Act.CLEAR:
				wait_timer = 0;
				jump_ray.advance_flg = false;
				enum_act = Enum_Act.JUMP;
                break;
            case Enum_Act.JUMP: //その場で小さくジャンプ
                transform.LookAt(transform.position + dist_to_player); //プレイヤーの方向を向く
                Jump(jump_power);
                enum_act = Enum_Act.WAIT;
                break;
            case Enum_Act.WAIT:	//着地まで待機
                if (is_ground)
                {
                    enum_act = Enum_Act.END;
                }
                break;
            case Enum_Act.END:
                Clear();
                enum_state = Enum_State.AWAY; //警戒stateに移行
                break;
        }
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

				//--カーブする向き
				CurveDir();

				goto case Enum_Act.RUN;
				//break;
            case Enum_Act.RUN:     //走る
                enum_act = Enum_Act.RUN;

				//少し曲がりながら走る
				transform.Rotate(0, curve_spd, 0);

				//--ジャンプ判定によるジャンプ
				JumpRayJump_Judge();

				//--壁判定による向き変更
				WallRayRotate_Judge();

                //--穴判定による向き変更
                HoleRayRotateJudge();


				//--振り向きspd変更
				Lookback_SpdChange();

				//二人の距離が(音探知範囲*away_act.mag)より離れたら
				if ((dist_to_player.magnitude >= enemy_sounddetect.Radius * away_act.mag) && (velocity.y == 0))
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
            case Enum_Act.SWING:
                enum_act = Enum_Act.SWING;
                break;
        }
    }

	//--カーブする向き
	void CurveDir() {
		if (Random.Range(0, 1) == 0) {
			curve_spd = CURVE_SPD;
		}
		else {
			curve_spd = -CURVE_SPD;
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

	//--壁判定による向き変更
	public override void WallRayRotate_Judge()
    {
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

    //----めり込み判定
    void WallRayCavein()
    {
		//両方めり込んでいた場合、めり込みが少ないほうに曲がる
		if (wall_ray.hit_right_flg && wall_ray.hit_left_flg)
        {
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
		else
        {
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
		else if(wall_ray.cavein_right_flg) {
			wall_ray.hit_right_flg = true;
			wall_ray.hit_left_flg = false;
			if (wall_ray.dist_right == 0) {
				wall_ray.cavein_right_flg = false;
			}
		}

	}

	//--振り向きspd変更
	void Lookback_SpdChange()
    {
        //120f毎にプレイヤーの方向に向いて60fほど速度が1 / 2になる(平面時のみ)
		//振り向き
        if (!away_act.lookback_flg && WaitTime(away_act.lookback_interval))
        {
			away_act.lookback_flg = true;
			if (velocity.y == 0) {
				velocity.x = transform.forward.x * (run_speed / 2);
				velocity.z = transform.forward.z * (run_speed / 2);
			}
			else {
				velocity.x = transform.forward.x * run_speed;
				velocity.z = transform.forward.z * run_speed;
			}
		}
		//正面
        if (away_act.lookback_flg && WaitTime(away_act.lookback_time))
        {
			away_act.lookback_flg = false;
			velocity.x = transform.forward.x * run_speed;
			velocity.z = transform.forward.z * run_speed;
		}

		//穴判定で曲がっている時も遅くなる
		if (hole_ray.hit_right_flg || hole_ray.hit_left_flg) {
			velocity.x = transform.forward.x * (run_speed / 2);
			velocity.z = transform.forward.z * (run_speed / 2);
		}

	}

	//--ジャンプ判定によるジャンプ
	void JumpRayJump_Judge() 
	{
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
		if (JumpRay_Base(jump_ray.up_limit-0.2f, 1, jump_ray.advance_length)) {
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



	//踏まれる判定(気絶)
	void TreadBy() {
		//踏まれた際にプレイヤー側で変更
		//IsFaint = true;
		switch (enum_act) {
			case Enum_Act.CLEAR:
				//Debug.Log(this.name + " がプレイヤーに踏まれた");
				enum_act = Enum_Act.FAINT;
				break;
			case Enum_Act.FAINT: //気絶
				//移動停止
				velocity.x = 0;
				velocity.z = 0;

				//時間経ったら
				if (WaitTime(FAINT_TIME)) {
					enum_act = Enum_Act.END;
				}
				break;
			case Enum_Act.END:
				Clear();
				is_faint = false;
				enum_state = Enum_State.WAIT;
				break;
		}
	}


	//攻撃
	void Attack() {
		velocity = transform.forward * (run_speed);
	}


	//捕獲
	void Wrap()
    {
		run_speed = 0;
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

    private void OnTriggerStay(Collider other)
    {

	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Area")
        {
            return;
        }

        if (other.gameObject.tag == "Shot")
        {
            if (!shot_touch_flg)
            {
                shot_touch_flg = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {

	}

    //set ------------------------------------------------------------
    public void Shot_touch_flg_false() { shot_touch_flg = false; }

	//get ------------------------------------------------------------
	public bool Shot_touch_flg
    {
        get { return shot_touch_flg; }
    }

    //public bool Shot_touch_flg_false
    //{
    // set { shot_touch_flg = false; }
    //}

    public Vector3 Transform_position
    {
        get { return transform.position; }
    }

	public bool IsFaint {
		get { return is_faint; }
		set { is_faint = value; }
	}

}

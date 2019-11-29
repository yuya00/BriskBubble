using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class Enemy : CharaBase
{
    public override void Start()
    {
        base.Start();
        chara_ray = transform.Find("CharaRay");
        Clear(); //初期化
        enum_state = Enum_State.WAIT;
        old_state = enum_state;
		dist = new Vector2(0, 0);
		wall_ray.Clear();
        wall_ray.both_count = 0;
        enemynear = GetComponentInChildren<EnemyNear>();
        enemy_sound_detect = GetComponentInChildren<EnemySoundDetect>();
		player_obj = GameObject.Find("Player");
		
		//new_angle = transform.eulerAngles;
		//old_angle = new_angle;
		//dist_angle = Vector3.zero;
	}

	void Update()
    {
        base.Move();
        StateChange();  // プレイヤーとの当たり判定でstate変更
        Action();       // stateに応じて個別関数に飛ぶ

		if (shot_touch_flg) {
			run_speed = 0;
		}

        DebugLog();
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
	private Vector2 leftScrollPos = Vector2.zero;   //uGUIスクロールビュー用
	private float scroll_height = 330;
	void OnGUI()
    {
        if (gui.on)
        {
			//スクロール高さを変更
			//(出来ればmaximize on playがonならに変更したい)
			if (gui.all_view) {
				scroll_height = 700;
			}
			else scroll_height = 330;

			GUILayout.BeginVertical("box", GUILayout.Width(190));
            leftScrollPos = GUILayout.BeginScrollView(leftScrollPos, GUILayout.Width(180), GUILayout.Height(scroll_height));
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
				GUILayout.TextArea("壁判定左めり込み距離\n" + wall_ray.dist_left);
				GUILayout.TextArea("壁判定右めり込み距離\n" + wall_ray.dist_right);

				//穴判定
				GUILayout.TextArea("穴判定左右\n" + hole_ray.hit_left_flg + "  " + hole_ray.hit_right_flg);

				//ジャンプ事前判定
				GUILayout.TextArea("ジャンプ事前判定\n" + jumpray.advance_flg);

				//ショットに当たった判定
				GUILayout.TextArea("shot_touch_flg\n" + shot_touch_flg);

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
				GUILayout.TextArea("ジャンプ事前判定\n" + jumpray.advance_flg);

			}
			#endregion
			#endregion


			GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }

    //ギズモ表示 --------------------------------------------------
    Vector3 localAngle = Vector3.zero;
    void OnDrawGizmos()
    {
		#region ※GUIの判定
		//※GUIの処理(ランタイム以外でも判定したいのでここに記述)
		if (!gui.on) {
			gui.all_view = false;
			gui.debug_view = false;
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
				(transform.forward * angle_mag + (transform.right)).normalized * wall_ray.length);	//上
			Gizmos.DrawRay(transform.position - transform.up * wall_ray.down_limit,
				(transform.forward * angle_mag + (transform.right)).normalized * wall_ray.length);   //下
			Gizmos.DrawRay(transform.position - transform.up * wall_ray.down_limit +
				(transform.forward * angle_mag + (transform.right)).normalized * wall_ray.length,
				transform.up * wall_ray.box_total);   //奥

			//左ray
			Gizmos.DrawRay(transform.position + transform.up * wall_ray.up_limit, 
				(transform.forward * angle_mag + (-transform.right)).normalized * wall_ray.length);	//上
			Gizmos.DrawRay(transform.position - transform.up * wall_ray.down_limit,
				(transform.forward * angle_mag + (-transform.right)).normalized * wall_ray.length);  //下
			Gizmos.DrawRay(transform.position - transform.up * wall_ray.down_limit +
				(transform.forward * angle_mag + (-transform.right)).normalized * wall_ray.length,
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

			Gizmos.DrawRay(transform.position + (transform.forward * angle_mag + transform.right).normalized * hole_ray.startLength, -transform.up * hole_ray.length);
			Gizmos.DrawRay(transform.position + (transform.forward * angle_mag + (-transform.right)).normalized * hole_ray.startLength, -transform.up * hole_ray.length);
		}
#endregion
		
		
		#region ジャンプ判定Ray
		if (jumpray.gizmo_on) 
		{
			jumpray.BoxCastCal(transform);

			Gizmos.color = Color.blue - new Color(0, 0, 0, 0.6f);
			Gizmos.DrawRay(transform.position + transform.up * jumpray.up_limit, transform.forward * jumpray.advance_length);   //上

			Gizmos.color = Color.cyan - new Color(0, 0, 0, 0.0f);
			Gizmos.DrawRay(transform.position - transform.up * jumpray.down_limit, transform.forward * jumpray.advance_length); //下
			Gizmos.DrawRay(transform.position - transform.up * jumpray.down_limit + transform.forward * jumpray.length, transform.up * jumpray.box_total);  //縦前
			Gizmos.DrawRay(transform.position - transform.up * jumpray.down_limit + transform.forward * jumpray.advance_length, transform.up * jumpray.box_total);  //縦奥
		}
#endregion
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
            case Enum_State.ATTACK:    //逃げる ---------------
                Attack();
                break;
            case Enum_State.WRAP:    //捕獲 ---------------
                Wrap();
                break;
            case Enum_State.END:     //消去 ---------------
                End();
                break;
        }
        old_state = enum_state;
    }

    //--個別行動関数 ----------------------------------------------
	#region 個別行動関数

    //初期化
    void Clear()
    {
        enum_act = Enum_Act.CLEAR;
        enum_swingact = Enum_SwingAct.SWING;
        velocity = Vector3.zero;
        timer = 0;
        player_touch_flg = false;
        for (int i = 0; i < 8; i++)
        {
            iwork[i] = 0;
        }
        for (int i = 0; i < 8; i++)
        {
            fwork[i] = 0;
        }
        once_random.num = 0;
        once_random.isfinish = false;
        wait_timer = 0;
		wall_ray.Clear();
    }


    //待機(定期的に正面から20度程左右に首を振る)
    void Wait()
    {
		#region enum_actが変更した時,ランダム値設定可能
        if (old_act != enum_act)
        {
            once_random.isfinish = false;
        }
        old_act = enum_act;
		#endregion

		//velocity = transform.forward * 10.0f;

        switch (enum_act)
        {
            case Enum_Act.CLEAR:
                Clear();
                clear_flg = false;
                enum_act = Enum_Act.WAIT;
                break;
            case Enum_Act.WAIT:     //待機
                once_random.num = OnceRandom(-waitact.wait_random, waitact.wait_random);  //ランダム値設定
                if (WaitTime(waitact.wait_time + once_random.num))
                {       //待機
                    enum_act = Enum_Act.SWING;
                }
                break;
            case Enum_Act.SWING:    //+首振り半分
                once_random.num = OnceRandom(-waitact.swing_random, waitact.swing_random);  //ランダム値設定
                Swing(waitact.swing_spd, waitact.swing_time / 2 + once_random.num, waitact.swing_space_time, Enum_Act.SWING2);
                break;
            case Enum_Act.SWING2:    //-首振り
                once_random.num = OnceRandom(-waitact.swing_random, waitact.swing_random);  //ランダム値設定
                Swing(-waitact.swing_spd, waitact.swing_time + once_random.num, waitact.swing_space_time, Enum_Act.SWING3);
                break;
            case Enum_Act.SWING3:    //+首振り半分
                once_random.num = OnceRandom(-waitact.swing_random, waitact.swing_random);  //ランダム値設定
                Swing(waitact.swing_spd, waitact.swing_time / 2 + once_random.num, 0, Enum_Act.WAIT);
                break;
        }
    }


    //警戒(ゆっくり首を振る)
    void Warning()
    {
		//近くにプレイヤーがいた場合なら
		if (enemynear.HitFlg) {
			Vector3 dist = player_obj.GetComponent<Player>().TransformPosition - transform.position;
			dist.y = 0;
			transform.LookAt(transform.position + dist); //プレイヤーの方向を向く
			Clear();
		}

		//音範囲内で音があったら
		if (enemy_sound_detect.HitFlg) {
			Vector3 dist = enemy_sound_detect.Hitpos - transform.position;
			dist.y = 0;
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
                    Swing(warningact.swing_spd, warningact.swing_time, warningact.swing_space_time, Enum_Act.SWING2);
                    break;
                case Enum_Act.SWING2:   //首振り(+45度)
                    Swing(-warningact.swing_spd, warningact.swing_time + warningact.swing_time, warningact.swing_space_time, Enum_Act.SWING3);
                    break;
                case Enum_Act.SWING3:   //首振り(-360度)
                    Swing((warningact.swing_spd + swing3_spd_add), (warningact.swing_time + swing3_time_add), warningact.swing_space_time, Enum_Act.END);
                    break;
                case Enum_Act.END:
                    enum_act = Enum_Act.SWING;
                    break;
            }

            //プレイヤーが近接範囲を出ていたら
            if (!enemynear.HitFlg)
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
				jumpray.advance_flg = false;
				enum_act = Enum_Act.JUMP;
                break;
            case Enum_Act.JUMP: //その場で小さくジャンプ
				Vector3 dist = player_obj.GetComponent<Player>().TransformPosition - transform.position;
				dist.y = 0;
                transform.LookAt(transform.position + dist); //プレイヤーの方向を向く

                Jump(jump_power);
                enum_act = Enum_Act.WAIT;
                break;
            case Enum_Act.WAIT:     //着地まで待機
                if (is_ground)
                {
                    enum_act = Enum_Act.END;
                }
                break;
            case Enum_Act.END:      //state変更
                Clear();
                enum_state = Enum_State.AWAY; //警戒stateに移行
                break;
        }
    }


    //逃走(プレイヤーから逆方向に逃げ、一定距離で止まる)
    void Away()
    {
		// プレイヤーと逆方向のベクトルを取得
		dist.x = player_obj.GetComponent<Player>().TransformPosition.x - transform.position.x;
        dist.y = player_obj.GetComponent<Player>().TransformPosition.z - transform.position.z;
                                                              
        switch (enum_act)
        {
            case Enum_Act.CLEAR:
                //正規化
                dist.Normalize();
                dist_normal_vec = dist;

				#region ±指定角度内でランダムにベクトル変更(保留)
				/*
				//±指定角度内でベクトル変更(-10度ではなく350度になるので注意!)
				float rand_num = Random.Range(-awayact.angle, awayact.angle);
				Vector3 localAngle = transform.localEulerAngles;
				localAngle += new Vector3(0, rand_num, 0);
				transform.localEulerAngles = localAngle;

				//float rand_num = Random.Range(-awayact.angle, awayact.angle);
				//dist_normal_vec.x += Mathf.Sin(rand_num * Mathf.Deg2Rad);
				//dist_normal_vec.y += Mathf.Cos(rand_num * Mathf.Deg2Rad);
				// */
				#endregion

				//プレイヤーと逆ベクトル
				Vector3 dir = new Vector3(0, 0, 0);
				dir.x = dist_normal_vec.x;
				dir.z = dist_normal_vec.y;

				//プレイヤーと逆ベクトルの方向を見る
				transform.LookAt(transform.position - dir);

				//穴に向かわないように向き変更
				for (int i = 0; i < 30; i++) {
					if (!hole_ray.hit_right_flg && !hole_ray.hit_left_flg) {
						break;
					}
					HoleRayRotateJudge(); //--穴判定による向き変更
				}

				//穴に向かわないように向き変更
				//do {
				//	HoleRayRotateJudge(); //--穴判定による向き変更
				//} while (hole_ray.hit_right_flg || hole_ray.hit_left_flg);


				//前方向の速さ代入
				velocity = transform.forward * run_speed;

				//カーブの向き
				if (Random.Range(0, 1) == 0) {
					curve_spd = 0.05f;
				}
				else {
					curve_spd = -0.05f;
				}


				goto case Enum_Act.RUN;
				//break;
            case Enum_Act.RUN:     //走る
                enum_act = Enum_Act.RUN;

				//少し曲がりながら走る
				transform.Rotate(0, curve_spd, 0);

				//--ジャンプ判定によるジャンプ
				JumpRay_Jump_Judge();

                //--壁判定による向き変更
                WallRayRotateJudge();

                //--穴判定による向き変更
                HoleRayRotateJudge();


				//--振り向き
				Away_LookBack();

                //二人の距離が(音探知範囲*awayact.mag)より離れたら
                if ((dist.magnitude >= enemy_sound_detect.Radius * awayact.mag) && (velocity.y == 0))
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
                enum_state = Enum_State.WAIT;
                break;
            case Enum_Act.SWING:
                enum_act = Enum_Act.SWING;
                break;
        }
    }

    //--壁判定による向き変更
    public override void WallRayRotateJudge()
    {
		if (!wall_ray.judge_on) {
			return;
		}
		//ジャンプの準備してたら飛ばす
		if (jumpray.advance_flg || (velocity.y != 0)) {
			return;
		}

        //----壁判定Ray当たり判定
        WallRayJudge();

		//----めり込み判定
		WallRay_Cavein();

        //----向き変更
        WallRayRotate();
	}

    //----めり込み判定
    void WallRay_Cavein()
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

	//--振り向き
	void Away_LookBack()
    {
        //120f毎にプレイヤーの方向に向いて60fほど速度が1 / 2になる(平面時のみ)
        if (!lookback_flg && WaitTime(awayact.lookback_interval))
        {
            lookback_flg = true;
			if (velocity.y == 0) {
				velocity.x = transform.forward.x * (run_speed / 2);
				velocity.z = transform.forward.z * (run_speed / 2);
			}
			else {
				velocity.x = transform.forward.x * run_speed;
				velocity.z = transform.forward.z * run_speed;
			}
		}
        if (lookback_flg && WaitTime(awayact.lookback_time))
        {
            lookback_flg = false;
			velocity.x = transform.forward.x * run_speed;
			velocity.z = transform.forward.z * run_speed;
		}

		//穴判定で曲がっている時は遅くなる
		if (hole_ray.hit_right_flg || hole_ray.hit_left_flg) {
			velocity.x = transform.forward.x * (run_speed / 2);
			velocity.z = transform.forward.z * (run_speed / 2);
		}

	}

	//--ジャンプ判定によるジャンプ
	void JumpRay_Jump_Judge() 
	{
		if (!jumpray.judge_on) {
			return;
		}
		//----ジャンプ事前判定Ray当たり判定
		JumpRay_Judge_Advance();

		//----ジャンプ判定Ray当たり判定
		JumpRay_Judge();

		//----ジャンプ
		JumpRay_Jump();
	}

	//----ジャンプ事前判定Ray当たり判定
	void JumpRay_Judge_Advance() {
		//RaycastHit hit;
		jumpray.BoxCastCal(transform);

		#region BoxCast
		/*
		//Box:true ジャンプ上限Ray:false
		if (Physics.BoxCast(jumpray.box_pos, new Vector3(0, jumpray.box_total / 2, jumpray.advance_length / 2),
			transform.forward, out hit,
			transform.rotation, jumpray.advance_length / 2)) 
			{
			if (hit.collider.gameObject.tag == "Wall") {
				if (!Physics.Raycast(transform.position + transform.up * jumpray.up_limit,
					transform.forward, jumpray.advance_length)) 
					{
					jumpray.advance_flg = true;
				}
				//else jumpray.advance_flg = false;
			}
		}
		// */
		#endregion

		#region RayCast_Three
		//右のレイ(上,下,真ん中)
		if (JumpRay_Base(jumpray.up_limit-0.2f, 1, jumpray.advance_length)) {
			if (JumpRay_Up()) {
				jumpray.advance_flg = true;
			}
		}
		else if (JumpRay_Base(wall_ray.down_limit, -1, jumpray.advance_length)) {
			if (JumpRay_Up()) {
				jumpray.advance_flg = true;
			}
		}
		else if (JumpRay_Base(0, 0, jumpray.advance_length)) {
			if (JumpRay_Up()) {
				jumpray.advance_flg = true;
			}
		}
		else {
			//jumpray.advance_flg = false;
		}
		#endregion

	}

	//------レイ判定
	bool JumpRay_Base(float limit, int limit_one, float length) {
		RaycastHit hit;

		if (Physics.Raycast(jumpray.box_pos + (transform.up * limit * limit_one),
			transform.forward, out hit, length)) {
			if (hit.collider.gameObject.tag == "Wall") {
				return true;
			}
		}
		return false;
	}

	//------ジャンプ出来ない判定(上側)
	bool JumpRay_Up() {
		if (!Physics.Raycast(transform.position + transform.up * jumpray.up_limit,
			transform.forward, jumpray.advance_length)) {
			return true;
		}
		return false;
	}


	//----ジャンプ判定Ray当たり判定
	void JumpRay_Judge() {
		//RaycastHit hit;

		#region RayCast
		if (jumpray.advance_flg) {
			if (JumpRay_Base(wall_ray.up_limit, 1, jumpray.length)) {
				jumpray.flg = true;
			}
			else if (JumpRay_Base(wall_ray.down_limit, -1, jumpray.length)) {
				jumpray.flg = true;
			}
			else if (JumpRay_Base(0, 0, jumpray.length)) {
				jumpray.flg = true;
			}
		}
		#endregion
		
		#region BoxCast
		/*
		if (jumpray.advance_flg) {
			if (Physics.BoxCast(jumpray.box_pos,jumpray.box_size,
				transform.forward, out hit,
				transform.rotation, jumpray.length / 2)) 
				{
				if (hit.collider.gameObject.tag == "Wall") {
					jumpray.flg = true;
				}
			}
		}
		// */
		#endregion
		
		#region BoxCast(事前判定無しver)
		/*
		//Box:true ジャンプ上限Ray:false
		if (Physics.BoxCast(jumpray.pos,new Vector3(0, jumpray.total / 2, jumpray.length / 2),
			transform.forward,out hit,
			Quaternion.identity,jumpray.length / 2)) 
			{
			if (hit.collider.gameObject.tag == "Wall") 
				{
				if (!Physics.Raycast(transform.position + new Vector3(0, jumpray.up_limit, 0),
					transform.forward, out hit, jumpray.length)) 
					{
					jumpray.flg = true;
				}
			}
		}
		// */
		#endregion

	}

	//----ジャンプ
	void JumpRay_Jump() {
		if (jumpray.flg) {
			Jump(jumpray.power);
			jumpray.flg = false;
			jumpray.advance_flg = false;
		}

	}


	//攻撃
	void Attack()
    {
        velocity = transform.forward * (run_speed);
    }


    //捕獲
    void Wrap()
    {

    }


    //消去
    void End()
    {

    }




    //首振り関数(首振る速さ、首振る時間、待機時間、次のstate)
    void Swing(int spd, int time, int wait_time, Enum_Act next_state)
    {
        switch (enum_swingact)
        {
            case Enum_SwingAct.SWING:    //首振り
                transform.Rotate(0, spd * Mathf.Deg2Rad, 0);    //回転
                if (WaitTime_Swing(time))
                {
                    enum_swingact = Enum_SwingAct.WAIT;
                }
                break;
            case Enum_SwingAct.WAIT:     //首振りの間
                if (WaitTime_Swing(wait_time))
                {
                    enum_swingact = Enum_SwingAct.SWING;
                    enum_act = next_state;
                }
                break;
        }
    }

    //ジャンプ
    void Jump(float power)
    {
        velocity.y = power;
        rigid.useGravity = false;
        is_ground = false;
        player_touch_flg = false;
    }

    //一度だけランダム値設定
    int OnceRandom(int min, int max)
    {
        if (!once_random.isfinish)
        {
            once_random.num = Random.Range(min, max);
            once_random.isfinish = true;
            return once_random.num;
        }
        else
        {
            return once_random.num;
        }
    }

    //一度だけ初期化
    void OnceClear()
    {
        if (!clear_flg)
        {
            Clear();
            clear_flg = true;
        }
    }

	#endregion

    //プレイヤーとの当たり判定でstate変更 ---------------------------
    void StateChange()
    {
        //近くにプレイヤーがいたら(待機の時)
        if (enemynear.HitFlg)
        {
            if (enum_state == Enum_State.WAIT)
            {
                enum_state = Enum_State.WARNING;   //警戒stateに移行
            }
        }
        //音範囲内で音があったら
        if (enemy_sound_detect.HitFlg)
        {
            if (enum_state == Enum_State.WAIT)
            {
                enum_state = Enum_State.WARNING;   //警戒stateに移行
            }
        }
        //プレイヤーに触れたら(待機か,警戒,逃走のRUNの時)
        if (player_touch_flg)
        {
            if (enum_state == Enum_State.WAIT ||
                enum_state == Enum_State.WARNING ||
                (enum_state == Enum_State.AWAY && enum_act == Enum_Act.RUN))
            {
                enum_state = Enum_State.FIND;   //発見stateに移行
            }
        }
        //視界にプレイヤーが入ったら(待機か,警戒,逃走のRUNの時)
        if (finder_flg)
        {
            if (enum_state == Enum_State.WAIT ||
                enum_state == Enum_State.WARNING ||
                (enum_state == Enum_State.AWAY && enum_act == Enum_Act.RUN))
            {
                enum_state = Enum_State.FIND;   //発見stateに移行
            }
        }

    }




    //当たり判定 ---------------------------------------------------
    private void OnCollisionEnter(Collision other)
    {
        //何かに当たったとき
        if (other.gameObject.tag == "Player")
        {
            if (player_touch_flg == false)
            {
                player_touch_flg = true;
            }
        }
        //if (other.gameObject.tag == "Shot") {
        //	if (shot_touch_flg == false) {
        //		shot_touch_flg = true;
        //	}
        //}

    }

    private void OnCollisionExit(Collision other)
    {
        //何にも当たっていないとき
        if (other.gameObject.tag == "Player")
        {
            if (player_touch_flg == true)
            {
                player_touch_flg = false;
            }
        }
        //if (other.gameObject.tag == "Shot") {
        //	if (shot_touch_flg == true) {
        //		shot_touch_flg = false;
        //	}
        //}


    }

    private void OnTriggerStay(Collider other)
    {
        //if (other.gameObject.name == "AreaSearch" ||
        //	other.gameObject.name == "AreaNear" ||
        //	other.gameObject.name == "AreaSoundDetect") {
        //	return;
        //}
        //if (other.gameObject.tag == "Shot") {
        //	if (shot_touch_flg == false) {
        //		shot_touch_flg = true;
        //		triggered = true;
        //		this.shotObject = other.gameObject;
        //		this.shotname = other.gameObject.name;
        //	}
        //}
        //else this.shotname = null;

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Area")
        {
            return;
        }

        if (other.gameObject.tag == "Shot")
        {
            if (shot_touch_flg == false)
            {
                shot_touch_flg = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //if (other.gameObject.tag == "Shot") {
        //	if (shot_touch_flg == true) {
        //		shot_touch_flg = false;
        //	}
        //}
        //Debug.Log("shotOther" + this.shotOther);
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


}

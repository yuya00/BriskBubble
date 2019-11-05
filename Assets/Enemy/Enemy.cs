using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Enemy : CharaBase
{

	// Start is called before the first frame update
	public override void Start()
    {
        base.Start();
        chara_ray			 = transform.Find("CharaRay");
		Clear(); //初期化
		enum_state			 = Enum_State.WAIT;
		old_state			 = enum_state;
		p_player			 = new Player();
		run_spd				 = p_player.Run_spd / spd_ratio;
		dist				 = new Vector2(0, 0);
		WallRay_Clear();
		wallray.both_count = 0;
		enemynear = GetComponentInChildren<EnemyNear>();
		enemy_sound_detect	 = GetComponentInChildren<EnemySoundDetect>();

		//new_angle = transform.eulerAngles;
		//old_angle = new_angle;
		//dist_angle = Vector3.zero;
	}

	// Update is called once per frame
	void Update()
    {
		base.Move();
		StateChange();  // プレイヤーとの当たり判定でstate変更
		Action();       // stateに応じて個別関数に飛ぶ

		Debug_Log();

	}

	//void OnGUI() {
	//	GUILayout.BeginVertical("box", GUILayout.Width(190));

	//	//スクロール
	//	leftScrollPos = GUILayout.BeginScrollView(leftScrollPos, GUILayout.Width(180), GUILayout.Height(330));

	////	//座標
	////	float posx = Mathf.Round(transform.position.x * 100.0f) / 100.0f;
	////	float posy = Mathf.Round(transform.position.y * 100.0f) / 100.0f;
	////	float posz = Mathf.Round(transform.position.z * 100.0f) / 100.0f;
	////	GUILayout.TextArea("座標\n (" + posx.ToString() + ", " + posy.ToString() + ", " + posz.ToString() + ")");

	////	//速さ
	////	float spdx = Mathf.Round(velocity.x * 100.0f) / 100.0f;
	////	float spdy = Mathf.Round(velocity.y * 100.0f) / 100.0f;
	////	float spdz = Mathf.Round(velocity.z * 100.0f) / 100.0f;
	////	GUILayout.TextArea("速さ\n (" + spdx.ToString() + ", " + spdy.ToString() + ", " + spdz.ToString() + ")");

	////	//汎用待機タイマー
	////	GUILayout.TextArea("汎用待機タイマー\n wait_timer：" + (wait_timer / 10).ToString());

	////	//状態(待機や警戒など)
	////	GUILayout.TextArea("状態\n enum_state：" + enum_state.ToString());

	////	//状態内の行動(首振りやジャンプなど)
	////	GUILayout.TextArea("行動\n act：" + enum_act.ToString());

	////	////ランダム値
	////	//GUILayout.TextArea("ランダム値\n num：" + once_random.num.ToString() + "\n"
	////	//	+ " isfinish：" + once_random.isfinish.ToString());

	////	////首振りの行動
	////	//GUILayout.TextArea("首振り\n swingact：" + enum_swingact.ToString());

	////	//回転
	////	GUILayout.TextArea("回転\n " + transform.localEulerAngles.ToString());

	//	//レイが両方当たった回数
	//	//GUILayout.TextArea("レイが両方当たった回数\n " + wallray.both_count.ToString());

	//	//斜めベクトル
	//	//GUILayout.TextArea("eulerAngles\n" + transform.eulerAngles);
	//	//GUILayout.TextArea("dist_angle\n" + dist_angle);

	//	//
	//	GUILayout.TextArea("holeray_flg\n" + holeray.hit_right_flg);
	//	GUILayout.TextArea("holeray_flg\n" + holeray.hit_left_flg);


	//	//スクロール終了
	//	GUILayout.EndScrollView();



 //       GUILayout.EndVertical();
 //   }



    //*******************************************
    // デバッグログ表示
    //*******************************************
    public override void Debug_Log() {
		base.Debug_Log();
		//Debug.Log("touch:" + player_touch_flg);
		//Debug.Log("enum:"+enum_state);
		//Debug.Log("enum_act:" + enum_act);
		//Debug.Log(velocity);
		//Debug.Log(transform.localEulerAngles);

		//Debug.DrawRay(
		//	transform.position,
		//	(transform.forward + transform.forward + transform.right).normalized * -30.0f);



		//new_angle = transform.eulerAngles;
		//dist_angle = new_angle - old_angle;

		////斜めのベクトルを出す方法 ※要修正
		//Debug.DrawRay(transform.position, (new Vector3(30 * Mathf.Deg2Rad, 0, 30 * Mathf.Deg2Rad) + dist_angle) * -wallray.langth);

		//old_angle = new_angle;

	}

	Vector3 localAngle = Vector3.zero;
	void OnDrawGizmos() {

		#region 斜めベクトル
		//new_angle = transform.eulerAngles;
		//dist_angle = new_angle - old_angle;

		////斜めのベクトルを出す方法 ※要修正
		//Gizmos.DrawRay(transform.position, (new Vector3(30 * Mathf.Deg2Rad, 0, 30 * Mathf.Deg2Rad) + dist_angle) * -wallray.langth);

		//old_angle = new_angle;
		#endregion

		Gizmos.DrawRay(transform.position, (transform.forward * angle_mag + transform.right).normalized * -wallray.langth);
		Gizmos.DrawRay(transform.position, (transform.forward * angle_mag + (-transform.right)).normalized * -wallray.langth);


		Gizmos.DrawRay(transform.position + (transform.forward * angle_mag + transform.right).normalized * -wallray.langth, -transform.up * holeray.length);
		Gizmos.DrawRay(transform.position + (transform.forward * angle_mag + (-transform.right)).normalized * -wallray.langth, -transform.up * holeray.length);

		//Gizmos.DrawRay(transform.position, (transform.forward * angle_mag + (-transform.right)).normalized * -wallray.langth);


	}


	// ********************************************
	// stateに応じて個別関数に飛ぶ
	// ********************************************
	void Action() {
		//他のstateに行くとき初期化
		if (old_state != enum_state) {
			Clear();
		}

		switch (enum_state) {
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

	// ********************************************
	// --個別行動関数
	// ********************************************
	#region 個別行動関数

	//初期化
	void Clear() {
		enum_act = Enum_Act.CLEAR;
		enum_swingact = Enum_SwingAct.SWING;
		velocity = Vector3.zero;
		timer = 0;
		player_touch_flg = false;
		for (int i = 0; i < 8; i++) {
			iwork[i] = 0;
		}
		for (int i = 0; i < 8; i++) {
			fwork[i] = 0;
		}
		once_random.num = 0;
		once_random.isfinish = false;
		wait_timer = 0;
		WallRay_Clear();
	}


	//待機(定期的に正面から20度程左右に首を振る)
	void Wait() {
		#region enum_actが変更した時,ランダム値設定可能
		if (old_act != enum_act) {
			once_random.isfinish = false;
		}
		old_act = enum_act;
		#endregion

		switch (enum_act) {
			case Enum_Act.CLEAR:
				Clear();
				clear_flg = false;
				enum_act = Enum_Act.WAIT;
				break;
			case Enum_Act.WAIT:     //待機
				once_random.num = OnceRandom(-waitact.wait_random, waitact.wait_random);  //ランダム値設定
				if (WaitTime(waitact.wait_time + once_random.num)) {		//待機
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
	void Warning() {
		//見つけてないとき
		if (!finder_flg) {
			//ゆっくり体を回して音源、プレイヤーを探す
			switch (enum_act) {
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
			if (!enemynear.HitFlg) {
				if (enum_act == Enum_Act.END) {
					Clear();
					enum_state = Enum_State.WAIT; //待機
				}
			} //if(enemynear.HitFlg)

			if (WaitTime(60*10)) {
				Clear();
				enum_state = Enum_State.WAIT; //待機
			} //if (WaitTime(180)

		} //if (!finder_flg)

	}


	//発見(ジャンプして逃走に移行)
	void Find() {
		switch (enum_act) {
			case Enum_Act.CLEAR:
				enum_act = Enum_Act.JUMP;
				break;
			//その場で小さくジャンプ
			case Enum_Act.JUMP:
				Vector3 distr = transform.position - player.transform.position;
				distr.y = 0;
				transform.LookAt(transform.position + distr); //プレイヤーの方向を向く
				Jump(jump_power);
				enum_act = Enum_Act.WAIT;
				break;
			case Enum_Act.WAIT:		//着地まで待機
				if (is_ground) {
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
	void Away() {
		// プレイヤーと逆方向のベクトルを取得
		dist.x = transform.position.x - player.transform.position.x;
		dist.y = transform.position.z - player.transform.position.z;

		switch (enum_act) {
			case Enum_Act.CLEAR:
				//正規化
				dist.Normalize();
				dist_normal_vec = dist;

				//±指定角度内でベクトル変更(-10度ではなく350度になるので注意!)
				float rand_num = Random.Range(-awayact.angle, awayact.angle);
				dist_normal_vec.x += Mathf.Sin(rand_num * Mathf.Deg2Rad);
				dist_normal_vec.y += Mathf.Cos(rand_num * Mathf.Deg2Rad);

				//プレイヤーと逆方向のベクトルの速さ代入
				velocity.x = dist_normal_vec.x;
				velocity.z = dist_normal_vec.y;

				//プレイヤーと逆+ランダムベクトル視点を見る
				transform.LookAt(transform.position - velocity);

				//前方向の速さ代入
				velocity = transform.forward * (-run_spd);

				goto case Enum_Act.RUN;
				//break;
			case Enum_Act.RUN:     //走る
				enum_act = Enum_Act.RUN;

				//transform.Rotate(0, 0.1f, 0);

				//--壁判定による向き変更
				WallRay_Rotate_Judge();

				//--穴判定による向き変更
				HoleRay_Rotate_Judge();

				//--振り向き
				Away_LookBack();

				//二人の距離が(音探知範囲*awayact.mag)より離れたら
				if (dist.magnitude >= enemy_sound_detect.Radius * awayact.mag) {
					enum_act = Enum_Act.END;
				}
				break;
			case Enum_Act.END:      //state変更
				//プレイヤーの方向を向く ※要修正
				transform.LookAt(transform.position + velocity);
				Clear();
				enum_state = Enum_State.WAIT;
				break;
		}
	}

	//--壁判定による向き変更
	void WallRay_Rotate_Judge() {
		//----壁判定Ray当たり判定
		WallRay_Judge();

		//----めり込み判定
		WallRay_Cavein();

		//----向き変更
		WallRay_Rotate();
	}

	//----壁判定Ray当たり判定
	void WallRay_Judge() {
		RaycastHit hit;

		//右のレイ
		if (Physics.Raycast(transform.position,
			(transform.forward * angle_mag + transform.right).normalized * -1, out hit, wallray.langth)) {
			if (hit.collider.gameObject.tag == "Wall") {
				wallray.dist_right = hit.distance;  //壁との距離保存
				wallray.hit_right_flg = true;       //壁との当たり判定
			}
		}
		else {
			wallray.dist_right = 0;
			wallray.hit_right_flg = false;
		}

		//左のレイ
		if (Physics.Raycast(transform.position,
			(transform.forward * angle_mag + (-transform.right)).normalized * -1, out hit, wallray.langth)) {
			if (hit.collider.gameObject.tag == "Wall") {
				wallray.dist_left = hit.distance;   //壁との距離保存
				wallray.hit_left_flg = true;        //壁との当たり判定
			}
		}
		else {
			wallray.dist_left = 0;
			wallray.hit_left_flg = false;
		}

	}

	//----めり込み判定
	void WallRay_Cavein() {
		//両方めり込んでいた場合、めり込み具合で片方false
		if (wallray.hit_right_flg && wallray.hit_left_flg) {
			//if (wall_dist1 <= wall_dist2) {
			//	wall_rotate_flg2 = false;
			//}
			//else {
			//	wall_rotate_flg1 = false;
			//}

			//どこで一回と区切るか
			//↓Clearで初期化
			if (!wallray.both_flg) {
				wallray.both_count++;
			}
			wallray.both_flg = true;

			//指定回数にめり込んだら、ATTACKに移行
			if (wallray.both_count >= 4) {
				//プレイヤーの方を向く
				transform.LookAt(transform.position + new Vector3(dist.x, 0, dist.y));
				enum_state = Enum_State.ATTACK;
				Clear();
			}
		}
		else {
			wallray.both_flg = false;
		}
	}

	//----向き変更
	void WallRay_Rotate() {
		if (wallray.hit_right_flg) {
			transform.Rotate(0.0f, -wallray.spd, 0.0f);
		}
		else if (wallray.hit_left_flg) {
			transform.Rotate(0.0f, wallray.spd, 0.0f);
		}
	}


	//--穴判定による向き変更
	void HoleRay_Rotate_Judge() {
		//----穴判定Ray当たり判定
		HoleRay_Judge();

		//----向き変更
		HoleRay_Rotate();
	}

	//----穴判定Ray当たり判定
	void HoleRay_Judge() {
		RaycastHit hit;
		//何にも当たっていなかったら

		//右のレイ
		if (!Physics.Raycast(transform.position + (transform.forward * angle_mag + transform.right).normalized * -wallray.langth,
			-transform.up, out hit, holeray.length)) {
			holeray.hit_right_flg = true;
		}
		else {
			holeray.hit_right_flg = false;
		}

		//左のレイ
		if (!Physics.Raycast(transform.position + (transform.forward * angle_mag + (-transform.right)).normalized * -wallray.langth,
			-transform.up, out hit, holeray.length)) {
			holeray.hit_left_flg = true;
		}
		else {
			holeray.hit_left_flg = false;
		}

	}

	//----向き変更
	void HoleRay_Rotate() {
		if (holeray.hit_right_flg) {
			transform.Rotate(0.0f, -holeray.spd, 0.0f);
		}
		else if (holeray.hit_left_flg) {
			transform.Rotate(0.0f, holeray.spd, 0.0f);
		}
	}



	//--振り向き
	void Away_LookBack() {
		//120f毎にプレイヤーの方向に向いて60fほど速度が1 / 2になる
		if (!lookback_flg && WaitTime(awayact.lookback_interval)) {
			lookback_flg = true;
			velocity = transform.forward * (-run_spd / 2);
		}
		if (lookback_flg && WaitTime(awayact.lookback_time)) {
			lookback_flg = false;
			velocity = transform.forward * (-run_spd);
		}
	}


	//攻撃
	void Attack() {
		velocity = transform.forward * (-run_spd);
	}


	//捕獲
	void Wrap() {
		
	}


	//消去
	void End() {

	}




	//首振り関数
	//首振る速さ、首振る時間、待機時間、次のstate
	void Swing(int spd, int time, int wait_time, Enum_Act next_state) {
		switch (enum_swingact) {
			case Enum_SwingAct.SWING:    //首振り
				transform.Rotate(0, spd * Mathf.Deg2Rad, 0);    //回転
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

	// ジャンプ
	void Jump(float power) {
		velocity.y = power;
		rigid.useGravity = false;
		is_ground = false;
		player_touch_flg = false;
		//Debug.Log("Jump");
	}

	//一度だけランダム値設定
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

	//一度だけ初期化
	void OnceClear() {
		if (!clear_flg) {
			Clear();
			clear_flg = true;
		}
	}

	#endregion

	// ********************************************
	// プレイヤーとの当たり判定でstate変更
	// ********************************************
	void StateChange() {
		//近くにプレイヤーがいたら(待機の時)
		if (enemynear.HitFlg) {
			if (enum_state == Enum_State.WAIT) {
				enum_state = Enum_State.WARNING;   //警戒stateに移行
			}
		}
		//音範囲内で音があったら
		if (enemy_sound_detect.HitFlg) {
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

	}





	//*******************************************
	// 当たり判定
	//*******************************************
	//何かに当たったとき

	private void OnCollisionEnter(Collision other) {
		if (other.gameObject.tag == "Player") {
			if (player_touch_flg == false) {
				player_touch_flg = true;
			}
		}
	}

	//何にも当たっていないとき
	private void OnCollisionExit(Collision other) {
		if (other.gameObject.tag == "Player") {
			if (player_touch_flg == true) {
				player_touch_flg = false;
			}
		}
	}


}

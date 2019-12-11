using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class Player : CharaBase
{
    // 初期値設定
    public override void Start()
    {
        base.Start();

        // コンポーネント取得
        game_manager	 = GameObject.FindGameObjectWithTag("GameManager");
        animator		 = GetComponent<Animator>();
		effect			 = GameObject.FindGameObjectWithTag("EffectManager").GetComponent<EffectManager>();
		//chara_ray       = transform.Find("CharaRay");

		// プレイヤーのパラメーター設定
		state = START;
        init_speed      = run_speed;
        init_fric       = stop_fric;
        init_back_speed = back_speed;
        COUNT           = 23 / ANIME_SPD;       // 着地アニメフレームを計算
        respawn_pos     = transform.position;   
        shot_jump_fg    = false;
		velocity	    = Vector3.zero;
		tread_on.size   = new Vector3(ground_cast.capsule_collider.radius * TreadOn_BoxCast.RADIUS_MAG_XZ, TreadOn_BoxCast.LENGTH_Y,
			ground_cast.capsule_collider.radius * TreadOn_BoxCast.RADIUS_MAG_XZ);
		// エフェクト関連
		//effect.effect_no = 0;
	}

	void Update()
    {
        switch (state)
        {
            // 待機
            case START: Wait();     break;

            // プレイヤー更新
            case GAME:  Game();     break;

            // クリア後
            case CLEAR: GameClear(); break;
        }

		//先行入力まとめ
		LeadKeyAll();

		DebugLog();
        RayDebug();
    }

    // Update内で待機
    void Wait()
    {
        base.Move();
        // ジャンプアニメーション
        AnimeJump();
        if (game_manager.GetComponent<Scene>().StartFg()) state = GAME;
    }

    // ゲームシーンでプレイヤーの動きまとめ
    void Game()
    {
        // アニメ初期化
        InitAnime();
        // 移動
        Move();
        // ショット
        Shot();

        // クリアしたら
        if (game_manager.GetComponent<Scene>().ClearFg()) state = CLEAR;
    }

    // アニメーションをとめる
    void GameClear()
    {

    }

    void RayDebug()
    {
        //*********************************************************************//
        //Debug.DrawLine(chara_ray.position, chara_ray.position + Vector3.down * chara_ray_length, Color.red);

        // きっちり足元判定
        for (int i = 0; i < 9; ++i)
        {
            //Debug.DrawRay(chara_ray.position + ofset_layer_pos[i], Vector3.Down * (chara_ray_length * 0.5f), Color.green);
        }
        //*********************************************************************//
    }

    public override void DebugLog()
    {
        /*
		base.DebugLog();

		//*/
    }


    public override void FixedUpdate()
    {
        switch (state)
        {
            case START:
                if (game_manager.GetComponent<Scene>().StartFg()) state = GAME;
                break;
            case GAME:
                base.FixedUpdate();
                // 移動
                LstickMove();
                break;
            case CLEAR:

                break;
        }
    }


    //GUI表示 -----------------------------------------------------
    private Vector2 left_scroll_pos = Vector2.zero;   //uGUIスクロールビュー用
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
           left_scroll_pos = GUILayout.BeginScrollView(left_scroll_pos, GUILayout.Width(180), GUILayout.Height(scroll_height));
           GUILayout.Box("Player");
		float spdx, spdy, spdz;

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

				//回転
				GUILayout.TextArea("回転\n " + transform.localEulerAngles.ToString());

				//着地判定
				GUILayout.TextArea("着地判定\n " + is_ground);

				//壁掴み判定
				GUILayout.TextArea("壁との当たり判定\n " + wall_touch_flg.ToString());
				GUILayout.TextArea("壁掴み準備判定\n " + wall_grab_ray.prepare_flg.ToString());
				GUILayout.TextArea("壁掴み判定\n " + wall_grab_ray.flg.ToString());

				////壁掴んだ瞬間
				//GUILayout.TextArea("壁前方向との内積\n" + wall_forward_angle.ToString());
				//GUILayout.TextArea("壁後方向との内積\n" + wall_back_angle.ToString());
				//GUILayout.TextArea("壁右方向との内積\n" + wall_right_angle.ToString());
				//GUILayout.TextArea("壁左方向との内積\n" + wall_left_angle.ToString());
				//GUILayout.TextArea("プレイヤーの角度\n" + transform.localEulerAngles.ToString());

				//壁判定
				GUILayout.TextArea("壁判定左右\n" + wall_ray.hit_left_flg + "  " + wall_ray.hit_right_flg);
				GUILayout.TextArea("壁判定両方左右\n" + wall_ray.cavein_left_flg + "  " + wall_ray.cavein_right_flg);
				GUILayout.TextArea("壁判定左めり込み距離\n" + wall_ray.dist_left);
				GUILayout.TextArea("壁判定右めり込み距離\n" + wall_ray.dist_right);

				//穴判定
				GUILayout.TextArea("穴判定左右\n " + hole_ray.hit_left_flg + "  " + hole_ray.hit_right_flg);

				//踏みつけジャンプ判定(着地まで)
				GUILayout.TextArea("踏みつけジャンプ\n " + tread_on.flg);

				//汎用タイマー
				GUILayout.TextArea("汎用タイマー\n " + wait_timer);

			}
			#endregion
		#region 開発用
		else if (gui.debug_view) {
			//GUILayout.TextArea("effect\n" + effect.effect_jump);

			//GUILayout.TextArea("先行入力キー\n" + lead_key);
			//GUILayout.TextArea("先行入力押されたキー");
			//for (int i = 0; i < lead_key_num; i++) {
			//	GUILayout.TextArea(""+lead_inputs[i].pushed_key);
			//	GUILayout.TextArea("" + lead_inputs[i].frame);
			//}

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

			//着地判定
			GUILayout.TextArea("着地判定\n " + is_ground);

			//踏みつけジャンプ判定(着地まで)
			GUILayout.TextArea("踏みつけジャンプ\n " + tread_on.flg);

			//気絶
			GUILayout.TextArea("気絶\n " + is_faint);

			//汎用タイマー
			GUILayout.TextArea("汎用タイマー\n" + wait_timer);

			////ジャンプアニメカウント
			//GUILayout.TextArea("ジャンプアニメカウント\n " + jump_anim_count);

		}
		#endregion
		#endregion


		GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    //ギズモ表示 --------------------------------------------------
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
		if (ground_cast.gizmo_on && ground_cast.capsule_collider) {
			Gizmos.color = Color.magenta - new Color(0, 0, 0, 0.6f);
			Gizmos.DrawWireSphere(ground_cast.pos - (transform.up * ground_cast.length), GroundCast.RADIUS);
		}
		#endregion


		#region 壁判定Ray
		if (wall_ray.gizmo_on)
        {
            Gizmos.color = Color.green - new Color(0, 0, 0, 0.3f);
            Gizmos.DrawRay(transform.position, (transform.forward * WallRay.ANGLE_MAG + transform.right).normalized * wall_ray.length);
            Gizmos.DrawRay(transform.position, (transform.forward * WallRay.ANGLE_MAG + (-transform.right)).normalized * wall_ray.length);
        }
		#endregion


		#region 穴判定Ray
		if (hole_ray.gizmo_on)
        {
            Gizmos.color = Color.green - new Color(0, 0, 0, 0.3f);
            Gizmos.DrawRay(transform.position + (transform.forward * WallRay.ANGLE_MAG + transform.right).normalized * wall_ray.length, -transform.up * hole_ray.length);
            Gizmos.DrawRay(transform.position + (transform.forward * WallRay.ANGLE_MAG + (-transform.right)).normalized * wall_ray.length, -transform.up * hole_ray.length);
        }
		#endregion


		#region 壁掴み判定Ray
		if (wall_grab_ray.gizmo_on)
        {
            Gizmos.color = Color.magenta - new Color(0, 0, 0, 0.2f);
            Gizmos.DrawRay(transform.position + new Vector3(0, wall_grab_ray.height, 0), transform.forward * wall_grab_ray.length);

            //--横移動制限Ray
            Gizmos.DrawRay(transform.position + transform.right * wall_grab_ray.side_length, transform.forward * wall_grab_ray.length);
            Gizmos.DrawRay(transform.position + transform.right * -wall_grab_ray.side_length, transform.forward * wall_grab_ray.length);
        }
		#endregion


		#region 踏みつけ判定
		if (tread_on.gizmo_on && ground_cast.capsule_collider) {
			Gizmos.color = Color.red - new Color(0, 0, 0, 0.6f);
			Gizmos.DrawWireCube(transform.position + 
								(transform.up * ground_cast.capsule_collider.center.y) - 
								(transform.up * (ground_cast.capsule_collider.height / 2)), tread_on.size);
			//Gizmos.DrawRay(ground_ray_pos, -transform.up * ground_ray_length);
		}
		#endregion

	}

	// 移動 -------------------------------------------------------
	public override void Move()
    {
        base.Move();

        // 速度設定
        run_speed = init_speed;
        stop_fric = init_fric;

        //ジャンプ時の移動慣性
        if (is_ground)
        {
            jump_fric = 1;
            fall_y = transform.position.y;  // 着地してるときは自分のy位置を保存
        }
        else jump_fric = jump_fric_power;

        // バブル状態のとき
        if (shot_state > 1) BubbleSpeed(2.0f, 0.1f);

        // ショット3を撃った後プレイヤーをとめる
        if (back_player) BackMove();

        // ジャンプまとめ
        JumpMove();

        // リスポーン
        RespawnFall();

        // 頭方向に何か当たったか
        HeadHit();

		//敵踏みつけ
		TreadOn();

		//敵に接触(気絶)
		Faint();


		////--壁判定による向き変更
		//WallRayRotate_Judge();

		////--穴判定による向き変更
		//HoleRayRotateJudge();

		//--壁掴み判定Rayによる掴み
		WallGrabRayGrabJudge();

    }

    // カメラの正面にプレイヤーが進むようにする(横移動したときにカメラも移動するように)
    void LstickMove()
    {
		if (is_faint || tread_on.flg) {
			//→ここに気絶アニメの処理
			animator.SetBool("Walk", false);
			animator.SetBool("Run", false);
			return;
		}

        Vector3 move = new Vector3(0, 0, 0);

        // スピード
        float axis_x = 0, axis_y = 0;

        // パッド情報代入
        float pad_x = Input.GetAxis("L_Stick_H");
        float pad_y = -Input.GetAxis("L_Stick_V");
        pad_x = Input.GetAxis("Horizontal");
        pad_y = Input.GetAxis("Vertical");

        axis_x += pad_x;
        axis_y += pad_y;

        // 平方根を求めて正規化
        float axis_length = Mathf.Sqrt(axis_x * axis_x + axis_y * axis_y);
        if (axis_length > NORMALIZE)
        {
            axis_x /= axis_length;
            axis_y /= axis_length;
        }

        // 進む方向
        Vector3 front = (transform.position - cam.transform.position).normalized;
        front.y = 0;

        // 横方向
        Vector3 right = Vector3.Cross(new Vector3(0, transform.position.y + 1.0f, 0), front).normalized;

        // 方向に値を設定
        move = right * axis_x;
        move += front * axis_y;

        //　方向キーが多少押されていたらその方向向く
        if (axis_x != 0f || axis_y != 0f) LookAt(move);

        #region 状態分け
        switch (StickState(axis_x, axis_y))
        {
            case WAIT:
                //停止時慣性(徐々に遅くなる)              
                velocity.x -= velocity.x * stop_fric;
                velocity.z -= velocity.z * stop_fric;
                animator.SetBool("Walk", false);
                animator.SetBool("Run", false);
                break;
            case WALK:
                // カメラから見てスティックを倒したほうへ進む
                velocity.x = move.normalized.x * walk_speed;
                velocity.z = move.normalized.z * walk_speed;
                animator.SetBool("Walk", true);
                animator.SetBool("Run", false);
                break;
            case RUN:
                // カメラから見てスティックを倒したほうへ進む
                velocity.x = move.normalized.x * run_speed;
                velocity.z = move.normalized.z * run_speed;
                animator.SetBool("Run", true);
                animator.SetBool("Walk", false);
                effect.Effect(PLAYER, EFC_RUN, transform.position + transform.up * run_down_pos);
                break;
        }

        #endregion
    }

    // その方向を向く
    void LookAt(Vector3 vec)
    {
        Vector3 target_pos = transform.position + vec.normalized;
        Vector3 target = Vector3.Lerp(transform.position + transform.forward, target_pos, rot_speed * Time.deltaTime);
        if (!wall_grab_ray.flg)
        {
            transform.LookAt(target);
        }
    }

    // スティックの倒し具合設定
    int StickState(float x, float y)
    {
        // 入力チェック
        if (x != 0f || y != 0f)
        {
            // スティックの傾きによって歩きと走りを切り替え
            if (Mathf.Abs(x) >= slope || Mathf.Abs(y) >= slope) return RUN;
            else return WALK;
        }
        // 入力してないときは待機
        return WAIT;
    }

    // バブル状態のときの速さ
    void BubbleSpeed(float multiply, float fric)
    {
        // 移動速度,慣性を上げる
        run_speed = init_speed * multiply;
        stop_fric = fric;
    }

    // ジャンプまとめ
    void JumpMove()
    {
        //　着地してるときにジャンプ そのときにエフェクト出す
        if (JumpOn())
        {
            Jump(jump_power);

            // TYPE : キャラ、EFFECT : ジャンプ、POS : 位置、 effect.jump_player : 何個出すか
            effect.Effect(PLAYER, JUMP, transform.position + transform.up * jump_down_pos, effect.jump_player);
        }

        // ショットに乗った時にジャンプをjump_power_up倍
        if (DownHitShot()) Jump(jump_power * jump_power_up);

        // ジャンプアニメーション
        AnimeJump();
    }

    // ジャンプの挙動
    void Jump(float jump_power)
    {
        rigid.useGravity = false;
        //is_ground = false;
        velocity.y = 0;
        velocity.y = jump_power;
        //animator.SetBool("JumpStart", jump_fg);
    }

    // ジャンプモーション用
    void AnimeJump()
    {
        // 着地してるとき
        if (is_ground)
        {
            animator.SetBool("Fall", false);
            // 着地したときに1回だけ着地をtrueにする
            JumpEndAnime();
        }

        // 落ちてる
        //if (Falling())
        if (!is_ground)
        {
            jump_anim_count = 0;
            animator.SetBool("JumpStart", false);
            animator.SetBool("Fall", true);
            //animator.SetBool("JumpEnd", true);
        }
    }

    // 着地したときに1回だけ着地をtrueにする
    void JumpEndAnime()
    {
        jump_anim_count++;
        // 地面ついたときにカウント(Fallと同時にfalseにしたらJumpEndまで来ないから少し間隔をあける)
        if (jump_anim_count < COUNT)
        {
            animator.SetBool("JumpEnd", true);
            animator.speed = ANIME_SPD;
        }
        else
        {
            animator.SetBool("JumpEnd", false);
            jump_anim_count = COUNT;
        }
    }

    // アニメ速度初期化
    void InitAnime()
    {
        // 標準速度初期化
        animator.speed = INIT_ANIME_SPD;
    }

    // リスポーン処理
    void RespawnFall()
    {
        // 最大限落ちた
        if(FallCheck(fall_y,fall_y_max))
        {
            transform.position = respawn_pos;
			velocity = Vector3.zero;
        }
    }

    // 飛んでる判定
    bool Jumping()
    {
        if (velocity.y > 0) return true;
        return false;
    }

    // ジャンプする判定
    bool JumpOn()
    {
        // モーションが終わってるときにジャンプできる
        if (is_ground && !is_faint)
        {
            if (!animator.GetBool("JumpEnd"))
            {
                if (Input.GetButtonDown("Jump") || (Input.GetMouseButtonDown(2) || (lead_key == LeadkeyKind.JUMP) ))
                {
                    //jump_fg = true;
                    //jump_fg = false;
                    //jump_timer = 0;
                    return true;
                }
            }
        }
        //if (jump_fg)
        //{
        //    animator.speed = anim_spd;
        //    jump_timer += Time.deltaTime;
        //}
        //if (jump_timer > jump_timer_max)
        //{
        //    jump_fg = false;
        //    jump_timer = 0;
        //    return true;
        //}

        return false;
    }

    // 落下中判定
    bool Falling()
    {
        // ショットに乗ったときの判定を初期化
        shot_jump_fg = false;
        // 落下判定
        if (velocity.y < 0.0f) return true;
        return false;
    }

    // 最大限落ちた
    bool FallCheck(float fall_y,float fall_y_max)
    {
        if (fall_y + transform.position.y < fall_y_max) return true;
        return false;
    }

    // 頭に何か当たった
    void HeadHit()
    {
        //Debug.DrawRay(transform.position, transform.up.normalized * 3, Color.green);
        // ジャンプ中
        if (Jumping())
        {
            // 頭当たった
            if(HeadHitJudge())
            {
                velocity.y = 0;
            }
        }
    }

	// 頭当たったか
	bool HeadHitJudge() {
		// 頭からレイ飛ばし
		RaycastHit hit;
		if (Physics.Raycast(transform.position,transform.up, out hit, 1.5f)) {
			if (hit.collider.tag == "Wall") {
				//Debug.Log("頭に壁が当たった");
				return true;
			}
		}
		return false;
	}



	//踏みつけ(踏んだらジャンプ)
	void TreadOn() {
		if (!tread_on.judge_on && is_faint) {
			return;
		}

		RaycastHit hit;
		//踏みつけ判定
		if (Physics.BoxCast(transform.position + 
			(transform.up * ground_cast.capsule_collider.center.y) - 
			(transform.up * (ground_cast.capsule_collider.height / 2)), 
			tread_on.size, -transform.up, out hit, Quaternion.identity, TreadOn_BoxCast.MAX_DISTANCE)
			//Physics.BoxCast(ground_ray_pos, tread_on.size, -transform.up, out hit, transform.rotation, 0.1f)
			&& hit.collider.tag == "Enemy"
			&& !hit.collider.GetComponent<Enemy>().IsFaint
			&& !tread_on.flg) 
			{
			tread_on.flg		 = true;
			hit.collider.GetComponent<Enemy>().IsFaint = true;
			velocity			 = (transform.forward * TreadOn_BoxCast.FOWARD_POWER);
			Jump(TreadOn_BoxCast.JUMP_POWER);
			//Debug.Log("敵を踏んだ");
		}

		//着地するまでが踏みつけ
		if (is_ground) {
			tread_on.flg = false;
		}
	}

	//気絶(ノックバック)
	void Faint() {
		if (!is_faint || tread_on.flg) {
			return;
		}
		//Debug.Log("敵に接触");

		switch (enum_faint) {
			case Enum_Faint.CLEAR:	//ノックバック
				velocity = Vector3.zero;
				velocity = -transform.forward * KnockBack.SPD_MAG;
				Jump(KnockBack.JUMP_POWER);
				enum_faint = Enum_Faint.WAIT;
				break;
			case Enum_Faint.WAIT:   //ノックバック時間
				if (WaitTimeOnce(30)) {
					wait_timer = 0;
					velocity = Vector3.zero;
					enum_faint = Enum_Faint.WAIT2;
				}
				break;
			case Enum_Faint.WAIT2:	//硬直時間
				if (WaitTimeOnce(KnockBack.FAINT_TIME)) {
					is_faint = false;
					wait_timer = 0;
					enum_faint = Enum_Faint.CLEAR;
				}
				break;

		}

	}



	//--壁掴み判定Rayによる掴み
	void WallGrabRayGrabJudge()
    {
		if (!wall_grab_ray.judge_on) {
			return;
		}

        //----当たり判定
        WallGrabRayJudge();

        //----掴む
        WallGrabRayGrab();
    }

	//----当たり判定
	void WallGrabRayJudge() {
		RaycastHit hit;

		//空中にいる、自身が壁に当たっている、レイが当たっていない
		if (!is_ground && wall_touch_flg && !wall_grab_ray.ray_flg) {
			wall_grab_ray.prepare_flg = true;
		}
		else {
			wall_grab_ray.prepare_flg = false;
		}

		//レイ判定
		if (Physics.Raycast(transform.position + new Vector3(0, wall_grab_ray.height, 0), transform.forward, out hit, wall_grab_ray.length) &&
			hit.collider.gameObject.tag == "Wall") {
			wall_grab_ray.ray_flg = true;
		}
		else {
			wall_grab_ray.ray_flg = false;
		}


		//上記二つが完了してたら掴む
		if (!wall_grab_ray.flg && wall_grab_ray.prepare_flg && wall_grab_ray.ray_flg) {
			wall_grab_ray.flg = true;
			//------掴んだ時の向き調整
			AngleAdjust();
		}

	}

	//------掴んだ時の向き調整
	void AngleAdjust()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position + new Vector3(0, wall_grab_ray.height, 0), transform.forward, out hit, wall_grab_ray.length))
        {
			//Vector2に保存
			wall_grab_adjust.forward		 = new Vector2(hit.transform.forward.x, hit.transform.forward.z);
			wall_grab_adjust.back			 = new Vector2(hit.transform.forward.x, -hit.transform.forward.z);
			wall_grab_adjust.right			 = new Vector2(hit.transform.right.x, hit.transform.right.z);
			wall_grab_adjust.left			 = new Vector2(-hit.transform.right.x, hit.transform.right.z);
			wall_grab_adjust.player_forward	 = new Vector2(transform.forward.x, transform.forward.z);

            //--------壁との角度
            DotWithWall();

            //--------1番小さい角度算出
            float[] angle = new float[4] { wall_grab_adjust.angle_forward, wall_grab_adjust.angle_back, wall_grab_adjust.angle_right, wall_grab_adjust.angle_left };
            float	smallest_angle = Smallest(angle, 4);

            //左右のレイのめり込み具合
            float right_dist	 = WallGrabAdjust.BIG_VALUE;
            float left_dist		 = WallGrabAdjust.BIG_VALUE;
            if (Physics.Raycast(transform.position + transform.right * wall_grab_ray.side_length, transform.forward, out hit, wall_grab_ray.length))
            {
                right_dist = hit.distance;
            }
            if (Physics.Raycast(transform.position + transform.right * -wall_grab_ray.side_length, transform.forward, out hit, wall_grab_ray.length))
            {
                left_dist = hit.distance;
            }

            //向き調整
            if (left_dist < right_dist)
            {
                transform.localEulerAngles -= new Vector3(0, smallest_angle * 2, 0);
            }
            else transform.localEulerAngles += new Vector3(0, smallest_angle * 2, 0);
        }

    }

    //--------壁との角度
    void DotWithWall()
    {
		//壁の4方向との内積
		wall_grab_adjust.angle_forward	 = Vector2.Dot(wall_grab_adjust.player_forward, wall_grab_adjust.forward);
		wall_grab_adjust.angle_back		 = Vector2.Dot(wall_grab_adjust.player_forward, wall_grab_adjust.back);
		wall_grab_adjust.angle_right	 = Vector2.Dot(wall_grab_adjust.player_forward, wall_grab_adjust.right);
		wall_grab_adjust.angle_left		 = Vector2.Dot(wall_grab_adjust.player_forward, wall_grab_adjust.left);

        //角度に変換
        wall_grab_adjust.angle_forward	 = (wall_grab_adjust.angle_forward	 * 100.0f - 100.0f) * -1.0f * 0.9f;
		wall_grab_adjust.angle_back		 = (wall_grab_adjust.angle_back		 * 100.0f - 100.0f) * -1.0f * 0.9f;
		wall_grab_adjust.angle_right	 = (wall_grab_adjust.angle_right	 * 100.0f - 100.0f) * -1.0f * 0.9f;
		wall_grab_adjust.angle_left		 = (wall_grab_adjust.angle_left		 * 100.0f - 100.0f) * -1.0f * 0.9f;
    }

    //--------1番小さい値算出(他でも使うなら場所移動)
    float Smallest(float[] aaa, int max_num)
    {
        float smallest = WallGrabAdjust.BIG_VALUE;

        for (int i = 0; i < max_num; i++)
        {
            if (aaa[i] < smallest)
            {
                smallest = aaa[i];
            }
        }
        return smallest;
    }

    //----掴む
    void WallGrabRayGrab()
    {
        RaycastHit hit;

		if (!wall_grab_ray.flg) {
			return;
		}

		velocity.x = 0; //横移動したかったらここだけコメント
		velocity.y = 0;
		velocity.z = 0;
		rigid.useGravity = false;

		//横移動制限
		if (!Physics.Raycast(transform.position + transform.right * wall_grab_ray.side_length, transform.forward, out hit, wall_grab_ray.length)) {
			velocity.x = 0;
			wall_grab_ray.flg = false;
		}
		if (!Physics.Raycast(transform.position + transform.right * -wall_grab_ray.side_length, transform.forward, out hit, wall_grab_ray.length)) {
			velocity.x = 0;
			wall_grab_ray.flg = false;
		}

		if (WaitTimeOnce(wall_grab_ray.delay_time)) {
			//上入力で登る
			if (Input.GetAxis("L_Stick_V") < -0.5f || Input.GetKeyDown(KeyCode.UpArrow)) {
				transform.position += new Vector3(0, 4.5f, 1.0f);
				wall_grab_ray.flg = false;
			}
			//下入力で降りる
			else if (Input.GetAxis("L_Stick_V") > 0.5f || Input.GetKeyDown(KeyCode.DownArrow)) {
				wall_grab_ray.flg = false;
			}
		}

	}


	//先行入力まとめ
	void LeadKeyAll() {
		if (!lead_input.on) {
			return;
		}
		KeyServe();			//--先行キー保存
		KeyFrameSub();		//--先行キーframe減算処理
		LeadKeyChoice();	//--frameを元にキー選択
	}

	//--先行キー保存
	void KeyServe() {
		LeadkeyKind leadkey_kind;

		//入力から一時保存
		if (Input.GetButtonDown("Jump")) {
			leadkey_kind = LeadkeyKind.JUMP;
		}
		//else if (Input.GetButtonDown("Jump")) {
		//	leadkey_kind = LeadkeyKind.JUMP;
		//}
		else {
			leadkey_kind = 0;
		}

		//入力されていなければスキップ
		if (leadkey_kind == 0) {
			return;
		}
		//配列に保存
		for (int i = 0; i < LeadInput.NUM; ++i) {
			//既に値があればスキップ
			if (lead_inputs[i].pushed_key != 0) {
				continue;
			}
			lead_inputs[i].pushed_key = leadkey_kind;
			break;
		}
	}

	//--先行キーframe減算処理
	void KeyFrameSub() {
		for (int i = 0; i < LeadInput.NUM; i++) {
			//値があれば減算
			if (lead_inputs[i].pushed_key != 0) {
				lead_inputs[i].frame--;
			}
			//一定フレーム経ったら消去
			if (lead_inputs[i].frame <= 0) {
				lead_inputs[i].pushed_key = 0;
				lead_inputs[i].frame = LeadInput.KEY_SERVE_TIME;
			}
		}
	}

	//--frameを元にキー選択
	void LeadKeyChoice() {
		int frame_max = 0;
		lead_key = 0;

		for (int i = 0; i < LeadInput.NUM; i++) {
			//値が無ければスキップ
			if (lead_inputs[i].pushed_key == 0) {
				continue;
			}
			//直近で入力されたものを代入
			if (frame_max < lead_inputs[i].frame) {
				frame_max = lead_inputs[i].frame;
				lead_key  = lead_inputs[i].pushed_key;
			}
		}
	}



    //当たり判定 -----------------------------------------------
    private void OnCollisionEnter(Collision other)
    {
        //壁との当たり判定
        if (other.gameObject.tag == "Wall")
        {
            if (!wall_touch_flg)
            {
                wall_touch_flg = true;
            }
        }

		//敵との当たり判定
		if (other.gameObject.tag == "Enemy") {
			//踏みつけジャンプ中ではなく、敵が捕獲以外なら気絶
			if (!is_faint && !tread_on.flg && !other.gameObject.GetComponent<Enemy>().IsWrap) {
				is_faint = true;
			}
		}

	}

	private void OnCollisionExit(Collision other)
    {
		//壁との当たり判定
		if (other.gameObject.tag == "Wall")
        {
            if (wall_touch_flg == true)
            {
                wall_touch_flg = false;
            }
        }

    }

    // 物体に当たってるときに呼ばれる
    private void OnCollisionStay(Collision other)
    {
        // 床
        if (other.gameObject.tag == "Ground")
        {
            foot = (int)FOOT.GROUND;
        }
        // 水
        if (other.gameObject.tag == "Water")
        {
            foot = (int)FOOT.WATER;
        }
    }

	private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Coin")
        {
            coin_count++;
            Destroy(other.gameObject);
        }

    }

    //get ------------------------------------------------------------
    public float RunSpeed
    {
        get { return run_speed; }
    }

    public Vector3 TransformPosition
    {
        get { return transform.position; }
    }

    public int CoinCount
    {
        get { return coin_count; }
    }

    public int Foot
    {
        get { return foot; }
    }

}


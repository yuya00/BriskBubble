using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*              　　やること
    ジャンプしたときの上限あっておかしいやつをなおす　できやん

    ショットのスクリプトを分ける              やった
    ショットの中心に乗ったときのバグをなおす  やった   
 */

public partial class Player : CharaBase//
{
    private Vector3 axis;                   //入力値
    private Vector3 input;                  //入力値
    public GameObject[] shot_object;
    public float jump_power_up;          // ショットに乗ったときにジャンプ力を何倍にするか

    private float init_spd;               // 初期速度
    private float init_fric;              // 初期慣性STOP
    public float rotate = 1.0f;
    public float slope = 0.3f;           // スティックの傾き具合設定用
    private float jump_anim_count = 0;
    private const float NORMALIZE = 1.0f;
    private bool ray_fg;
	private bool wall_touch_flg = false;        //壁との当たり判定

	// あにめ
	private Animator animator;
    private float COUNT;
    private float anim_spd = 3.0f;

    // 状態
    private const int WAIT = 0;
    private const int WALK = 1;
    private const int RUN = 2;


    //--- カメラ用↓ ---//
    public GameObject cam;
    public float rot_spd = 10.0f;

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


	//壁掴み判定Ray ---------------------------------------------
	[System.Serializable]
	public struct WallGrabRay {

		[Header("Gizmoの表示")]
		public bool gizmo_on;

		[SerializeField, Range(0.0f, 2.0f), Header("Rayの高さ")]
		public float height;		//1.3f

		[SerializeField, Range(0.0f, 5.0f), Header("Rayの長さ")]
		public float length;		//2.0f

		[System.NonSerialized]	//掴み準備判定
		public bool prepare_flg;

		[System.NonSerialized]	//レイ判定
		public bool ray_flg;

		[System.NonSerialized]	//掴み判定
		public bool flg;

		[SerializeField, Range(0.0f, 2.0f), Header("横移動制限")]
		public float side_length;	//1.2f;

		[Header("入力待ち")]
		public int delaytime;	//10
	}
	[Header("壁掴み判定Ray")]
	public WallGrabRay wallGrabRay;


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

	[Header("プレイヤーGUIの表示")]
	public bool gui_on;




	// Start is called before the first frame update
	public override void Start()
    {
        base.Start();
        // 初期値設定
        init_spd = run_spd;
        init_fric = stop_fric;
        init_back_spd = back_spd;
        chara_ray = transform.Find("CharaRay");
        animator = GetComponent<Animator>();
        COUNT = 23 / anim_spd;
    }

    // Update is called once per frame
    void Update()
    {
        // 移動
        Move();

        // ショット
        Shot();

        // ジャンプアニメーション
        anime_jump();

        raydebug();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // 移動
        lstick_move();
    }

    private Vector2 leftScrollPos = Vector2.zero;
    void OnGUI()
    {
		if (gui_on) {
			GUILayout.BeginVertical("box", GUILayout.Width(190));

			// スクロール
			leftScrollPos = GUILayout.BeginScrollView(leftScrollPos, GUILayout.Width(180), GUILayout.Height(330));

			//debug();

			// スペース
			GUILayout.Space(10);

			////壁掴み判定
			//GUILayout.TextArea("壁との当たり判定\n " + wall_touch_flg.ToString());
			//GUILayout.TextArea("壁掴み準備判定\n " + wallGrabRay.prepare_flg.ToString());
			//GUILayout.TextArea("壁掴み判定\n " + wallGrabRay.flg.ToString());

			////縦入力
			//GUILayout.TextArea("L_Stick_V\n" + Input.GetAxis("L_Stick_V").ToString());

			//壁
			GUILayout.TextArea("壁前方向との内積\n" + wall_forward_angle.ToString());
			GUILayout.TextArea("壁後方向との内積\n" + wall_back_angle.ToString());
			GUILayout.TextArea("壁右方向との内積\n" + wall_right_angle.ToString());
			GUILayout.TextArea("壁左方向との内積\n" + wall_left_angle.ToString());

			GUILayout.TextArea("プレイヤーの角度\n" + transform.localEulerAngles.ToString());



			//スクロール終了
			GUILayout.EndScrollView();

			GUILayout.EndVertical();
		}
    }

	void debug()
    {
        GUILayout.TextArea("is_ground\n" + is_ground);
        GUILayout.TextArea("velocity\n" + velocity);
        GUILayout.TextArea("status.velocity\n" + status.velocity);
    }

	void OnDrawGizmos() {

		if (wallray.gizmo_on) {
			//壁判定Ray
			Gizmos.color = new Color(0.4f, 0.4f, 0.5f, 0.8f);
			Gizmos.DrawRay(transform.position, (transform.forward * angle_mag + transform.right).normalized * wallray.length);
			Gizmos.DrawRay(transform.position, (transform.forward * angle_mag + (-transform.right)).normalized * wallray.length);
		}

		if (holeray.gizmo_on) {
			//穴判定Ray
			Gizmos.color = new Color(0.4f, 0.4f, 0.5f, 0.8f);
			Gizmos.DrawRay(transform.position + (transform.forward * angle_mag + transform.right).normalized * wallray.length, -transform.up * holeray.length);
			Gizmos.DrawRay(transform.position + (transform.forward * angle_mag + (-transform.right)).normalized * wallray.length, -transform.up * holeray.length);
		}

		if (wallGrabRay.gizmo_on) {
			//壁掴み判定Ray
			Gizmos.color = new Color(0.5f, 0.0f, 1.0f, 0.8f);
			Gizmos.DrawRay(transform.position + new Vector3(0, wallGrabRay.height, 0), transform.forward * wallGrabRay.length);

			//--横移動制限Ray
			Gizmos.DrawRay(transform.position + transform.right * wallGrabRay.side_length, transform.forward * wallGrabRay.length);
			Gizmos.DrawRay(transform.position + transform.right * -wallGrabRay.side_length, transform.forward * wallGrabRay.length);
		}

	}


	void raydebug()
    {
        //*********************************************************************//
        Debug.DrawLine(chara_ray.position, chara_ray.position + Vector3.down * chara_ray_length, Color.red);

        // きっちり足元判定
        for (int i = 0; i < 9; ++i)
        {
            //Debug.DrawRay(chara_ray.position + ofset_layer_pos[i], chara_ray.position + ofset_layer_pos[i] + Vector3.down * (chara_ray_length * 0.5f), Color.green);
        }
        //*********************************************************************//
    }


	//---------------------------------------------
	// 移動                     
	//---------------------------------------------

	// 移動まとめ
	public override void Move()
    {
        base.Move();

        // 速度設定
        run_spd = init_spd;
        stop_fric = init_fric;

        //ジャンプ時の移動慣性
        if (is_ground) jump_fric = 1;
        else jump_fric = jump_fric_power;

        // バブル状態のとき
        if (shot_state > 1) bubble_spd(2.0f, 0.1f);

        // ショット3を撃った後プレイヤーをとめる
        if (back_player) back_move();

        //　着地してるときにジャンプ
        if (jump_on()) jump(jump_power);

        // ショットに乗った時にジャンプをjump_power_up倍
        if (down_hit_shot()) jump(jump_power * jump_power_up);


		////--壁判定による向き変更
		//WallRay_Rotate_Judge();

		////--穴判定による向き変更
		//HoleRay_Rotate_Judge();

		//--壁掴み判定Rayによる掴み
		WallGrabRay_Grab_Judge();

	}

    // カメラの正面にプレイヤーが進むようにする(横移動したときにカメラも移動するように)
    void lstick_move()
    {
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
        if (axis_x != 0f || axis_y != 0f) look_at(move);

        #region 状態分け
        switch (stick_state(axis_x, axis_y))
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
                velocity.x = move.normalized.x * walk_spd;
                velocity.z = move.normalized.z * walk_spd;
                animator.SetBool("Walk", true);
                animator.SetBool("Run", false);
                break;
            case RUN:
                // カメラから見てスティックを倒したほうへ進む
                velocity.x = move.normalized.x * run_spd;
                velocity.z = move.normalized.z * run_spd;
                animator.SetBool("Run", true);
                animator.SetBool("Walk", false);
                break;
        }

        #endregion
    }

    // その方向を向く
    void look_at(Vector3 vec)
    {
        Vector3 target_pos = transform.position + vec.normalized;
        Vector3 target = Vector3.Lerp(transform.position + transform.forward, target_pos, rot_spd * Time.deltaTime);
		if (!wallGrabRay.flg) {
			transform.LookAt(target);
		}
    }

    // スティックの倒し具合設定
    int stick_state(float x, float y)
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
    void bubble_spd(float multiply, float fric)
    {
        // 移動速度,慣性を上げる
        run_spd = init_spd * multiply;
        stop_fric = fric;
    }

    // ジャンプの挙動
    void jump(float jump_power)
    {
        rigid.useGravity = false;
        //is_ground = false;
        velocity.y = 0;
        velocity.y = jump_power;
        //animator.SetBool("JumpStart", jump_fg);
    }

    // ジャンプモーション用
    void anime_jump()
    {
        // 標準速度初期化
        //if (!jump_fg)
        animator.speed = 1.0f;
        //animator.SetBool("JumpStart", jump_fg);

        // 着地してるとき
        if (is_ground)
        {
            animator.SetBool("Fall", false);
            // 着地したときに1回だけ着地をtrueにする
            jump_end_anim();
        }

        // 落ちてる
        //if (fall())
        if (!is_ground)
        {
            jump_anim_count = 0;
            animator.SetBool("JumpStart", false);
            animator.SetBool("Fall", true);
            //animator.SetBool("JumpEnd", true);
        }
    }

    // 着地したときに1回だけ着地をtrueにする
    void jump_end_anim()
    {
        jump_anim_count++;
        // 地面ついたときにカウント(Fallと同時にfalseにしたらJumpEndまで来ないから少し間隔をあける)
        if (jump_anim_count < COUNT)
        {
            animator.SetBool("JumpEnd", true);
            animator.speed = anim_spd;
        }
        else
        {
            animator.SetBool("JumpEnd", false);
            jump_anim_count = COUNT;
        }
    }

    // 飛んでる判定
    bool jump_now()
    {
        if (velocity.y > 0) return true;
        return false;
    }

    // ジャンプする判定
    bool jump_on()
    {
        // モーションが終わってるときにジャンプできる
        if (is_ground)
        {
            if (!animator.GetBool("JumpEnd"))
            {
                if (Input.GetButtonDown("Jump"))
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
    bool fall()
    {
        // 落下判定
        if (velocity.y < 0.0f) return true;
        return false;
    }




	//--壁掴み判定Rayによる掴み
	void WallGrabRay_Grab_Judge() {
		//----当たり判定
		WallGrabRay_Judge();

		//----掴む
		WallGrabRay_Grab();
	}

	//----当たり判定
	void WallGrabRay_Judge() {
		RaycastHit hit;

		//空中にいる、自身が壁に当たっている、レイが当たっていない
		if (!is_ground && wall_touch_flg && !wallGrabRay.ray_flg) {
			wallGrabRay.prepare_flg = true;
		}
		else wallGrabRay.prepare_flg = false;

		//レイ判定
		if (Physics.Raycast(transform.position + new Vector3(0, wallGrabRay.height, 0), transform.forward, out hit, wallGrabRay.length) &&
			hit.collider.gameObject.tag == "Wall") {
			wallGrabRay.ray_flg = true;
		}
		else wallGrabRay.ray_flg = false;


		//上記二つが完了してたら掴む
		if (!wallGrabRay.flg && wallGrabRay.prepare_flg && wallGrabRay.ray_flg) {
			wallGrabRay.flg = true;
			//------掴んだ時の向き調整
			Angle_Adjust();
		}

	}

	//------掴んだ時の向き調整
	void Angle_Adjust() {
		RaycastHit hit;

		if (Physics.Raycast(transform.position + new Vector3(0, wallGrabRay.height, 0), transform.forward, out hit, wallGrabRay.length)) {
			//Vector2に保存
			wall_forward	 = new Vector2(hit.transform.forward.x, hit.transform.forward.z);
			wall_back		 = new Vector2(hit.transform.forward.x, -hit.transform.forward.z);
			wall_right		 = new Vector2(hit.transform.right.x, hit.transform.right.z);
			wall_left		 = new Vector2(-hit.transform.right.x, hit.transform.right.z);
			player_forward	 = new Vector2(transform.forward.x, transform.forward.z);

			//--------壁との角度
			Dot_With_Wall();

			//--------1番小さい角度算出
			float[] angle = new float[4] { wall_forward_angle, wall_back_angle, wall_right_angle, wall_left_angle };
			float smallest_angle = smallest(angle, 4);

			//左右のレイのめり込み具合
			float right_dist = NOTEXIST_BIG_VALUE;
			float left_dist = NOTEXIST_BIG_VALUE;
			if (Physics.Raycast(transform.position + transform.right * wallGrabRay.side_length, transform.forward, out hit, wallGrabRay.length)) {
				right_dist = hit.distance;
			}
			if (Physics.Raycast(transform.position + transform.right * -wallGrabRay.side_length, transform.forward, out hit, wallGrabRay.length)) {
				left_dist = hit.distance;
			}

			//向き調整
			if (left_dist < right_dist) {
				transform.localEulerAngles -= new Vector3(0, smallest_angle * 2, 0);
			}
			else transform.localEulerAngles += new Vector3(0, smallest_angle * 2, 0);
		}

	}

	//--------壁との角度
	void Dot_With_Wall() {
		//壁の4方向との内積
		wall_forward_angle = Vector2.Dot(player_forward, wall_forward);
		wall_back_angle = Vector2.Dot(player_forward, wall_back);
		wall_right_angle = Vector2.Dot(player_forward, wall_right);
		wall_left_angle = Vector2.Dot(player_forward, wall_left);

		//角度に変換
		wall_forward_angle = (wall_forward_angle * 100.0f - 100.0f) * -1.0f * 0.9f;
		wall_back_angle = (wall_back_angle * 100.0f - 100.0f) * -1.0f * 0.9f;
		wall_right_angle = (wall_right_angle * 100.0f - 100.0f) * -1.0f * 0.9f;
		wall_left_angle = (wall_left_angle * 100.0f - 100.0f) * -1.0f * 0.9f;
	}

	//--------1番小さい値算出(他でも使うなら場所移動)
	float smallest(float[] aaa, int max_num) {
		float smallest = NOTEXIST_BIG_VALUE;

		for (int i = 0; i < max_num; i++) {
			if (aaa[i] < smallest) {
				smallest = aaa[i];
			}
		}
		return smallest;
	}

	//----掴む
	void WallGrabRay_Grab() {
		RaycastHit hit;

		if (wallGrabRay.flg) {
			velocity.x = 0; //横移動したかったらここだけコメント
			velocity.y = 0;
			velocity.z = 0;

			//横移動制限
			if (!Physics.Raycast(transform.position + transform.right * wallGrabRay.side_length, transform.forward, out hit, wallGrabRay.length)) {
				velocity.x = 0;
				wallGrabRay.flg = false;
			}
			if (!Physics.Raycast(transform.position + transform.right * -wallGrabRay.side_length, transform.forward, out hit, wallGrabRay.length)) {
				velocity.x = 0;
				wallGrabRay.flg = false;
			}

			if (WaitTime_Once(wallGrabRay.delaytime)) {
				//上入力で登る
				if (Input.GetAxis("L_Stick_V") < -0.5f || Input.GetKeyDown(KeyCode.UpArrow)) {
					transform.position += new Vector3(0, 4.5f, 1.0f);
					wallGrabRay.flg = false;
				}
				//下入力で降りる
				else if (Input.GetAxis("L_Stick_V") > 0.5f || Input.GetKeyDown(KeyCode.DownArrow)) {
					wallGrabRay.flg = false;
				}
			}

		}
		else {
			wait_timer = 0;
		}

	}






	//---------------------------------------------//

	public override void Debug_Log()
    {
        base.Debug_Log();
    }

    public float Run_spd
    {
        get { return run_spd; }
    }



	//*******************************************
	// 当たり判定
	//*******************************************
	//何かに当たったとき
	void OnCollisionEnter(Collision col) {
		// 上方向に進んでる途中
		if (jump_now()) {
			// 頭当たった時に落下
			velocity.y = 0;
		}

		//壁との当たり判定
		if (col.gameObject.tag == "Wall") {
			if (wall_touch_flg == false) {
				wall_touch_flg = true;
			}
		}

	}

	//何にも当たっていないとき
	private void OnCollisionExit(Collision other) {
		if (other.gameObject.tag == "Wall") {
			if (wall_touch_flg == true) {
				wall_touch_flg = false;
			}
		}

	}


}

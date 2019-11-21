using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class Player : CharaBase
{
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
        respawn_pos = transform.position;
    }

    void Update()
    {
        // 移動
        Move();

        // ショット
        Shot();

        // ジャンプアニメーション
        anime_jump();

        Debug_Log();
        raydebug();
    }

    void raydebug()
    {
        //*********************************************************************//
        Debug.DrawLine(chara_ray.position, chara_ray.position + Vector3.down * chara_ray_length, Color.red);

        // きっちり足元判定
        for (int i = 0; i < 9; ++i)
        {
            //Debug.DrawRay(chara_ray.position + ofset_layer_pos[i], Vector3.down * (chara_ray_length * 0.5f), Color.green);
        }
        //*********************************************************************//
    }

    public override void Debug_Log()
    {
        /*
		base.Debug_Log();

		//*/
    }


    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // 移動
        lstick_move();
    }


    //GUI表示 -----------------------------------------------------
    private Vector2 leftScrollPos = Vector2.zero;   //uGUIスクロールビュー用
    void OnGUI()
    {
        if (gui_on)
        {
            GUILayout.BeginVertical("box", GUILayout.Width(190));
            leftScrollPos = GUILayout.BeginScrollView(leftScrollPos, GUILayout.Width(180), GUILayout.Height(330));
            GUILayout.Box("Player");


            #region ここに追加
            GUILayout.TextArea("fall_y\n" + fall_y);
            //GUILayout.TextArea("速さ\n" + velocity);

            ////壁掴み判定
            //GUILayout.TextArea("壁との当たり判定\n " + wall_touch_flg.ToString());
            //GUILayout.TextArea("壁掴み準備判定\n " + wallGrabRay.prepare_flg.ToString());
            //GUILayout.TextArea("壁掴み判定\n " + wallGrabRay.flg.ToString());

            //壁掴んだ瞬間
            //GUILayout.TextArea("壁前方向との内積\n" + wall_forward_angle.ToString());
            //GUILayout.TextArea("壁後方向との内積\n" + wall_back_angle.ToString());
            //GUILayout.TextArea("壁右方向との内積\n" + wall_right_angle.ToString());
            //GUILayout.TextArea("壁左方向との内積\n" + wall_left_angle.ToString());
            //GUILayout.TextArea("プレイヤーの角度\n" + transform.localEulerAngles.ToString());


            #endregion


            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }

    //ギズモ表示 --------------------------------------------------
    void OnDrawGizmos()
    {

        if (wallray.gizmo_on)
        {
            //壁判定Ray
            Gizmos.color = Color.green - new Color(0, 0, 0, 0.3f);
            Gizmos.DrawRay(transform.position, (transform.forward * angle_mag + transform.right).normalized * wallray.length);
            Gizmos.DrawRay(transform.position, (transform.forward * angle_mag + (-transform.right)).normalized * wallray.length);
        }

        if (holeray.gizmo_on)
        {
            //穴判定Ray
            Gizmos.color = Color.green - new Color(0, 0, 0, 0.3f);
            Gizmos.DrawRay(transform.position + (transform.forward * angle_mag + transform.right).normalized * wallray.length, -transform.up * holeray.length);
            Gizmos.DrawRay(transform.position + (transform.forward * angle_mag + (-transform.right)).normalized * wallray.length, -transform.up * holeray.length);
        }

        if (wallGrabRay.gizmo_on)
        {
            //壁掴み判定Ray
            Gizmos.color = Color.magenta - new Color(0, 0, 0, 0.2f);
            Gizmos.DrawRay(transform.position + new Vector3(0, wallGrabRay.height, 0), transform.forward * wallGrabRay.length);

            //--横移動制限Ray
            Gizmos.DrawRay(transform.position + transform.right * wallGrabRay.side_length, transform.forward * wallGrabRay.length);
            Gizmos.DrawRay(transform.position + transform.right * -wallGrabRay.side_length, transform.forward * wallGrabRay.length);
        }

    }

    // 移動 -------------------------------------------------------
    public override void Move()
    {
        base.Move();

        // 速度設定
        run_spd = init_spd;
        stop_fric = init_fric;

        //ジャンプ時の移動慣性
        if (is_ground)
        {
            jump_fric = 1;
            fall_y = transform.position.y;  // 着地してるときは自分のy位置を保存
        }
        else jump_fric = jump_fric_power;

        // バブル状態のとき
        if (shot_state > 1) bubble_spd(2.0f, 0.1f);

        // ショット3を撃った後プレイヤーをとめる
        if (back_player) back_move();

        //　着地してるときにジャンプ
        if (jump_on()) jump(jump_power);

        // ショットに乗った時にジャンプをjump_power_up倍
        if (down_hit_shot()) jump(jump_power * jump_power_up);

        // リスポーン
        fall_max();

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
        if (!wallGrabRay.flg)
        {
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

    void fall_max()
    {
        if(fall_check(fall_y,fall_y_max))
        {
            transform.position = respawn_pos;
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

    bool fall_check(float fall_y,float fall_y_max)
    {
        if (fall_y + transform.position.y < fall_y_max) return true;
        return false;
    }

    //--壁掴み判定Rayによる掴み
    void WallGrabRay_Grab_Judge()
    {
        //----当たり判定
        WallGrabRay_Judge();

        //----掴む
        WallGrabRay_Grab();
    }

    //----当たり判定
    void WallGrabRay_Judge()
    {
        RaycastHit hit;

        //空中にいる、自身が壁に当たっている、レイが当たっていない
        if (!is_ground && wall_touch_flg && !wallGrabRay.ray_flg)
        {
            wallGrabRay.prepare_flg = true;
        }
        else wallGrabRay.prepare_flg = false;

        //レイ判定
        if (Physics.Raycast(transform.position + new Vector3(0, wallGrabRay.height, 0), transform.forward, out hit, wallGrabRay.length) &&
            hit.collider.gameObject.tag == "Wall")
        {
            wallGrabRay.ray_flg = true;
        }
        else wallGrabRay.ray_flg = false;


        //上記二つが完了してたら掴む
        if (!wallGrabRay.flg && wallGrabRay.prepare_flg && wallGrabRay.ray_flg)
        {
            wallGrabRay.flg = true;
            //------掴んだ時の向き調整
            Angle_Adjust();
        }

    }

    //------掴んだ時の向き調整
    void Angle_Adjust()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position + new Vector3(0, wallGrabRay.height, 0), transform.forward, out hit, wallGrabRay.length))
        {
            //Vector2に保存
            wall_forward = new Vector2(hit.transform.forward.x, hit.transform.forward.z);
            wall_back = new Vector2(hit.transform.forward.x, -hit.transform.forward.z);
            wall_right = new Vector2(hit.transform.right.x, hit.transform.right.z);
            wall_left = new Vector2(-hit.transform.right.x, hit.transform.right.z);
            player_forward = new Vector2(transform.forward.x, transform.forward.z);

            //--------壁との角度
            Dot_With_Wall();

            //--------1番小さい角度算出
            float[] angle = new float[4] { wall_forward_angle, wall_back_angle, wall_right_angle, wall_left_angle };
            float smallest_angle = smallest(angle, 4);

            //左右のレイのめり込み具合
            float right_dist = NOTEXIST_BIG_VALUE;
            float left_dist = NOTEXIST_BIG_VALUE;
            if (Physics.Raycast(transform.position + transform.right * wallGrabRay.side_length, transform.forward, out hit, wallGrabRay.length))
            {
                right_dist = hit.distance;
            }
            if (Physics.Raycast(transform.position + transform.right * -wallGrabRay.side_length, transform.forward, out hit, wallGrabRay.length))
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
    void Dot_With_Wall()
    {
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
    float smallest(float[] aaa, int max_num)
    {
        float smallest = NOTEXIST_BIG_VALUE;

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
    void WallGrabRay_Grab()
    {
        RaycastHit hit;

        if (wallGrabRay.flg)
        {
            velocity.x = 0; //横移動したかったらここだけコメント
            velocity.y = 0;
            velocity.z = 0;

            //横移動制限
            if (!Physics.Raycast(transform.position + transform.right * wallGrabRay.side_length, transform.forward, out hit, wallGrabRay.length))
            {
                velocity.x = 0;
                wallGrabRay.flg = false;
            }
            if (!Physics.Raycast(transform.position + transform.right * -wallGrabRay.side_length, transform.forward, out hit, wallGrabRay.length))
            {
                velocity.x = 0;
                wallGrabRay.flg = false;
            }

            if (WaitTime_Once(wallGrabRay.delaytime))
            {
                //上入力で登る
                if (Input.GetAxis("L_Stick_V") < -0.5f || Input.GetKeyDown(KeyCode.UpArrow))
                {
                    transform.position += new Vector3(0, 4.5f, 1.0f);
                    wallGrabRay.flg = false;
                }
                //下入力で降りる
                else if (Input.GetAxis("L_Stick_V") > 0.5f || Input.GetKeyDown(KeyCode.DownArrow))
                {
                    wallGrabRay.flg = false;
                }
            }

        }
        else
        {
            wait_timer = 0;
        }
    }

    //当たり判定 -----------------------------------------------
    private void OnCollisionEnter(Collision other)
    {
        // 上方向に進んでる途中
        if (jump_now())
        {
            // 頭当たった時に落下
            velocity.y = 0;
        }

        //壁との当たり判定
        if (other.gameObject.tag == "Wall")
        {
            if (wall_touch_flg == false)
            {
                wall_touch_flg = true;
            }
        }

    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.tag == "Wall")
        {
            if (wall_touch_flg == true)
            {
                wall_touch_flg = false;
            }
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
    public float Run_spd
    {
        get { return run_spd; }
    }

    public Vector3 Transform_position
    {
        get { return transform.position; }
    }

    public int Coin_count
    {
        get { return coin_count; }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : CharaBase
{
    private Vector3 axis;                   //入力値
    private Vector3 input;                  //入力値
    public GameObject[] shot_object;
    public float jump_power_up;          // ショットに乗ったときにジャンプ力を何倍にするか

    private float init_spd;               // 初期速度
    private float init_fric;              // 初期慣性STOP
    private float fall_time;              // 落下判定用
    public float rotate = 1.0f;
    public float slope = 0.3f;           // スティックの傾き具合設定用
    private float jump_anim_count = 0;

    // あにめ
    private Animator animator;

    // 状態
    private const int WAIT = 0;
    private const int WALK = 1;
    private const int RUN = 2;

    //--- shot用↓ ---//
    private const float SHOT_POSITION = 2.8f;

    public int shot_state;             // debugでpublicにしてる
    private float charge_time;            // チャージ時間
    private float shot_interval_time;     // ショットの間隔
    public float shot_interval_time_max; // ショットを撃つまでの間隔
    private bool back_player;            // ショット3を撃った後にプレイヤーを後ろに飛ばす
    private float stop_time;              // 動けない時間
    public float stop_time_max;          // どれだけ動けないか
    public float back_spd = 0.5f;        // 後ろ方向に進む速度
    private float init_back_spd;          // 初期速度保存用

    //--- カメラ用↓ ---//
    public GameObject cam;
    public float rot_spd = 10.0f;

    //public Text txt;

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
    }

    bool fg = false;

    // Update is called once per frame
    void Update()
    {
        Move();
        Shot();

        if (is_ground)
        {
            fg = true;
        }
        else
        {
            fg = false;
        }
        debug();
        //Debug_Log();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // 移動
        lstick_move();

        // モーション
        anime_jump();
    }



    private Vector2 leftScrollPos = Vector2.zero;
    void OnGUI()
    {
        GUILayout.BeginVertical("box");

        // スクロールビュー
        leftScrollPos = GUILayout.BeginScrollView(leftScrollPos, GUILayout.Width(200), GUILayout.Height(400));

        GUILayout.TextArea("ジャンプフラグ\n" + fg);

        // スペース
        GUILayout.Space(10);

        GUILayout.TextArea("velocity.y\n" + velocity.y);

        // スペース
        GUILayout.Space(10);

        GUILayout.TextArea("地面着地\n" + is_ground);

        // スペース
        GUILayout.Space(10);

        // スペース
        GUILayout.Space(10);

        // スペース
        GUILayout.Space(10);

        //velocity.y += jump_power;
        //rigid.useGravity = false;
        //is_ground = false;

        //// jumpした後すぐは上に動いてるからそのときは当たり判定なくす
        //fall_time = 0;

        GUILayout.EndScrollView();

        GUILayout.EndVertical();
    }

    //---------------------------------------------//
    //                    移動                     //
    //---------------------------------------------//
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

        //　ジャンプ
        if (Input.GetButtonDown("Jump") && fg) jump(jump_power);

        // ショットに乗った時にジャンプをjump_power_up倍
        if (down_hit_shot()) jump(jump_power * jump_power_up);

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
        if (axis_length > 1.0f)
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
        transform.LookAt(target);
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
        velocity.y += jump_power;
        rigid.useGravity = false;
        is_ground = false;

        // jumpした後すぐは上に動いてるからそのときは当たり判定なくす
        fall_time = 0;
        animator.SetBool("JumpStart", true);
        //animator.SetBool("Jump", true);
    }

    // ジャンプモーション用
    void anime_jump()
    {
        // ジャンプしたとき
        if (jump_on())
        {
            animator.SetBool("JumpStart", true);
            animator.SetBool("Fall", true);
        }
        // 浮いてるとき
        if (!is_ground)
        {
            jump_anim_count = 0;
            animator.SetBool("JumpStart", false);
            animator.SetBool("Fall", true);
            animator.SetBool("Walk", false);
            animator.SetBool("Run", false);
        }
        // 着地してるとき
        if (is_ground)
        {
            animator.SetBool("JumpStart", false);
            animator.SetBool("Fall", false);
            // 着地したときに1回だけ着地をtrueにする
            jump_end_anim();
        }

    }

    // 着地したときに1回だけ着地をtrueにする
    void jump_end_anim()
    {
        // 地面ついたときにカウント
        if (jump_anim_count++ < 1)
        {
            animator.SetBool("JumpEnd", true);
        }
        else
        {
            animator.SetBool("JumpEnd", false);
        }
    }

    // ジャンプする判定
    bool jump_on()
    {
        if (Input.GetButtonDown("Jump") && is_ground) return true;
        return false;
    }

    void debug()
    {
        //animator.SetBool("JumpStart", true);
    }

    //---------------------------------------------//


    //---------------------------------------------//
    //                  ショット                   //
    //---------------------------------------------//
    void Shot()
    {
        // 次ショットまでの時間加算
        shot_interval();

        // 撃てるとき
        if (shot_interval_check())
        {
            // ショットの時間を固定
            shot_interval_time = shot_interval_time_max;

            // ショットのチャージ
            if (Input.GetButton("Shot"))
            {
                shot_charge();
            }

            // ショットの発射
            if (Input.GetButtonUp("Shot"))
            {
                // ショットをstateの値で選択
                shot_select(shot_object[shot_state]);

                // ショット、チャージリセット
                charge_time = 0;
                shot_state = 0;

                // ショット間隔の時間リセット
                shot_interval_time = 0;
            }
        }
    }

    // ショットの間隔
    void shot_interval()
    {
        // ショットを撃った後
        if (!shot_interval_check())
        {
            shot_interval_time += Time.deltaTime;
        }
    }

    // ショットが撃てるか
    bool shot_interval_check()
    {
        // ショットを撃った後、時間がたったら再度発射可能
        if (shot_interval_time >= shot_interval_time_max) return true;
        return false;
    }

    // ショットのチャージ
    void shot_charge()
    {
        // ショットのチャージ
        charge_time += Time.deltaTime;

        // ショットをstateで管理
        if (charge_time <= 1) shot_state = 0;
        if (charge_time > 1 && charge_time <= 2) shot_state = 1;
        if (charge_time > 2) shot_state = 2;
    }

    // ショットの選択
    void shot_select(GameObject obj)
    {
        // ショットのオブジェクトを設定
        GameObject shot = Instantiate(shot_object[shot_state], transform.position + (transform.forward * SHOT_POSITION), Quaternion.identity);

        switch (shot_state)
        {
            case 0:
                // 1段階目
                shot.GetComponent<Shot01>().SetCharacterObject(gameObject);
                break;
            case 1:
                // 2段階目
                shot.GetComponent<Shot02>().SetCharacterObject(gameObject);
                break;
            case 2:
                // 3段階目
                shot.GetComponent<Shot03>().SetCharacterObject(gameObject);
                back_spd = init_back_spd;   // 初期化
                back_player = true;
                break;
        }
    }

    // ショット3を撃った後、プレイヤーを後ろに飛ばす
    void back_move()
    {
        // 徐々に遅く
        if (back_spd > 0) back_spd -= 1.0f;

        // バックの速度をもとに後退
        velocity = -transform.forward * back_spd;

        // 待機時間が過ぎたらプレイヤーが動ける
        stop_time += Time.deltaTime;
        if (stop_time > stop_time_max)
        {
            // プレイヤーが操作可能になる
            back_player = false;
            stop_time = 0;
        }
    }
    //---------------------------------------------//

    //---------------------------------------------//
    //           ショットに乗ったとき              //
    //---------------------------------------------//
    // ショットに乗った判定
    bool down_hit_shot()
    {
        // ショットのレイヤーを指定
        LayerMask layer = 1 << 8;

        // 落下中判定
        if (fall())
        {
            // きっちり足元判定
            for (int i = 0; i < 9; ++i)
            {
                //下レイが当たっていたら着地
                if (Physics.Linecast(
                    chara_ray.position + ofset_layer_pos[i],
                    chara_ray.position + ofset_layer_pos[i] + Vector3.down * (chara_ray_length * 2.5f), layer))
                {
                    return true;
                }
            }
        }
        return false;
    }

    // 落下中判定
    bool fall()
    {
        // 地面についてる時は初期化
        if (is_ground) fall_time = 0;
        else fall_time += Time.deltaTime;

        // 落下時
        if (fall_time > 0.4f) return true;
        return false;
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


}

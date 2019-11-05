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
    // あにめ
    private Animator animator;
    private const int COUNT = 23;

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
        GUILayout.BeginVertical("box");

        // スクロールビュー
        leftScrollPos = GUILayout.BeginScrollView(leftScrollPos, GUILayout.Width(200), GUILayout.Height(400));

        debug();

        // スペース
        GUILayout.Space(10);

        // スペース
        GUILayout.EndScrollView();

        GUILayout.EndVertical();
    }

    void debug()
    {
        GUILayout.TextArea("is_ground\n" + is_ground);
        GUILayout.TextArea("velocity\n" + velocity);
        GUILayout.TextArea("status.velocity\n" + status.velocity);
    }

    void raydebug()
    {
        //*********************************************************************//
        Debug.DrawLine(chara_ray.position, chara_ray.position + Vector3.down * chara_ray_length, Color.red);

        // きっちり足元判定
        for (int i = 0; i < 9; ++i)
        {
            Debug.DrawRay(chara_ray.position + ofset_layer_pos[i], chara_ray.position + ofset_layer_pos[i] + Vector3.down * (chara_ray_length * 0.5f), Color.green);
        }
        //*********************************************************************//
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

        //　着地してるときにジャンプ
        if (jump_on()) jump(jump_power);

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
        rigid.useGravity = false;
        //is_ground = false;
        velocity.y = 0;
        velocity.y = jump_power;
        animator.SetBool("JumpStart", true);
        animator.SetBool("Fall", true);
        //animator.SetBool("JumpEnd", true);
    }

    // ジャンプモーション用
    void anime_jump()
    {
        // 着地してるとき
        if (is_ground)
        {
            animator.SetBool("JumpStart", false);
            animator.SetBool("Fall", false);

            // 着地したときに1回だけ着地をtrueにする
            jump_end_anim();
        }

        // 浮いてるとき
        if (!is_ground)
        {
            jump_anim_count = 0;
            animator.SetBool("JumpStart", false);
            animator.SetBool("Fall", true);
            animator.SetBool("JumpEnd", true);
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
        }
        else
        {
            jump_anim_count = COUNT;
            animator.SetBool("JumpEnd", false);
        }
    }

    // ジャンプする判定
    bool jump_on()
    {
        // モーションが終わってるときにジャンプできる
        if (Input.GetButtonDown("Jump") && is_ground && !animator.GetBool("JumpEnd")) return true;
        return false;
    }

    // 落下中判定
    bool fall()
    {
        // 落下判定
        if (velocity.y < 0.0f) return true;
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

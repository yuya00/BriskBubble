using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : CharaBase
{
    private const float SHOT_POSITION = 2.8f;

    private Vector3 axis;           //入力値
    private Vector3 input;          //入力値
    public GameObject[] shot_object;
    public float jump_power_up;

    private float init_spd;     // 初期速度
    private float init_fric;    // 初期慣性STOP
    private float fall_time;    // 落下判定用

    //--- shot用↓ ---//
    public int      shot_state;             // debugでpublicにしてる
    private float   charge_time;            // チャージ時間
    private float   shot_interval_time;     // ショットの間隔
    public float    shot_interval_time_max; // ショットを撃つまでの間隔
    private bool    stop_player;            // ショット3を撃った後にプレイヤーを後ろに飛ばす
    private float   stop_time;              // 動けない時間
    public float    stop_time_max;          // どれだけ動けないか
    public float    back_spd = 0.5f;        // 後ろ方向に進む速度
    private float   init_back_spd;          // 初期速度保存用

    public Text txt;

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

	GUIStyle style;
	GUIStyleState style_state;

	// Start is called before the first frame update
	public override void Start()
    {
        base.Start();
        // 初期値設定
        init_spd = run_spd;
        init_fric = stop_fric;
        init_back_spd = back_spd;
        chara_ray = transform.Find("CharaRay");

		style = new GUIStyle();
		style.fontSize = 15;

		style_state = new GUIStyleState();
		style_state.textColor = Color.gray;
		style.normal = style_state;

	}

    // Update is called once per frame
    void Update()
    {
        Move();
        Shot();

		//Debug_Log();
    }

	void OnGUI() {
		//float posx = Mathf.Round(transform.position.x * 100.0f)/100.0f;
		//GUI.Label(new Rect(0, 180, 1000, 500), "pos.x:" +posx.ToString(), style);
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
        if (stop_player) back_move();

        // 移動
        ground_move();

        //　ジャンプ
        if (Input.GetButtonDown("Jump") && is_ground)
        {
            jump(jump_power);
        }

        // ショットに乗った時にジャンプをjump_power_up倍
        if (down_hit_shot())
        {
            jump(jump_power * jump_power_up);
        }

        //txt.text = "sub : " + transform.position.y;
    }

    // 地面移動
    void ground_move()
    {
        //移動
        axis.x = Input.GetAxis("Horizontal");
        axis.z = Input.GetAxis("Vertical");
        input = new Vector3(axis.x, 0f, axis.z);

        #region 速さ代入
        //　方向キーが多少押されていたら
        if (input.magnitude > 0f)
        {
            // プレイヤーが止まってないとき
            if (!stop_player)
            {
                //向き変更
                transform.LookAt(transform.position + input);

                //慣性を考慮した速さ(等速)
                velocity.x = input.normalized.x * (run_spd * jump_fric);
                velocity.z = input.normalized.z * (run_spd * jump_fric);
            }
        }
        else
        {
            //停止時慣性(徐々に遅くなる)
            velocity.x -= velocity.x * stop_fric;
            velocity.z -= velocity.z * stop_fric;
        }
        #endregion
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
        if(!shot_interval_check())
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
        if (charge_time <= 1)                       shot_state = 0;
        if (charge_time > 1 && charge_time <= 2)    shot_state = 1;
        if (charge_time > 2)                        shot_state = 2;
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
                stop_player = true;
                break;
        }
    }

    // プレイヤーを後ろに飛ばす
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
            stop_player = false;
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
        if (is_ground)  fall_time = 0;
        else            fall_time += Time.deltaTime;

        // 落下時
        if(fall_time > 0.4f)     return true;
        return false;
    }

    public override void Debug_Log()
    {
        base.Debug_Log();
    }






	public float Run_spd {
		get { return run_spd; }
	}


}

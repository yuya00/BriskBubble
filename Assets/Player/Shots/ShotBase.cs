using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotBase : MonoBehaviour
{
    //----------------------------------------------------------//
    //                       変数宣言                           //
    //----------------------------------------------------------//
    protected GameObject chara_object;  // ショット撃つ本体
    protected Rigidbody rigid;          // 物理
    protected Vector3 forward;          // 正面方向
    protected float timer;              // 経過時間
    protected float spd_down_timer;     // 減速するまでの時間
    public float spd_down_timer_max;
    public float speed = 15;                   // 速度
    public int destroy_time = 3;            // 何秒たったら消すか

    public float down_spd;
    public float down_pos;

    public float timer_fg;              // 何秒経ったら判定を消すか
    public float timer_fg_max;          // 時間設定
    public bool apper_fg;               // 出現したときの判定
    protected Color col;                  // 色取得
    protected bool hit_fg;                // 消えてからは当たり判定なくす

    public int test;

    public Vector3 target_pos;　　　　　　//やまなりショットの着地位置

    protected GameObject player;

    protected EffectManager effect;
    // キャラ指定
    protected EffectManager.TYPE SHOT         = EffectManager.TYPE.SHOT;
    private EffectManager.EFFECT APPER      = EffectManager.EFFECT.APPER;
    protected EffectManager.EFFECT DESTROY    = EffectManager.EFFECT.DESTROY;
    private EffectManager.EFFECT TRAJECTORY = EffectManager.EFFECT.TRAJECTORY;

    // エフェクトの種類指定
    private enum NUM
    {
        APPER = 0,
        DESTROY,
        TRAJECTORY,
    }


    protected float PLR_SPD = 0.2f;

	//----------------------------------------------------------//

	

    //----------------------------------------------------------//
    //                         初期化                           //
    //----------------------------------------------------------//
    public virtual void Start()
    {
        spd_down_timer = 0;

        player = GameObject.FindGameObjectWithTag("Player");
        effect = GameObject.FindGameObjectWithTag("EffectManager").GetComponent<EffectManager>();

        // 物理取得
        rigid = this.GetComponent<Rigidbody>();

        // 正面方向（進む方向）取得
        forward = chara_object.transform.forward;

        // 判定用
        timer_fg = 0;

        // 最大時間設定
        timer_fg_max = 0.2f;

        // 出現フラグon
        apper_fg = true;

        // 当たり判定つける
        hit_fg = true;

        col = gameObject.GetComponent<Renderer>().material.color;
        col.a = 0.0f;

        effect.Effect(SHOT, APPER, transform.position, effect.apper_shot);
    }

    //----------------------------------------------------------//
    //                         更新　                           //
    //----------------------------------------------------------//
    public virtual void Update()
    {
        FgManager();
    }

    void FixedUpdate()
    {
        Effect();

    }

    void OnGUI()
    {
        //GUILayout.TextArea("apper_fg\n" + apper_fg);
        //GUILayout.TextArea("timer_fg\n" + timer_fg);
    }

    //----------------------------------------------------------//
    //           出現したときの判定をfalseに初期化              //
    //----------------------------------------------------------//
    void FgManager()
    {
        // 出現したとき、壊れたときだけ
        if (apper_fg) timer_fg += Time.deltaTime;

        // 条件でショットのインターバルタイムくらいでfalseにする
        if (timer_fg >= timer_fg_max)
        {
            apper_fg = false;
            timer_fg = 0;
        }

    }

    //----------------------------------------------------------//
    //                   オブジェクトセット                     //
    //----------------------------------------------------------//
    public virtual void SetCharacterObject(GameObject chara_object)
    {
        this.chara_object = chara_object;
    }

    //----------------------------------------------------------//
    //                  速度と位置を落とす                      //
    //----------------------------------------------------------//
    public virtual void Down(float down_spd,float down_pos)
    {
        // 速度あるときだけ速度落とす
        if (speed >= 0)
        {
            speed -= down_spd * Time.deltaTime;

            // 下に移動
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y - (down_pos * Time.deltaTime),
                transform.position.z);
        }
    }

    // エフェクトを出す
    void Effect()
    {
        if (speed >= 0)
        {
            if (!effect) return;
            effect.Effect(SHOT, TRAJECTORY, transform.position, effect.trajectory_shot);
        }
    }

    //----------------------------------------------------------//
    //                    ショットの演出                        //
    //----------------------------------------------------------//
    void Performance()
    {
        Destroy(gameObject);
    }

    //----------------------------------------------------------//
    //                   　ショット消去   　                     //
    //----------------------------------------------------------//
    public virtual void Destroy()
    {
        timer += Time.deltaTime;

        //一定時間経ったら透明にして、その後に時間はかって消す
        if (timer >= destroy_time)
        {
            // ショットが消えた判定
            apper_fg = true;

            // 当たり判定なくす
            hit_fg = false; 

            // 透明にする
            gameObject.GetComponent<Renderer>().material.color = col;

            // レイヤーで当たり判定なくす
            //gameObject.layer = LayerMask.NameToLayer("ShotDestroy");

            // +0.2fの時間で消去する
            if (timer >= destroy_time + 0.2f)
            {
                Destroy(gameObject);
                effect.Effect(SHOT, DESTROY, transform.position, effect.destroy_shot);
            }
        }
    }

    // 何かに当たった時に呼ばれる
    void OnTriggerEnter(Collider col)
    {
        // ショットが消えてなくて敵と当たった時
        if(hit_fg && col.tag == "Enemy")
        {
            Performance();
        }

        if(col.tag == "Player")
        {
            Destroy(gameObject);
        }
    }

    //----------------------------------------------------------//
    //          　  速度を下げるまでの時間を設定                //
    //----------------------------------------------------------//
    public virtual bool SpeedDownCheck(float time_max)
    {
        spd_down_timer += Time.deltaTime;

        // 時間たったらtrue
        if (spd_down_timer > time_max)
        {
            return true;
        }
        return false;
    }

    //----------------------------------------------------------//
    //     　 出現したときと壊れたときのフラグをゲット          //
    //----------------------------------------------------------//
    public bool ApperFg
    {
        get { return apper_fg; }
    }

}

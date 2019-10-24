using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotBase : MonoBehaviour
{
    protected GameObject chara_object;  // ショット撃つ本体
    protected Rigidbody rigid;          // 物理
    protected Vector3 forward;          // 正面方向
    protected float timer;              // 経過時間
    protected float spd_down_timer;     // 減速するまでの時間
    public float spd_down_timer_max;
    public float spd;                   // 速度
    public int destroy_time;            // 何秒たったら消すか

    public float down_spd;
    public float down_pos;

    public float timer_fg;  // 何秒経ったら判定を消すか
    public float timer_fg_max;  // 時間設定
    public bool destroy_fg; // ショットが消えた後判定
    public bool apper_fg;   // 出現したときの判定

    // Start is called before the first frame update
    public virtual void Start()
    {
        spd_down_timer = 0;

        // 取得
        rigid = this.GetComponent<Rigidbody>();

        // 正面方向（進む方向）取得
        forward = chara_object.transform.forward;

        // 判定用
        timer_fg = 0;

        // 最大時間設定
        timer_fg_max = 1;

        // 出現フラグon
        apper_fg = true;
    }

    // Update is called once per frame
    public virtual void Update()
    {
        fg_manager();
        //if(destroy_fg)
        //{
        //    fg_manager(destroy_fg);
        //}
    }

    void fg_manager()
    {
        timer_fg += Time.deltaTime;
        // 条件でショットのインターバルタイムくらいでfalseにする
        if (timer_fg > timer_fg_max)
        {
            apper_fg = false;
        }
    }

    public bool Apper_fg
    {
        get { return apper_fg; }
    }

    public virtual void SetCharacterObject(GameObject chara_object)
    {
        this.chara_object = chara_object;
    }

    // 速度と位置を落とす
    public virtual void down(float down_spd,float down_pos)
    {
        // 速度あるときだけ速度落とす
        if (spd >= 0)
        {
            spd -= down_spd * Time.deltaTime;

            // 下に移動
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y - (down_pos * Time.deltaTime),
                transform.position.z);
        }
    }

    // 速度を下げるまでの時間を設定
    public virtual bool spd_down_check(float time_max)
    {
        spd_down_timer += Time.deltaTime;

        // 時間たったらtrue
        if (spd_down_timer > time_max)
        {
            return true;
        }
        return false;
    }



    // ショット消去
    public virtual void Destroy()
    {
        timer += Time.deltaTime;

        //一定時間経ったら消去
        if (timer >= destroy_time)
        {
            destroy_fg = true;
            Destroy(gameObject);
        }
    }

}

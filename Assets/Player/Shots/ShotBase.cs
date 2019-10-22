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


    // Start is called before the first frame update
    public virtual void Start()
    {
        spd_down_timer = 0;
        rigid = this.GetComponent<Rigidbody>();
        forward = chara_object.transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        
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
            Destroy(gameObject);
        }
    }

}

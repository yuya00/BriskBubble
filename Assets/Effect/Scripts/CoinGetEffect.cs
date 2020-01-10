using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinGetEffect : MonoBehaviour
{
    private Vector3 front;
    private Vector3 pos;

    private int state;

    public float spd = 3.0f;
    public float slow_spd;
    private float scale_min = 0.1f; 		// エフェクトの下限

    // 大きさの範囲
    public float init_scale_max = 0.6f;
    public float init_scale_min = 0.2f;
    public float scale_spd = 0.3f;

    // 角度の範囲
    public float init_rot_max = 180;
    public float init_rot_min = 0;
    public float rot_x = 0;

    public float alpha_spd = 0.2f;  // 透明になる速さ
    private float destroy_timer;    // 壊れるまでの待機時間
    public float destroy_timer_max = 1.0f;

    public Vector3 rotation;

    void Start()
    {
        pos = transform.position;

        // 初期の大きさをランダムにする
        float rand_scale = Random.Range(init_scale_min, init_scale_max);
        transform.localScale = new Vector3(rand_scale, rand_scale, rand_scale);

        // 初期の角度
        float rand_rot = Random.Range(init_rot_min, init_rot_max);
        transform.Rotate(new Vector3(rot_x, rand_rot, rand_rot));

        // 角度を保存
        front = transform.forward;
        state = 0;
    }

    void Update()
    {
        Destroy();
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        switch (state)
        {
            case 0:
                Up();
                if (!SpeedCheck()) state++;
                break;
            case 1:
                Scale();
                Rotate();
                Down();
                break;
        }

    }

    // 上にあげる
    void Up()
    {
        spd -= slow_spd * Time.deltaTime;

        // 飛ばす
        transform.position -= (front + transform.forward.normalized) * (spd * Time.deltaTime);
    }

    // 下に落とす
    void Down()
    {
        spd += (slow_spd * 0.5f) * Time.deltaTime;
        pos.y = pos.y - 2;

        // 落とす方向
        Vector3 v1 = (pos - transform.position).normalized;

        transform.position -= v1 * (spd * Time.deltaTime);
    }

    void Rotate()
    {
        transform.Rotate(rotation * Time.deltaTime);
    }

    void Scale()
    {
        transform.localScale = new Vector3(
            transform.localScale.x + scale_spd * Time.deltaTime,
            transform.localScale.y + scale_spd * Time.deltaTime,
            transform.localScale.z + scale_spd * Time.deltaTime);
    }

    // 壊れる条件
    void Destroy()
    {
        // 時間経ったら消す
        destroy_timer += Time.deltaTime;
        if (destroy_timer > destroy_timer_max)
        {
            Destroy(gameObject);
            destroy_timer = 0;
        }

        // 小さくなったら消す
        if (transform.localScale.x < 0)
        {
            Destroy(gameObject);
        }
    }

    // 速度チェック
    bool SpeedCheck()
    {
        if (spd > 2) return true;
        return false;
    }
}

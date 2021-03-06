﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Shot03 : ShotBase
{
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        speed += player.GetComponent<Player>().RunSpeed * PLR_SPD;



        ////射出角度
        float angle = 50.0f;

        Vector3 velocity = CalVelocity(rigid.position, target_pos, angle);

        rigid.AddForce(velocity, ForceMode.Impulse);

    }

    // Update is called once per frame
    public override void Update()
    {
        Move();
        base.Update();
        Destroy();

    }

    void Move()
    {
        // 移動
        //rigid.velocity = forward * speed;

        // 速度を落とす
        //if (SpeedDownCheck(spd_down_timer_max)) Down(down_spd, down_pos);
    }

    public override void Destroy()
    {
        timer += Time.deltaTime;

        //一定時間経ったら透明にして、その後に時間はかって消す
        if (timer >= destroy_time+10)
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


    private Vector3 CalVelocity(Vector3 pointA, Vector3 pointB, float angle)
     {
    //角度をラジアンに変換
        float rad = angle * Mathf.PI / 180;

    //水平方向の距離x
        float x = Vector2.Distance(new Vector2(pointA.x, pointA.z), new Vector2(pointB.x, pointB.z));

    // 垂直方向の距離y
        float y = pointA.y - pointB.y;

       // 斜方投射の公式
        float speed = Mathf.Sqrt(-Physics.gravity.y * Mathf.Pow(x, 2) / (2 * Mathf.Pow(Mathf.Cos(rad), 2) * (x * Mathf.Tan(rad) + y)));

        return (new Vector3(pointB.x - pointA.x, x * Mathf.Tan(rad), pointB.z - pointA.z).normalized * speed);

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Shot03 : ShotBase
{
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        speed += player.GetComponent<Player>().RunSpeed * PLR_SPD;
    }

    // Update is called once per frame
    public override void Update()
    {
        Move();
        base.Update();
        base.Destroy();
        //Debug.Log(transform.position);
    }

    void Move()
    {
        // 移動
        rigid.velocity = forward * speed;

        // 速度を落とす
        if (SpeedDownCheck(spd_down_timer_max)) Down(down_spd, down_pos);
    }

}

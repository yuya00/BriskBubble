using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Shot03 : ShotBase
{
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        spd += player.GetComponent<Player>().Run_spd * PLR_SPD;
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
        rigid.velocity = forward * spd;

        // 速度を落とす
        if (spd_down_check(spd_down_timer_max)) down(down_spd, down_pos);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Shot01 : ShotBase
{

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        Move();
        base.Update();
        base.Destroy();
		//Debug.Log(transform.position);
    }

    // 移動
    void Move()
    {
        // 進む方向設定
        rigid.velocity = forward * spd;

        if (spd_down_check(spd_down_timer_max)) down(down_spd, down_pos);
    }



}

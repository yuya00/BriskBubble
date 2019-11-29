﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Shot02 : ShotBase
{
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        base.Destroy();

        if (player.GetComponent<Player>().ShotJumpFg)
        {
            Destroy(gameObject);
        }
    }

}

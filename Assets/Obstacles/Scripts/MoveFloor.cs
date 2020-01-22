﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveFloor : MonoBehaviour
{
    enum MOVE
    {
        GO = 0,
        WAIT1,
        BACK,
        WAIT2,

    }

    private MOVE state;     // 前進か後退か
    public float spd;       // 速度

    private Vector3 init_pos;   // 初期位置からどれだけ動くか
    public float len_max;           // この値分動いたら切り替え

    private float timer = 0;
    public float wait_timer = 1;

    public int type = 0;
    private Vector3 move_vector;

    private bool hit_obj;           // objが当たったか

    // Start is called before the first frame update
    void Start()
    {
        state = MOVE.GO;
        init_pos = transform.position;
        hit_obj = false;   
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    // 移動まとめ
    void Move()
    {
        switch(state)
        {
            case MOVE.GO:
                SpeedCheck(spd);            // 速度調整
                ChangeState(MOVE.WAIT1);    // ステート管理
                break;
            case MOVE.WAIT1:
                TimerCheck(MOVE.BACK);      // 待機時間
                break;
            case MOVE.BACK:
                SpeedCheck(-spd);
                ChangeState(MOVE.WAIT2);
                break;
            case MOVE.WAIT2:
                TimerCheck(MOVE.GO);
                break;
        }
        Debug.Log(timer);
    }

    // 速度設定
    void SpeedCheck(float spd)
    {
        float move = spd * Time.deltaTime;

        // 正面or縦に移動
        switch(type)
        {
            case 0: // 縦に動く
                move_vector = transform.up.normalized * move;
                transform.position += move_vector;
                break;
            case 1: // 正面に動く
                move_vector = transform.forward.normalized * move;
                transform.position += move_vector;
                break;
        }
    }

    // 切り替え
    void ChangeState(MOVE state)
    {
        float l1 = (transform.position - init_pos).magnitude;
        if (l1 > len_max) this.state = state;
    }

    // 時間で切り替え
    void TimerCheck(MOVE state)
    {
        // 加算
        timer += Time.deltaTime;

        // 時間がおかしかったから、加算してる分で差分とって合わせた
        if(timer >= wait_timer - Time.deltaTime)
        {
            timer = 0;
            this.state = state;
        }
    }

    /*
     * 当たり判定で当たったものの位置を
     * 床のうえにプラスしたらいけるかも
     * 床に当たった位置を移動する位置にしたら
     */

    private void OnCollisionEnter(Collision other)
    {
        // プレイヤーが当たった
        if (other.gameObject.name == "Player")
        {
            hit_obj = true;
        }
    }

    private void OnCollisionExit(Collision other)
    {
        // プレイヤーが当たった
        if (other.gameObject.name == "Player")
        {
            hit_obj = false;
        }
    }

    // 物体に当たってるときに呼ばれる
    private void OnCollisionStay(Collision other)
    {
    }

    public bool Hit
    {
        get { return hit_obj; }
    }        

    public Vector3 MoveVector
    {
        get { return move_vector; }
    }

}

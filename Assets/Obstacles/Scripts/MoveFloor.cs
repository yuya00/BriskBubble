using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveFloor : MonoBehaviour
{
    enum MOVE
    {
        GO = 0,
        BACK,
    }

    private MOVE state;     // 前進か後退か
    public float spd;       // 速度

    private Vector3 init_pos;   // 初期位置からどれだけ動くか
    public float len_max;           // この値分動いたら切り替え


    // Start is called before the first frame update
    void Start()
    {
        state = MOVE.GO;
        init_pos = transform.position;
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
                SpeedCheck(spd);
                ChangeState(MOVE.BACK);
                break;
            case MOVE.BACK:
                SpeedCheck(-spd);
                ChangeState(MOVE.GO);
                break;
        }
    }

    // 速度設定
    void SpeedCheck(float spd)
    {
        float my = spd * Time.deltaTime;

        // 正面に移動
        transform.position += transform.up.normalized * my;
    }

    // 切り替え
    void ChangeState(MOVE state)
    {
        float l1 = (transform.position - init_pos).magnitude;
        if (l1 > len_max) this.state = state;
    }

    /*
     * 当たり判定で当たったものの位置を
     * 床のうえにプラスしたらいけるかも
     * 床に当たった位置を移動する位置にしたら
     */

}

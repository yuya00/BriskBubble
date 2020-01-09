using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Focus : MonoBehaviour
{
    private const int END_PRODACTION = 2;

    private GameObject effect;

    private int state;

    public float spd = 2;
    private float init_spd;
    private float timer;
    public float timer_max = 0.5f;

    private Vector3 focus_pos;

    // Start is called before the first frame update
    void Start()
    {
        state = 0;
        init_spd = spd * 0.05f;
        timer = 0;

        // 位置情報取得
        effect = GameObject.FindGameObjectWithTag("EffectManager");
        focus_pos = effect.GetComponent<EffectManager>().Focus_pos;
    }

    // Update is called once per frame
    void Update()
    {
        // 動きまとめ
        Move();

        // 削除処理
        if ((focus_pos - transform.position).magnitude < 1.0f) Destroy(gameObject);
    }

    void Move()
    {
        StateChange();

        switch (state)
        {
            case 0:// spdを-にして欠片を反対側に
                Focusing(focus_pos, -init_spd);
                break;
            case 1:// 集束させる
                Focusing(focus_pos, spd);
                break;
        }
    }

    void StateChange()
    {
        timer += Time.deltaTime;
        if(timer > timer_max)
        {
            state = 1;
        }
    }

    // 集束
    void Focusing(Vector3 pos, float spd)
    {
        Vector3 v1 = (pos - transform.position).normalized;
        transform.position = transform.position + v1 * (spd * Time.deltaTime);
    }

}

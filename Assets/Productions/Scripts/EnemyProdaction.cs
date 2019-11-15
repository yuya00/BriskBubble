using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProdaction : MonoBehaviour
{
    [Header("カメラGUIの表示")]
    public bool gui_on;

    public GameObject obj;
    public GameObject prodaction_shot;

    private Vector3 init_pos;

    public float big_spd = 0.5f;
    public float small_spd = 2.0f;

    private const float NULL = 0.0f;
    private bool scale_chenge_on = false;
    private int state;

    private float timer = 0;
    public float timer_max = 1;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // ショットが敵に当たった時に実行
        if (obj.GetComponent<Enemy>().Shot_touch_flg) scale_chenge_on = true;

        // スケール変更
        if (scale_chenge_on)
        {
            // 子の演出用ショットを出現
            prodaction_shot.SetActive(true);
            scale();
        }

        // 位置修正
        //pos_check();
    }

    // スケール変更まとめ
    void scale()
    {
        switch (state)
        {
            case 0:
                scale_chenge(big_spd);
                state_chenge(timer_max);// 待機時間
                break;
            case 1:
                scale_chenge(-small_spd);
                destroy();              // 削除処理
                break;
        }
    }

    // 時間経ったらステート変更
    void state_chenge(float timer_max)
    {
        timer += Time.deltaTime;
        if (timer > timer_max)
        {
            timer = 0;
            state++;
        }
    }

    // 大きさを変える
    void scale_chenge(float spd)
    {
        transform.localScale = new Vector3(
            transform.localScale.x + spd * Time.deltaTime,
            transform.localScale.y + spd * Time.deltaTime,
            transform.localScale.z + spd * Time.deltaTime);
    }

    // 位置修正
    void pos_check()
    {
        //transform.position = obj.transform.position;

        obj.transform.position = transform.position;
    }

    // 削除処理
    void destroy()
    {
        if (transform.localScale.x < NULL)
        {
            state = 0;
            scale_chenge_on = false;
            Destroy(gameObject);
        }
    }

    public int State
    {
        get { return state; }
    }

    void OnGUI()
    {
        if (gui_on)
        {
            GUILayout.BeginVertical("box");

            //uGUIスクロールビュー用
            Vector2 leftScrollPos = Vector2.zero;

            // スクロールビュー
            leftScrollPos = GUILayout.BeginScrollView(leftScrollPos, GUILayout.Width(200), GUILayout.Height(400));

            GUILayout.TextArea("init_pos\n" + init_pos);
            GUILayout.TextArea("transform.position\n" + transform.position);
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);     
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);     
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);     
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);     

            // スペース
            GUILayout.Space(200);
            GUILayout.Space(10);

            // スペース
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProdaction : MonoBehaviour
{
    [Header("カメラGUIの表示")]
    public bool gui_on;

    public GameObject obj;
    public GameObject prodaction_shot;

    private Vector3 save_pos;

    public float big_spd = 0.5f;
    public float small_spd = 2.0f;

    private const float NULL = 0.0f;
    private bool scale_chenge_on = false;
    private int state;

    private float timer = 0;
    public float timer_max = 1;

    private EffectManager effect;   // エフェクト用
    private Vector3 focus_pos;  // 集束位置


    // Start is called before the first frame update
    void Start()
    {
        save_pos = Vector3.zero;
        effect = GameObject.FindGameObjectWithTag("EffectManager").GetComponent<EffectManager>();
    }

    // Update is called once per frame
    void Update()
    {
        //敵の耐久度がなくなった時に実行
        if (obj.GetComponent<Enemy>().ShotToDefense <= 0)
        {
            scale_chenge_on = true;

            // どこに集束するかを設定
            focus_pos = transform.position;

            // 位置修正
            PosCheck();
        }

        //// ショットが敵に当たった時に実行
        //if (obj.GetComponent<Enemy>().ShotTouchFlg)
        //      {
        //          scale_chenge_on = true;
        //          // 位置修正
        //          PosCheck();
        //      }

        // スケール変更
        if (scale_chenge_on)
        {
            // 子の演出用ショットを出現
            prodaction_shot.SetActive(true);
            Scale();
        }


    }

    //void FixedUpdate()    {    }

    // スケール変更まとめ
    void Scale()
    {
        switch (state)
        {
            case 0:
                ScaleChenge(big_spd);
                StateChenge(timer_max);// 待機時間
                break;
            case 1:
                ScaleChenge(-small_spd);
                Destroy();              // 削除処理
                break;
        }
    }

    // 時間経ったらステート変更
    void StateChenge(float timer_max)
    {
        timer += Time.deltaTime;
        if (timer > timer_max)
        {
            timer = 0;
            state++;
        }
    }

    // 大きさを変える
    void ScaleChenge(float spd)
    {
        transform.localScale = new Vector3(
            transform.localScale.x + spd * Time.deltaTime,
            transform.localScale.y + spd * Time.deltaTime,
            transform.localScale.z + spd * Time.deltaTime);
    }

    // 位置修正
    void PosCheck()
    {
        transform.position = obj.transform.position;
        obj.transform.position = transform.position;
    }

    // キャラ指定
    private EffectManager.TYPE ENEMY = EffectManager.TYPE.ENEMY;

    // エフェクトの種類指定
    private EffectManager.EFFECT EXPLOSION = EffectManager.EFFECT.EXPLOSION;
    private EffectManager.EFFECT FOCUSING = EffectManager.EFFECT.FOCUSING;

    // 削除処理
    void Destroy()
    {
        if (transform.localScale.x < NULL)
        {
            scale_chenge_on = false;

            // エフェクトの出現
            effect.Effect(ENEMY, EXPLOSION, transform.position, effect.explosion_effect);
            effect.Effect(ENEMY, FOCUSING, transform.position, effect.focusing_effect);

            state = 0;
            Destroy(gameObject);
        }
    }

    public int State
    {
        get { return state; }
    }

    // 集束位置渡す
    public Vector3 Focus_pos
    {
        get { return focus_pos; }
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

            GUILayout.TextArea("位置\n" + transform.position);
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

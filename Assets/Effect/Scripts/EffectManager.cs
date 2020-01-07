using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixeye.Unity;

public sealed partial class EffectManager : MonoBehaviour
{

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        enemy = GameObject.FindGameObjectsWithTag("ProdactionEnemy");

        // 待機時間の初期化
        for (int i = 0; i < (int)TYPE.SHOT; ++i)
        {
            app_timer_type[i] = 0;
        }
    }

    void Update()
    {
    }


    // エフェクト--------------------------------------------------------
    public void Effect(TYPE type, EFFECT state, Vector3 pos, int num = 0)
    {
        // typeに値が入ってる
        switch (type)
        {
            case TYPE.PLAYER:
                Player(state, pos, num);
                break;
            case TYPE.ENEMY:
                Enemy(state, pos, num);
                break;
            case TYPE.SHOT:
                Shot(state, pos, num);
                break;
        }
    }


    // プレイヤーのエフェクト--------------------------------------------
    void Player(EFFECT state, Vector3 pos, int num)
    {
        debug_state = state;
        // stateでどのエフェクトかを決める
        switch (state)
        {
            case EFFECT.JUMP:
                EffectSet(effect_jump, pos, num);
                break;
            case EFFECT.RUN:
                PlayerRun((RUN)GetFoot(), pos);
                break;
            case EFFECT.SHOT:
                EffectSet(effect_shot, pos, num);
                break;
        }
    }

    // 位置情報をもらって、出す物を変える
    void PlayerRun(RUN state, Vector3 pos)
    {
        int num = 0;
        switch (state)
        {
            case RUN.GROUND:
                num = run_ground_player;
                // 何フレーム置きに出現するか
                if (WaitCheck(TYPE.PLAYER, run_ground_timer_player))
                {
                    EffectSet(effect_run_ground, pos, num);
                }
                break;
            case RUN.WATER:
                num = run_water_player;
                // 何フレーム置きに出現するか
                if (WaitCheck(TYPE.PLAYER, run_water_timer_player))
                {
                    EffectSet(effect_run_water, pos, num);
                }
                break;
        }
    }

    // プレイヤーの足元情報をもらう
    int GetFoot()
    {
        return player.GetComponent<Player>().Foot;
    }


    // エネミーのエフェクト----------------------------------------------
    void Enemy(EFFECT state, Vector3 pos, int num)
    {
        switch (state)
        {
            case EFFECT.EXPLOSION:
                EffectSet(effect_explosion, pos, num);
                break;
            case EFFECT.FOCUSING:
                // 集束位置
                focus_pos = pos;

                // 爆発の欠片位置設定
                DebrisSet(pos, num);

                break;
        }
    }

    // 爆発の欠片位置設定
    void DebrisSet(Vector3 pos, int num)
    {
        /*
         最初の位置をRGEみたいに角度で出す
         その角度の延長線上にオブジェクト配置する
         */
        for (int n = 0; n < FOCUS_NUM; ++n)
        {
            pos.x = pos.x + (Random.Range(-FOCUS_NUM, FOCUS_NUM));
            pos.y = pos.y + (Random.Range(-FOCUS_NUM, FOCUS_NUM));
            pos.z = pos.z + (Random.Range(-FOCUS_NUM, FOCUS_NUM));

            EffectSet(effect_focusing, pos, num);
        }
    }

    public Vector3 Focus_pos
    {
        get { return focus_pos; }
    }


    // ショットのエフェクト----------------------------------------------
    void Shot(EFFECT state, Vector3 pos, int num)
    {
        switch (state)
        {
            case EFFECT.APPER:
                EffectSet(effect_trajectory, pos, num);
                break;
            case EFFECT.DESTROY:
                EffectSet(effect_trajectory, pos, num);
                break;
            case EFFECT.TRAJECTORY:
                // 何フレーム置きに出現するか
                if (WaitCheck(TYPE.SHOT, trajectory_timer_shot))
                {
                    EffectSet(effect_trajectory, pos, num);
                }
                break;
        }
    }


    // 実際にエフェクトを出す処理----------------------------------------
    void EffectSet(GameObject effect, Vector3 pos, int num)
    {
        if (!effect) return;
        for (int i = 0; i < num; ++i)
        {
            Instantiate(effect, pos, effect.transform.rotation);
        }
    }


    // 待機時間----------------------------------------------------------
    bool WaitCheck(TYPE no, float timer_max)
    {
        // 何フレーム置きに出現するか
        app_timer_type[(int)no] += Time.deltaTime;
        if (app_timer_type[(int)no] > timer_max)
        {
            app_timer_type[(int)no] = 0;
            return true;
        }
        return false;
    }

    public bool gui_on;
    // GUI---------------------------------------------------------------
    private Vector2 left_scroll_pos = Vector2.zero;   //uGUIスクロールビュー用
    private float scroll_height = 330;
    void OnGUI()
    {
        if (!gui_on)
        {
            return;
        }

        //スクロール高さを変更
        //(出来ればmaximize on playがonならに変更したい)
        GUILayout.BeginVertical("box", GUILayout.Width(190));
        left_scroll_pos = GUILayout.BeginScrollView(left_scroll_pos, GUILayout.Width(180), GUILayout.Height(scroll_height));
        GUILayout.Box("Effect");

        //着地判定
        GUILayout.TextArea("focus_pos\n" + focus_pos);
        //GUILayout.TextArea("debug_type\n" + debug_type);
        //GUILayout.TextArea("debug_state\n" + debug_state);
        //GUILayout.TextArea("debug_pos\n" + debug_pos);
        //GUILayout.TextArea("debug_obj\n" + debug_obj);
        //GUILayout.TextArea("debug_obj\n" + debug_obj);
        //GUILayout.TextArea("debug_obj\n" + debug_obj);
        //GUILayout.TextArea("debug_obj\n" + debug_obj);


        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }
}


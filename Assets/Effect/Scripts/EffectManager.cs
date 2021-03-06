﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixeye.Unity;

public partial class EffectManager : MonoBehaviour
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
            case TYPE.UI:
                UI(state, pos, num);
                break;
        }
    }


    // プレイヤーのエフェクト--------------------------------------------
    void Player(EFFECT state, Vector3 pos, int num)
    {
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
            case EFFECT.COIN:
                for(int i = 0;i < 3;++i)
                {
                    EffectSet(effect_coin_get, new Vector3(pos.x, pos.y - 1.5f + (i), pos.z), num);
                }
                break;
        }
    }

    // 位置情報をもらって、出す物を変える
    void PlayerRun(RUN state, Vector3 pos)
    {
        int num = 0;
        float rand = 0.5f;
        switch (state)
        {
            case RUN.NONE:
                break;
            case RUN.GROUND:
                num = run_ground_player;
                // 何フレーム置きに出現するか
                if (WaitCheck(TYPE.PLAYER, run_ground_timer_player))
                {
                    GroundEffect(pos, num);
                }
                break;
            case RUN.WATER:
                num = run_water_player;
                // 何フレーム置きに出現するか
                if (WaitCheck(TYPE.PLAYER, run_water_timer_player))
                {
                    for (int i = 0; i < num; ++i)
                    {
                        pos = new Vector3(pos.x + Random.Range(-rand, rand), pos.y, pos.z + Random.Range(-rand, rand));
                        EffectSet(effect_run_water, new Vector3(pos.x, pos.y + 0.2f, pos.z), 1);
                    }
                }
                break;
        }
        debug_state = state;
    }

    void GroundEffect(Vector3 pos, int num)
    {
        Vector3 front = player.GetComponent<Player>().Front;
        float rand_x = 0.3f, rand_z = 0.5f;

        // いっきにnum個のeffectを出す
        for (int i = 0; i < num; ++i)
        {
            // 生成する物体、生成場所、回転軸の設定
            pos = new Vector3(
                    pos.x + Random.Range(-rand_x, rand_x),
                    pos.y,
                    pos.z + Random.Range(-rand_z, rand_z));
            EffectSet(effect_run_ground, pos, 1);
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
                DebrisSet(focus_pos, num);

                break;
        }
    }

    // 爆発の欠片位置設定
    void DebrisSet(Vector3 pos, int num)
    {
        // 送られてきたnumの数だけループ
        for (int n = 0; n < num; ++n)
        {
            // 位置の設定
            pos = new Vector3(pos.x + x[data_no_x], pos.y + 0.3f, pos.z + 0.5f + z[data_no_z]);

            // データの値を交互に使う
            data_no_x++;
            data_no_z++;
            if (data_no_x > max_focus_data) data_no_x = 0;
            if (data_no_z > max_focus_data) data_no_z = 0;

            // 1個だけ出す
            EffectSet(effect_focusing, new Vector3(pos.x - (data_focus_z * 0.5f), pos.y - 1, pos.z), 1);
        }

        // 初期化
        data_no_x = 0;
        data_no_z = 0;
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

    // UIのエフェクト----------------------------------------------------
    void UI(EFFECT state, Vector3 pos, int num)
    {
        // stateでどのエフェクトかを決める
        switch (state)
        {
            case EFFECT.UI_FLASH:
                UIEffectSet(pos, num);
                break;
        }
    }

    void UIEffectSet(Vector3 pos, int num)
    {
        int[] pos_data_x = { 1, -1, 0, 1, -1 };
        int[] pos_data_y = { 1, 1, 0, -1, -1 };

        for (int i = 0; i < num; ++i)
        {
            pos = new Vector3(pos.x + pos_data_x[i], pos.y + pos_data_y[i], pos.z);
            EffectSet(effect_ui, pos, 1);
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
        GUILayout.TextArea("GetFoot\n" + GetFoot());
        GUILayout.TextArea("debug_state\n" + debug_state);
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


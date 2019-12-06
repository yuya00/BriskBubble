using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixeye.Unity;

public class EffectManager : MonoBehaviour
{
    // エフェクトを出すobjの種類
    public enum TYPE
    {
        PLAYER = 0,
        ENEMY,
        SHOT,
    }

    // エフェクトを出す上でのSTATE
    public enum EFFECT
    {
        JUMP = 0,
        RUN,
        SHOT,
    }

    // RUNで地上情報に合わせてエフェクトを分ける
    public enum RUN
    {
        GROUND = 0,
        WATER,
    }

    // エフェクトを使うTYPEをきめる(呼び出すobjで使う)
    //[System.NonSerialized] public TYPE PLAYER_EFFECT = TYPE.PLAYER;
    [System.NonSerialized] public TYPE ENEMY_EFFECT = TYPE.ENEMY;
    [System.NonSerialized] public TYPE SHOT_EFFECT = TYPE.SHOT;


    //[Foldout("Effect", true)]

    public GameObject effect_jump;      // jump時のエフェクト
    public GameObject effect_run;       // run時のエフェクト  
    public GameObject effect_shot;      // shot時のエフェクト

    //[Foldout("Effect", false)]


    // エフェクト-----------------------------------------------------
    public void Effect(TYPE type, EFFECT state, Vector3 pos, int num)
    {
        // typeに値を入れる
        switch (type)
        {
            case TYPE.PLAYER:
                Player(state, pos, num);
                break;
            case TYPE.ENEMY:
                Enemy(state, pos);
                break;
            case TYPE.SHOT:
                Shot(state, pos);
                break;
        }

    }

    // プレイヤーのエフェクト----------------------------------------------
    void Player(EFFECT state, Vector3 pos, int num)
    {
        // typeに値を入れる
        switch (state)
        {
            case EFFECT.JUMP:
                EffectSet(effect_jump, pos, num);
                break;
            case EFFECT.RUN:
                //PlayerRun(pos, EFFECT_NUM);
                break;
            case EFFECT.SHOT:
                EffectSet(effect_shot, pos, num);
                break;
        }
    }

    // 位置情報をもらって、オブジェクトをだすものを変える
    void PlayerRun(RUN state, Vector3 pos, int num)
    {
        switch (state)
        {
            case RUN.GROUND:
                EffectSet(effect_jump, pos, num);
                break;
            case RUN.WATER:
                EffectSet(effect_jump, pos, num);
                break;
        }
    }

    // エネミーのエフェクト----------------------------------------------
    void Enemy(EFFECT state, Vector3 pos)
    {
    }

    // ショットのエフェクト----------------------------------------------
    void Shot(EFFECT state, Vector3 pos)
    {
    }

    // 実際にエフェクトを出す処理----------------------------------------
    public void EffectSet(GameObject effect, Vector3 pos, int num)
    {
        for (int i = 0; i < num; ++i) Instantiate(effect, pos, effect.transform.rotation);
    }


	public bool gui_on;
    // GUI---------------------------------------------------------------
    private Vector2 left_scroll_pos = Vector2.zero;   //uGUIスクロールビュー用
    private float scroll_height = 330;
    void OnGUI()
    {
		if (!gui_on) {
			return;
		}

        //スクロール高さを変更
        //(出来ればmaximize on playがonならに変更したい)
        GUILayout.BeginVertical("box", GUILayout.Width(190));
        left_scroll_pos = GUILayout.BeginScrollView(left_scroll_pos, GUILayout.Width(180), GUILayout.Height(scroll_height));
        GUILayout.Box("Effect");

        //着地判定
        //GUILayout.TextArea("debug_obj\n" + debug_obj);
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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixeye.Unity;

/*
    [Foldout("EffectObject", true)]
    [Foldout("EffectObject", false)]

    #region
    #endregion
 */

public sealed partial class EffectManager : MonoBehaviour
{
    #region /* enum宣言 */
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

        APPER,
        DESTROY,
        TRAJECTORY, // 軌跡
    }

    // RUNで地上情報に合わせてエフェクトを分ける
    public enum RUN
    {
        GROUND = 0,
        WATER,
    }
    #endregion

    #region /* オブジェクトまとめ */
    [Foldout("EffectObject", true)]
    public GameObject effect_jump;          // jump時のエフェクト
    public GameObject effect_run_ground;    // run時のエフェクト  
    public GameObject effect_run_water;     // run時のエフェクト  
    public GameObject effect_shot;          // shot時のエフェクト
    public GameObject effect_trajectory;    // 軌跡エフェクト
    [Foldout("EffectObject", false)]

    private GameObject player;          // 情報を貰う用
                                        //private GameObject enemy;
                                        //private GameObject shot;

    #endregion

    #region /* エフェクトの数まとめ */
    [Foldout("エフェクトの数まとめ", true)]

    public int jump_player          = 10;
    public int run_ground_player    = 5;
    public int run_water_player     = 7;
    public int shot_player          = 5;

    public int jump_enemy           = 10;
    public int run_ground_enemy     = 5;
    public int run_water_enemy      = 7;

    public int apper_shot           = 10;
    public int trajectory_shot      = 3;
    public int destroy_shot         = 8;

    [Foldout("エフェクトの数まとめ", false)]

    #endregion

    #region /* 待機時間まとめ */
    // 待機時間用
    private float[] app_timer_type = { 0, 0, 0 };

    [Foldout("待機時間まとめ", true)]

    public float run_ground_timer_player = 0.2f;
    public float run_water_timer_player = 0.2f;

    public float run_ground_timer_enemy = 0.2f;
    public float run_water_timer_enemy = 0.2f;

    public float trajectory_timer_shot = 0.2f;

    [Foldout("待機時間まとめ", false)]
    #endregion

    EFFECT debug_state;

}

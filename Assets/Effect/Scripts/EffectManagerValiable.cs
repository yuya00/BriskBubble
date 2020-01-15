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

public partial class EffectManager : MonoBehaviour
{
    #region /* enum宣言 */
    // エフェクトを出すobjの種類
    public enum TYPE
    {
        PLAYER = 0,
        ENEMY,
        SHOT,
        UI,
    }

    // エフェクトを出す上でのSTATE
    public enum EFFECT
    {
        JUMP = 0,
        RUN,
        SHOT,
        COIN,

        APPER,
        DESTROY,
        TRAJECTORY, // 軌跡

        EXPLOSION,  // 敵破壊演出（爆発）
        FOCUSING,   // 敵破壊演出（集束）

        UI_FLASH,   // UI点滅
    }

    // RUNで地上情報に合わせてエフェクトを分ける
    public enum RUN
    {
        NONE = 0,
        GROUND,
        WATER,
    }
    #endregion

    #region /* オブジェクトまとめ */
    [Foldout("EffectObject", true)]
    public GameObject effect_jump;          // jump時のエフェクト
    public GameObject effect_run_ground;    // run時のエフェクト  
    public GameObject effect_run_water;     // run時のエフェクト  
    public GameObject effect_shot;          // shot時のエフェクト
    public GameObject effect_coin_get;      // coinとった時のエフェクト
    public GameObject effect_trajectory;    // 軌跡エフェクト
    public GameObject effect_explosion;    // 敵破壊エフェクト1
    public GameObject effect_focusing;    // 敵破壊エフェクト2
    public GameObject effect_ui;        // uiの演出エフェクト
    [Foldout("EffectObject", false)]
    #endregion

    private GameObject player;          // 情報を貰う用
    private GameObject[] enemy;
    //private GameObject shot;
    //private const int END_PRODACTION = 2;
    private const int FOCUS_NUM = 8;

    // focus変数
    private Vector3 focus_pos;
    private int data_no_x = 0;
    private int data_no_z = 0;
    private const int data_focus_x = 2;
    private const int data_focus_z = 2;
    private const int max_focus_data = 5;
    private float[] x = { data_focus_x, -data_focus_x, data_focus_x, -data_focus_x, data_focus_x, -data_focus_x };
    private float[] z = { -data_focus_z, -data_focus_z, -data_focus_z, data_focus_z, data_focus_z, data_focus_z };

    // プレイヤーの足元情報
    private int foot;


    #region /* エフェクトの数まとめ */
    [Foldout("エフェクトの数まとめ", true)]

    public int jump_player          = 10;
    public int run_ground_player    = 5;
    public int run_water_player     = 7;
    public int shot_player          = 5;
    public int coin_get_player      = 10;

    public int jump_enemy           = 10;
    public int run_ground_enemy     = 5;
    public int run_water_enemy      = 7;

    public int apper_shot           = 10;
    public int trajectory_shot      = 3;
    public int destroy_shot         = 8;

    public int explosion_effect     = 1;
    public int focusing_effect      = 8;

    public int ui_flash_effect      = 5;
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

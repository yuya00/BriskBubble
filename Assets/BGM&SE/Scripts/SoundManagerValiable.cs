using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixeye.Unity;

public partial class SoundManager : MonoBehaviour
{
    #region /* enum宣言 */
    // 音を出すobjの種類
    public enum CHARA_TYPE
    {
        PLAYER = 0,
        ENEMY,
        SHOT,
        UI,
        SCENE,
    }

    // 音の種類
    public enum SE_TYPE
    {
        JUMP = 0,
        SHOT,
        ENEMY_GET,
        SHOT_CRACK,
        START_COUNT,
        DAMAGE,

        STAGE_SELECT,

        ENEMY_DESTROY,
        ENEMY_SHOT,
        ENEMY_FIND,

        WEAPON_CHANGE,
    }

    #endregion

    #region /* オブジェクトまとめ */
    [Foldout("SEObject", true)]
    public AudioClip get;
    public AudioClip crack;
    public AudioClip shot;
    public AudioClip jump;
    public AudioClip start_count;
    public AudioClip stage_select;
    //public AudioClip stage;
    //public AudioClip title;
    public AudioClip enemy_shot;
    public AudioClip enemy_destroy;
    public AudioClip enemy_find;
    public AudioClip weapon_change;
    public AudioClip damage;
    [Foldout("SEObject", false)]
    #endregion

    private AudioSource audioSource;

}

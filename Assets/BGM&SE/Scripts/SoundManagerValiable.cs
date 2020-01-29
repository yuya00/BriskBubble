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
    public enum BGM_TYPE
    {
        STAGE = 0,
        TITLE,
    }

    // 音の種類
    public enum SE_TYPE
    {
        JUMP = 0,
        SHOT,
        ENEMY_GET,
        SHOT_CRACK,
        START_COUNT,
        ENEMY_DESTROY,
    }

    #endregion

    #region /* オブジェクトまとめ */
    [Foldout("EffectObject", true)]
    public AudioClip get;
    public AudioClip crack;
    public AudioClip shot;
    public AudioClip jump;
    public AudioClip start_count;
    public AudioClip stage;
    public AudioClip title;
    public AudioClip enemy_destroy;
    [Foldout("EffectObject", false)]
    #endregion

    private AudioSource audioSource;

}

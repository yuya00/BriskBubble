using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixeye.Unity;

public partial class BGMManager : MonoBehaviour
{
    #region /* enum宣言 */
    // 音を出すobjの種類
    //public enum CHARA_TYPE
    //{
    //    SCENE,
    //}

    // 音の種類
    public enum BGM_TYPE
    {
        STAGE = 0,
        TITLE,
    }

    #endregion

    #region /* オブジェクトまとめ */
    [Foldout("BGMObject", true)]
    public AudioClip stage;
    public AudioClip title;
    [Foldout("BGMObject", false)]
    #endregion

    private AudioSource audioSource;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixeye.Unity;


public partial class SoundManager : MonoBehaviour
{
    private SE_TYPE debug_se;
    void Start()
    {
        //Componentを取得
        audioSource = GetComponent<AudioSource>();
        debug_se = 0;
    }

    void Update()
    {
    }

    #region /* BGMまとめ */
    public void SoundBGM(BGM_TYPE bgm)
    {
        // シーンのBGM
        Scene(bgm);
    }

    void Scene(BGM_TYPE bgm)
    {
        // stateでどの音かを決める
        switch (bgm)
        {
            case BGM_TYPE.STAGE:
                AudioSet(stage);
                //debug_se = SE_TYPE.JUMP;
                break;
            case BGM_TYPE.TITLE:
                AudioSet(title);
                //debug_se = SE_TYPE.SHOT;
                break;
        }
    }
    #endregion

    #region /* SEまとめ */
    public void SoundSE(CHARA_TYPE chara, SE_TYPE se)
    {
        // charaに値が入ってる
        switch (chara)
        {
            case CHARA_TYPE.PLAYER:
                Player(se);
                break;
            case CHARA_TYPE.ENEMY:
                Enemy(se);
                break;
            case CHARA_TYPE.SHOT:
                Shot(se);
                break;
            case CHARA_TYPE.SCENE:
                Scene(se);
                break;

                //case CHARA_TYPE.UI:
                //    //UI(se);
                //    break;
        }
    }

    // プレイヤーのSE--------------------------------------------
    void Player(SE_TYPE se)
    {
        // stateでどのエフェクトかを決める
        switch (se)
        {
            case SE_TYPE.JUMP:
                AudioSet(jump);
                debug_se = SE_TYPE.JUMP;
                break;
            case SE_TYPE.SHOT:
                AudioSet(shot);
                debug_se = SE_TYPE.SHOT;
                break;
        }
    }

    // エネミーのSE--------------------------------------------
    void Enemy(SE_TYPE se)
    {
        // stateでどのエフェクトかを決める
        switch (se)
        {
            case SE_TYPE.START_COUNT:
                AudioSet(start_count);
                debug_se = SE_TYPE.START_COUNT;
                break;
            case SE_TYPE.ENEMY_DESTROY:
                AudioSet(enemy_destroy);
                debug_se = SE_TYPE.ENEMY_DESTROY;
                break;
        }
    }

    // ショットのSE--------------------------------------------
    void Shot(SE_TYPE se)
    {
        // stateでどのエフェクトかを決める
        switch (se)
        {
            case SE_TYPE.ENEMY_GET:
                AudioSet(get);
                debug_se = SE_TYPE.ENEMY_GET;
                break;
            case SE_TYPE.SHOT_CRACK:
                AudioSet(crack);
                debug_se = SE_TYPE.SHOT_CRACK;
                break;
        }
    }

    // シーンのSE--------------------------------------------
    void Scene(SE_TYPE se)
    {
        // stateでどのエフェクトかを決める
        switch (se)
        {
            case SE_TYPE.START_COUNT:
                AudioSet(start_count);
                debug_se = SE_TYPE.ENEMY_GET;
                break;
        }
    }


    #endregion

    // 実際に音を出す処理----------------------------------------
    void AudioSet(AudioClip audio)
    {
        if (!audio) return;
        audioSource.PlayOneShot(audio);
    }

    // 音を止める
    public void AudioStop()
    {
        audioSource.Stop();
    }


    public bool gui_on;

    void OnGUI()
    {
        if (gui_on)
        {
            GUILayout.BeginVertical("box");

            //uGUIスクロールビュー用
            Vector2 leftScrollPos = Vector2.zero;

            // スクロールビュー
            leftScrollPos = GUILayout.BeginScrollView(leftScrollPos, GUILayout.Width(200), GUILayout.Height(400));
            GUILayout.Box("Weapon");


            #region ここに追加

            GUILayout.TextArea("debug_se\n" + debug_se);
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);     

            // スペース
            GUILayout.Space(200);
            GUILayout.Space(10);
            #endregion


            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }


}

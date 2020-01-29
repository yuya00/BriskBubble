using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BGMManager : MonoBehaviour
{
    private BGM_TYPE debug;
    // Start is called before the first frame update
    void Start()
    {
        //Componentを取得
        audioSource = GetComponent<AudioSource>();
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
                debug = BGM_TYPE.STAGE;
                break;
            case BGM_TYPE.TITLE:
                AudioSet(title);
                debug = BGM_TYPE.TITLE;
                break;
        }
    }
    #endregion

    // 実際に音を出す処理----------------------------------------
    void AudioSet(AudioClip audio)
    {
        if (!audio) return;
        
        audioSource.clip = audio;
        audioSource.Play();
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

            GUILayout.TextArea("debug\n" + debug);
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

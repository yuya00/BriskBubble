using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Scene : MonoBehaviour
{
    /*
     * クリア演出
     * カメラに情報送って、プレイヤーの前に固定して、
     * 画面にクリア文字を描画
     */
    public Fade fade;

    public string scene_name;

    private bool fade_fg = false;
    private bool fadeout_fg = false;
    private bool clear_fg = false;

    public float fade_in_time = 0.5f;
    public float fade_out_time = 1.0f;

    private float interval_time;
    public float interval_time_max;

    private GameObject cam;
    public Text end_text;
    private string[] buf = { "C O M P L E T E !!", "G A M E O V E R" };
    private int buf_no = 0;

    private const int CLEAR = 0;
    private const int OVER = 1;

    void Start()
    {
        fade_fg = false;
        fadeout_fg = true;
        clear_fg = false;
        //GameObject.FindGameObjectsWithTag("Enemy");
        cam = GameObject.FindGameObjectWithTag("Camera");
        end_text.text = "";
        buf_no = 0;
    }

    void Update()
    {
    }

    void FixedUpdate()
    {
        // 最初にフェードアウトする
        scene_init_fadeout();

        // シーンごとに移行条件を設定
        scene_select();

        // シーン切り替え
        if (fade_fg)
        {
            TransScene(scene_name, interval_time_max);
        }
    }

    // シーンはじまったときにfadeoutする
    void scene_init_fadeout()
    {
        // Initializeでtrueを設定して、1フレーム計算
        if (fadeout_fg)
        {
            fade.FadeOut(fade_out_time);
            fadeout_fg = false;
        }
    }

    // クリア演出後にfadeoutする
    void scene_last_fadein()
    {
        fade_fg = true;
        fade.FadeIn(fade_in_time);
    }

    // シーンごとに移行条件を設定
    void scene_select()
    {
        // タイトル
        if (SceneManager.GetActiveScene().name == "title")
        {
            if (Input.GetButtonDown("Start"))
            {
                scene_last_fadein();
            }
        }

        // ゲームシーンで敵全滅させたらシーン移行
        if (SceneManager.GetActiveScene().name == "yusuke_scene" || SceneManager.GetActiveScene().name == "stage_1")
        {
            if (GetComponent<EnemyKillCount>().Enm_num_max <= 0)
            {
                //scene_last_fadein();
                // クリア演出にはいる
                clear_fg = true;
                buf_no = CLEAR;
                end_text.text = buf[buf_no];
                end_text.gameObject.SetActive(true);
            }
        }

        // カメラのクリア演出が終わったらfadein
        if (cam.GetComponent<Camera_Script>().Clear_end())
        {
            // 初期化
            clear_fg = false;
            scene_last_fadein();
        }

        // 時間切れ
        if(GetComponent<LimitTimer>().Timer_stop)
        {
            // クリア演出にはいる
            clear_fg = true;
            buf_no = OVER;
            end_text.text = buf[buf_no];
            end_text.gameObject.SetActive(true);
        }
    }

    // シーン遷移用コルーチン .
    void TransScene(string scene_name, float interval_time_max)
    {
        interval_time += Time.deltaTime;
        if (interval_time > interval_time_max)
        {
            scene_change(scene_name);
            fade_fg = false;
        }
    }

    //シーン切替
    void scene_change(string scene_name)
    {
        SceneManager.LoadScene(scene_name);
    }

    //public bool Clear_fg
    //{
    //    get { return clear_fg; }
    //}

    // get関数
    public bool Clear_fg()
    {
        return clear_fg;
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
            GUILayout.Box("Camera");


            #region ここに追加
            GUILayout.TextArea("カメラの演出終了fg\n" + cam.GetComponent<Camera_Script>().Clear_end());
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);
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

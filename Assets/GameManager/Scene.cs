﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Scene : MonoBehaviour
{
    public Fade fade;

    public string scene_name;

    private bool fade_fg = false;
    private bool fade_out_fg = false;
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

    public Text start_text;
    //private string[] start_buf = { "", "3", "2", "1", "S T A R T !!" };
    private string[] start_buf = { "", "", "", "", "S T A R T !!" };
    private int start_buf_no = 0;
    private int send_buf_no = 4;
    private float start_timer = 0;
    private float start_timer_max = 1.0f;
    private float alpha = 1;
    private bool start_fg;

    public bool debug_fg;

    private GameObject sprite;

    void Start()
    {
        fade_fg = false;
        fade_out_fg = true;
        clear_fg = false;
        //GameObject.FindGameObjectsWithTag("Enemy");
        cam = GameObject.FindGameObjectWithTag("Camera");
        end_text.text = "";
        buf_no = 0;

        start_fg = false;
        start_text.gameObject.SetActive(true);
        // この番号を配列の番目にして文字を切り替え
        start_buf_no = 0;
        send_buf_no = 4;
        start_text.text = start_buf[start_buf_no];
        alpha = 1;

        //最初は消す
        sprite = GameObject.FindGameObjectWithTag("Sprite");
        sprite.SetActive(false);
    }

    void Update()
    {
        // debug
        if (debug_fg) start_fg = true;
    }

    void FixedUpdate()
    {
        // 最初にフェードアウトする
        SceneInitFadeOut();

        // カメラから送られてきた情報でフェード
        CameraFade();

        // シーンごとに移行条件を設定
        SceneSelect();

        // シーン切り替え
        if (fade_fg)
        {
            TransScene(scene_name, interval_time_max);
        }
    }

    // シーンはじまったときにfadeoutする(Startでやったらがたつき起きる)
    void SceneInitFadeOut()
    {
        // Initializeでtrueを設定して、1フレーム計算
        if (fade_out_fg)
        {
            fade.FadeOut(fade_out_time);
            fade_out_fg = false;
        }
    }

    // クリア演出後にfadeoutする
    void SceneLastFadeIn()
    {
        fade_fg = true;
        fade.FadeIn(fade_in_time);
    }

    // カメラ情報をもとにフェード
    void CameraFade()
    {
        // 情報取得
        float fadein = cam.GetComponent<CameraScript>().camera_fadein_time;
        float fadeout = cam.GetComponent<CameraScript>().camera_fadeout_time;

        // カメラ情報をもとにフェード
        if (cam.GetComponent<CameraScript>().Scene_camera_state == 2)
        {
            fade.FadeIn(fadein);
        }
        if (cam.GetComponent<CameraScript>().Scene_camera_state == 0 || cam.GetComponent<CameraScript>().Camera_state == 0)
        {
            fade.FadeOut(fadeout);
        }
    }

    // シーンごとに移行条件を設定
    void SceneSelect()
    {
        // タイトル
        if (SceneManager.GetActiveScene().name == "title")
        {
            if (Input.GetButtonDown("Start"))
            {
                SceneLastFadeIn();
            }
        }

        // ゲームシーン
        if (SceneManager.GetActiveScene().name == "yusuke_scene" ||
            SceneManager.GetActiveScene().name == "stage_1" || 
            SceneManager.GetActiveScene().name == "stage_2" ||
            SceneManager.GetActiveScene().name == "stage_3")
        {
            // スタート文字(カメラの初期動作が終わってから)
            if (cam.GetComponent<CameraScript>().Camera_state == 0) SetText();

            // 敵全滅させたらシーン移行
            if (GetComponent<EnemyKillCount>().EnemyNumMax <= 0)
            {
                //SceneLastFadeIn();
                // クリア演出にはいる
                clear_fg = true;
                buf_no = CLEAR;
                end_text.text = buf[buf_no];
                end_text.gameObject.SetActive(true);
            }
        }

        // カメラのクリア演出が終わったらfadein
        if (cam.GetComponent<CameraScript>().ClearEnd())
        {
            // 初期化
            clear_fg = false;
            SceneLastFadeIn();
        }

        // 時間切れ
        if (GetComponent<LimitTimer>().TimerStop)
        {
            // クリア演出にはいる
            clear_fg = true;
            buf_no = OVER;
            end_text.text = buf[buf_no];
            end_text.gameObject.SetActive(true);
        }

        //ステージセレクトシーン
        if(SceneManager.GetActiveScene().name == "stage_select")
        {
            if(GameObject.Find("StageSelectManager").GetComponent<StageSelectManager>().next_scene_name!="")
            {
                scene_name = GameObject.Find("StageSelectManager").GetComponent<StageSelectManager>().next_scene_name;
                SceneLastFadeIn();
                
            }
        }

    }

    // シーン遷移用コルーチン .
    void TransScene(string scene_name, float interval_time_max)
    {
        interval_time += Time.deltaTime;
        if (interval_time > interval_time_max)
        {
            SceneChange(scene_name);
            fade_fg = false;
        }
    }

    //シーン切替
    void SceneChange(string scene_name)
    {
        SceneManager.LoadScene(scene_name);
    }

    // 文字切り替え
    void SetText()
    {
        // スプライトつける
        sprite.SetActive(true);

        // START!!がなくなるまで加算
        if (start_buf_no < 5)
        {
            // 文字を更新
            start_text.text = start_buf[start_buf_no];
            start_timer += Time.deltaTime;
        }

        if (start_timer > start_timer_max)
        {
            start_buf_no++;

            // スタートタイマーに送る用
            if (send_buf_no > 0) send_buf_no--;
            start_timer = 0;
        }

        // START!!の文字になったら
        if (start_buf_no > 3)
        {
            start_timer_max = 2.0f;
            start_fg = true;
        }

        if (start_buf_no > 4)
        {
            TextAlpha();
        }
    }

    // 文字を消す
    void TextAlpha()
    {
        alpha -= Time.deltaTime;
        start_text.color = new Color(start_text.color.r, start_text.color.g, start_text.color.b, alpha);
        if (alpha <= 0) start_text.gameObject.SetActive(false);
    }

    public bool StartFg()
    {
        return start_fg;
    }

    // get関数
    public bool ClearFg()
    {
        return clear_fg;
    }

    // スタート文字
    public int Send_buf_no
    {
        get { return send_buf_no; }
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
            GUILayout.TextArea("start_fg\n" + start_fg);
            //GUILayout.TextArea("start_buf_no\n" + start_buf_no);
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

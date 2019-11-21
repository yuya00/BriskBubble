using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixeye.Unity;


/*
 *  L1押したら表示を変える、そのときの演出をさせる
 *  (その1)
 *  大きさを変える
 *  (その2)
 *  上下させる
 */

public class UIscript : MonoBehaviour
{
    #region 前回
#if false
    public bool gui_on;
    [Foldout("UIParameter", true)]

    public GameObject normal;
    public GameObject bubble;

    public float scale_spd = 1.0f;

    public float SCALE_MIN = 0.1f;
    public float SCALE_MAX = 0.2f;

    [Foldout("UIParameter", false)]

    private Vector3 scale_normal;
    private Vector3 scale_bubble;

    private int state;

    private const int NONE = 0;
    private const int SCALE_UP = 1;
    private const int SCALE_DOWN = 2;

    private int none_timer = 0;
    private int none_timer_max = 60;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        state_check();
        prodaction();
        replace(bubble);
    }

    // 演出判定
    void state_check()
    {
        if (state == SCALE_UP)          state = SCALE_DOWN;
        if (Input.GetButton("Shot_L"))  state = SCALE_UP;
    }

    // でかくしたり小さくする
    void prodaction()
    {
        switch (state)
        {
            case NONE:
                if (none_timer++ > none_timer_max) state = SCALE_DOWN;
                break;
            case SCALE_UP:
                if (!scale_limit_max(bubble)) scale_chenge(bubble, scale_spd);
                break;
            case SCALE_DOWN:
                if (!scale_limit_min(bubble)) scale_chenge(bubble, -scale_spd);
                break;
        }
    }

    // 大きさ変える
    void scale_chenge(GameObject ui, float spd)
    {
        ui.transform.localScale = new Vector3(
            ui.transform.localScale.x + spd * Time.deltaTime,
            ui.transform.localScale.y + spd * Time.deltaTime,
            ui.transform.localScale.z);
    }

    // 大きさの制限
    bool scale_limit_max(GameObject ui)
    {
        if (ui.transform.localScale.x > SCALE_MAX)
        {
            return true;
        }
        return false;
    }

    bool scale_limit_min(GameObject ui)
    {
        if (ui.transform.localScale.x < SCALE_MIN)
        {
            return true;
        }
        return false;
    }

    // L1？押したら即入れ替え
    void replace(GameObject ui)
    {
        // バブル画像の表示を最前面に
        first_draw(ui);

        if (ui.transform.localScale.x > (SCALE_MAX * 0.5f))
        {
            // バブル画像の表示を最奥に
            last_draw(ui);
        }
    }

    // 表示を最奥に
    void first_draw(GameObject ui) { ui.transform.SetAsFirstSibling(); }

    // 表示を最前面に
    void last_draw(GameObject ui) { ui.transform.SetAsLastSibling(); }

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

            GUILayout.TextArea("bubble.transform.localScale\n" + bubble.transform.localScale);
            GUILayout.TextArea("state\n" + state);
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);
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
#endif
    #endregion

    #region 今回
#if true
    public bool gui_on;
    [Foldout("UIParameter", true)]

    public GameObject normal;
    public GameObject bubble;

    public float scale_spd = 1.0f;
    public float move_spd = 100.0f;

    public float SCALE_MIN = 0.1f;
    public float SCALE_MAX = 0.2f;
    public float SCALE_PER = 0.8f;

    public float POS_MIN = 0.1f;
    public float POS_MAX = 30.0f;


    [Foldout("UIParameter", false)]

    // 共通
    private const int NONE = 0;
    private bool replace_fg;

    // 大きさ
    private Vector3 scale_bubble_max;
    private Vector3 scale_bubble_min;
    private int state_scale;
    private int none_timer = 0;
    private int none_timer_max = 60;
    private const int SCALE_UP = 1;
    private const int SCALE_DOWN = 2;

    // 位置
    private Vector3 pos_bubble;
    private Vector3 init_pos_bubble;

    private float len;

    private bool pos_fg;                 // trueのときはずっと演出

    private int state_pos;
    private const int POS_UP = 1;
    private const int POS_DOWN = 2;


    // Start is called before the first frame update
    void Start()
    {
        scale_bubble_max = new Vector3(SCALE_MAX, SCALE_MAX, 0);
        scale_bubble_min = new Vector3(SCALE_MIN, SCALE_MIN, 0);
        init_pos_bubble = pos_bubble = bubble.transform.position;
        state_pos = POS_UP;
        replace_fg = false;
    }

    // Update is called once per frame
    void Update()
    {
        state_check();
        prodaction_scale();
        prodaction_pos();
        replace(bubble);

        bubble.transform.position = pos_bubble;
    }

    // 演出判定
    void state_check()
    {
        state_scale = SCALE_DOWN;
        if (button_state()) state_scale = SCALE_UP;
    }

    // でかくしたり小さくする
    void prodaction_scale()
    {
        switch (state_scale)
        {
            case NONE:
                if (none_timer++ > none_timer_max) state_scale = SCALE_DOWN;
                break;
            case SCALE_UP:
                if (!scale_limit_max(bubble)) scale_chenge(bubble, scale_spd);
                if (scale_limit_max(bubble)) bubble.transform.localScale = scale_bubble_max;
                    break;
            case SCALE_DOWN:
                if (!scale_limit_min(bubble)) scale_chenge(bubble, -scale_spd * 0.5f);
                if (scale_limit_min(bubble)) bubble.transform.localScale = scale_bubble_min;
                break;
        }
    }

    // 大きさ変える
    void scale_chenge(GameObject ui, float spd)
    {
        ui.transform.localScale = new Vector3(
            ui.transform.localScale.x + spd * Time.deltaTime,
            ui.transform.localScale.y + spd * Time.deltaTime,
            ui.transform.localScale.z);
    }

    // 大きさの制限
    bool scale_limit_max(GameObject ui)
    {
        if (ui.transform.localScale.x > SCALE_MAX) return true;
        return false;
    }

    bool scale_limit_min(GameObject ui)
    {
        if (ui.transform.localScale.x < SCALE_MIN) return true;
        return false;
    }

    // 位置演出----------------------------------------------------------------------------
    void prodaction_pos()
    {
        // トリガーの入力があったら演出させる
        if (button_state())
        {
            pos_fg = true;
            replace_fg = true;
        }

        // 1回だけトリガー入力したらステートを設定
        if (button_triger())    state_pos = POS_UP;

        // 演出条件
        if (pos_fg)             pos_chenge(move_spd);
    }

    // 位置変える
    void pos_chenge(float spd)
    {
        // 初期位置からどれだけ動いたか
        len = pos_bubble.y - init_pos_bubble.y;

        switch (state_pos)
        {
            case NONE:
                // トリガーの入力が無かったら初期化
                if (!button_state()) state_pos = POS_UP;
                break;
            case POS_UP:
                // 上に上がる
                pos_bubble.y += spd * Time.deltaTime;

                // 上限まで行ったらステート変え
                if (len > POS_MAX) state_pos = POS_DOWN;
                break;
            case POS_DOWN:
                // 入力無かったら描画の順序を変える
                if (!button_state()) replace_fg = false;

                // 下に下がる
                pos_bubble.y -= spd * Time.deltaTime;

                // 下限まで行ったらステート変え
                if ((len - spd * Time.deltaTime) < 0)
                {
                    // 位置調整
                    pos_bubble.y = init_pos_bubble.y;
                    pos_fg = false;
                    state_pos = NONE;
                }
                break;
        }
    }

    // L1？押したら即入れ替え
    void replace(GameObject ui)
    {
        // バブル画像の表示を最奥に
        first_draw(ui);

        // バブル画像の表示を最前面に
        if (replace_fg && ui.transform.localScale.x > (SCALE_MAX * SCALE_PER))
        {
            last_draw(ui);
        }
    }

    // 表示を最奥に
    void first_draw(GameObject ui) { ui.transform.SetAsFirstSibling(); }

    // 表示を最前面に
    void last_draw(GameObject ui) { ui.transform.SetAsLastSibling(); }

    bool button_state()
    {
        if (Input.GetButton("Shot_L")) return true;
        return false;
    }
    bool button_triger()
    {
        if (Input.GetButtonDown("Shot_L")) return true;
        return false;
    }

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

            GUILayout.TextArea("state_pos\n" + state_pos);
            GUILayout.TextArea("len\n" + len);
            GUILayout.TextArea("pos_fg\n" + pos_fg);
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
#endif
    #endregion

}

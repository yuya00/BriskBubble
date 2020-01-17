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

    private int state;
    private int state_pos;
    private const int POS_UP = 1;
    private const int POS_DOWN = 2;

    private bool production_fg;

    // 演出エフェクト
    private GameObject ui_effect;

    private GameObject game_manager;

    // Start is called before the first frame update
    void Start()
    {
        game_manager = GameObject.FindGameObjectWithTag("GameManager");

        // 存在チェック
        ui_effect = GameObject.FindGameObjectWithTag("UIEffect");

        scale_bubble_max = new Vector3(SCALE_MAX, SCALE_MAX, 0);
        scale_bubble_min = new Vector3(SCALE_MIN, SCALE_MIN, 0);
        init_pos_bubble = pos_bubble = bubble.transform.position;
        state_pos = POS_UP;
        replace_fg = false;
        production_fg = false;

        state = 0;

        // 演出存在off
        SetActive(production_fg);
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case 0:
                if (game_manager.GetComponent<Scene>().StartFg()) state++;
                break;
            case 1:
                StateCheck();
                ProdactionScale();
                ProdactionPos();
                Replace(bubble);

                // 存在をフラグで管理
                SetActive(production_fg);

                // 回転演出
                //ProdactionRotation(bubble, debug);
                //ProdactionRotation(normal, debug);

                bubble.transform.position = pos_bubble;

                break;
        }
    }

    // 演出判定
    void StateCheck()
    {
        state_scale = SCALE_DOWN;
        if (ButtonState()) state_scale = SCALE_UP;
    }

    // でかくしたり小さくする
    void ProdactionScale()
    {
        switch (state_scale)
        {
            case NONE:
                if (none_timer++ > none_timer_max) state_scale = SCALE_DOWN;
                break;
            case SCALE_UP:
                if (!ScaleLimitMax(bubble)) ScaleChenge(bubble, scale_spd);
                if (ScaleLimitMax(bubble)) bubble.transform.localScale = scale_bubble_max;
                break;
            case SCALE_DOWN:
                if (!ScaleLimitMin(bubble)) ScaleChenge(bubble, -scale_spd * 0.5f);
                if (ScaleLimitMin(bubble)) bubble.transform.localScale = scale_bubble_min;
                break;
        }
    }

    private float rot_spd;

    // 回転
    void ProdactionRotation(GameObject ui, float rot)
    {
        if (ButtonState()) rot *= -1;

        rot_spd += rot * Time.deltaTime;

        // 回転制限
        if (rot_spd > 85) rot_spd = 85;
        if (rot_spd < 0) rot_spd = 0;

        // 回転
        ui.transform.localEulerAngles = new Vector3(rot_spd, rot_spd, 0);
    }

    // 大きさ変える
    void ScaleChenge(GameObject ui, float spd)
    {
        ui.transform.localScale = new Vector3(
            ui.transform.localScale.x + spd * Time.deltaTime,
            ui.transform.localScale.y + spd * Time.deltaTime,
            ui.transform.localScale.z);
    }

    // 大きさの制限
    bool ScaleLimitMax(GameObject ui)
    {
        if (ui.transform.localScale.x > SCALE_MAX)
        {
            return true;
        }
        return false;
    }

    bool ScaleLimitMin(GameObject ui)
    {
        if (ui.transform.localScale.x < SCALE_MIN) return true;
        return false;
    }

    // 位置演出----------------------------------------------------------------------------
    void ProdactionPos()
    {
        // トリガーの入力があったら演出させる
        if (ButtonState())
        {
            pos_fg = true;
            replace_fg = true;
        }

        // 1回だけトリガー入力したらステートを設定
        if (ButtonTriger()) state_pos = POS_UP;

        // 演出条件
        if (pos_fg) PosChenge(move_spd);
    }

    // 位置変える
    void PosChenge(float spd)
    {
        // 初期位置からどれだけ動いたか
        len = pos_bubble.y - init_pos_bubble.y;

        // 演出けす
        if (!ButtonState()) production_fg = false;

        switch (state_pos)
        {
            case NONE:
                // トリガーの入力が無かったら初期化
                if (!ButtonState()) state_pos = POS_UP;
                // 演出つける
                if (ButtonState()) production_fg = true;
                break;
            case POS_UP:
                // 上に上がる
                pos_bubble.y += spd * Time.deltaTime;

                // 上限まで行ったらステート変え
                if (len > POS_MAX) state_pos = POS_DOWN;
                break;
            case POS_DOWN:
                // 入力無かったら描画の順序を変える
                if (!ButtonState()) replace_fg = false;

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
    void Replace(GameObject ui)
    {
        // バブル画像の表示を最奥に
        FirstDraw(ui);

        // バブル画像の表示を最前面に
        if (replace_fg && ui.transform.localScale.x > (SCALE_MAX * SCALE_PER))
        {
            LastDraw(ui);
        }
    }

    // 存在管理
    void SetActive(bool fg)
    {
        if (!ui_effect) return;
        ui_effect.SetActive(fg);
    }

    // 表示を最奥に
    void FirstDraw(GameObject ui) { ui.transform.SetAsFirstSibling(); }

    // 表示を最前面に
    void LastDraw(GameObject ui) { ui.transform.SetAsLastSibling(); }

    bool ButtonState()
    {
        if (Input.GetButton("Shot_L")) return true;
        return false;
    }
    bool ButtonTriger()
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

            GUILayout.TextArea("rot_spd\n" + rot_spd);
            //GUILayout.TextArea("pos\n" + pos);
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

}

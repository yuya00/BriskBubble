using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponUIScript : MonoBehaviour
{    
    public int no = 0;              // 位置番号
    public float scale_spd = 3;     // 大きくなる速度

    private GameObject ui;

    public Vector3[] pos_data =     // 位置データ
    {
        new Vector3(25, -20, 0),
        new Vector3(0,   25, 0),
        new Vector3(-25,-25, 0),
    };

    public Vector3 scale_min = new Vector3(0.05f, 0.05f, 1);
    public Vector3 scale_max = new Vector3(0.07f, 0.07f, 1);

    private Vector3 scale = Vector3.zero;

    private GameObject game_manager;

    private int state = 0;

    // Start is called before the first frame update
    void Start()
    {
        game_manager = GameObject.FindGameObjectWithTag("GameManager");
        ui = GameObject.FindGameObjectWithTag("pivot");
        scale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case 0:
                ScaleManager();
                if (game_manager.GetComponent<Scene>().StartFg()) state++;
                break;
            case 1:
                MovePos();
                ScaleManager();
                break;
        }
    }

    void MovePos()
    {
        // 切り替え（位置変え）
        if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Shot_L")) no++;
        if (no >= 3) no = 0;

        // 移動する位置
        Vector3 pos = ui.transform.position + pos_data[no];
        transform.position = pos;
    }

    // 大きさ変更まとめ
    void ScaleManager()
    {
        // 使ってる武器
        if (no == 0)
        {
            ScaleUp(scale_spd);
            LastDraw(gameObject);
        }
        else
        {
            ScaleDown(scale_spd);
        }
    }


    // 大きく
    void ScaleUp(float spd)
    {
        if (transform.localScale.x < scale_max.x - (spd * Time.deltaTime))
        {            transform.localScale = new Vector3(
                transform.localScale.x + (spd * Time.deltaTime),
                transform.localScale.y + (spd * Time.deltaTime),
                transform.localScale.z);
        }
        else// 大きさ制限
        {
            transform.localScale = scale_max;
        }
    }

    // 小さく
    void ScaleDown(float spd)
    {
        if (transform.localScale.x > scale_min.x + (spd * Time.deltaTime))
        {
            transform.localScale = new Vector3(
                transform.localScale.x - (spd * Time.deltaTime),
                transform.localScale.y - (spd * Time.deltaTime),
                transform.localScale.z);
        }
        else// 大きさ制限
        {
            transform.localScale = scale_min;
        }
    }

    // 表示を最奥に
    void FirstDraw(GameObject ui) { ui.transform.SetAsFirstSibling(); }

    // 表示を最前面に
    void LastDraw(GameObject ui) { ui.transform.SetAsLastSibling(); }


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

            GUILayout.TextArea("no\n" + no);
            GUILayout.TextArea("pos_data[no]\n" + pos_data[no]);
            GUILayout.TextArea("transform.position\n" + transform.position);
            GUILayout.TextArea("transform.localScale\n" + transform.localScale);     
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

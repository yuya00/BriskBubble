using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpEffect : MonoBehaviour
{
    public Color init_col;
    private Vector3 front;

    //private GameObject player;

    public float spd = 3.0f;
    public float slow_spd;
    private float scale_min = 0.1f; 		// エフェクトの下限

    // 大きさの範囲
    public float init_scale_max = 1.0f;
    public float init_scale_min = 0.2f;

    // 角度の範囲
    public float init_rot_max = 360;
    public float init_rot_min = 0;
    public float rot_x = 0;

    // 透明になる速さ
    public float alpha_spd = 0.2f;

    // 壊れるまでの待機時間
    private float destroy_timer;
    public float destroy_timer_max = 1.0f;

    public Vector3 rot = new Vector3(30, 80, 30);
    void Start()
    {
        // モデルの色
        //init_col = gameObject.GetComponent<MeshRenderer>().material.color;
        init_col = new Color(1, 1, 1, 1);

        //player = GameObject.FindGameObjectWithTag("Player");

        // 初期の大きさをランダムにする
        float rand_scale = Random.Range(init_scale_min, init_scale_max);
        transform.localScale = new Vector3(rand_scale, rand_scale, rand_scale);

        // 初期の角度
        float rand_rot = Random.Range(init_rot_min, init_rot_max);
        transform.Rotate(new Vector3(rot_x, rand_rot, rand_rot));

        // 角度を保存
        front = transform.forward;
    }
    void Update()
    {
        AlphaChange();
        Destroy();
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        spd -= slow_spd * Time.deltaTime;

        // 回転させて動きを入れる
        transform.Rotate(new Vector3(rot.x, rot.y, rot.z) * Time.deltaTime);
        
        // 飛ばす
        transform.position -= (front + transform.forward.normalized) * (spd * Time.deltaTime);
    }

    void AlphaChange()
    {
        // 徐々に透明にする
        init_col.r -= alpha_spd * Time.deltaTime;
        init_col.g -= alpha_spd * Time.deltaTime;
        init_col.b -= alpha_spd * Time.deltaTime;
        //init_col.a = 1;
        //Mathf.Clamp(init_col.a,0,1);

        gameObject.GetComponent<MeshRenderer>().sharedMaterial.color = init_col;
    }

    // 壊れる条件
    void Destroy()
    {
        // 時間経ったら消す
        destroy_timer += Time.deltaTime;
        if (destroy_timer > destroy_timer_max)
        {
            Destroy(gameObject);
            destroy_timer = 0;
        }

        // (シェーダーが)透明になったら消す
        if (init_col.r < 0)
        {
            Destroy(gameObject);
        }
    }

    //private Vector2 left_scroll_pos = Vector2.zero;   //uGUIスクロールビュー用
    //private float scroll_height = 330;
    //void OnGUI()
    //{
    //    GUILayout.BeginVertical("box", GUILayout.Width(250));
    //    left_scroll_pos = GUILayout.BeginScrollView(left_scroll_pos, GUILayout.Width(250), GUILayout.Height(scroll_height));
    //    //GUILayout.Box("Player");

    //    GUILayout.TextArea("init_col : \n" + init_col);
    //    GUILayout.TextArea("color : \n" + gameObject.GetComponent<MeshRenderer>().sharedMaterial.color);
    //    //GUILayout.TextArea("init_col : \n" + init_col);
    //    //GUILayout.TextArea("init_col : \n" + init_col);
    //    //GUILayout.TextArea("init_col : \n" + init_col);
    //    //GUILayout.TextArea("init_col : \n" + init_col);

    //    GUILayout.EndScrollView();
    //    GUILayout.EndVertical();
    //}
}



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunGroundEffect : MonoBehaviour
{
    public Color init_col;                  // 初期色(徐々に透明)
    private Vector3 front;                  // 移動方向
    private GameObject player;

    public float spd = 3.0f;                // 移動速さ
    public float alpha_spd = 0.2f;          // 透明になる速さ

    public float scale_max = 0.6f;
    public float scale_min = 0.1f;

    public float[] rot_max = { 0, 180, 180 };
    public float[] rot_min = { 0, 0, 0 };
    
    private float destroy_timer;            // 壊れるまでの待機時間
    public float destroy_timer_max = 1.0f;

    void Start()
    {
        // モデルの色
        gameObject.GetComponent<MeshRenderer>().material.color = init_col = new Color(1, 1, 1, 1);

        player = GameObject.FindGameObjectWithTag("Player");
        // 角度を保存
        front = player.transform.forward;

        // 初期の大きさをランダムにする
        float rand_scale = Random.Range(scale_min, scale_max);
        transform.localScale = new Vector3(rand_scale, rand_scale, rand_scale);

        // 初期の角度
        float[] rand_rot = { 0, 0, 0 };
        
        for(int i = 0;i < 2;++i)
        {
            rand_rot[i] = Random.Range(rot_min[i], rot_max[i]);
        }
        transform.Rotate(new Vector3(rand_rot[0], rand_rot[1], rand_rot[2]));
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
        //spd -= slow_spd * Time.deltaTime;

        // 飛ばす
        transform.position -= (front + transform.forward) * (spd * Time.deltaTime);
    }

    void AlphaChange()
    {
        // 徐々に透明にする
        init_col.r -= alpha_spd * Time.deltaTime;
        init_col.g -= alpha_spd * Time.deltaTime;
        init_col.b -= alpha_spd * Time.deltaTime;

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

}

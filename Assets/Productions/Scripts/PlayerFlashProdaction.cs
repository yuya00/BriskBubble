using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFlashProdaction : MonoBehaviour
{

    /*
     * 存在を消す
     */

    private GameObject player;
    private int type;               // 透明、不透明を交互にやる用
    private bool judge;
    private float timer = 0;
	public float TIME_MAX;          //0.15f
	private GameObject material_obj;

    // Start is called before the first frame update
    void Start()
    {
		player		 = GameObject.FindGameObjectWithTag("Player");
		material_obj = player.GetComponentInChildren<SkinnedMeshRenderer>().gameObject;
		Clear();    // 初期化
    }

    // Update is called once per frame
    void Update()
    {
        if (player.GetComponent<Player>().FaintData > 0) {
            Flash();
		}
        else Clear();

		//judgeによって切り替え
		material_obj.SetActive(judge);
	}

    // 点滅
    void Flash()
    {
        timer += Time.deltaTime;
        if (timer > TIME_MAX)
        {
            timer = 0;
            type *= -1;
			// 存在点滅
			//player.SetActive(judge);
			//material_obj.SetActive(judge);
		}

        // つけるか消すか
        if (type > 0) judge = true;
        else judge = false;
    }

    // 初期化
    void Clear()
    {
        type = 1;
        timer = 0;
        judge = true;
    }

    /* ボツ でもMeshRenderer使ってるやつには使えるからおいとく
     * 色情報とって、当たった時の判定の時にこのスクリプトで書いたやつを
     * プレイヤーのほうで呼び出す
     */
#if false
    private Color col;
    private MeshRenderer meshrender;

    private float timer = 0;
    private float timer_max = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        // モデルの色
        meshrender = GetComponent<MeshRenderer>();
        col = meshrender.material.color;
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Flash();
    }

    void Flash()
    {
        timer += Time.deltaTime;
        if(timer > timer_max)
        {
            timer = 0;
            col.a *= -1;
        }

        //col.a = 1;
        //if (Input.GetMouseButton(0))
        //    col.a = 0;
        meshrender.material.color = col;
    }
#endif


}

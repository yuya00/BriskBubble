using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class EnemyKillCount : MonoBehaviour
{
    private GameObject[] enemy_prodaction;
    private int kill_count;
    private int enm_num_max;
    private bool kill_fg;

    public Text enemy_num_text;

    void Start()
    {
        EnemyCountInit();
        //enemy_num_text.text = "残り : " + enm_num_max + "体";
        enemy_num_text.text = "残り : ";
    }

    void Update()
    {
        kill_fg = false;
        EnemyCount();
        //enemy_num_text.text = "残り : " + enm_num_max + "体";
        enemy_num_text.text = "残り : ";
    }

    // Start is called before the first frame update
    void EnemyCountInit()
    {
        kill_fg = false;
        enemy_prodaction = GameObject.FindGameObjectsWithTag("ProdactionEnemy");
        enm_num_max = enemy_prodaction.Length;
        kill_count = 0;
    }

    // 敵を倒した数をカウント
    void EnemyCount()
    {
        for (int i = 0; i < enm_num_max; ++i)
        {
            // 存在チェック
            if (!enemy_prodaction[i])
            {
                kill_fg = true;
                enemy_prodaction = GameObject.FindGameObjectsWithTag("ProdactionEnemy");
                kill_count += Mathf.Abs(enemy_prodaction.Length - enm_num_max);
                enm_num_max--;
            }
        }
    }

    public int EnemyNumMax
    {
        get { return enm_num_max; }
    }

    public bool KillFg
    {
        get { return kill_fg; }
    }
}

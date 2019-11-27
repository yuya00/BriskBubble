using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class EnemyKillCount : MonoBehaviour
{
    private GameObject[] enemy_prodaction;
    private int kill_count;
    private int enm_num_max;

    public Text enemy_num_text;

    void Start()
    {
        enm_count_init();
        enemy_num_text.text = "残り : " + enm_num_max + "体";
    }

    void Update()
    {
        enemy_kill_count();
        enemy_num_text.text = "残り : " + enm_num_max + "体";
    }

    // Start is called before the first frame update
    void enm_count_init()
    {
        enemy_prodaction = GameObject.FindGameObjectsWithTag("ProdactionEnemy");
        enm_num_max = enemy_prodaction.Length;
        kill_count = 0;
    }

    // 敵を倒した数をカウント
    void enemy_kill_count()
    {
        for (int i = 0; i < enm_num_max; ++i)
        {
            // 存在チェック
            if (!enemy_prodaction[i])
            {
                enemy_prodaction = GameObject.FindGameObjectsWithTag("ProdactionEnemy");
                kill_count += Mathf.Abs(enemy_prodaction.Length - enm_num_max);
                enm_num_max--;
            }
        }
    }

    public int Enm_num_max
    {
        get { return enm_num_max; }
    }
}

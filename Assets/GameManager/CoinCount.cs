using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinCount : MonoBehaviour
{
    private GameObject player;
    private int coin_num;

    // Start is called before the first frame update
    void Start()
    {
        coin_num = 0;
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        coin_num = player.GetComponent<Player>().Coin_count;
    }

    public bool gui_on;
    void OnGUI()
    {
        if (gui_on)
        {
            GUILayout.BeginVertical("box");
            Vector2 leftScrollPos = Vector2.zero;
            leftScrollPos = GUILayout.BeginScrollView(leftScrollPos, GUILayout.Width(200), GUILayout.Height(400));
            GUILayout.Box("Camera");
            #region ここに追加
            GUILayout.TextArea("coin_num\n" + coin_num);
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

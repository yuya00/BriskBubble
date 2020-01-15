using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public bool gui_on;
    private EffectManager effect;   // エフェクト用

    // Start is called before the first frame update
    void Start()
    {
        effect = GameObject.FindGameObjectWithTag("EffectManager").GetComponent<EffectManager>();

    }
    // キャラ指定
    private EffectManager.TYPE ENEMY = EffectManager.TYPE.ENEMY;

    // エフェクトの種類指定
    private EffectManager.EFFECT EXPLOSION = EffectManager.EFFECT.EXPLOSION;
    private EffectManager.EFFECT FOCUSING = EffectManager.EFFECT.FOCUSING;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            effect.Effect(ENEMY, FOCUSING, transform.position, effect.focusing_effect);
    }

    //GUI表示 -----------------------------------------------------
    private Vector2 left_scroll_pos = Vector2.zero;   //uGUIスクロールビュー用
    private float scroll_height = 330;
    void OnGUI()
    {

        if (gui_on)
        {
            GUILayout.BeginVertical("box", GUILayout.Width(190));
            left_scroll_pos = GUILayout.BeginScrollView(left_scroll_pos, GUILayout.Width(180), GUILayout.Height(scroll_height));
            GUILayout.Box("effect");

            //着地判定
            GUILayout.TextArea("位置\n" + transform.position);


            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageSelectManager : MonoBehaviour
{
    Button tutorial;
    Button stage1;
    Button stage2;
    Button stage3;

    public string next_scene_name;

    // Start is called before the first frame update
    void Start()
    {
        tutorial = GameObject.Find("tutorial").GetComponent<Button>();
        stage1 =   GameObject.Find("stage_1").GetComponent<Button>();
        stage2 =   GameObject.Find("stage_2").GetComponent<Button>();
        stage3 =   GameObject.Find("stage_3").GetComponent<Button>();

        tutorial.Select();

        next_scene_name = "";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSceneName(string name)
    {
        if(next_scene_name=="")next_scene_name = name;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class StageSelectManager : MonoBehaviour
{
    Button tutorial;
    Button stage1;
    Button stage2;
    Button stage3;

    public Image tutorial_image;
    public Image stage1_image;
    public Image stage2_image;
    public Image stage3_image;

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
        stage1_image.enabled = false;
        stage2_image.enabled = false;
        stage3_image.enabled = false;
        tutorial_image.enabled = false;

        switch (EventSystem.current.currentSelectedGameObject.name)
            {
                case "stage_1":
                    stage1_image.enabled = true;
                    break;
                case "stage_2":
                    stage2_image.enabled = true;
                    break;
                case "stage_3":
                    stage3_image.enabled = true;
                    break;
                case "tutorial":
                    tutorial_image.enabled = true;
                break;
            }
    }

    public void SetSceneName(string name)
    {
        if(next_scene_name=="")next_scene_name = name;
    }
}

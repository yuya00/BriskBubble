using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Signboard : MonoBehaviour
{

    public GameObject signboard_object = null;
    private Image signboard_image;
    //private GameObject read_button;
    private ImageSetPos read_button;
    private GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        //チュートリアル用の画像を取得
        signboard_image = signboard_object.GetComponent<Image>();

        //非表示にする
        signboard_image.enabled = false;

        //プレイヤーのゲームオブジェクトを取得
        player = GameObject.Find("Player");

        //ボタン表示のコンポーネントを取得  
        read_button = GameObject.Find("WorldCanvas").GetComponent<ImageSetPos>();
    }

    // Update is called once per frame
    void Update()
    {



        float distans = Vector3.Distance(player.transform.position,this.transform.position);

        if (distans < 10.0f)
        {

          if (Input.GetButtonDown("Read")) signboard_image.enabled = !signboard_image.enabled;
          read_button.SetReadButton(this.transform.position);
        }
        else
        {
            signboard_image.enabled = false;
        }
    }

    
}

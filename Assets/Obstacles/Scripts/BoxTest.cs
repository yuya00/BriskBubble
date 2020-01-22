using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxTest : MonoBehaviour
{
    private Rigidbody rigid;
    private GameObject floor;
    private GameObject cam;              // カメラオブジェ
    private Vector3 velocity;           //速さ(rigd.velocityでも良いかも)
    private Vector3 floor_pos;

    private float stop_fric = 0.3f;         //慣性(停止)
    public float run_speed = 15.0f;
    public float walk_speed = 3.0f;
    public float slope = 0.3f;          // スティックの傾き具合設定用
    public float rot_speed = 10.0f;     // カメラの回転速度
    public float jump_power = 15.0f;			//ジャンプ力
    private float fric = 0;
    private bool floor_fg;

    private Vector3 scale;

    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        cam = GameObject.FindGameObjectWithTag("Camera");
        floor = GameObject.FindGameObjectWithTag("Ground");
        scale = transform.localScale;
        floor_pos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = scale;
        Move();
        if (Input.GetButtonDown("Jump") || (Input.GetMouseButtonDown(2)))            Jump(jump_power);
    }

    void FixedUpdate()
    {
        //floor_pos = transform.position;
        velocity.y -= fric;
        //キャラクターを移動させる処理
        //rigid.MovePosition(transform.position + velocity * Time.deltaTime);
        if (floor_fg)   transform.position = floor_pos + velocity * Time.deltaTime;
        else            transform.position = transform.position + velocity * Time.deltaTime;
    }

    void Move()
    {
        Vector3 move = new Vector3(0, 0, 0);

        // スピード
        float axis_x = 0, axis_y = 0;

        // パッド情報代入
        float pad_x = Input.GetAxis("L_Stick_H");
        float pad_y = -Input.GetAxis("L_Stick_V");
        pad_x = Input.GetAxis("Horizontal");
        pad_y = Input.GetAxis("Vertical");

        axis_x += pad_x;
        axis_y += pad_y;

        // 平方根を求めて正規化
        float axis_length = Mathf.Sqrt(axis_x * axis_x + axis_y * axis_y);
        if (axis_length > 1.0f)
        {
            axis_x /= axis_length;
            axis_y /= axis_length;
        }

        // 進む方向
        Vector3 front = (transform.position - cam.transform.position).normalized;
        front.y = 0;

        // 横方向
        Vector3 right = Vector3.Cross(new Vector3(0, transform.position.y + 1.0f, 0), front).normalized;

        // 方向に値を設定
        move = right * axis_x;
        move += front * axis_y;

        //　方向キーが多少押されていたらその方向向く
        if (axis_x != 0f || axis_y != 0f) LookAt(move);

        #region 状態分け
        switch (StickState(axis_x, axis_y))
        {
            case 0:
                //停止時慣性(徐々に遅くなる)              
                velocity.x -= velocity.x * stop_fric;
                velocity.z -= velocity.z * stop_fric;
                break;
            case 1:
                // カメラから見てスティックを倒したほうへ進む
                velocity.x = move.normalized.x * walk_speed;
                velocity.z = move.normalized.z * walk_speed;
                break;
            case 2:
                // カメラから見てスティックを倒したほうへ進む
                velocity.x = move.normalized.x * run_speed;
                velocity.z = move.normalized.z * run_speed;
                break;
        }

        #endregion

    }

    // ジャンプの挙動
    void Jump(float jump_power)
    {
        rigid.useGravity = false;
        //is_ground = false;
        velocity.y = 0;
        velocity.y = jump_power;
        //animator.SetBool("JumpStart", jump_fg);
    }


    // その方向を向く
    void LookAt(Vector3 vec)
    {
        Vector3 target_pos = transform.position + vec.normalized;
        Vector3 target = Vector3.Lerp(transform.position + transform.forward, target_pos, rot_speed * Time.deltaTime);
        
            transform.LookAt(target);
    }

    // スティックの倒し具合設定
    int StickState(float x, float y)
    {
        // 入力チェック
        if (x != 0f || y != 0f)
        {
            // スティックの傾きによって歩きと走りを切り替え
            if (Mathf.Abs(x) >= slope || Mathf.Abs(y) >= slope) return 2;
            else return 1;
        }
        // 入力してないときは待機
        return 0;
    }

    //当たり判定 -----------------------------------------------
    private void OnCollisionExit(Collision other)
    {
        // 何にも当たってなかったら
        floor_pos = transform.position;
        floor_fg = false;

        if (other.gameObject.tag == "Wall" || other.gameObject.tag == "Ground")
        {
            fric = 3;
        }

    }

    // 物体に当たってるときに呼ばれる
    private void OnCollisionStay(Collision other)
    {
        // 床
        //if (other.gameObject.tag == "Ground")
        //{
        //    transform.SetParent(other.transform);
        //}

        // 床
        if (other.gameObject.tag == "Ground")
        {
            //floor.GetComponent<MoveFloor>().MoveVector;
            // 床の位置を設定
            floor_pos = new Vector3(
                transform.position.x + floor.GetComponent<MoveFloor>().MoveVector.x, 
                transform.position.y,
                transform.position.z + floor.GetComponent<MoveFloor>().MoveVector.z);

            floor_fg = true;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        // 床
        //if (other.gameObject.tag == "Ground")
        //{
        //    transform.SetParent(other.transform);
        //}
        if (other.gameObject.tag == "Wall" || other.gameObject.tag == "Ground")
        {
            velocity.y = 0;
            fric = 0;
        }

    }

    //GUI表示 -----------------------------------------------------
    private Vector2 left_scroll_pos = Vector2.zero;   //uGUIスクロールビュー用
    private float scroll_height = 330;
    public bool gui;
    void OnGUI()
    {
        if (!gui)
        {
            return;
        }

        GUILayout.BeginVertical("box", GUILayout.Width(190));
        left_scroll_pos = GUILayout.BeginScrollView(left_scroll_pos, GUILayout.Width(180), GUILayout.Height(scroll_height));
        GUILayout.Box("Test");

        if (gui)
        {
            //着地判定
            GUILayout.TextArea("velocity\n " + velocity);
            GUILayout.TextArea("fric\n " + fric);
            GUILayout.TextArea("floor_pos\n " + floor_pos);
            GUILayout.TextArea("transform.position\n " + transform.position);
            GUILayout.TextArea("fg\n " + floor_fg);
            //GUILayout.TextArea("velocity\n " + velocity);
            //GUILayout.TextArea("velocity\n " + velocity);
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }


}

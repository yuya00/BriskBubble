using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaBase : MonoBehaviour {

	protected Rigidbody rigid;
	protected Vector3 velocity;                 //速さ(rigd.velocityでも良いかも)
	public float run_spd = 15.0f;   //通常の速さ
	public float jump_power = 15.0f;    //ジャンプ力
	public float stop_fric = 0.3f;  //慣性(停止)
	protected float jump_fric = 0;      //慣性(ジャンプ)
	protected float jump_fric_power = 0.7f;    //慣性(ジャンプ)
	protected bool is_ground = false;   //地面接地判定
	protected Transform chara_ray;                  //レイを飛ばす位置(地面判別に使用)
	public float chara_ray_length = 2f;      //レイの距離
	public float gravity_power = 5;       //重力の倍率
	protected int[] iwork = new int[8];     //汎用
	protected float[] fwork = new float[8];     //汎用


	// Start is called before the first frame update
	public virtual void Start()
    {
		rigid = GetComponent<Rigidbody>();
		velocity = Vector3.zero;
	}


	//Update is called once per frame
	void Update()
    {

	}


	public virtual void Move()
    {
		//下レイが当たっていたら着地
		if (Physics.Linecast(chara_ray.position, chara_ray.position + Vector3.down * chara_ray_length)){
			is_ground = true;
        }
        else{
			is_ground = false;
		}

		//地面に接している時は初期化
		if (is_ground){
			velocity.y = 0;
			rigid.useGravity = true;
		}
		else{
			//地面に接していない時は重力
			velocity.y += Physics.gravity.y * gravity_power * Time.deltaTime;
		}

	}


	public virtual void Debug_Log(){
		//Debug.DrawRay(chara_ray.position, Vector3.down * chara_ray_length, Color.red);
		//Debug.Log("ground:" + is_ground);
		//Debug.Log("vel:"+velocity);
		//Debug.Log("vel:"+velocity);
		//add
		//add2
	}



	public virtual void FixedUpdate(){
		//キャラクターを移動させる処理
		rigid.MovePosition(transform.position + velocity * Time.deltaTime);
	}

}

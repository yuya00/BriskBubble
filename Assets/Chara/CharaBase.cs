using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaBase : MonoBehaviour {

	protected			 Rigidbody			 rigid;
	protected			 Vector3			 velocity;			//速さ(rigd.velocityでも良いかも)
	public float		 run_spd			 = 15.0f;			//通常の速さ
    public float		 walk_spd			 = 3.0f;			//歩きの速さ
    public float		 jump_power			 = 15.0f;			//ジャンプ力
	public float		 stop_fric			 = 0.3f;			//慣性(停止)
	protected float		 jump_fric			 = 0;				//慣性(ジャンプ)
	protected float		 jump_fric_power	 = 0.7f;			//慣性(ジャンプ)
	protected bool		 is_ground			 = false;           //地面接地判定
    protected Transform			 chara_ray;			//レイを飛ばす位置(地面判別に使用)
	public float		 chara_ray_length	 = 2f;				//レイの距離
	public float		 gravity_power		 = 5;				//重力の倍率
	protected int[]		 iwork				 = new int[8];		//汎用
	protected float[]	 fwork				 = new float[8];    //汎用

	//壁判定Ray ---------------------------------------------
	public int angle_mag = 3; //角度調整
	[System.Serializable]
	public struct WallRay {
		[Header("Gizmoの表示")]
		public bool gizmo_on;

		[SerializeField, Header("Rayの角度")]
		public float angle;     //00.0f 未使用

		[SerializeField, Header("Rayの長さ")]
		public float length;    //20.0f

		[System.NonSerialized] //壁との距離保存用
		public float dist_right, dist_left;

		[System.NonSerialized] //壁との当たり判定
		public bool hit_right_flg, hit_left_flg;

		[System.NonSerialized] //両方のRayが当たった回数
		public int both_count;

		[System.NonSerialized] //両方のRayが当たった回数判定
		public bool both_flg;

		[SerializeField, Header("向き変更の速さ")]
		public float spd;       //2.0f
	}
	[Header("壁判定Ray")]
	public WallRay wallray;

	public void WallRay_Clear() {
		wallray.dist_right = 0;
		wallray.dist_left = 0;
		wallray.hit_right_flg = false;
		wallray.hit_left_flg = false;
		wallray.both_flg = false;
	}

	//穴判定Ray ---------------------------------------------
	[System.Serializable]
	public struct HoleRay {
		[Header("Gizmoの表示")]
		public bool gizmo_on;

		[SerializeField, Header("Rayの角度")]
		public float angle;     //00.0f 未使用

		[SerializeField, Header("Rayの長さ")]
		public float length;    //100.0f

		//[System.NonSerialized] //穴との距離保存用
		//public float dist_right, dist_left;

		[System.NonSerialized] //穴との当たり判定
		public bool hit_right_flg, hit_left_flg;

		//[System.NonSerialized] //両方のRayが当たった回数
		//public int both_count;

		//[System.NonSerialized] //両方のRayが当たった回数判定
		//public bool both_flg;

		[SerializeField, Header("向き変更の速さ")]
		public float spd;       //2.0f
	}
	[Header("穴判定Ray")]
	public HoleRay holeray;

	public void HoleRay_Clear() {
		holeray.hit_right_flg = false;
		holeray.hit_left_flg = false;
	}


	//*********************************************************************//
	//*********************************************************************//
	//*********************************************************************//
	public struct STATUS
    {
        public bool ray_fg;
        public bool is_ground;
        public Vector3 velocity;
        public bool line_cast;
        public Vector3 ray_pos;
    }
    public STATUS status;
    //*********************************************************************//
    //*********************************************************************//
    //*********************************************************************//

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



	//--壁判定による向き変更
	public virtual void WallRay_Rotate_Judge() {
		//----壁判定Ray当たり判定
		WallRay_Judge();

		//----向き変更
		WallRay_Rotate();
	}

	//----壁判定Ray当たり判定
	public void WallRay_Judge() {
		RaycastHit hit;

		//右のレイ
		if (Physics.Raycast(transform.position,
			(transform.forward * angle_mag + transform.right).normalized * 1, out hit, wallray.length)) {
			Debug.Log(hit.collider.gameObject.name);
			if (hit.collider.gameObject.tag == "Wall") {
				wallray.dist_right = hit.distance;  //壁との距離保存
				wallray.hit_right_flg = true;       //壁との当たり判定
			}
		}
		else {
			wallray.dist_right = 0;
			wallray.hit_right_flg = false;
		}

		//左のレイ
		if (Physics.Raycast(transform.position,
			(transform.forward * angle_mag + (-transform.right)).normalized * 1, out hit, wallray.length)) {
			Debug.Log(hit.collider.gameObject.name);
			if (hit.collider.gameObject.tag == "Wall") {
				wallray.dist_left = hit.distance;   //壁との距離保存
				wallray.hit_left_flg = true;        //壁との当たり判定
			}
		}
		else {
			wallray.dist_left = 0;
			wallray.hit_left_flg = false;
		}

	}

	//----向き変更
	public void WallRay_Rotate() {
		if (wallray.hit_right_flg) {
			transform.Rotate(0.0f, -wallray.spd, 0.0f);
		}
		else if (wallray.hit_left_flg) {
			transform.Rotate(0.0f, wallray.spd, 0.0f);
		}
	}


	//--穴判定による向き変更
	public void HoleRay_Rotate_Judge() {
		//----穴判定Ray当たり判定
		HoleRay_Judge();

		//----向き変更
		HoleRay_Rotate();
	}

	//----穴判定Ray当たり判定
	public void HoleRay_Judge() {
		RaycastHit hit;
		//何にも当たっていなかったら

		//右のレイ
		if (!Physics.Raycast(transform.position + (transform.forward * angle_mag + transform.right).normalized * wallray.length,
			-transform.up, out hit, holeray.length)) {
			holeray.hit_right_flg = true;
		}
		else {
			holeray.hit_right_flg = false;
		}

		//左のレイ
		if (!Physics.Raycast(transform.position + (transform.forward * angle_mag + (-transform.right)).normalized * wallray.length,
			-transform.up, out hit, holeray.length)) {
			holeray.hit_left_flg = true;
		}
		else {
			holeray.hit_left_flg = false;
		}

	}

	//----向き変更
	public void HoleRay_Rotate() {
		if (holeray.hit_right_flg) {
			transform.Rotate(0.0f, -holeray.spd, 0.0f);
		}
		else if (holeray.hit_left_flg) {
			transform.Rotate(0.0f, holeray.spd, 0.0f);
		}
	}




	public virtual void Move()
    {
        /***********************/
        // 試しに
        // ショットのレイヤーは8番
        // shotのレイヤーを設定している物とだけ衝突しない( ～ ←で条件を反転するから ～ を取ったらショットとだけ衝突するようになる )
        LayerMask shot_layer = ~(1 << 8);
        /***********************/
        //下レイが当たっていたら着地
        if (Physics.Linecast(chara_ray.position, chara_ray.position + Vector3.down * chara_ray_length, shot_layer))
        {
            rigid.useGravity = true;
            is_ground = true;
            velocity.y = 0;
            status.velocity = velocity;
        }
        else
        {
            is_ground = false;
			if (velocity.y >= -60.0f) { //落下速度の上限
				velocity.y += Physics.gravity.y * gravity_power * Time.deltaTime;
			}
			else {
				velocity.y += Physics.gravity.y * gravity_power/10 * Time.deltaTime;
			}
        }

		/*
		//地面に接している時は初期化
		if (is_ground) {
			rigid.useGravity = true;
			//velocity.y = 0;
		}
		else {
			//地面に接していない時は重力
			velocity.y += Physics.gravity.y * gravity_power * Time.deltaTime;
		}
		// */
	}


	public virtual void Debug_Log(){
		/*
        Debug.DrawRay(chara_ray.position, Vector3.down * chara_ray_length, Color.red);
        Debug.Log("ground:" + is_ground);
        Debug.Log("vel:"+velocity);
        Debug.Log("vel:"+velocity);
		// */
    }



    public virtual void FixedUpdate(){
		//キャラクターを移動させる処理
		rigid.MovePosition(transform.position + velocity * Time.deltaTime);
	}

}

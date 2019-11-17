using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixeye.Unity;


public class CharaBase : MonoBehaviour {

	[Header("GUIの表示")]
	public bool gui_on;

	[Foldout("BaseParameter" ,true)]
	protected			 Rigidbody			 rigid;
	protected			 Vector3			 velocity;			//速さ(rigd.velocityでも良いかも)
	[Tooltip("走りの速さ")]
	public float		 run_spd			 = 15.0f;			//走りの速さ
	[Tooltip("歩きの速さ")]
    public float		 walk_spd			 = 3.0f;			//歩きの速さ
	[Tooltip("ジャンプ力")]
    public float		 jump_power			 = 15.0f;			//ジャンプ力
	[Tooltip("慣性(停止)")]
	public float		 stop_fric			 = 0.3f;			//慣性(停止)
	protected float		 jump_fric			 = 0;				//慣性(ジャンプ)
	protected float		 jump_fric_power	 = 0.7f;			//慣性(ジャンプ)
	protected bool		 is_ground			 = false;           //地面接地判定
    protected Transform	 chara_ray;								//レイを飛ばす位置(地面判別に使用)
	[Tooltip("レイの距離")]
	public float		 chara_ray_length	 = 2f;              //レイの距離
    [Tooltip("重力の倍率")]
	public float		 gravity_power		 = 5;				//重力の倍率
	protected int[]		 iwork				 = new int[8];		//汎用
	protected float[]	 fwork				 = new float[8];    //汎用
	[Tooltip("落下速度の速さ上限")]
	public float		 fallspd_limit		 = 30.0f;
	[Foldout("BaseParameter" ,false)]
	protected int        wait_timer;         //汎用待機タイマー





	// Ray基底 ------------------------------------------
	public class Ray_Base {
		[Header("Gizmoの表示")]
		public bool gizmo_on;

		[SerializeField, Header("Rayの長さ")]
		public float length;
	}

	// BoxCast基底 ------------------------------------------
	public class BoxCast_Base : Ray_Base {
		[System.NonSerialized]
		public float box_total;

		[System.NonSerialized]
		public Vector3 box_pos;

		[System.NonSerialized]
		public Vector3 box_size;

		[SerializeField, Range(0.0f, 4.0f), Header("Rayの高さ(上限)")]
		public float up_limit;

		[SerializeField, Range(0.0f, 4.0f), Header("Rayの高さ(下限)")]
		public float down_limit;

		//Boxcastの計算
		public void BoxCast_Cal(Transform self_trans) {
			box_total = down_limit + up_limit;
			box_size = new Vector3(0, box_total / 2, length / 2);
			box_pos = self_trans.position;
			box_pos = box_pos - (self_trans.up * down_limit) + (self_trans.transform.up * box_total / 2);
		}
	}


	//壁判定Ray ---------------------------------------------
	[System.NonSerialized]
	protected int angle_mag = 3; //角度調整
	[System.Serializable]
	public class WallRay : BoxCast_Base {
		//public float length;		//20.0f
		//public float up_limit;	//
		//public float down_limit;	//

		[SerializeField, Header("Rayの角度")]
		public float angle;     //00.0f 未使用

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

		//初期化
		public void Clear() {
			dist_right		 = 0;
			dist_left		 = 0;
			hit_right_flg	 = false;
			hit_left_flg	 = false;
			both_flg		 = false;
		}

	}
	[Header("壁判定Ray")]
	public WallRay wallray;


	//穴判定Ray ---------------------------------------------
	[System.Serializable]
	public class HoleRay : Ray_Base {
		//public float length;    //100.0f

		[SerializeField, Header("Rayの角度")]
		public float angle;     //00.0f 未使用

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

		public void Clear() {
			hit_right_flg = false;
			hit_left_flg = false;
		}

	}
	[Header("穴判定Ray")]
	public HoleRay holeray;




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

		#region BoxCast
		//*
		//右のレイ
		if (Physics.BoxCast(wallray.box_pos, wallray.box_size,
				(transform.forward * angle_mag + transform.right).normalized, out hit,
				transform.rotation, wallray.length / 2)) 
			{
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
		if (Physics.BoxCast(wallray.box_pos, wallray.box_size,
			(transform.forward * angle_mag + (-transform.right)).normalized, out hit,
			transform.rotation, wallray.length / 2)) 
			{
			if (hit.collider.gameObject.tag == "Wall") {
				wallray.dist_left = hit.distance;  //壁との距離保存
				wallray.hit_left_flg = true;       //壁との当たり判定
			}
		}
		else {
			wallray.dist_left = 0;
			wallray.hit_left_flg = false;
		}
		// */
		#endregion

		#region RayCast
		/*
		//右のレイ
		if (Physics.Raycast(transform.position,
			(transform.forward * angle_mag + transform.right).normalized * 1, out hit, wallray.length)) {
			//Debug.Log(hit.collider.gameObject.name);
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
			//Debug.Log(hit.collider.gameObject.name);
			if (hit.collider.gameObject.tag == "Wall") {
				wallray.dist_left = hit.distance;   //壁との距離保存
				wallray.hit_left_flg = true;        //壁との当たり判定
			}
		}
		else {
			wallray.dist_left = 0;
			wallray.hit_left_flg = false;
		}
		// */
		#endregion
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



	//着地時にfalse
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
        }
        else
        {
            is_ground = false;
			if (velocity.y >= -fallspd_limit) { //落下速度の上限
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






	protected bool WaitTime_Once(int wait_time) {
		if (wait_timer >= wait_time) {
			return true;
		}
		else {
			wait_timer++;
			return false;
		}
	}

}

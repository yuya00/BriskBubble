﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixeye.Unity;


public class CharaBase : MonoBehaviour {

	[System.Serializable]
	public struct GUI {
		[Header("GUIの表示")]
		public bool on;

		[Header("全値表示")]
		public bool all_view;

		[Header("開発用値表示")]
		public bool debug_view;
	}
	[Header("GUI")]
	public GUI gui;


	[Foldout("BaseParameter" ,true)]
	protected			 Rigidbody			 rigid;
	protected			 Vector3			 velocity;			//速さ(rigd.velocityでも良いかも)
	[Tooltip("走りの速さ")]
	public float		 run_speed			 = 15.0f;			//走りの速さ
	[Tooltip("歩きの速さ")]
    public float		 walk_speed			 = 3.0f;			//歩きの速さ
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

    protected const float HALF = 0.5f;  // 半分計算用



	// Ray基底 ------------------------------------------
	public class RayBase {
		public bool gizmo_on;

		[Header("判定の実行")]
		public bool judge_on;

		[SerializeField, Header("Rayの長さ")]
		public float length;
	}

	// BoxCast基底 ------------------------------------------
	public class BoxCastBase : RayBase {
		[System.NonSerialized]
		public float box_total;

		[System.NonSerialized]
		public Vector3 box_pos;

		[System.NonSerialized]
		public Vector3 box_size;

		[SerializeField, Range(0.0f, 8.0f), Header("Rayの高さ(上限)")]
		public float up_limit;

		[SerializeField, Range(0.0f, 8.0f), Header("Rayの高さ(下限)")]
		public float down_limit;

		//Boxcastの計算
		public void BoxCastCal(Transform self_trans)
        {
			box_total   = down_limit + up_limit;
			box_size    = new Vector3(0, box_total * HALF, length * HALF);
			box_pos     = self_trans.position;
			box_pos     = box_pos - (self_trans.up * down_limit) + (self_trans.transform.up * box_total * HALF);
		}

	}


	//壁判定Ray ---------------------------------------------
	[System.NonSerialized]
	protected int angle_mag = 3; //角度調整
	[System.Serializable]
	public class WallRay : BoxCastBase {
		//public float length;		//20.0f
		//public float up_limit;	//
		//public float down_limit;	//

		[SerializeField, Header("Rayの角度")]
		public float angle;     //00.0f 未使用

		[System.NonSerialized] //壁との距離保存用
		public float dist_right, dist_left;

		[System.NonSerialized] //壁との当たり判定
		public bool hit_right_flg, hit_left_flg;

		[System.NonSerialized] //めり込み判定
		public bool cavein_right_flg, cavein_left_flg;

		[System.NonSerialized] //両方のRayが当たった回数
		public int both_count;

		[System.NonSerialized] //両方のRayが当たった回数判定
		public bool both_flg;

		[SerializeField, Header("向き変更の速さ")]
		public float speed;       //2.0f

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
	public WallRay wall_ray;


	//穴判定Ray ---------------------------------------------
	[System.Serializable]
	public class HoleRay : RayBase {
		//public float length;    //100.0f

		[SerializeField, Header("Rayの始点")]
		public float startLength;	//11.0f

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
		public float speed;       //15.0f

		public void Clear() {
			hit_right_flg = false;
			hit_left_flg = false;
		}

	}
	[Header("穴判定Ray")]
	public HoleRay hole_ray;




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
	public virtual void WallRayRotate_Judge() {
		if (!wallray.judge_on) {
			return;
		}
		//----壁判定Ray当たり判定
		WallRayJudge();

		//----向き変更
		WallRayRotate();
	}

	//----壁判定Ray当たり判定
	public void WallRayJudge() {
		//RaycastHit hit;
		wall_ray.BoxCastCal(transform);

		#region BoxCast
		/*
		//右のレイ
		if (Physics.BoxCast(wall_ray.box_pos, wall_ray.box_size,
				(transform.forward * angle_mag + transform.right).normalized, out hit,
				transform.rotation, wall_ray.length / 2)) 
			{
			if (hit.collider.gameObject.tag == "Wall") {
				wall_ray.dist_right = hit.distance;  //壁との距離保存
				wall_ray.hit_right_flg = true;       //壁との当たり判定
			}
		}
		else {
			wall_ray.dist_right = 0;
			wall_ray.hit_right_flg = false;
		}

		//左のレイ
		if (Physics.BoxCast(wall_ray.box_pos, wall_ray.box_size,
			(transform.forward * angle_mag + (-transform.right)).normalized, out hit,
			transform.rotation, wall_ray.length / 2)) 
			{
			if (hit.collider.gameObject.tag == "Wall") {
				wall_ray.dist_left = hit.distance;  //壁との距離保存
				wall_ray.hit_left_flg = true;       //壁との当たり判定
			}
		}
		else {
			wall_ray.dist_left = 0;
			wall_ray.hit_left_flg = false;
		}
		// */
		#endregion

		#region RayCast_Three
		//*
		//右のレイ(上,下,真ん中)
		if (WallRayRight(wall_ray.up_limit, 1, 1)) {
			wall_ray.hit_right_flg = true;
		}
		else if (WallRayRight(wall_ray.down_limit, -1, 1)) {
			wall_ray.hit_right_flg = true;
		}
		else if (WallRayRight(0, 0, 1)) {
			wall_ray.hit_right_flg = true;
		}
		else {
			wall_ray.dist_right = 0;
			wall_ray.hit_right_flg = false;
		}

		//左のレイ(上,下,真ん中)
		if (WallRayLeft(wall_ray.up_limit, 1, -1)) {
			wall_ray.hit_left_flg = true;
		}
		else if (WallRayLeft(wall_ray.down_limit, -1, -1)) {
			wall_ray.hit_left_flg = true;
		}
		else if (WallRayLeft(0, 0, -1)) {
			wall_ray.hit_left_flg = true;
		}
		else {
			wall_ray.dist_left = 0;
			wall_ray.hit_left_flg = false;
		}
		// */
		#endregion

		#region RayCast
		/*
		//右のレイ
		if (Physics.Raycast(transform.position,
			(transform.forward * angle_mag + transform.right).normalized, out hit, wall_ray.length)) {
			if (hit.collider.gameObject.tag == "Wall") {
				wall_ray.dist_right = hit.distance;  //壁との距離保存
				wall_ray.hit_right_flg = true;       //壁との当たり判定
			}
		}
		else {
			wall_ray.dist_right = 0;
			wall_ray.hit_right_flg = false;
		}

		//左のレイ
		if (Physics.Raycast(transform.position,
			(transform.forward * angle_mag + (-transform.right)).normalized, out hit, wall_ray.length)) {
			if (hit.collider.gameObject.tag == "Wall") {
				wall_ray.dist_left = hit.distance;   //壁との距離保存
				wall_ray.hit_left_flg = true;        //壁との当たり判定
			}
		}
		else {
			wall_ray.dist_left = 0;
			wall_ray.hit_left_flg = false;
		}
		// */
		#endregion
	}

	//------右レイ
	bool WallRayRight(float limit, int limit_one, int right_one) {
		RaycastHit hit;

		if (Physics.Raycast(wall_ray.box_pos + (transform.up * limit * limit_one),
			(transform.forward * angle_mag + (transform.right * right_one)).normalized, out hit, wall_ray.length)) {
			//if (hit.collider.gameObject.tag == "Wall") {
				wall_ray.dist_right = hit.distance;  //壁との距離保存
				return true;
			//}
		}
		return false;
	}

	//------左レイ
	bool WallRayLeft(float limit, int limit_one, int right_one) {
		RaycastHit hit;

		if (Physics.Raycast(wall_ray.box_pos + (transform.up * limit * limit_one),
			(transform.forward * angle_mag + (transform.right * right_one)).normalized, out hit, wall_ray.length)) {
			//if (hit.collider.gameObject.tag == "Wall") {
				wall_ray.dist_left = hit.distance;  //壁との距離保存
				return true;
			//}
		}
		return false;
	}



	//----向き変更
	public void WallRayRotate() {
		if (wall_ray.hit_right_flg) {
			transform.Rotate(0.0f, -wall_ray.speed, 0.0f);
		}
		else if (wall_ray.hit_left_flg) {
			transform.Rotate(0.0f, wall_ray.speed, 0.0f);
		}
	}


	//--穴判定による向き変更
	public void HoleRayRotateJudge() {
		if (!hole_ray.judge_on) {
			return;
		}
		//----穴判定Ray当たり判定
		HoleRayJudge();

		//----向き変更
		HoleRayRotate();
	}

	//----穴判定Ray当たり判定
	public void HoleRayJudge() {
		RaycastHit hit;

		//何にも当たっていなかったら
		#region 4RayCast 縦横
		//*
		//右のレイ
		float var = 1;
		if ((!Physics.Raycast((transform.position + (transform.forward * angle_mag + (transform.right)).normalized * (hole_ray.startLength)) + (transform.right * var),
			-transform.up, out hit, hole_ray.length)) &&
			(!Physics.Raycast((transform.position + (transform.forward * angle_mag + (transform.right)).normalized * (hole_ray.startLength)) - (transform.right * var),
			-transform.up, out hit, hole_ray.length)) &&
			(!Physics.Raycast((transform.position + (transform.forward * angle_mag + (transform.right)).normalized * (hole_ray.startLength + var)),
			-transform.up, out hit, hole_ray.length)) &&
			(!Physics.Raycast((transform.position + (transform.forward * angle_mag + (transform.right)).normalized * (hole_ray.startLength - var)),
			-transform.up, out hit, hole_ray.length)) ) {
			hole_ray.hit_right_flg = true;
		}
		else {
			hole_ray.hit_right_flg = false;
		}

		//左のレイ
		if ((!Physics.Raycast((transform.position + (transform.forward * angle_mag + (-transform.right)).normalized * (hole_ray.startLength)) + (transform.right * var),
			-transform.up, out hit, hole_ray.length)) &&
			(!Physics.Raycast((transform.position + (transform.forward * angle_mag + (-transform.right)).normalized * (hole_ray.startLength)) - (transform.right * var),
			-transform.up, out hit, hole_ray.length)) &&
			(!Physics.Raycast((transform.position + (transform.forward * angle_mag + (-transform.right)).normalized * (hole_ray.startLength + var)),
			-transform.up, out hit, hole_ray.length)) &&
			(!Physics.Raycast((transform.position + (transform.forward * angle_mag + (-transform.right)).normalized * (hole_ray.startLength - var)),
			-transform.up, out hit, hole_ray.length))) {
			hole_ray.hit_left_flg = true;
		}
		else {
			hole_ray.hit_left_flg = false;
		}
		// */
		#endregion

		#region 2RayCast 横
		/*
		//右のレイ
		float var = 1;
		if ((!Physics.Raycast((transform.position + (transform.forward * angle_mag + (transform.right)).normalized * (hole_ray.startLength)) + (transform.right*var),
			-transform.up, out hit, hole_ray.length)) &&
			(!Physics.Raycast((transform.position + (transform.forward * angle_mag + (transform.right)).normalized * (hole_ray.startLength)) - (transform.right* var),
			-transform.up, out hit, hole_ray.length))) {
			hole_ray.hit_right_flg = true;
		}
		else {
			hole_ray.hit_right_flg = false;
		}

		//左のレイ
		if ((!Physics.Raycast((transform.position + (transform.forward * angle_mag + (-transform.right)).normalized * (hole_ray.startLength)) + (transform.right* var),
			-transform.up, out hit, hole_ray.length)) &&
			(!Physics.Raycast((transform.position + (transform.forward * angle_mag + (-transform.right)).normalized * (hole_ray.startLength)) - (transform.right* var),
			-transform.up, out hit, hole_ray.length))) {
			hole_ray.hit_left_flg = true;
		}
		else {
			hole_ray.hit_left_flg = false;
		}
		// */
		#endregion

		#region 2RayCast 縦
		/*
		//右のレイ
		if ((!Physics.Raycast(transform.position + (transform.forward * angle_mag + (transform.right)).normalized * (hole_ray.startLength + 1),
			-transform.up, out hit, hole_ray.length)) &&
			(!Physics.Raycast(transform.position + (transform.forward * angle_mag + (transform.right)).normalized * (hole_ray.startLength - 1),
			-transform.up, out hit, hole_ray.length))) {
			hole_ray.hit_right_flg = true;
		}
		else {
			hole_ray.hit_right_flg = false;
		}

		//左のレイ
		if ((!Physics.Raycast(transform.position + (transform.forward * angle_mag + (-transform.right)).normalized * (hole_ray.startLength + 1),
			-transform.up, out hit, hole_ray.length)) &&
			(!Physics.Raycast(transform.position + (transform.forward * angle_mag + (-transform.right)).normalized * (hole_ray.startLength - 1),
			-transform.up, out hit, hole_ray.length))) {
			hole_ray.hit_left_flg = true;
		}
		else {
			hole_ray.hit_left_flg = false;
		}
		// */
		#endregion

		#region RayCast
		/*
		//右のレイ
		if (!Physics.Raycast(transform.position + (transform.forward * angle_mag + transform.right).normalized * wall_ray.length,
			-transform.up, out hit, hole_ray.length)) {
			hole_ray.hit_right_flg = true;
		}
		else {
			hole_ray.hit_right_flg = false;
		}

		//左のレイ
		if (!Physics.Raycast(transform.position + (transform.forward * angle_mag + (-transform.right)).normalized * wall_ray.length,
			-transform.up, out hit, hole_ray.length)) {
			hole_ray.hit_left_flg = true;
		}
		else {
			hole_ray.hit_left_flg = false;
		}
		// */
		#endregion

	}

	//----向き変更
	public void HoleRayRotate() {
		if (hole_ray.hit_right_flg) {
			transform.Rotate(0.0f, -hole_ray.speed, 0.0f);
		}
		else if (hole_ray.hit_left_flg) {
			transform.Rotate(0.0f, hole_ray.speed, 0.0f);
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
		if (Physics.Linecast(chara_ray.position, chara_ray.position + Vector3.down * chara_ray_length, shot_layer)) {
			rigid.useGravity = true;
			is_ground = true;
			velocity.y = 0;
		}
		else {
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


	public virtual void DebugLog(){
		/*
        Debug.DrawRay(chara_ray.position, Vector3.Down * chara_ray_length, Color.red);
        Debug.Log("ground:" + is_ground);
        Debug.Log("vel:"+velocity);
        Debug.Log("vel:"+velocity);
		// */
    }



    public virtual void FixedUpdate(){
		//キャラクターを移動させる処理
		rigid.MovePosition(transform.position + velocity * Time.deltaTime);
	}

	protected bool WaitTimeOnce(int wait_time)
    {
		if (wait_timer >= wait_time)
        {
			return true;
		}
		else
        {
			wait_timer++;
			return false;
		}
	}

}

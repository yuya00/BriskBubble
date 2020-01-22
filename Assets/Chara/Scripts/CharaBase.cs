using System.Collections;
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
	protected				Rigidbody			 rigid;
	protected				Vector3				 velocity;			//速さ(rigd.velocityでも良いかも)
	[Tooltip("走りの速さ")]
	public float			run_speed			 = 15.0f;
	[Tooltip("歩きの速さ")]
    public float			walk_speed			 = 3.0f;
	[Tooltip("ジャンプ力")]
    public float			jump_power			 = 15.0f;			//ジャンプ力
	protected float			stop_fric			 = 0.3f;			//慣性(停止)
	protected float			jump_fric			 = 0;				//慣性(ジャンプ)
	protected float			jump_fric_power		 = 0.7f;            //慣性(ジャンプ)
	protected float         water_fric           = 0;	            //慣性(水)
	public float	        water_fric_power     = 0.7f;            //慣性(水)
	protected bool			is_ground			 = false;           //地面接地判定
    protected bool          is_floor             = false;           //動く床接地判定

    //protected Transform		chara_ray;							//レイを飛ばす位置(地面判別に使用)
    //protected float			chara_ray_length	 = 0.4f;
    //protected CapsuleCollider   capsule_collider;
    protected Vector3       ground_ray_pos       = Vector3.zero;
	protected float         ground_ray_upadjust  = 0.1f;
	protected float         ground_ray_length    = 0.5f;
	[Tooltip("重力の倍率")]
	public float			gravity_power		 = 5;               //重力の倍率
	protected const int		WORK_NUM			 = 8;
	protected int[]			iwork				 = new int[WORK_NUM];
	protected float[]		fwork				 = new float[WORK_NUM];
	[Tooltip("落下速度の速さ上限")]
	public float			fallspd_limit		 = 30.0f;
	[Tooltip("気絶時間")]
	protected bool          is_faint			 = false;			//気絶判定
	[Foldout("BaseParameter" ,false)]
	protected int			wait_timer			 = 0;               //待機タイマー
	protected const int     WAIT_BOX_NUM		 = 5;
	protected int[]         wait_timer_box		 = new int[WAIT_BOX_NUM];               //待機タイマー
	protected const float	HALF				 = 0.5f;            // 半分計算用



	// Gizmo基底 -------------------------------------------
	public class GizmoBase {
		public bool gizmo_on;

		[Header("判定の実行")]
		public bool judge_on;
	}

	// Ray基底 ---------------------------------------------
	public class RayBase : GizmoBase {
		[Header("Rayの長さ")]
		public float length;
	}

	// BoxCast基底 ------------------------------------------
	[System.Serializable]
	public class BoxCastBase : GizmoBase {
		[System.NonSerialized]	//判定
		public bool flg;

		[System.NonSerialized]	//BoxCastの大きさ
		public Vector3 size;
	}

	// BoxCast調整基底 --------------------------------------
	public class BoxCastAdjustBase : RayBase {
		[System.NonSerialized]
		public float box_total;

		[System.NonSerialized]
		public Vector3 box_pos;

		[System.NonSerialized]
		public Vector3 box_size;

		[Header("Rayの高さ(上限)")]
		public float up_limit;

		[Header("Rayの高さ(下限)")]
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


	//着地判定 ---------------------------------------------
	[System.Serializable]
	public class GroundCast : GizmoBase {
		[System.NonSerialized]
		public CapsuleCollider  capsule_collider;
		[System.NonSerialized]
		public Vector3          pos;
		[System.NonSerialized]
		public float            length;

		public const float      RADIUS   = 0.5f;    //半径
		public const float      UPADJUST = 0.2f;    //長さ調整
	}
	[Header("着地判定SphereCast")]
	public GroundCast ground_cast;


	//壁判定Ray ---------------------------------------------
	[System.Serializable]
	public class WallRay : BoxCastAdjustBase {
		//public float length;		//20.0f
		//public float up_limit;	//1.9f	2.7f
		//public float down_limit;	//3.0f	2.5f

		public const int ANGLE_MAG = 3; //角度調整

		[Header("Rayの角度")]	//0.0f 未使用
		public float angle;

		[System.NonSerialized]	//壁との距離保存用
		public float dist_right, dist_left;

		[System.NonSerialized]	//壁との当たり判定
		public bool hit_right_flg, hit_left_flg;

		[System.NonSerialized]	//めり込み判定
		public bool cavein_right_flg, cavein_left_flg;

		[System.NonSerialized]	//両方のRayが当たった回数
		public int both_count;

		[System.NonSerialized]	//両方のRayが当たった回数判定
		public bool both_flg;

		[Header("向き変更の速さ")]
		public float spd;       //1.5f

		public void Clear() {
			dist_right		 = 0;
			dist_left		 = 0;
			hit_right_flg	 = false;
			hit_left_flg	 = false;
			cavein_right_flg = false;
			cavein_left_flg	 = false;
			both_count		 = 0;
			both_flg		 = false;
		}

	}
	[Header("壁判定Ray")]
	public WallRay wall_ray;


	//穴判定Ray ---------------------------------------------
	[System.Serializable]
	public class HoleRay : RayBase {
		//public float length;    //100.0f

		[Header("Rayの始点")]		//11.0f
		public float startLength;

		[Header("Rayの角度")]		//00.0f 未使用
		public float angle;

		//[System.NonSerialized] //穴との距離保存用
		//public float dist_right, dist_left;

		[System.NonSerialized]		//穴との当たり判定
		public bool hit_right_flg, hit_left_flg;

		//[System.NonSerialized] //両方のRayが当たった回数
		//public int both_count;

		//[System.NonSerialized] //両方のRayが当たった回数判定
		//public bool both_flg;

		[Header("向き変更の速さ")]	//15.0f
		public float speed;

		public void Clear() {
			hit_right_flg = false;
			hit_left_flg = false;
		}

	}
	[Header("穴判定Ray")]
	public HoleRay hole_ray;

    // 動く床用
    private GameObject[] floor;
    protected Vector3 floor_pos;

    public virtual void Start()
    {
		rigid = GetComponent<Rigidbody>();
        floor = GameObject.FindGameObjectsWithTag("Ground");

        velocity = Vector3.zero;
		is_ground = false;
        is_floor = false;

        ground_cast.capsule_collider = GetComponent<CapsuleCollider>();
		ground_cast.length = (ground_cast.capsule_collider.height / 2) - GroundCast.UPADJUST;
		for (int i = 0; i < WORK_NUM; i++) {
			iwork[i] = 0;
		}
		for (int i = 0; i < WORK_NUM; i++) {
			fwork[i] = 0;
		}
		for (int i = 0; i < WAIT_BOX_NUM; i++) {
			wait_timer_box[i] = 0;
		}
		wall_ray.Clear();
		hole_ray.Clear();
	}


	void Update()
    {

	}



	//--壁判定による向き変更
	public virtual void WallRayRotate_Judge() {
		if (!wall_ray.judge_on) {
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
			(transform.forward * WallRay.ANGLE_MAG + (transform.right * right_one)).normalized, out hit, wall_ray.length)) {
			if (hit.collider.gameObject.tag == "Wall") {
				wall_ray.dist_right = hit.distance;  //壁との距離保存
				return true;
			}
		}
		return false;
	}

	//------左レイ
	bool WallRayLeft(float limit, int limit_one, int right_one) {
		RaycastHit hit;

		if (Physics.Raycast(wall_ray.box_pos + (transform.up * limit * limit_one),
			(transform.forward * WallRay.ANGLE_MAG + (transform.right * right_one)).normalized, out hit, wall_ray.length)) {
			if (hit.collider.gameObject.tag == "Wall") {
				wall_ray.dist_left = hit.distance;  //壁との距離保存
				return true;
			}
		}
		return false;
	}

	//----向き変更
	public void WallRayRotate() {
		if (wall_ray.hit_right_flg) {
			transform.Rotate(0.0f, -wall_ray.spd, 0.0f);
		}
		else if (wall_ray.hit_left_flg) {
			transform.Rotate(0.0f, wall_ray.spd, 0.0f);
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
		//RaycastHit hit;
		LayerMask wall_layer = (1 << 14);

		//何にも当たっていなかったら
		#region BoxCast
		//右のレイ
		if (!Physics.BoxCast((transform.position + (transform.forward * WallRay.ANGLE_MAG + (transform.right)).normalized * (hole_ray.startLength)),
			Vector3.one,-transform.up, transform.rotation, hole_ray.length,wall_layer)) {
			hole_ray.hit_right_flg = true;
		}
		else {
			hole_ray.hit_right_flg = false;
		}

		//左のレイ
		if (!Physics.BoxCast((transform.position + (transform.forward * WallRay.ANGLE_MAG + (-transform.right)).normalized * (hole_ray.startLength)),
			Vector3.one, -transform.up, transform.rotation, hole_ray.length, wall_layer)) {
			hole_ray.hit_left_flg = true;
		}
		else {
			hole_ray.hit_left_flg = false;
		}
		#endregion

		#region 4RayCast 縦横
		/*
		//右のレイ
		float var = 1;
		if ((!Physics.Raycast((transform.position + (transform.forward * WallRay.ANGLE_MAG + (transform.right)).normalized * (hole_ray.startLength)) + (transform.right * var),
			-transform.up, out hit, hole_ray.length)) &&
			(!Physics.Raycast((transform.position + (transform.forward * WallRay.ANGLE_MAG + (transform.right)).normalized * (hole_ray.startLength)) - (transform.right * var),
			-transform.up, out hit, hole_ray.length)) &&
			(!Physics.Raycast((transform.position + (transform.forward * WallRay.ANGLE_MAG + (transform.right)).normalized * (hole_ray.startLength + var)),
			-transform.up, out hit, hole_ray.length)) &&
			(!Physics.Raycast((transform.position + (transform.forward * WallRay.ANGLE_MAG + (transform.right)).normalized * (hole_ray.startLength - var)),
			-transform.up, out hit, hole_ray.length)) ) {
			hole_ray.hit_right_flg = true;
		}
		else {
			hole_ray.hit_right_flg = false;
		}

		//左のレイ
		if ((!Physics.Raycast((transform.position + (transform.forward * WallRay.ANGLE_MAG + (-transform.right)).normalized * (hole_ray.startLength)) + (transform.right * var),
			-transform.up, out hit, hole_ray.length)) &&
			(!Physics.Raycast((transform.position + (transform.forward * WallRay.ANGLE_MAG + (-transform.right)).normalized * (hole_ray.startLength)) - (transform.right * var),
			-transform.up, out hit, hole_ray.length)) &&
			(!Physics.Raycast((transform.position + (transform.forward * WallRay.ANGLE_MAG + (-transform.right)).normalized * (hole_ray.startLength + var)),
			-transform.up, out hit, hole_ray.length)) &&
			(!Physics.Raycast((transform.position + (transform.forward * WallRay.ANGLE_MAG + (-transform.right)).normalized * (hole_ray.startLength - var)),
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
		//LayerMask shot_layer = ~(1 << 8);

		//Wallのレイヤーが設定されている物とだけ当たる
		LayerMask wall_layer = (1 << 14);
		//LayerMask wall_layer = (1 << 14) | (1 << 16);

		// 動く床
		LayerMask ground_layer = (1 << 16);
        /***********************/


        #region SphereCast
        //足元(に加え,少し後ろにすることで壁に接触しながらジャンプするとすぐ着地してしまう問題を回避)
        ground_cast.pos = transform.position + (transform.up * ground_cast.capsule_collider.center.y) - (transform.forward * 0.2f);

		RaycastHit hit;
		//中心から、足元より少し上の位置までsphereで判定
		if (Physics.SphereCast(ground_cast.pos, GroundCast.RADIUS, -transform.up, out hit, ground_cast.length, wall_layer))
        {
			rigid.useGravity = true;
			is_ground		 = true;
			velocity.y		 = 0;
		}
		else
        {
			is_ground = false;
			//rigid.useGravity = false;

			//落下速度の上限
			if (velocity.y >= -fallspd_limit)
			{
				velocity.y += Physics.gravity.y * gravity_power * Time.deltaTime;
			}
			else
			{
				velocity.y += Physics.gravity.y * gravity_power / 10 * Time.deltaTime;
			}

		}

        // 動く床
        for (int i = 0; i < floor.Length; ++i)
        {
            if (Physics.SphereCast(ground_cast.pos, GroundCast.RADIUS, -transform.up, out hit, ground_cast.length, ground_layer))
            {
                rigid.useGravity = true;
                is_ground = true;
                is_floor = true;
                velocity.y = 0;

                floor_pos = transform.position + floor[i].GetComponent<MoveFloor>().MoveVector;
            }
            else
            {
                is_floor = false;
            }
        }

        #endregion


        #region Ray
        //足元から少し上の位置
        ground_ray_pos = transform.position +
			(transform.up * ground_cast.capsule_collider.center.y) -
			(transform.up * (ground_cast.capsule_collider.height / 2)) +
			(transform.up * ground_ray_upadjust);

		/*
		//下レイが当たっていたら着地
		RaycastHit hit;
		if (Physics.Raycast(ground_ray_pos, -transform.up, out hit, ground_ray_length, shot_layer)) {
			if (hit.collider.tag != "Wall") {
				return;
			}
			rigid.useGravity = true;
			is_ground = true;
			velocity.y = 0;
			//if (this.gameObject.name == "Player") {
			//	Debug.Log("プレイヤー着地");
			//}
		}
		else {
			is_ground = false;
			//落下速度の上限
			if (velocity.y >= -fallspd_limit) {
				velocity.y += Physics.gravity.y * gravity_power * Time.deltaTime;
			}
			else {
				velocity.y += Physics.gravity.y * gravity_power / 10 * Time.deltaTime;
			}
		}
		// */
		#endregion

		#region Line
		/*
		//if (Physics.Linecast(chara_ray.position, chara_ray.position + Vector3.down * chara_ray_length, shot_layer)) {
		//	rigid.useGravity = true;
		//	is_ground = true;
		//	velocity.y = 0;
		//}
		//else {
		//	is_ground = false;
		//	if (velocity.y >= -fallspd_limit) { //落下速度の上限
		//		velocity.y += Physics.gravity.y * gravity_power * Time.deltaTime;
		//	}
		//	else {
		//		velocity.y += Physics.gravity.y * gravity_power / 10 * Time.deltaTime;
		//	}
		//}
		// */
		#endregion


	}



	public virtual void DebugLog(){
		/*
        Debug.DrawRay(chara_ray.position, Vector3.Down * chara_ray_length, Color.red);
        Debug.Log("ground:" + is_ground);
        Debug.Log("vel:"+velocity);
        Debug.Log("vel:"+velocity);
		// */
    }



    public virtual void FixedUpdate()
    {
        //キャラクターを移動させる処理
        //if (this.gameObject.tag == "Player") {
        //	Debug.Log(velocity);
        //}
        rigid.MovePosition(transform.position + velocity * Time.deltaTime);

        //transform.position = transform.position + velocity * Time.deltaTime;
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


	//汎用タイマー(指定時間になったらtrue)
	protected bool WaitTimeBox(int box_num, int wait_time) {
		if (wait_timer_box[box_num] >= wait_time) {
			wait_timer_box[box_num] = 0;
			return true;
		}
		else {
			wait_timer_box[box_num]++;
			return false;
		}
	}

}

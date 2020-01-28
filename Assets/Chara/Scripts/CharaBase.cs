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
	protected const int     WAIT_BOX_NUM		 = 7;
	protected int[]         wait_timer_box		 = new int[WAIT_BOX_NUM];               //待機タイマー
	protected const float	HALF				 = 0.5f;            // 半分計算用
	protected Vector3		floor_spd			 = Vector3.zero;	//動く床のspd


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




    public virtual void Start()
    {
		rigid = GetComponent<Rigidbody>();

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
	}

	void Update()
    {

	}



    //着地時にfalse
    public virtual void Move()
    {
		//Wallのレイヤーが設定されている物とだけ当たる
		//LayerMask wall_layer = (1 << 14);
        LayerMask wall_layer = (1 << 14) | (1 << 16);

		#region SphereCast
		//足元から少し後ろ(壁に接触しながらジャンプするとすぐ着地してしまう問題を回避)
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
			if (velocity.y >= -fallspd_limit){
				velocity.y += Physics.gravity.y * gravity_power * Time.deltaTime;
			}
			else{
				velocity.y += Physics.gravity.y * gravity_power / 10 * Time.deltaTime;
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
		//rigid.MovePosition(transform.position + velocity * Time.deltaTime);
		rigid.MovePosition(transform.position = transform.position + floor_spd + velocity * Time.deltaTime);

		//transform.position = transform.position + velocity * Time.deltaTime;
    }

    // 動く床との判定
    public virtual void FloorHit()
    {
        RaycastHit hit;
        // 動く床
        LayerMask ground_layer = (1 << 16);

		//着地判定より長い判定
        if (Physics.SphereCast(ground_cast.pos, GroundCast.RADIUS, -transform.up, out hit, ground_cast.length * 2, ground_layer)){
			floor_spd = hit.collider.GetComponent<MoveFloor>().MoveVector;
        }
        else{
			floor_spd = Vector3.zero;
		}

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

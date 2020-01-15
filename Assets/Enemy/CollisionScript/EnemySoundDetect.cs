using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySoundDetect : MonoBehaviour {
	private SphereCollider m_sphereCollider = null;
	private bool hit_flg;
	private Vector3 hit_pos;

	private bool found_shot_flg;
	private Vector3 found_hit_pos;

	public bool gizmo_on;
	[SerializeField]
	private Color gizmo_color;

	private void Awake() {
		m_sphereCollider = GetComponent<SphereCollider>();
	}

	// 当たり判定 -------------------------------------------------------
	private void OnTriggerStay(Collider other) {
		if (other.gameObject.tag == "Shot" && other.GetComponent<ShotBase>().ApperFg) {
			hit_flg = true;
			hit_pos = other.gameObject.transform.position;
		}
		if (other.gameObject.tag == "Shot") {
			found_hit_pos = other.gameObject.transform.position;
		}
	}
	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "Shot" && other.GetComponent<ShotBase>().ApperFg) {
			hit_flg = true;
			hit_pos = other.gameObject.transform.position;
		}
		if (other.gameObject.tag == "Shot") {
			found_shot_flg = true;
		}
	}


	// GUI表示 -----------------------------------------------------------
	void OnGUI() {
		//GUILayout.Space(50);
		//GUILayout.TextArea("hit_flg\n" + hit_flg);
	}


	// get -----------------------------------------------------------
	//当たり判定を返す
	public bool HitFlg {
		get { return hit_flg; }
		set { hit_flg = value; }
	}
	//座標を返す
	public Vector3 Hitpos {
		get { return hit_pos; }
	}

	//当たり判定を返す
	public bool FoundShotFlg {
		get { return found_shot_flg; }
		set { found_shot_flg = value; }
	}
	//座標を返す
	public Vector3 FoundHitPos {
		get { return found_hit_pos; }
	}

	//ギズモ用の色を返す
	public Color Gizmo_Color {
		get {
			if (gizmo_color == null) {
				gizmo_color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
			}
			return gizmo_color;
		}
	}

	//コライダーの半径を返す
	public float Radius {
		get {
			if (m_sphereCollider == null) {
				m_sphereCollider = GetComponent<SphereCollider>();
			}
			if (m_sphereCollider != null) return m_sphereCollider.radius;
			else return 0.0f;
			//return m_sphereCollider != null ? m_sphereCollider.radius : 0.0f;
		}
	}

	//Colliderを返す
	public SphereCollider getCollider {
		get {
			if (m_sphereCollider == null) {
				m_sphereCollider = GetComponent<SphereCollider>();
			}
			if (m_sphereCollider != null) return m_sphereCollider;
			return m_sphereCollider;
		}
	}


}

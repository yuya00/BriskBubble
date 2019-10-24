using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySoundDetect : MonoBehaviour {
	private SphereCollider m_sphereCollider = null;
	[System.NonSerialized]
	public bool hit_flg;
	[SerializeField]
	private Color gizmo_color;

	private void Awake() {
		m_sphereCollider = GetComponent<SphereCollider>();
		//m_sphereCollider.radius = 10;
		hit_flg = false;
	}

	// ****************************************************************
	// 当たり判定
	// ****************************************************************
	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "Shot" /*&&other.hitFlg*/ ) {
			hit_flg = true;
		}
	}

	private void OnTriggerExit(Collider other) {
		if (other.gameObject.tag == "Shot" /*&&other.hitFlg*/) {
			hit_flg = false;
		}
	}



	// ****************************************************************
	// get
	// ****************************************************************

	//当たり判定を返す
	public bool HitFlg {
		get { return hit_flg; }
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

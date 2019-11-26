using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyNear : MonoBehaviour
{
	private SphereCollider m_sphereCollider = null;
	[System.NonSerialized]
	public bool hit_flg;

	public bool gizmo_on;
	[SerializeField]
	private Color gizmo_color;


	private void Awake() {
		m_sphereCollider = GetComponent<SphereCollider>();
		hit_flg = false;
	}


	// ****************************************************************
	// 当たり判定
	// ****************************************************************
	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "Player") {
			hit_flg = true;
		}
	}

	private void OnTriggerExit(Collider other) {
		if (other.gameObject.tag == "Player") {
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

	//ギズモ用の色を返す
	public Color Gizmo_Color {
		get {
			if (gizmo_color == null) {
				gizmo_color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
			}
			return gizmo_color;
		}
	}

}

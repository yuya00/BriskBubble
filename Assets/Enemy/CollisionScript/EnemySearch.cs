using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySearch : MonoBehaviour
{
	public bool gizmo_on;
	[SerializeField, Range(0.0f, 360.0f),Header("視界の広さ")]
	private float m_angle = 0.0f;
	private float m_cos = 0.0f;

	[SerializeField]
	private Color gizmo_color;

	private SphereCollider m_sphereCollider = null;
	private List<FoundData> m_foundList = new List<FoundData>();


	private void Awake() {
		m_sphereCollider = GetComponent<SphereCollider>();
		ApplySearchAngle();
	}

	//シリアライズされた値がインスペクター上で変更されたら呼ばれる
	private void OnValiable() {
		ApplySearchAngle();
	}

	private void OnDisable() {
		m_foundList.Clear();
	}

	//cos保存
	private	void ApplySearchAngle() {
		float searchRad = m_angle * 0.5f * Mathf.Deg2Rad;
		m_cos = Mathf.Cos(searchRad);
	}



	// ****************************************************************
	// 角度判定
	// ****************************************************************
	#region 角度判定
	private void Update() {
		UpdateFoundObject();
	}

	private void UpdateFoundObject() {
		foreach (var foundData in m_foundList) {
			GameObject targetObject = foundData.Obj;
			if (targetObject == null) {
				continue;
			}

			bool isFound = CheckFoundObject(targetObject);
			foundData.Update(isFound);

			if (foundData.IsFound()) {
				onFound(targetObject);
			}
			else if (foundData.IsLost()) {
				onLost(targetObject);
			}
		}
	}

	private bool CheckFoundObject(GameObject i_target) {
		Vector3 targetPosition	 = i_target.transform.position;
		Vector3 myPosition		 = transform.position;

		Vector3 myPositionXZ	 = Vector3.Scale(myPosition, new Vector3(1.0f, 0.0f, 1.0f));
		Vector3 targetPositionXZ = Vector3.Scale(targetPosition, new Vector3(1.0f, 0.0f, 1.0f));

		Vector3 toTargetFlatDir	 = (targetPositionXZ - myPositionXZ).normalized;

		Vector3 myForward = transform.forward;
		//↑が正しいはずだけど、逆方向に扇型判定がついてしまうので、無理やり逆にした。
		//Vector3 myForward		= transform.forward * -1;

		if (!IsWithinRangeAngle(myForward, toTargetFlatDir, m_cos)) {
			return false;
		}

		Vector3 toTargetDir = (targetPosition - myPosition).normalized;
		if (!IsHitRay(myPosition, toTargetDir, i_target)) {
			return false;
		}


		return true;
	}



	private bool IsWithinRangeAngle(Vector3 i_forwardDir, Vector3 i_toTargetDir, float i_cosTheta) {
		// 方向ベクトルが無い場合は、同位置にあるものだと判断する。
		if (i_toTargetDir.sqrMagnitude <= Mathf.Epsilon) {
			return true;
		}

		float dot = Vector3.Dot(i_forwardDir, i_toTargetDir);
		return dot >= i_cosTheta;
	}


	private bool IsHitRay(Vector3 i_fromPosition, Vector3 i_toTargetDir, GameObject i_target) {
		// 方向ベクトルが無い場合は、同位置にあるものだと判断する。
		if (i_toTargetDir.sqrMagnitude <= Mathf.Epsilon) {
			return true;
		}

		RaycastHit onHitRay;
		if (!Physics.Raycast(i_fromPosition, i_toTargetDir, out onHitRay, Radius)) {
			return false;
		}

		if (onHitRay.transform.gameObject != i_target) {
			return false;
		}

		return true;
	}
	#endregion




	// ****************************************************************
	// get関数
	// ****************************************************************
	#region get関数
	//角度を返す
	public float Angle { get { return m_angle; } }
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


	//関数を入れられる変数(delegate,event)
	//見つけたら返す
	public event System.Action<GameObject> onFound	 = (obj) => { };
	//見失ったら返す
	public event System.Action<GameObject> onLost	 = (obj) => { };


	//物に当たったら、その物を返す
	private void OnTriggerEnter(Collider other) {
		if (other.tag != "Player") return;
		GameObject enterObject = other.gameObject;

		// 念のため多重登録されないようにする。
		if (m_foundList.Find(value => value.Obj == enterObject) == null) {
			m_foundList.Add(new FoundData(enterObject));
		}
	}

	//物に当たるのをやめたら、その物を返す
	private void OnTriggerExit(Collider other) {
		if (other.tag != "Player") return;
		GameObject exitObject = other.gameObject;

		var foundData = m_foundList.Find(value => value.Obj == exitObject);
		if (foundData == null) {
			return;
		}

		if (foundData.IsCurrentFound()) {
			onLost(foundData.Obj);
		}

		m_foundList.Remove(foundData);

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

	#endregion




	//発見状態を管理
	private class FoundData {

		public FoundData(GameObject i_object) {
			m_obj = i_object;
		}

		private GameObject m_obj = null;
		private bool m_isCurrentFound = false;
		private bool m_isPrevFound = false;

		public GameObject Obj { get { return m_obj; }}
		public Vector3 Position { get { return Obj != null ? Obj.transform.position : Vector3.zero; } }

		public void Update(bool i_isFound ) {
			m_isPrevFound = m_isCurrentFound;
			m_isCurrentFound = i_isFound;
		}



		public bool IsFound() {
			return m_isCurrentFound && !m_isPrevFound;
		}

		public bool IsLost() {
			return !m_isCurrentFound && m_isPrevFound;
		}

		public bool IsCurrentFound() {
			return m_isCurrentFound;
		}
	}
}



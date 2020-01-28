using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class EnemySearchGizmo : MonoBehaviour
{
	/*
	private static readonly int     TRIANGLE_COUNT   = 12;  //
	private static readonly Color   GIZMO_COLOR      = new Color(1.0f, 1.0f, 0.0f, 0.5f);


	//ギズモ描画
	[DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
	private static void DrawEnemyGizmo(EnemySearch search_object, GizmoType gizmo_type) {
		//オフならスキップ
		if (!search_object.gizmo_on)
			return;
		//設定されてなかったらスキップ
		if (search_object.Radius <= 0.0f)
			return;
		if (search_object.Angle <= 0.0f)
			return;

		//ギズモにパラメータ代入
		Transform transform = search_object.transform;
		Vector3 pos = transform.position + Vector3.up * 0.01f; //地面より少し上
		Quaternion rot = transform.rotation;
		Vector3 scale = Vector3.one * search_object.Radius;
		//↑が正しいはずだけど、逆方向に扇型ギズモがついてしまうので、無理やり逆にした。
		//Vector3 scale			 = Vector3.one * search_object.Radius * -1;
		//Gizmos.color			 = GIZMO_COLOR;
		Gizmos.color = search_object.Gizmo_Color;


		if (search_object.Angle > 0.0f) {
			//三角形の頂点順に頂点インデックス配列を作る
			Mesh fan_mesh = CreateFanMesh(search_object.Angle, TRIANGLE_COUNT);
			Gizmos.DrawMesh(fan_mesh, pos, rot, scale);
			//Gizmos.DrawWireSphere(pos, search_object.Radius);
		}
	}


	//--三角形の頂点順に頂点インデックス配列を作る
	private static Mesh CreateFanMesh(float angle, int triangleCount) {

		// ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
		var mesh = new Mesh();
		var vertices = CreateFanVertices(angle, triangleCount); //----頂点の配列
		var triangleIndexes = new List<int>(triangleCount * 3);

		for (int i = 0; i < triangleCount; i++) {
			triangleIndexes.Add(0);
			triangleIndexes.Add(i + 1);
			triangleIndexes.Add(i + 2);
		}

		// ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊

		mesh.vertices = vertices;
		mesh.triangles = triangleIndexes.ToArray();
		mesh.RecalculateNormals();  //三角形と頂点からメッシュの法線を再計算


		return mesh;
	}


	//----頂点の配列
	//----頂点の位置を指定するには、サイン値とコサイン値を使って計算しましょう。
	private static Vector3[] CreateFanVertices(float angle, int triangleCount) {
		//エラー処理
		if (angle <= 0.0f) {
			throw new System.ArgumentException(string.Format("角度がおかしい"));
		}
		if (triangleCount <= 0) {
			throw new System.ArgumentException(string.Format("数がおかしい"));
		}

		var vertices = new List<Vector3>(triangleCount + 2);
		vertices.Add(Vector3.zero); //始点

		//ラジアン角
		angle = Mathf.Min(angle, 360.0f);
		float radian = angle * Mathf.Deg2Rad;
		float startRad = -radian / 2;
		float incRad = radian / triangleCount;

		for (int i = 0; i < triangleCount + 1; i++) {
			float currentRad = startRad + (incRad * i);
			Vector3 vertex = new Vector3(Mathf.Sin(currentRad), 0.0f, Mathf.Cos(currentRad));
			vertices.Add(vertex);
		}

		//配列に変換して返す
		return vertices.ToArray();
	}

	// */


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemySet : MonoBehaviour
{
	[System.Serializable]
	public struct Enemy_set {		//敵セット
		public Enemy	enemy;		//敵プレハブ(番号でも良いかも)
		public Vector3	pos;		//座標
		private int		ai_level;	//AIレベル
		[System.NonSerialized]
		public bool		exist;		//生存判定
	}
	public Enemy_set[] enemy_set;


	// Start is called before the first frame update
	void Start()
    {
		Set();  //敵セット
	}

	//敵セット
	void Set() {
		for (int i = 0; i < enemy_set.Length; i++) {
			Instantiate(
				enemy_set[i].enemy,
				enemy_set[i].pos,
				enemy_set[i].enemy.transform.rotation);
			enemy_set[i].exist = true;
		}
	}


	// Update is called once per frame
	void Update()
    {

	}



}

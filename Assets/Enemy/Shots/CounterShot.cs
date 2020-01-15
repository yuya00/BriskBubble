using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CounterShot : MonoBehaviour
{
	Rigidbody rigid;
	Collider col;

	private int timer;

	[SerializeField]
	private float spd;

	[SerializeField]
	private int delete_time;
	private int gravity_adjust_time;


	void Start()
    {
		rigid = GetComponent<Rigidbody>();
		col = GetComponent<BoxCollider>();
    }


	void Update()
    {
		//前移動
		transform.position += transform.forward * spd;


		//指定時間経ったら消去
		if (timer >= delete_time) {
			Destroy(gameObject);
		}
		//消去の少し前に重力をつける
		else if (timer >= (delete_time - gravity_adjust_time)) {
			rigid.useGravity = true;
		}
		timer++;


		//プレイヤーか、ショットに当たったら消去
		if (col.isTrigger) {
			Destroy(gameObject);
		}
	}


	private void OnCollisionEnter(Collision other) {
		if (other.gameObject.tag == "Wall") {
			Destroy(gameObject);
		}

		if (other.gameObject.tag == "Player") {
			col.isTrigger = true;
		}
	}

	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "Shot") {
			col.isTrigger = true;
		}
	}

}

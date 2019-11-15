using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class EnemyFinder : MonoBehaviour
public partial class Enemy : CharaBase 
{
	private Renderer		 m_renderer	 = null;
	private List<GameObject> m_targets	 = new List<GameObject>();
	[System.NonSerialized]
	public bool finder_flg;


	private void Awake() {
		m_renderer			 = GetComponentInChildren<Renderer>();

		var searching		 = GetComponentInChildren<EnemySearch>();
		searching.onFound	 += OnFound;
		searching.onLost	 += OnLost;
	}

	private void OnFound(GameObject i_foundObject) {
		if (i_foundObject.tag != "Player") {
			return;
		}
		m_targets.Add(i_foundObject);
		finder_flg = true;
	}

	private void OnLost(GameObject i_lostObject) {
		m_targets.Remove(i_lostObject);
		if (m_targets.Count == 0) {
			finder_flg = false;
		}
	}

}

using UnityEngine;
using System.Collections;

public class NavMeshSimpleFollow : MonoBehaviour {
	NavMeshAgent agent;
	public GameObject player; 
	enum Behaviors { idle, chase, attack, eat};
	
	// Use this for initialization
	void Start () {
		
		agent = GetComponent<NavMeshAgent>(); 
		ZombieControl zCtrl = GetComponent<ZombieControl>(); 
		if (zCtrl.usePursuitScript == false) {
			agent.destination = player.transform.position;
		}
	}
	
	// Update is called once per frame
	void Update () {
		ZombieControl zCtrl = GetComponent<ZombieControl>(); 
		if (zCtrl.usePursuitScript == false && 
		   (zCtrl.behavior == (int) Behaviors.attack || zCtrl.behavior == (int) Behaviors.chase)) {
			agent.enabled = true; 
			Vector3 offSet = (player.transform.position - transform.position);
			offSet.Normalize(); 
			agent.destination = player.transform.position + offSet;
		}
	}
}
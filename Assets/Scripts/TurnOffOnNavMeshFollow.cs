using UnityEngine;
using System.Collections;

/*Script turns off navMesh if turnNavOffOnTrigger = true, 
 * otherwise it turns it on.
 */
public class TurnOffOnNavMeshFollow : MonoBehaviour {

	float timer; 
	// Use this for initialization
	void Start () {
		timer = 0.0f;
	}
	
	
	// Update is called once per frame
	void Update () {
		timer += Time.deltaTime; 	
	}
	
	void OnTriggerEnter(Collider c) {
		if (c.CompareTag("Zombie")){
			if (timer > 0.01){
			ZombieControl zC = c.GetComponent<ZombieControl>();
			zC.usePursuitScript = !zC.usePursuitScript; 
			}
		}
	}
	
	void OnTriggerExit(Collider c) {
		if (c.CompareTag("Zombie")){
			if (timer > 0.01){
			ZombieControl zC = c.GetComponent<ZombieControl>();
			zC.usePursuitScript = !zC.usePursuitScript; 
			}
		}
	}
}

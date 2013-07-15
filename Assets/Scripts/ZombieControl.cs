using UnityEngine;
using System.Collections;

public class ZombieControl : MonoBehaviour {
	
	enum Behaviors { idle, chase, attack, eat};
	
	//**************************************************************
	//MAKE SURE TO SET THIS WHEN YOU ADD A ZOMBIE TO THE SCENE!!!!
	public bool usePursuitScript = false; 	
	//**************************************************************
	//Variables to play around w/ to get better results
	public float timeToHungerChange; 
	public float timeToStopChasing; //Tells how long to chase for after player has left line of sight
	//********************************************************
	public Vector3 lastPosition;
	//These may not need to be public
	public int behavior; 
	public int hungerLevel; //Max is 5
	public float baseSpeed; 
	
	float hungerChangeTimer; 
	float justSpottedPlayerTimer; //Use to make initial zombie war cry
	bool hasSeenPlayer; //Used to turn off chase after lost player
	float hasSeenPlayerTimer; 
	float distanceToPlayer; 
	AutonomousVehicle aVScript; 
	NavMeshAgent nA;	
	GameObject player; 
	// Use this for initialization
	void Start () {
		lastPosition = transform.position; 
		aVScript = GetComponent<AutonomousVehicle>(); 
		nA = GetComponent<NavMeshAgent>();
		player = GameObject.FindGameObjectWithTag("Player"); 
		
		//Initial values for the zombie
		behavior = (int) Behaviors.idle; 
		hungerLevel = 1;
		distanceToPlayer = Mathf.Infinity;
		hungerChangeTimer = 0.0f; 
		timeToHungerChange = 40.0f; 
		justSpottedPlayerTimer = 0.0f; 
		hasSeenPlayer = false; 
		
		//Set initial speed for two speed control
		baseSpeed = 6.0f; 
		nA.speed = baseSpeed;
		aVScript.MaxSpeed = baseSpeed;
	}
	
	// Update is called once per frame
	void Update () {
		//Turn on or off navMeshAgent component depending on what script using
		if (usePursuitScript) {
			nA.enabled = false;
		} else {
			 nA.enabled = true;
		}
		
		//Update hunger level based on timer
		hungerChangeTimer += Time.deltaTime; 
		if (hungerChangeTimer > timeToHungerChange && hungerLevel < 5) {
			hungerLevel += 1; 
			nA.speed += 1.0f; 
			aVScript.MaxSpeed += 1.0f; 
			hungerChangeTimer = 0.0f; 
		}
		
		//Check if can see player & act based on distance
		if (inLineOfSight()) {
			hasSeenPlayer = true; 
			if (distanceToPlayer < 1) behavior = (int) Behaviors.eat; 
			else if (distanceToPlayer < 5) behavior = (int) Behaviors.attack; 
			else behavior = (int) Behaviors.chase; 		
		} else {
			//Check to see if player just left zombie's line of sight
			if (hasSeenPlayer) {
				hasSeenPlayerTimer += Time.deltaTime; 
				if (hasSeenPlayerTimer < timeToStopChasing) {
					behavior = (int) Behaviors.chase; 
				} else 	{
					hasSeenPlayer = false; 
					hasSeenPlayerTimer = 0.0f; 
					behavior = (int) Behaviors.idle; 
				}
			}
		}		
	}
	
	bool inLineOfSight() {
		//TODO - fill in w/real line of sight
		distanceToPlayer = Vector3.Distance(player.transform.position, transform.position); 
		if (distanceToPlayer < 40.0f) return true; 
		return false; 
		/*
		float fov = 80.0f;
		RaycastHit hit; 
		Vector3 agentPos = transform.position;
		Vector3 playerPos = playerTrans.position;
		//playerPos.y = 1.0f; 
		//agentPos.y = 1.0f; 
	    if (Vector3.Angle(playerTrans.position - transform.position, transform.forward) <= fov) {
			if (Physics.Linecast(agentPos, playerPos, out hit)) {
				Debug.Log("Hit: " + hit.collider.gameObject.name); 
				if (hit.collider.gameObject.name == player.name) return true;
				if (Vector3.Distance(hit.collider.transform.position, transform.position) > 
					Vector3.Distance(player.transform.position, transform.position))
					return true;
			}		
	    }

		return false; 	*/	
	}
	
	//Make sure that char controller collisions don't cause weird behavior
	//public float pushPower = 2.0F;
    void OnControllerColliderHit(ControllerColliderHit hit) {
		if(hit.collider.GetType() == typeof(CharacterController) || hit.gameObject.CompareTag("Player"))  {
			transform.position = lastPosition;
		}		
		//Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
		//transform.Translate (-pushDir * pushPower);
    }
}

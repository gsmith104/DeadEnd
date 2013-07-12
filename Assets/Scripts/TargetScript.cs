using UnityEngine;
using System.Collections;

public class TargetScript: MonoBehaviour 
{
	public Camera perspCamera;
	public Camera topCamera;
	RaycastHit hitInfo; 
	
	void Update () //set target using Ctrl and mouse
	{
		
		if (Input.GetKey(KeyCode.LeftControl))
		{
		
			if (Input.GetMouseButton(0) )
			{
				Ray ray;
				
				if (perspCamera.camera.enabled == true)
					ray =  perspCamera.ScreenPointToRay(Input.mousePosition);
				else
					ray =  topCamera.ScreenPointToRay(Input.mousePosition);
		
				if (Physics.Raycast(ray, out hitInfo, perspCamera.farClipPlane) && (hitInfo.collider.name == "Tile 1" || hitInfo.collider.name == "Tile 2")) 
				{
					transform.position = hitInfo.point;
					transform.position = new Vector3 (transform.position.x, 0.0f, transform.position.z);
				}
			}
		}
	}
}

/*Quantum Theory Entertainment - Mesh Combine Utility 12/31/2012
 * http://www.qt-ent.com
 * 
 * This Mesh Combine editor script will take the selected gameobjects with meshfilters and create new objects based on the
 * number of materials used. It's very useful for reducing drawcalls for faster GPU performance.
 * 
 * Generating UVs for lightmaps is supported.
 * 
 * */

using UnityEditor;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class QT_CombineMeshes : EditorWindow 
{
	
	[MenuItem("Window/UCP - Combine Meshes %&c")]
	 static void Init () 
     {
          QT_CombineMeshes window = (QT_CombineMeshes)EditorWindow.GetWindow(typeof(QT_CombineMeshes));
          window.Show();
		window.title="Combiner";
		window.maxSize = new Vector2(460,155);
		window.minSize = window.maxSize;
     }
	
	public bool destroyAfterOptimized = false;
	private bool keepLayer=true;
	private bool isLightmapped=true;
	private bool castShadows=true,receiveShadows=true,addMeshCollider=true,isStatic=true,createParentGO=true;
	private string newName="";	
	private int layer=0;
	private List<GameObject> newObjects = new List<GameObject>(); //holds new objects.
	
	//private List<Vector3> oldPositions = new List<Vector3>(); //contains positions of GOs prior to combine
	//private List<Quaternion> oldRotations = new List<Quaternion>();//rotations prior to combine
	

	

	
	private void OnGUI()
	{	
		int matcount  = GetMaterialCount();
		int combinedmatcount = GetCombinedMaterialCount();
		
		if(matcount>0)
		{	
			if(newName.Equals(""))
				newName=Selection.gameObjects[0].name+"-Combined";
			newName=EditorGUILayout.TextField("New Mesh Name:",newName);		
			
			EditorGUILayout.LabelField("Draw Calls in Selection:",matcount.ToString());
			EditorGUILayout.LabelField("Draw Calls Combined:",combinedmatcount.ToString());
			EditorGUILayout.BeginHorizontal();
			isStatic = EditorGUILayout.Toggle("is Static",isStatic);	
			isLightmapped = EditorGUILayout.Toggle("Generate Lightmap UVs:",isLightmapped);			
			EditorGUILayout.EndHorizontal();			
			EditorGUILayout.BeginHorizontal();
			castShadows = EditorGUILayout.Toggle("Cast Shadows:",castShadows);
			receiveShadows = EditorGUILayout.Toggle("Receive Shadows:",receiveShadows);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			keepLayer = EditorGUILayout.Toggle("Keep Layer Choice:",keepLayer);
			if(keepLayer)			
				layer = Selection.gameObjects[0].layer;
			else
				layer=0;
			
			addMeshCollider=EditorGUILayout.Toggle("Add Mesh Collider:",addMeshCollider);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			destroyAfterOptimized = EditorGUILayout.Toggle("Remove Originals:",destroyAfterOptimized);	
			createParentGO = EditorGUILayout.Toggle("Parent to Game Object",createParentGO);
			EditorGUILayout.EndHorizontal();
			if(GUILayout.Button("Combine Selected"))
			{
					newObjects.Clear();				
					Undo.RegisterSceneUndo("Combined Meshes");
					Combine();	
					Selection.objects = newObjects.ToArray();
				
			}
			
		}
		else		
			EditorGUILayout.LabelField("Draw Calls in Selection: 0");
	}
	

	
	private Component[] GetMeshFilters()
	{
		List<Component> filters = new List<Component>();
		Component[] temp=null;
		for (int x=0;x<Selection.gameObjects.Length;x++)
		{
			temp = Selection.gameObjects[x].GetComponentsInChildren(typeof(MeshFilter));
			for(int y=0;y<temp.Length;y++)
				filters.Add(temp[y]);
			
		}
		return filters.ToArray();
		
	}
	
		private int GetMaterialCount()
	{		
		List<Material> mats = new List<Material>();
		
		Material[] tempmats = null;
		
		for(int x=0;x<Selection.gameObjects.Length;x++)
		{
			GameObject thisGO = Selection.gameObjects[x];
			if(thisGO.GetComponent(typeof(Renderer)))
			{
				Component tempcomp = thisGO.GetComponent(typeof(Renderer));
				tempmats = tempcomp.renderer.sharedMaterials;
				
			}
			else
			{
				Component[] tempcomp = thisGO.GetComponentsInChildren(typeof(Renderer));
				List<Material> tm = new List<Material>();
				foreach(Component c in tempcomp)
				{
					Material[] tm2 = c.renderer.sharedMaterials;
					foreach(Material m in tm2)
						tm.Add(m);
				}
				tempmats = tm.ToArray();
			}
			
			for(int y=0;y<tempmats.Length;y++)
			{
				//if(!mats.Contains(tempmats[y]))
					mats.Add(tempmats[y]);
			}
			
		}
		return mats.Count;
	
	}
	
	private int GetCombinedMaterialCount()
	{
		List<Material> mats = new List<Material>();
		
		Material[] tempmats = null;
		
		for(int x=0;x<Selection.gameObjects.Length;x++)
		{
			GameObject thisGO = Selection.gameObjects[x];
			if(thisGO.GetComponent(typeof(Renderer)))
			{
				Component tempcomp = thisGO.GetComponent(typeof(Renderer));
				tempmats = tempcomp.renderer.sharedMaterials;
				
			}
			else
			{
				Component[] tempcomp = thisGO.GetComponentsInChildren(typeof(Renderer));
				List<Material> tm = new List<Material>();
				foreach(Component c in tempcomp)
				{
					Material[] tm2 = c.renderer.sharedMaterials;
					foreach(Material m in tm2)
						tm.Add(m);
				}
				tempmats = tm.ToArray();
			}
			
			for(int y=0;y<tempmats.Length;y++)
			{
				if(!mats.Contains(tempmats[y]))
					mats.Add(tempmats[y]);
			}
			
		}
		
		
		
		return mats.Count;
		
	}
	
	private int GetMeshFilterCount()
	{
		List<Component> filters = new List<Component>();
		Component[] temp=null;
		for (int x=0;x<Selection.gameObjects.Length;x++)
		{
			temp = Selection.gameObjects[x].GetComponentsInChildren(typeof(MeshFilter));
			for(int y=0;y<temp.Length;y++)
				filters.Add(temp[y]);
			
		}
		return filters.Count;
		
	}	
	private List<Quaternion> StoreOriginalQuaternions(GameObject[] GO)
	{		
		List<Quaternion> quats = new List<Quaternion>();
		for(int x=0;x<GO.Length;x++)
		{			
			Quaternion q = new Quaternion(GO[x].transform.localRotation.x,GO[x].transform.localRotation.y,GO[x].transform.localRotation.z,GO[x].transform.localRotation.w);
			quats.Add(q);
		}
		return quats;
	}
	
	private List<Vector3> StoreOriginalPositions(GameObject[] GO)
	{
		List<Vector3> pos = new List<Vector3>();
		for(int x=0;x<GO.Length;x++)
		{			
			Vector3 p = new Vector3(GO[x].transform.position.x,GO[x].transform.position.y,GO[x].transform.position.z);
			pos.Add(p);
		
		}
		return pos;
	}
	
	private void OnInspectorUpdate()
	{
		Repaint();
		
	}
	/// <summary>
	/// Combines selected gameobjects that have meshfilters to the lowest possible meshcount.
	/// </summary>
	private void Combine() 
	{
		GameObject GO_Parent = Selection.gameObjects[0];
		GameObject[] oldGameObjects = Selection.gameObjects;
		Vector3 oldPosition = new Vector3(GO_Parent.transform.position.x,GO_Parent.transform.position.y,GO_Parent.transform.position.z);
		
//		oldPositions.Clear();
//		oldRotations.Clear();
//		oldPositions = StoreOriginalPositions(oldGameObjects);
//		oldRotations = StoreOriginalQuaternions(oldGameObjects);
		
		Component[] filters  = GetMeshFilters();
		Matrix4x4 myTransform = GO_Parent.transform.worldToLocalMatrix;
		Hashtable materialToMesh= new Hashtable();
		
		for (int i=0;i<filters.Length;i++) 
		{
			MeshFilter filter = (MeshFilter)filters[i];
			Renderer curRenderer  = filters[i].renderer;
			MeshCombineUtility.MeshInstance instance = new MeshCombineUtility.MeshInstance ();
			instance.mesh = filter.sharedMesh;
			if (curRenderer != null && curRenderer.enabled && instance.mesh != null)
			{
				instance.transform = myTransform * filter.transform.localToWorldMatrix;
				
				Material[] materials = curRenderer.sharedMaterials;
				for (int m=0;m<materials.Length;m++) 
				{
					instance.subMeshIndex = System.Math.Min(m, instance.mesh.subMeshCount - 1);
	
					ArrayList objects = (ArrayList)materialToMesh[materials[m]];
					if (objects != null) 
						objects.Add(instance);					
					else
					{
						objects = new ArrayList ();
						objects.Add(instance);
						materialToMesh.Add(materials[m], objects);
					}
				}
			}
		}
		
		int nameCount =1; //used for multimesh naming.
		
	//for each material found
		foreach (DictionaryEntry de  in materialToMesh) 
		{
			ArrayList elements = (ArrayList)de.Value;
			MeshCombineUtility.MeshInstance[] instances = (MeshCombineUtility.MeshInstance[])elements.ToArray(typeof(MeshCombineUtility.MeshInstance));
			
				GameObject go = new GameObject("Combined Mesh");
                if (keepLayer) 
					go.layer = GO_Parent.layer;	
				// transforms should be zeroed out, then reset when we place the new object em.
				go.transform.localScale = Vector3.one;
				go.transform.localRotation = GO_Parent.transform.localRotation;
				go.transform.localPosition = Vector3.zero;		
				go.transform.position = Vector3.zero;
				go.AddComponent(typeof(MeshFilter));
				go.AddComponent("MeshRenderer");
				go.renderer.material = (Material)de.Key;
				MeshFilter filter = (MeshFilter)go.GetComponent(typeof(MeshFilter));				
                filter.sharedMesh = MeshCombineUtility.Combine(instances, false);
               	filter.renderer.receiveShadows = receiveShadows;
				filter.renderer.castShadows = castShadows;
				go.isStatic = isStatic;
				if(isLightmapped)
					Unwrapping.GenerateSecondaryUVSet(filter.sharedMesh);
				if(addMeshCollider)
					go.AddComponent<MeshCollider>();		
				//add the new object to our list.
				newObjects.Add(go);		
				
				
		}
		
		
		if(destroyAfterOptimized)
		{
			for(int x=0;x<oldGameObjects.Length;x++)
				DestroyImmediate(oldGameObjects[x]);
		}

		//if we found unique materials, make sure we name the GO's properly.
		if(newObjects.Count>1)
		{
		for(int x=0;x<newObjects.Count;x++)
			{
				if(x>0)
					newObjects[x].name=newName+nameCount;
				else
					newObjects[0].name = newName;
				nameCount++;
				newObjects[x].transform.position = oldPosition;
			}
		}
		else
		{
			newObjects[0].name=newName;		
			newObjects[0].transform.position = oldPosition;
		}
		
		if(createParentGO)
		{
			GameObject p = new GameObject(newName);
			p.transform.position = oldPosition;
			foreach (GameObject g in newObjects)
				g.transform.parent = p.transform;
		}
	}
	
	
	private Transform[] GetTransforms(List<GameObject> gameObjects)
	{
		List<Transform> lt = new List<Transform>();
		for(int x=0;x<gameObjects.Count;x++)
		{
			lt.Add(gameObjects[x].transform);
		}
		return lt.ToArray();
	} 
}


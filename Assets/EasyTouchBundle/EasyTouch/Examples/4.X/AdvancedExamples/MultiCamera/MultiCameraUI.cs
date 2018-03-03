using UnityEngine;
using System.Collections;
using HedgehogTeam.EasyTouch;

public class MultiCameraUI : MonoBehaviour {

	public Camera cam2;
	public Camera cam3;

	public void AddCamera2(bool value){

		AddCamera( cam2,value);	
	}

	public void AddCamera3(bool value){
		AddCamera( cam3,value);		
	}

	public void AddCamera(Camera cam,bool value){

		if (value){
			EasyTouch.AddCamera( cam,false);
		}
		else{
			EasyTouch.RemoveCamera( cam);
		}
	}


}

using UnityEngine;
using System.Collections;
using HedgehogTeam.EasyTouch;

public class MultiLayerUI : MonoBehaviour {

	public void SetAutoSelect(bool value){
		EasyTouch.SetEnableAutoSelect( value );
	}

	public void SetAutoUpdate( bool value){
		EasyTouch.SetAutoUpdatePickedObject( value);
	}


	public void Layer1( bool value){

		LayerMask mask = EasyTouch.Get3DPickableLayer();

		if (value)
			mask = mask | (1<<8);
		else{
			mask = ~mask; 
			mask = ~(mask | (1<<8));
		}

		EasyTouch.Set3DPickableLayer( mask);
	}

	public void Layer2( bool value){

		LayerMask mask = EasyTouch.Get3DPickableLayer();

		if (value)
				mask = mask | (1<<9);
		else{
			mask = ~mask; 
			mask = ~(mask | (1<<9));
		}

		EasyTouch.Set3DPickableLayer( mask);
	}

	public void Layer3( bool value){

		LayerMask mask = EasyTouch.Get3DPickableLayer();

		if (value)
			mask = mask | (1<<10);
		else{
			mask = ~mask; 
			mask = ~(mask | (1<<10));
		}

		EasyTouch.Set3DPickableLayer( mask);
	}

}

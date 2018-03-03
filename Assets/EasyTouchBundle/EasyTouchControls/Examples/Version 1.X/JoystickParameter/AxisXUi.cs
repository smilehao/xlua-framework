using UnityEngine;
using System.Collections;

public class AxisXUi : MonoBehaviour {

	public void ActivateAxisX( bool value){
		ETCInput.SetAxisEnabled( "Horizontal",value);
	}

	public void InvertedAxisX( bool value){
		ETCInput.SetAxisInverted("Horizontal",value);
	}

	public void DeadAxisX( float value){
		ETCInput.SetAxisDeadValue("Horizontal",value);
	}

	public void SpeedAxisX(float value){
		ETCInput.SetAxisSensitivity( "Horizontal",value);
	}

	public void IsInertiaX(bool value){
		ETCInput.SetAxisInertia( "Horizontal",value);
	}

	public void InertiaSpeedX( float value){
		ETCInput.SetAxisInertiaSpeed( "Horizontal",value);
	}


	public void ActivateAxisY( bool value){
		ETCInput.SetAxisEnabled( "Vertical",value);
	}
	
	public void InvertedAxisY( bool value){
		ETCInput.SetAxisInverted("Vertical",value);
	}
	
	public void DeadAxisY( float value){
		ETCInput.SetAxisDeadValue("Vertical",value);
	}
	
	public void SpeedAxisY(float value){
		ETCInput.SetAxisSensitivity( "Vertical",value);
	}
	
	public void IsInertiaY(bool value){
		ETCInput.SetAxisInertia( "Vertical",value);
	}
	
	public void InertiaSpeedY( float value){
		ETCInput.SetAxisInertiaSpeed( "Vertical",value);
	}
}

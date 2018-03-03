using UnityEngine;
using System.Collections;
using HedgehogTeam.EasyTouch;

public class BallRunPlayer : MonoBehaviour {

	public Transform ballModel;

	private bool start =false;
	private Vector3 moveDirection;
	private CharacterController characterController;
	private Vector3 startPosition;
	private bool isJump = false;

	void OnEnable(){
		EasyTouch.On_SwipeEnd += On_SwipeEnd;
	}

	void OnDestroy(){
		EasyTouch.On_SwipeEnd -= On_SwipeEnd;
	}

	void Start(){
		characterController = GetComponent<CharacterController>();
		startPosition = transform.position;

	}

	void Update () {

		if (start){
			moveDirection = transform.TransformDirection(Vector3.forward)* 10f * Time.deltaTime;
			moveDirection.y -= 9.81f * Time.deltaTime;

			if (isJump){
				moveDirection.y = 8f;
				isJump = false;
			}
			characterController.Move(  moveDirection);
			ballModel.Rotate( Vector3.right * 400 * Time.deltaTime);
		}

		if (transform.position.y<0.5){
			start=false;
			transform.position = startPosition;
		}
	}

	void OnCollision(){
		Debug.Log("ok");
	}

	void On_SwipeEnd (Gesture gesture){
	
		if (start){
			switch (gesture.swipe){
				case EasyTouch.SwipeDirection.DownLeft:
				case EasyTouch.SwipeDirection.UpLeft:
				case EasyTouch.SwipeDirection.Left:
					transform.Rotate(Vector3.up * -90);
			
					break;
				case EasyTouch.SwipeDirection.DownRight:
				case EasyTouch.SwipeDirection.UpRight:
				case EasyTouch.SwipeDirection.Right:
					transform.Rotate(Vector3.up * 90);
					break;
				case EasyTouch.SwipeDirection.Up:
					if (characterController.isGrounded){
						isJump = true;
					}
					break;
			}
		}
		
		
	}

	public void StartGame(){
		start = true;
	}
}

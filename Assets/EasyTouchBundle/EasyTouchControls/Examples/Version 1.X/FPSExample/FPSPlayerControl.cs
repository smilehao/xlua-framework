using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FPSPlayerControl : MonoBehaviour {

	public AudioClip gunSound;
	public AudioClip reload;
	public AudioClip needReload;

	public ParticleSystem shellParticle;
	public GameObject muzzleEffect;
	public GameObject impactEffect;
	public Text armoText;

	//private bool inJump = false;
	//private float jumpStart;

	private bool inFire = false;
	private bool inReload = false;
	private Animator anim;
	private int armoCount = 30;

	private AudioSource audioSource;

	void Awake(){
		anim = GetComponentInChildren<Animator>();
		audioSource = GetComponent<AudioSource>();
	}

	void Update(){

		// Firing
		if (ETCInput.GetButton("Fire")){
			if (!inFire && armoCount>0 && !inReload){
				inFire = true;
				anim.SetBool( "Shoot", true);
				InvokeRepeating( "GunFire", 0.12f,0.12f);
				GunFire();
			}
		}

		if (ETCInput.GetButtonDown("Fire") && armoCount==0 && !inReload){
			audioSource.PlayOneShot(needReload,1);
		}

		if (ETCInput.GetButtonUp("Fire")){

			anim.SetBool("Shoot", false);
			muzzleEffect.SetActive(false);
			inFire = false;
			CancelInvoke();
		}

		// reload
		if (ETCInput.GetButtonDown("Reload")){

			inReload = true;
			audioSource.PlayOneShot(reload,1);
			anim.SetBool( "Reload",true);
			StartCoroutine( Reload() );
		}

		// UTurn
		if (ETCInput.GetButtonDown("Back")){
			transform.Rotate( Vector3.up * 180 );
		}

		// Jump
		/*
		if (ETCInput.GetButtonDown("Jump")){
			inJump = true;
			jumpStart = transform.position.y;
		}

		if (inJump && transform.position.y - jumpStart <3f){
			GetComponent<CharacterController>().Move( Vector3.up * 0.5f);
		}
		else{
			inJump = false;
		}*/

		//armo
		armoText.text= armoCount.ToString();
	}
	
	public void MoveStart(){
		anim.SetBool("Move",true);
	}

	public void MoveStop(){
		anim.SetBool("Move",false);
	}
	
	public void GunFire(){

		if (armoCount>0){
			// Muzzle and sound
			muzzleEffect.transform.Rotate( Vector3.forward * Random.Range(0f,360f));
			muzzleEffect.transform.localScale= new Vector3(Random.Range(0.1f,0.2f),Random.Range(0.1f,0.2f),1);

			muzzleEffect.SetActive( true);
			StartCoroutine( Flash ());
			audioSource.PlayOneShot(gunSound,1);

			// shell
			shellParticle.Emit(1);

			// Impact
			Vector3 screenPos = new Vector3(Screen.width/2,Screen.height/2,0);
			screenPos += new Vector3(Random.Range(-10,10),Random.Range(-10,10),0);

			Ray ray = Camera.main.ScreenPointToRay( screenPos);

			RaycastHit[] hits = Physics.RaycastAll(ray);
			if (hits.Length>0){
				Instantiate( impactEffect, hits[0].point - hits[0].normal * -0.2f,Quaternion.identity);
			}
		}
		else{
			anim.SetBool("Shoot", false);
			muzzleEffect.SetActive(false);
			inFire = false;
		}
		//
		armoCount--;
		if (armoCount<0) armoCount=0;
	}
	  
	public void TouchPadSwipe(bool value){
		ETCInput.SetControlSwipeIn("FreeLookTouchPad",value);
	}

	IEnumerator Flash(){
		yield return new WaitForSeconds(0.08f);
		muzzleEffect.SetActive( false);
	}

	IEnumerator Reload(){
		yield return new WaitForSeconds(0.50f);
		armoCount =30;
		inReload = false;
		anim.SetBool( "Reload",false);
	}
}

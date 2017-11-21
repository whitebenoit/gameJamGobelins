using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

	[System.Serializable]
	public struct SoundEvent{
		public string TileName;
		public AudioSource Source;
	}

	public SoundEvent [] Sounds;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnTriggerEnter2D(Collider2D col){
		Debug.Log (col.gameObject.tag);
		for (int i = 0; i < Sounds.Length; i++) {
			if (Sounds [i].TileName == col.gameObject.tag) {
				Sounds [i].Source.Play ();
				Debug.Log (Sounds [i].TileName);
			}
				
		}
	}
}

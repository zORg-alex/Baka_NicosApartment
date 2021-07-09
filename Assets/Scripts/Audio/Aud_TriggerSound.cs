using UnityEngine;
using System.Collections;

public class Aud_TriggerSound : MonoBehaviour {
	
	public AudioClip[] EnterClips;
	bool enterClipsNotDefined = false;
	public AudioClip[] StayClips;
	public bool CycleOne;
	bool stayClipsNotDefined = false;
	public AudioClip[] ExitClips;
	bool exitClipsNotDefined = false;
	AudioSource AudSrc;

	void Start() {
		AudSrc = GetComponent<AudioSource>();
		if (StayClips.Length > 0) AudSrc.clip = StayClips[1];
		if (EnterClips.Length == 0) enterClipsNotDefined = true;
		if (StayClips.Length == 0) stayClipsNotDefined = true;
		if (ExitClips.Length == 0) exitClipsNotDefined = true;
	}
	
	void OnTriggerEnter (Collider other) {
		//Debug.Log("Enter");
		if (enterClipsNotDefined) return;
		AudSrc.PlayOneShot(EnterClips[Random.Range(0,EnterClips.Length)]);
	}

	void OnTriggerStay (Collider other) {
		if (stayClipsNotDefined) return;
		if (!AudSrc.isPlaying) {
			if (!CycleOne)AudSrc.clip = StayClips[Random.Range(0,StayClips.Length)];
			AudSrc.Play();
		}
	}

	void OnTriggerExit (Collider other) {
		if (exitClipsNotDefined) return;
		if (AudSrc.isPlaying) AudSrc.Stop();
		AudSrc.PlayOneShot(ExitClips[Random.Range(0,ExitClips.Length)]);		
	}
}

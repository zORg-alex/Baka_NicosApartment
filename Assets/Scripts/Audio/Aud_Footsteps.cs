using UnityEngine;
using System.Collections;

public class Aud_Footsteps : MonoBehaviour {
	public AudioSource AudSrc;
	public float MasterVolume;
	//public string zzz;
	[System.Serializable]
	public struct ClipField{
		public AudioClip clip;
		public float strength;
	}
	public ClipField[] Footsteps_wood;
	public float WoodVolume = 1f;//In case need to correct whole set
	public ClipField[] Footsteps_carpet;
	public float CarpetVolume = 1f;
	//public AudioClip[] Footsteps;

	public void Footstep (float strength) {
		if (AudSrc == null) return;
		RaycastHit hit;
		ClipField[] Clips = Footsteps_wood;
		if (Physics.Raycast(transform.position+Vector3.up,Vector3.down, out hit)){
			if (hit.transform.tag == "Muffled") Clips = Footsteps_carpet;
		}
		if (Clips.Length == 0) return;

		int i = Random.Range(0,Clips.Length);
		//if (curClip[i].strength>strength) i = Random.Range(0,curClip.Length);

		//if (strength > curClip[i].strength) 
		float mult = strength/Clips[i].strength;

		if (strength <= 0) strength = 1f;
		if (!AudSrc.isPlaying) {
			if (Clips == Footsteps_carpet) {
				AudSrc.volume = mult * MasterVolume * CarpetVolume;
			}
			else {
				AudSrc.volume = mult * MasterVolume * WoodVolume;
			}
			AudSrc.clip = Clips[i].clip;
			AudSrc.Play();
		}
	}
}

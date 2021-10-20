using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GeorgeAnimationControllerScript))]
public class TestGeorge : MonoBehaviour
{
	public Vector3 startPos;
	public Quaternion startRot;
	public GeorgeAnimationControllerScript george;

	private void OnEnable() {
		startPos = transform.position;
		startRot = transform.rotation;
		george = GetComponent<GeorgeAnimationControllerScript>();
	}

	[Button]
	public void TestTurnTo(float angleDeg) {
		stoppedTurning = false;
		startPos = transform.position;
		startRot = transform.rotation;
		var rot = Quaternion.Euler(0, angleDeg, 0);
		TestData d;
		d = new TestData() { supposedRot = rot, animInitialTurn = george.ConvertDegAngleToAnimAngle(angleDeg)};
		testDataList.Add(d);
		george.SetDestination(george.transform.position + george.transform.rotation * rot * Vector3.forward);
		StopAllCoroutines();
		StartCoroutine(TurnToEnded(d));
	}
	IEnumerator TurnToEnded(TestData d) {
		yield return new WaitUntil(() => stoppedTurning);
		stoppedTurning = false;
		george.StopMoving();
		d.actualRot = transform.rotation * Quaternion.Inverse(startRot);
		d.posDelta = transform.position - startPos;
		d.animActualTurn = george.ConvertDegAngleToAnimAngle(transform.rotation.eulerAngles.y) - george.ConvertDegAngleToAnimAngle(startRot.eulerAngles.y);
	}
	public bool stoppedTurning;
	public void StopTurning() => stoppedTurning = true;

	public List<TestData> testDataList = new List<TestData>();
}

[Serializable]
public class TestData {
	public Vector3 posDelta;
	public Quaternion actualRot;
	public Quaternion supposedRot;
	public float animActualTurn;
	public float animInitialTurn;
}

using System;
using UnityEngine;
/// <summary>
/// Use this to hide Monobehaviour from editor and make them no way to be added manually
/// </summary>
public static partial class Utils {
	/// <summary>
	/// Set <see cref="Initialize"/> <see cref="Action"/> in public ctor.
	/// It will be called on Start and after assembly reload in OnEnable message.
	/// You can set it to a method, like: Initialize = MethodName;
	/// or use Lambda expression like: Initialize = () => { //DoSomething }; if you feel to be extra pervert
	/// </summary>
	public class AssemblyReloadableMonoBehaviour : MonoBehaviour {
		protected void OnEnable() {
			if (!initilized && Initialize != null) {
				Initialize();
				initilized = true;
			}
		}
		private void OnDisable() {
			initilized = false;
		}

		internal bool initilized { get; private set; }
		/// <summary>
		/// Assign it's value in ctor. Here should go anything that don't get serialized by Unity.
		/// </summary>
		internal Action Initialize;
	}


	public static float DistanceTo(this Vector3 vector1, Vector3 vector2) =>
		Vector3.Distance(vector1, vector2);
}
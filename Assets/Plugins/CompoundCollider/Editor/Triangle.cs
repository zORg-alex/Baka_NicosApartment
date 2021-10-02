using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZorgsCompoundColliders {
	public struct Triangle : IEnumerable<Vector3> {
		public Vector3 v0;
		public Vector3 v1;
		public Vector3 v2;
		public int meshIndex;
		public Vector3 this[int i] {
			get {
				return i == 0 ? v0 : i == 1 ? v1 : i == 2 ? v2 : default;
			}
		}

		public Triangle(int meshIndex, Vector3[] vertexes) {
			if (vertexes.Count() != 3) throw new ArgumentException("Triangle must be initialized with 3 vertexes.");
			v0 = vertexes[0];
			v1 = vertexes[1];
			v2 = vertexes[2];
			this.meshIndex = meshIndex;
		}

		public override bool Equals(object obj) {
			return obj is Triangle triangle &&
				   v0.Equals(triangle.v0) &&
				   v1.Equals(triangle.v1) &&
				   v2.Equals(triangle.v2);
		}

		public override int GetHashCode() {
			int hashCode = -396124428;
			hashCode = hashCode * -1521134295 + v0.GetHashCode();
			hashCode = hashCode * -1521134295 + v1.GetHashCode();
			hashCode = hashCode * -1521134295 + v2.GetHashCode();
			return hashCode;
		}

		public IEnumerator<Vector3> GetEnumerator() {
			yield return v0;
			yield return v1;
			yield return v2;
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}
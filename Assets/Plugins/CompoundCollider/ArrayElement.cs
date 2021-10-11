using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace ZorgsCompoundColliders {
	/// <summary>
	/// Support for Unity's refusal for multidimensional arrays <para/>
	/// Use: ArrayElement<T>[]a; <para/>
	/// a[x][y]
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Serializable]
	public class ArrayElement<T> : IEnumerable, IEnumerable<T> {
		[SerializeField]
		private T[] m_array;
		public T[] Array { get => m_array; }
		public T this[int i] => m_array[i];

		public IEnumerator GetEnumerator() => m_array.GetEnumerator();

		IEnumerator<T> IEnumerable<T>.GetEnumerator() {
			foreach (var item in m_array) {
				yield return item;
			}
		}

		public ArrayElement(T[] array) {
			m_array = array;
		}
		public ArrayElement(IEnumerable<T> collection) {
			m_array = collection.ToArray();
		}

		public static implicit operator T[](ArrayElement<T> arr) => arr == null ? new T[0] : arr.Array;
		public static implicit operator ArrayElement<T>(T[] arr) => new ArrayElement<T>(arr == null ? new T[0] : arr);
	}
	public static class ArrayElementExtensions {
		public static ArrayElement<T>[] ToArrayElemets<T>(this IEnumerable<IEnumerable<T>> collectionOfCollections) =>
			collectionOfCollections.Select(t => (ArrayElement<T>)t.ToArray()).ToArray();
		public static ArrayElement<T>[] ToArrayElemets<T>(this IEnumerable<T[]> collectionOfCollections) =>
			collectionOfCollections.Select(t => (ArrayElement<T>)t).ToArray();
	}
}

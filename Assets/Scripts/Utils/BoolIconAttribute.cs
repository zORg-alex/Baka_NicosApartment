using System;
using UnityEngine;
/// <summary>
/// Use on <see cref="bool"/> Fields and Properties
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class BoolIconAttribute : PropertyAttribute {
	public string icon;
	public string negativeIcon;

	public BoolIconAttribute(string iconAssetAdress, string negativeIconAssetAdress) {
		icon = iconAssetAdress;
		negativeIcon = negativeIconAssetAdress;
	}
}


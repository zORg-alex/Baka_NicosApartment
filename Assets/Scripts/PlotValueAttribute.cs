using UnityEngine;

public class PlotValueAttribute : PropertyAttribute {
	public Vector2Int Size;
	public float TimeScale = 1f;
	public PlotValueAttribute() {
		Size = new Vector2Int(128, 32);
	}
	public PlotValueAttribute(int x, int y) {
		Size = new Vector2Int(x,y);
	}
	public PlotValueAttribute(int x, int y, float timeScale) : this(x, y) {
		TimeScale = timeScale;
	}
}

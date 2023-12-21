using UnityEngine;

public sealed class Crosshair : MonoBehaviour
{
	[SerializeField] private RectTransform m_crosshair;

	private void Start()
	{
		Cursor.visible = false;
	}

	private void Update()
	{
		m_crosshair.position = Input.mousePosition;
	}
}

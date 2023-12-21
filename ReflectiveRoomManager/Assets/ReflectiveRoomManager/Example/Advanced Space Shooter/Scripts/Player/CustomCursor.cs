using UnityEngine;

public sealed class CustomCursor : MonoBehaviour
{
	[SerializeField] private Texture2D m_cursorTexture;

	private void Start()
	{
		Cursor.SetCursor(m_cursorTexture, new Vector2(m_cursorTexture.width * 0.5f, m_cursorTexture.height * 0.5f), CursorMode.Auto);
	}
}

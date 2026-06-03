using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class gridpanel : MonoBehaviour
{
	private enum EVecDir
	{
		None = 0,
		Up = 1,
		Left = 2,
		Right = 3,
		Down = 4
	}

	public Image m_GemImage;

	public Sprite[] m_BGTexture;

	private GameObject m_Tile;

	private UAP_BaseElement m_AccessibilityHelper;

	private Button m_Button;

	private int m_Index;

	private int m_xWidth;

	private int m_TileType;

	private static string[] typeList;

	public static int tileTypeCount;

	public Sprite[] m_GemTextures;

	private int m_BGType;

	private EVecDir m_LastPreviewDir;

	public string GetTileTypeName()
	{
		return null;
	}

	public static string GetTileTypeName(int tileType)
	{
		return null;
	}

	private void SetBGType(int bgIndex = 0)
	{
	}

	public void SetTileType(int tileType)
	{
	}

	public int GetTileType()
	{
		return 0;
	}

	public void SetIndex(int index, int xWidth)
	{
	}

	public int GetIndex()
	{
		return 0;
	}

	public void OnPointerDownDelegate(PointerEventData data)
	{
	}

	public void OnPointerUpDelegate(PointerEventData data)
	{
	}

	public void OnDragUpdateDelegate(PointerEventData data)
	{
	}

	public void OnDragEndDelegate(PointerEventData data)
	{
	}

	private bool IsSameTile(GameObject pointerEnter)
	{
		return false;
	}

	private int GetNeighbourIndex(EVecDir dir)
	{
		return 0;
	}

	private EVecDir GetVectorDirection(Vector2 vector)
	{
		return default(EVecDir);
	}

	public void OnButtonPress()
	{
	}
}

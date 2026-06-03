using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class gameplay : MonoBehaviour
{
	public GridLayoutGroup grid;

	public AccessibleUIGroupRoot container;

	public Text m_MovesLabel;

	public UAP_BaseElement m_MovesLabel_Access;

	public Text[] m_GoalsLabel;

	public Image[] m_GoalsImages;

	public GameObject[] m_GoalsCheckmarks;

	public GameObject[] m_GoalsHighlightPos;

	public GameObject m_SelectionFrame;

	public AudioSource m_SFXPlayer;

	public Image m_SoundToggle;

	public Sprite m_SoundOn;

	public Sprite m_SoundOff;

	public AudioSource m_MusicPlayer;

	public AudioClip m_ActiveTile;

	public AudioClip m_SwapAborted;

	public AudioClip m_NoMatch3;

	public AudioClip m_Match3;

	public AudioClip m_GoalsMatch3;

	public AudioClip m_FallingPieces;

	public GameObject m_LevelGoalHighlightEffect;

	public Sprite[] m_GemTextures;

	private int m_CellCountX;

	private int m_CellCountY;

	private List<gridpanel> m_GridTiles;

	public bool m_MakeSquares;

	private int m_BaseCellSize;

	private int m_BaseCellCountX;

	private int m_BaseCellCountY;

	public static gameplay Instance;

	private int m_MovesLeft;

	private int m_MoveCount;

	private float m_GameDuration;

	private List<int> m_Cleared;

	private List<int> m_LevelGoals;

	private int m_TileTypeCount;

	private bool m_Paused;

	private string m_LevelGoalsString;

	private int m_MovesGained;

	private gridpanel m_SelectedTile;

	private bool m_levelGoalUpdatedWithMove;

	public static int DifficultyLevel;

	private bool m_IsPreviewingSwap;

	private float m_SwapPreviewTimer;

	private float m_SwapPreviewDuration;

	private int m_PreviewIndex1;

	private int m_PreviewIndex2;

	private Vector3 m_Previewposition1;

	private Vector3 m_Previewposition2;

	private gameplay()
	{
	}

	private void OnEnable()
	{
	}

	public Sprite GetTileTypeSprite(int tileType)
	{
		return null;
	}

	public void InitBoard(int countX, int countY, int tiletypeCount, int moveCount, List<int> levelGoals)
	{
	}

	private void Start()
	{
	}

	public void OnRepeatLevelGoals()
	{
	}

	public void OnUserPause()
	{
	}

	private void Update()
	{
	}

	private int GetRandomTile()
	{
		return 0;
	}

	private void RandomizeTiles()
	{
	}

	private Vector2 ConvertIndexToXYCoordinates(int index)
	{
		return default(Vector2);
	}

	private void AbortSelection()
	{
	}

	public void ActivateTile(int index)
	{
	}

	private gridpanel GetGridTile(int index)
	{
		return null;
	}

	private void UpdateMoveLabel()
	{
	}

	private void UpdateLevelGoalsLabels()
	{
	}

	private int GetLevelGoalIndex(int tileType)
	{
		return 0;
	}

	private void EvaluateBoard()
	{
	}

	private void DropDownTiles()
	{
	}

	private int GetIndex(int x, int y)
	{
		return 0;
	}

	private void FinishBoardEvaluation()
	{
	}

	private void SaveGameState()
	{
	}

	private List<int> FindMatch3()
	{
		return null;
	}

	private int GetTileType(float xCoord, float yCoord)
	{
		return 0;
	}

	public void AbortGame()
	{
	}

	private void DestroyMyself()
	{
	}

	public void ResumeGame()
	{
	}

	private void OnDestroy()
	{
	}

	public void OnSoundToggle()
	{
	}

	private void EnableMusic(bool enable)
	{
	}

	public void PreviewDrag(int index1, int index2)
	{
	}

	public void CancelPreview(bool swapSuccessful = false)
	{
	}
}

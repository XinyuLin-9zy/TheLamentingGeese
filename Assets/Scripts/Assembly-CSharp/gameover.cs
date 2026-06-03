using UnityEngine;
using UnityEngine.UI;

public class gameover : MonoBehaviour
{
	public GameObject m_GameLostHeading;

	public GameObject m_GameWonHeading;

	public GameObject m_GameLostText;

	public GameObject m_GameWonText;

	public Text m_MovesLabel;

	public Text m_TimeLabel;

	public AudioClip m_GameWon;

	public AudioClip m_GameLost;

	public AudioSource m_AudioPlayer;

	public static int MoveCount;

	public static float GameDuration;

	public static bool GameWon;

	private int m_WaitingForSilence;

	public void OnReturnButtonPressed()
	{
	}

	public void OnPlayAnotherMatchButtonPressed()
	{
	}

	private void OnEnable()
	{
	}

	private void Update()
	{
	}
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    
    public TMP_Text waveText;
    public TMP_Text playerHealthText;

    [Header("Game End Panel")]
    [SerializeField] private GameObject gameEndPanel;
    [SerializeField] private TMP_Text gameEndText;
    public Button revivePlayerButton;
    [SerializeField] private Button restartButton;
    
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void GameEnded(bool playerIsDead=false)
    {
        gameEndPanel.SetActive(true);
        restartButton.onClick.AddListener(RestartGame);
        
        if (playerIsDead)
        {
            gameEndText.text = "!!!You are dead!!!";
            revivePlayerButton.gameObject.SetActive(true);
            revivePlayerButton.onClick.AddListener(RevivePlayer);
        }
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(0);
    }

    private void RevivePlayer()
    {
        gameEndPanel.SetActive(false);
        revivePlayerButton.gameObject.SetActive(false);
    }
}

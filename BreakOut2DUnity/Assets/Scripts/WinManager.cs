using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


public class WinManager : MonoBehaviour
{
    [Header("Required references")]
    public Button playAgainButton;
    public Button quitButton;
    public TextMeshProUGUI scoreText;

    void Start()
    {
        this.scoreText.SetText(GetScoreText(BricksManager.score));

        this.playAgainButton.onClick.AddListener(() => SceneManager.LoadScene("Game"));
        this.quitButton.onClick.AddListener(() => Application.Quit());
    }

    private static string GetScoreText(float score) => $@"<color=#b8d6ce>your score is {BricksManager.score:F0}</color>";
}

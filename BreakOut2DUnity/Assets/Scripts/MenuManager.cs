using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Required references")]
    public Button playButton;
    public Button quitButton;

    void Start()
    {
        this.playButton.onClick.AddListener(() => SceneManager.LoadSceneAsync("Game"));
        this.quitButton.onClick.AddListener(() => Application.Quit());
    }

    //void OnGUI()
    //{
    //    var fontSize = new GUIStyle(GUI.skin.GetStyle("label")) { fontSize = 11 };
    //    var s = Directory.GetCurrentDirectory();
    //    GUI.Label(new Rect(6, Screen.height - 20, 500, 60), s, fontSize);
    //}
}

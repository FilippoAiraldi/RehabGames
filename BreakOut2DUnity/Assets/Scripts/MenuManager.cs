using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


[Serializable]
public class GameConfig
{
    public string TcpAddress;
    public int TcpPort;
    public int TcpReadWriteIntervalMs;

    public float PaddleSpeed;
    public float PaddleForce;
    public float BallSpeed;
    public float BallMaxRngStartAngle;

    public float BricksMargin;
}


public class MenuManager : MonoBehaviour
{
    [Header("Required references")]
    public Button playButton;
    public Button quitButton;

    private string configErrorMsg = string.Empty;

    public static GameConfig Config { get; private set; } = null;

    void Start()
    {
        var ok = this.ReadAndCheckConfigJson("config.json", out var config, out this.configErrorMsg);
        if (ok) Config = config;
        this.playButton.interactable = ok && Config != null;

        this.playButton.onClick.AddListener(() => SceneManager.LoadScene("Game"));
        this.quitButton.onClick.AddListener(() => Application.Quit());
    }

    void OnGUI()
    {
        if (string.IsNullOrEmpty(this.configErrorMsg))
            return;
        
        var font = new GUIStyle(GUI.skin.GetStyle("label")) { fontSize = 11 };
        GUI.Label(new Rect(6, Screen.height - 20, 750, 60), this.configErrorMsg, font);
    }

    private bool ReadAndCheckConfigJson(string path, out GameConfig config, out string error)
    {
        config = null;
        error = null;

        if (!File.Exists(path))
        {
            error = $"No config.json found in {Directory.GetCurrentDirectory()}.";
            return false;
        }

        try
        {
            config = JsonUtility.FromJson<GameConfig>(File.ReadAllText(path));
            error = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            error = $"Invalid config.json: {ex.Message}";
            return false;
        }
    }
}

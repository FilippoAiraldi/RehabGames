using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;


[DisallowMultipleComponent]
public class GameController : MonoBehaviour
{
    [Header("Required references")]
    public TcpServer server;
    public GameObject paddle;
    public Rigidbody2D ball;

    [Header("Gameplay parameters")]
    [Range(1f, 100f)] public float paddleSpeed = 10f;
    [Range(0.1f, 10f)] public float paddleForce = 2f;
    [Range(1f, 100f)] public float ballSpeed = 10f;
    [Range(0f, 45f)] public float ballStartMaxRngAngle = 10f;

    private float paddleSpeed_scaled;
    private float deltaTime = 0.25f;

    void Start()
    {
        //this.SpawnBricks();
        _ = this.SpawnBall();
    }

    void Update()
    {
        // increase time for FPS computation
        this.deltaTime += (Time.unscaledDeltaTime - this.deltaTime) * 0.1f;

        // get paddle command
        this.paddleSpeed_scaled = (this.server.IsClientConnected
              ? this.GetPaddleCommand()
              : (Input.GetAxisRaw("Horizontal") * 2f)) * Time.deltaTime * this.paddleSpeed;

        // check if user wants to exit
        if (Input.GetKey(KeyCode.Escape))
            SceneManager.LoadSceneAsync("Menu");
    }

    void FixedUpdate() => this.paddle.transform.Translate(this.paddleSpeed_scaled, 0, 0);

    void OnGUI()
    {
        var fontSize = new GUIStyle(GUI.skin.GetStyle("label")) { fontSize = 11 };
        var s = $"{1f / this.deltaTime:00.0} FPS. ";
        s += this.server.IsClientConnected
             ? $"Command = {this.GetPaddleCommand():+0.0;-0.0;0.0}"
             : "No client connected.";
        GUI.Label(new Rect(6, Screen.height - 20, 300, 60), s, fontSize);
    }

    public async Task SpawnBall()
    {
        // halt ball
        this.ball.position = new Vector2(this.paddle.transform.position.x,
            this.paddle.transform.position.y + (this.paddle.transform.localScale.y + this.ball.transform.localScale.x) / 2f);
        this.ball.velocity = Vector2.zero;

        // start 3s timer here - after which start the ball
        await Task.Delay(1500);
        const float meanAngle = 90 * Mathf.Deg2Rad;
        var aperture = this.ballStartMaxRngAngle * Mathf.Deg2Rad;
        var angle = Random.Range(meanAngle - aperture, meanAngle + aperture);
        this.ball.velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * this.ballSpeed;
    }

    private float GetPaddleCommand() => -Mathf.Lerp(-this.paddleForce, this.paddleForce, this.server.PaddleCommand);
}

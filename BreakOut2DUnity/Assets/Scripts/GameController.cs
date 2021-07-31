using UnityEngine;

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
    [Range(-7.5f, 7.5f)] public float ballStartHeight = -7.5f;
    [Range(0f, 45f)] public float ballStartMaxRandomAngle = 20f;

    private float paddleSpeed_scaled;
    private float deltaTime = 0f;

    void Start()
    {
        Debug.Assert(this.server != null, "Controller server expected to be non-null");
        Debug.Assert(this.paddle != null, "Controller paddle expected to be non-null");
        Debug.Assert(this.paddleSpeed > 0f, "Controller paddle speed must be positive");
        Debug.Assert(this.ball != null, "Controller ball rigid body expected to be non-null");
        Debug.Assert(this.ballSpeed > 0f, "Controller ball speed must be positive");

        this.RespawnBall();
    }

    void Update()
    {
        this.deltaTime += (Time.unscaledDeltaTime - this.deltaTime) * 0.1f;

        this.paddleSpeed_scaled = (this.server.IsClientConnected
              ? this.GetPaddleCommand()
              : (Input.GetAxisRaw("Horizontal") * 2f)) * Time.deltaTime * this.paddleSpeed;
    }

    void FixedUpdate() => this.paddle.transform.Translate(this.paddleSpeed_scaled, 0, 0);

    public void RespawnBall()
    {
        this.ball.position = new Vector2(0, this.ballStartHeight);

        const float meanAngle = 90 * Mathf.Deg2Rad;
        var aperture = this.ballStartMaxRandomAngle * Mathf.Deg2Rad;
        var angle = Random.Range(meanAngle - aperture, meanAngle + aperture);
        this.ball.velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * this.ballSpeed;
    }

    void OnGUI()
    {
        var fontSize = new GUIStyle(GUI.skin.GetStyle("label")) { fontSize = 11 };
        var s = $"{1f / this.deltaTime:00.0} FPS. ";
        s += this.server.IsClientConnected
             ? $"Command = {this.GetPaddleCommand():+0.0;-0.0;0.0}"
             : "No client connected.";
        GUI.Label(new Rect(6, Screen.height - 20, 300, 60), s, fontSize);
    }

    private float GetPaddleCommand() => -Mathf.Lerp(-this.paddleForce, this.paddleForce, this.server.PaddleCommand);
}

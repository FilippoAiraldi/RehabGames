using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GameController : MonoBehaviour
{
    [Header("Required references")]
    public TcpServer server;
    public GameObject paddle;
    public GameObject paddleBaseline;
    public Rigidbody2D ball;
    public Brick brick;
    public GameObject topWall;
    public GameObject leftWall;
    public GameObject rightWall;

    [Header("Gameplay parameters")]
    [Range(1f, 100f)] public float paddleSpeed = 10f;
    [Range(0.1f, 10f)] public float paddleForce = 2f;
    [Range(1f, 100f)] public float ballSpeed = 10f;
    [Range(0f, 45f)] public float ballStartMaxRngAngle = 10f;
    public float bricksMargin = 0.25f;

    private float paddleSpeed_scaled;
    private float deltaTime = 0.25f;
    private readonly List<GameObject> bricks = new List<GameObject>(100);

    void Start()
    {
        Debug.Assert(this.server != null, "Controller server expected to be non-null");
        Debug.Assert(this.paddle != null, "Controller paddle expected to be non-null");
        Debug.Assert(this.paddle != null, "Controller paddle baseline expected to be non-null");
        Debug.Assert(this.ball != null, "Controller ball rigid body expected to be non-null");
        Debug.Assert(this.brick != null, "Controller ball brick expected to be non-null");
        Debug.Assert(this.topWall != null, "Collision manager top wall expected to be non-null");
        Debug.Assert(this.leftWall != null, "Collision manager left wall expected to be non-null");
        Debug.Assert(this.rightWall != null, "Collision manager right wall expected to be non-null");

        this.SpawnBricks();
        this.SpawnBall();
    }

    void Update()
    {
        this.deltaTime += (Time.unscaledDeltaTime - this.deltaTime) * 0.1f;

        this.paddleSpeed_scaled = (this.server.IsClientConnected
              ? this.GetPaddleCommand()
              : (Input.GetAxisRaw("Horizontal") * 2f)) * Time.deltaTime * this.paddleSpeed;
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

    public void SpawnBall()
    {
        this.ball.position = new Vector2(this.paddle.transform.position.x,
            this.paddle.transform.position.y + (this.paddle.transform.localScale.y + this.ball.transform.localScale.x) / 2f);
        const float meanAngle = 90 * Mathf.Deg2Rad;
        var aperture = this.ballStartMaxRngAngle * Mathf.Deg2Rad;
        var angle = Random.Range(meanAngle - aperture, meanAngle + aperture);
        this.ball.velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * this.ballSpeed;
    }

    public void NotifyBrickHit(GameObject brickHit)
    {
        this.bricks.Remove(brickHit);
        Destroy(brickHit);
        if (this.bricks.Count == 0)
        {
            // end game...
        }
    }

    private void SpawnBricks()
    {
        // compute dimensions of field
        var width = this.rightWall.transform.position.x - this.leftWall.transform.position.x 
            - (this.rightWall.transform.localScale.x + this.leftWall.transform.localScale.x) / 2f;
        var height = this.topWall.transform.position.y - this.paddle.transform.position.y
            - (this.topWall.transform.localScale.y + this.paddle.transform.localScale.y) / 2f;
        height /= 2f;

        // compute number of bricks that fit in the field
        (float width, float height) brickSize = (this.brick.transform.localScale.x, this.brick.transform.localScale.y);
        var nHorBricks = Mathf.FloorToInt((width + this.bricksMargin) / (brickSize.width + this.bricksMargin));
        var nVerBricks = Mathf.FloorToInt((height + this.bricksMargin) / (brickSize.height + this.bricksMargin));
        var wallToBrickHorMargin = (width - nHorBricks * brickSize.width  - (nHorBricks - 1) * this.bricksMargin) / 2f;
        var wallToBrickVerMargin = (height - nVerBricks * brickSize.height - (nVerBricks - 1) * this.bricksMargin) / 2f;

        // populate of bricks
        this.ClearBricks();
        var startX = this.leftWall.transform.position.x + this.leftWall.transform.localScale.x / 2f + wallToBrickHorMargin + brickSize.width / 2;
        var startY = this.topWall.transform.position.y - this.topWall.transform.localScale.y / 2f - wallToBrickVerMargin - brickSize.height / 2;
        for (var i = 1; i < nHorBricks - 1; ++i)
        {
            var px = startX + i * (brickSize.width + this.bricksMargin);
            for (var j = 1; j < nVerBricks - 1; ++j)
            {
                var py = startY - j * (brickSize.height + this.bricksMargin);
                var b = Instantiate(this.brick, new Vector3(px, py), Quaternion.identity);
                b.Controller = this;
                this.bricks.Add(b.gameObject);
            }
        }
    }

    private void ClearBricks()
    {
        this.bricks.ForEach(b => Destroy(b));
        this.bricks.Clear();
    }

    private float GetPaddleCommand() => -Mathf.Lerp(-this.paddleForce, this.paddleForce, this.server.PaddleCommand);
}

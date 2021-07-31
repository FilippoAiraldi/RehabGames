using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Required references")]
    public TcpServer server;
    public GameObject paddle;
    //public GameObject paddleBaseline;
    //public GameObject ball;
    public Rigidbody2D ball;
    //public GameObject topWall;
    //public GameObject bottomWall;
    //public GameObject leftWall;
    //public GameObject rightWall;

    [Header("Gameplay parameters")]
    [Range(1f, 100f)] public float paddleSpeed = 10f;
    [Range(0.1f, 10f)] public float paddleForce = 2f;
    [Range(1f, 100f)] public float ballSpeed = 10f;

    private float paddleSpeed_scaled;
    //private float fieldWidth;
    //private float fieldDiag;
    private float deltaTime = 0f;

    void Start()
    {
        Debug.Assert(this.server != null, "Controller server expected to be non-null");
        Debug.Assert(this.paddle != null, "Controller paddle expected to be non-null");
        Debug.Assert(this.paddleSpeed > 0f, "Controller paddle speed must be positive");
        //Debug.Assert(this.paddleBaseline != null, "Controller paddle baseline expected to be non-null");
        //Debug.Assert(this.ball != null, "Controller ball expected to be non-null");
        Debug.Assert(this.ball != null, "Controller ball rigid body expected to be non-null");
        Debug.Assert(this.ballSpeed > 0f, "Controller ball speed must be positive");
        //Debug.Assert(this.topWall != null, "Controller top wall expected to be non-null");
        //Debug.Assert(this.bottomWall != null, "Controller bottom wall expected to be non-null");
        //Debug.Assert(this.leftWall != null, "Controller left wall expected to be non-null");
        //Debug.Assert(this.rightWall != null, "Controller right wall expected to be non-null");

        //(this.fieldWidth, this.fieldDiag) = this.ComputeFieldDimensions();
        this.RespawnBall();
    }

    void Update()
    {
        this.deltaTime += (Time.unscaledDeltaTime - this.deltaTime) * 0.1f;

        this.paddleSpeed_scaled = (this.server.IsClientConnected
              ? this.GetPaddleCommand()
              : (Input.GetAxisRaw("Horizontal") * 2f)) * Time.deltaTime * this.paddleSpeed;

        //(this.server.PaddleActualPosition, this.server.PaddleDesiredPosition, this.server.BallDistanceFromPaddleDesiredPosition) = 
        //    this.ComputePaddleActualAndDesiredPositionAndDistance();
    }

    void FixedUpdate() => this.paddle.transform.Translate(this.paddleSpeed_scaled, 0, 0);

    //public (float width, float diag) ComputeFieldDimensions()
    //{
    //    var w = this.rightWall.transform.position.x - this.leftWall.transform.position.x 
    //        - (this.rightWall.transform.localScale.x + this.leftWall.transform.localScale.x) / 2f
    //        - this.paddle.transform.localScale.x;
    //    var h = this.topWall.transform.position.y - this.paddle.transform.position.y
    //        - (this.topWall.transform.localScale.y + this.paddle.transform.localScale.y) / 2f
    //        - this.ball.transform.localScale.x;

    //    return (w, Mathf.Sqrt(w * w + h * h));
    //}

    public void RespawnBall()
    {
        this.ball.position = new Vector2(0, -7.5f);

        const float meanAngle = 90 * Mathf.Deg2Rad;
        const float aperture = 20 * Mathf.Deg2Rad;
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

    //private float NormalizedPaddlePosition(float p) => 1f - (p / this.fieldWidth + 0.5f); // range [1, 0] left to right

    //private (float pos, float desPos, float normDist) ComputePaddleActualAndDesiredPositionAndDistance()
    //{
    //    const int maxCollisionIterations = 15;
    //    const int collidableLayerMask = 1 << 6;

    //    var paddlePos = this.NormalizedPaddlePosition(this.paddle.transform.position.x);
    //    var ballRadius = this.ball.transform.localScale.x / 2f;
    //    var start = this.ballRigidBody.position;
    //    var dir = this.ballRigidBody.velocity.normalized; 
    //    for (var i = 0; i < maxCollisionIterations; ++i)
    //    {
    //        // get next collision
    //        var hit = Physics2D.CircleCast(start, ballRadius, dir, this.fieldDiag, collidableLayerMask);
    //        Debug.Assert(hit.collider != null, "no collider found!");
    //        Debug.DrawLine(start, hit.centroid, Color.magenta);

    //        // check the collided object
    //        var hitObject = hit.collider.gameObject;
    //        if (hitObject == this.paddleBaseline || hitObject == this.paddle)
    //        {
    //            // from the hit centroid, extract the desired paddle position and the distance 
    //            var xHit = Mathf.Clamp(hit.centroid.x, -this.fieldWidth / 2f, this.fieldWidth / 2f);
    //            var paddleDesiredPos = this.NormalizedPaddlePosition(xHit);
    //            var ballHittingPaddlePos = new Vector2(this.paddle.transform.position.x, this.paddle.transform.position.y + this.paddle.transform.localScale.y / 2f + ballRadius);
    //            var dist = (this.ballRigidBody.position - ballHittingPaddlePos).magnitude / this.fieldDiag;
    //            return (paddlePos, paddleDesiredPos, dist);
    //        }
    //        else
    //        {
    //            // propagate collision
    //            var n = hit.normal;
    //            dir = (dir - 2 * (Vector2.Dot(dir, n)) * n).normalized;
    //            start = hit.centroid + 0.015f * dir;
    //        }
    //    }
    
    //    // no collision with baseline detected; return middle of screen with some magnitude
    //    return (paddlePos, 0.5f, 0.5f); 
    //}
}

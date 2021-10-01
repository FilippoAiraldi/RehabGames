using UnityEngine;


public class CollisionManager : MonoBehaviour
{
    [Header("Required references")]
    private TcpServer server;
    public GameObject paddle;
    public GameObject paddleBaseline;
    public Rigidbody2D ball;
    public GameObject topWall;
    public GameObject leftWall;
    public GameObject rightWall;

    private float fieldWidth;
    private float fieldHeight;
    private float fieldDiag;
    private int maxCollisionIterations; 

    void Start()
    {
        this.server = FindObjectOfType<TcpServer>();
        this.maxCollisionIterations = MenuManager.Config.MaxCollisionPredictionIters;
        (this.fieldWidth, this.fieldHeight, this.fieldDiag) = this.ComputeFieldDimensions();
    }

    void Update() => (this.server.PaddleActualPosition,
                      this.server.PaddleDesiredPosition,
                      this.server.BallDistanceFromPaddleDesiredPosition,
                      this.server.BallDistanceFromPaddleDesiredInversePositionWithDeadband) = this.ComputePaddleActualAndDesiredPositionAndDistance();

    public (float width, float height, float diag) ComputeFieldDimensions()
    {
        var w = this.rightWall.transform.position.x - this.leftWall.transform.position.x
            - (this.rightWall.transform.localScale.x + this.leftWall.transform.localScale.x) / 2f
            - this.paddle.transform.localScale.x;
        var h = this.topWall.transform.position.y - this.paddle.transform.position.y
            - (this.topWall.transform.localScale.y + this.paddle.transform.localScale.y) / 2f
            - this.ball.transform.localScale.x;

        return (w, h, Mathf.Sqrt(w * w + h * h));
    }

    private float NormalizedPaddlePosition(float p) => 1f - (p / this.fieldWidth + 0.5f); // range [1, 0] left to right

    private (float pos, float desPos, float normDist, float invDist) ComputePaddleActualAndDesiredPositionAndDistance()
    {
        const int collidableLayerMask = 1 << 6;

        // compute paddle position and the point where it will hit the paddle and the distance to it
        var paddlePos = this.NormalizedPaddlePosition(this.paddle.transform.position.x);
        var ballRadius = this.ball.transform.localScale.x / 2f;
        var ballHittingPaddlePos = (Vector2)this.paddle.transform.position + new Vector2(0, this.paddle.transform.localScale.y / 2f + ballRadius);
        var ballToPaddle = this.ball.position - ballHittingPaddlePos;
        var dist = ballToPaddle.magnitude / this.fieldDiag;

        // compute inverse y distance (1 - d) from ball to paddle pos taking into account the deadband
        var deadFieldHeight = MenuManager.Config.DistanceDeadband * this.fieldHeight;
        var invDist = this.ball.position.y - ballHittingPaddlePos.y > deadFieldHeight
            ? 0f
            : 1f - (this.ball.position.y - ballHittingPaddlePos.y) / deadFieldHeight;

        var start = this.ball.position;
        var dir = this.ball.velocity.normalized;
        if (dir != Vector2.zero) 
        {
            for (var i = 0; i < maxCollisionIterations; ++i)
            {
                // get next collision
                var hit = Physics2D.CircleCast(start, ballRadius, dir, this.fieldDiag, collidableLayerMask);
                Debug.DrawLine(start, hit.centroid, Color.magenta);

                // check the collided object
                var hitObject = hit.collider.gameObject;
                if (hitObject == this.paddleBaseline || hitObject == this.paddle)
                {
                    // from the hit centroid, extract the desired paddle position and the distance 
                    var xHit = Mathf.Clamp(hit.centroid.x, -this.fieldWidth / 2f, this.fieldWidth / 2f);
                    var paddleDesiredPos = this.NormalizedPaddlePosition(xHit);
                    return (paddlePos, paddleDesiredPos, dist, invDist);
                }
                else
                {
                    // propagate collision
                    var n = hit.normal;
                    dir = (dir - 2 * (Vector2.Dot(dir, n)) * n).normalized;
                    start = hit.centroid + 0.015f * dir;
                }
            }
        }

        // no collision with baseline detected; return the position itself of the paddle
        return (paddlePos, paddlePos, dist, invDist);
    }
}

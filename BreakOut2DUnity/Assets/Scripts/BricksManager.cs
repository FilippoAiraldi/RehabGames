using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class BricksManager : MonoBehaviour
{
    [Header("Required references")]
    public GameObject paddle;
    public Brick brick;
    public GameObject topWall;
    public GameObject leftWall;
    public GameObject rightWall;
    public Text scoreText;

    [Header("Gameplay parameters")]
    private float bricksMargin = 0.25f;

    private readonly List<GameObject> bricks = new List<GameObject>(100);
    private float score;
    private float pointsPerBlock;
    private float pointsPerSecond;

    private Timer timer;

    void Start()
    {
        this.bricksMargin = MenuManager.Config.BricksMargin;
        this.pointsPerBlock = MenuManager.Config.PointsPerBlock;
        this.pointsPerSecond = MenuManager.Config.PointsPerSecond;
        this.score = 0f;
        
        this.SpawnBricks();

        // start timer
        this.timer = new Timer(1000);
        this.timer.Elapsed += OnTimedEvent;
        this.timer.AutoReset = this.timer.Enabled = true;
    }

    void Update() => this.scoreText.text = this.score.ToString("F2");

    public void NotifyBrickHit(GameObject brickHit)
    {
        this.bricks.Remove(brickHit);
        Destroy(brickHit);
        this.score += this.pointsPerBlock;

        if (this.bricks.Count == 0)
            SceneManager.LoadSceneAsync("Menu");
    }

    private void SpawnBricks(bool clean = true)
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
        var wallToBrickHorMargin = (width - nHorBricks * brickSize.width - (nHorBricks - 1) * this.bricksMargin) / 2f;
        var wallToBrickVerMargin = (height - nVerBricks * brickSize.height - (nVerBricks - 1) * this.bricksMargin) / 2f;

        // populate of bricks
        if (clean) 
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
                b.Manager = this;
                this.bricks.Add(b.gameObject);
            }
        }
    }

    private void ClearBricks()
    {
        this.bricks.ForEach(b => Destroy(b));
        this.bricks.Clear();
    }

    private void OnTimedEvent(object sender, ElapsedEventArgs e)
    {
        this.score = Mathf.Max(0, this.score + this.pointsPerSecond);
    }

    private void OnDestroy()
    {
        this.timer.Elapsed -= OnTimedEvent;
        this.timer.Stop();
        this.timer.Dispose();
    }
}

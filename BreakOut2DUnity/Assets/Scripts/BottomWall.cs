using UnityEngine;


public class BottomWall : MonoBehaviour
{
    [Header("Required references")]
    public GameController controller;
    public BricksManager bricksManager;

    void OnTriggerEnter2D(Collider2D other)
    {
        this.bricksManager.NotifyDeath();
        _ = this.controller.SpawnBall();
    }
}

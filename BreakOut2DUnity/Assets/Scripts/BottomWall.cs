using UnityEngine;


public class BottomWall : MonoBehaviour
{
    [Header("Required references")]
    public GameController controller;

    void OnTriggerEnter2D(Collider2D other) => _ = this.controller.SpawnBall();
}

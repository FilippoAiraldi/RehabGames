using UnityEngine;

public class BottomWall : MonoBehaviour
{
    [Header("Required references")]
    public GameController controller;

    void Start()
    {
        Debug.Assert(this.controller != null, "Bottom wall controller expected to be non-null");
    }

    void OnTriggerEnter2D(Collider2D _)
    {
        // trigger countdown without respawning bricks...
        this.controller.SpawnBall();
    }
}

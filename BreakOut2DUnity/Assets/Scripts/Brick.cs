using UnityEngine;


public class Brick : MonoBehaviour
{
    public BricksManager Manager { private get; set; }

    private void OnCollisionEnter2D(Collision2D other) => _ = this.Manager.NotifyBrickHit(this.gameObject);
}

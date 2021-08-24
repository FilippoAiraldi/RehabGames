using UnityEngine;

public class Brick : MonoBehaviour
{
    public GameController Controller { private get; set; }

    private void OnCollisionEnter2D(Collision2D _) => this.Controller.NotifyBrickHit(this.gameObject);
}

using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    bool canMove, dragging;
    new Collider2D collider;

    void Start()
    {
        this.collider = this.GetComponent<Collider2D>();
        this.canMove = false;
        this.dragging = false;
    }

    void Update()
    {
        var mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            this.canMove = this.collider == Physics2D.OverlapPoint(mouse);
            if (this.canMove)
                this.dragging = true;
        }

        // if (this.dragging)
            // this.transform.position = new Vector3(mouse.x, mouse.y, this.transform.position.z);

        if (Input.GetMouseButtonUp(0))
            this.canMove = this.dragging = false;
    }
}

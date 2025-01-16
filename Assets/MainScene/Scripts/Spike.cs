using UnityEngine;

public class Spike : MovingObject
{
    public float rotSpeed;
    public bool isRotating;

    /*protected void OnTriggerEnter2D(Collider2D collision)
    {
        print(collision);
        if (collision.CompareTag("Player"))
            PlayerController.S.OnSpikeHit(); //6.823173 1.490649
    }*/
    public override void SetComponents(bool isTurnedOn)
    {
        base.SetComponents(isTurnedOn);
        BoxCollider2D[] childrenColliders = transform.GetComponentsInChildren<BoxCollider2D>();
        foreach (BoxCollider2D child in childrenColliders)
            child.enabled = isTurnedOn;
    }
    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (isOptimized) return;
        if (isRotating) 
            transform.Rotate(0,0,rotSpeed * Time.deltaTime);
    }
}

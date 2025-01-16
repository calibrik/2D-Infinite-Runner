using UnityEngine;

public class Coin : MovingObject
{
    public float rotationSpeed = 100f;
    //public int frameCount;
    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Main.S.CollectedCoin();
            RemoveFromScene();
        }
    }
    /*public override void TurnOff()
    {
        base.TurnOff();
        this.enabled = true;
    }*/
    // Update is called once per frame
    protected override void Update()
    {
        /*if (transform.position.x <= deleteAtCoordinateX)
            RemoveFromScene();*/
        //transform.position = new Vector3(transform.position.x, 0.05f * Mathf.Sin(10f*transform.position.x)+_initialY, 0);
        /*if (++frameCount == Main.S.coinUpdateFrames)
        {
            frameCount = 0;
            transform.Rotate(0, rotationSpeed * Time.deltaTime*Main.S.coinUpdateFrames, 0);
        }*/
        base.Update();
        if (isOptimized) return;
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}

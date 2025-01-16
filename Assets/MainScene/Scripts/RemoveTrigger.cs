using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D collision)
    {
        MovingObject obj = collision.gameObject.GetComponent<MovingObject>();
        if (obj) obj.RemoveFromScene();
    }
}

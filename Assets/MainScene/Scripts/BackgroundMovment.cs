using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BackgroundMovement : MonoBehaviour
{
    //public GameObject pairedTile;
    //public float offset = 42.27f;
    public Vector2 range;
    public float parallax=0;

    // Update is called once per frame
    void Update()
    {
        transform.Translate(-Main.S.gameSpeed * parallax * Time.deltaTime, 0, 0);
        //print(Mathf.Abs(pairedTile.transform.position.x - transform.position.x)-offset);
        if (transform.position.x <= range.x)
            transform.position = new Vector3(range.y, transform.position.y,transform.position.z);
    }
}

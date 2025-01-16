using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Section : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        transform.Translate(new Vector2(-Main.S.gameSpeed * Time.deltaTime, 0), Space.World);
        if (transform.childCount == 0) Destroy(gameObject);
    }
}

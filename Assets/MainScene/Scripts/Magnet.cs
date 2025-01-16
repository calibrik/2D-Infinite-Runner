using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magnet : MonoBehaviour
{
    public float force = 3;
    public float radius=2.52f;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<CircleCollider2D>().radius=radius;
    }

    // Update is called once per frame
    private void OnTriggerStay2D(Collider2D collision)
    {
        Vector2 direction = transform.position - collision.transform.position;
        collision.transform.Translate(direction.normalized*force*(1-direction.magnitude/radius)*Time.deltaTime,Space.World);
        //collision.transform.Translate(direction.normalized * force  * Time.deltaTime, Space.World);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float accelarationRot = 3f;
    public float g = 3f;
    public float initialV = 150f;
    public float distance = 1000f;

    private float _vRot = 0;
    private float _velocity = 0;
    private Boundaries _bounds;
    // Start is called before the first frame update
    void Start()
    {
        _bounds = GetComponent<Boundaries>();
    }
    // Update is called once per frame
    void Update()
    {
        _vRot += accelarationRot * Time.deltaTime;
        transform.Rotate(0,0,_vRot*Time.deltaTime);
        _velocity-=g*Time.deltaTime;
        transform.Translate(0, _velocity*Time.deltaTime, 0,Space.World);
        if (_bounds.isOnBottom)
        {
            PlayerController.S.Explode(initialV,distance,Main.S.GameOver);
            Destroy(gameObject);
        }       
    }
}

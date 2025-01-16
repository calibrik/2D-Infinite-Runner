using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundaries : MonoBehaviour
{
    public Vector2 vertical;
    public bool isActive = true;
    public bool isOnBottom
    {
        get { return _isOnBottom; }
    }
    public bool isOnTop
    { 
        get { return _isOnTop; } 
    }

    private bool _isOnBottom=false;
    private bool _isOnTop=false;
    // Start is called before the first frame update


    // Update is called once per frame
    void Update()
    {
        if (!isActive) return;
        _isOnBottom = false;
        _isOnTop = false;
        if (transform.position.y<=vertical.x)
        {
            _isOnBottom = true;
            transform.position = new Vector2(transform.position.x, vertical.x);
        }
        else if (transform.position.y>=vertical.y)
        {
            _isOnTop = true;
            transform.position = new Vector2(transform.position.x, vertical.y);
        }
    }
}

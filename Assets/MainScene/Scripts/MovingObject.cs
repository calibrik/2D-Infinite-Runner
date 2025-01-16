using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

abstract public class MovingObject : MonoBehaviour
{
    //public MonoBehaviour[] toTurnOn;
    public float turnOnOnX;
    public float removeX;
    public bool isOptimized;
    public Transform pool;

    public virtual void SetComponents(bool isTurnedOn)
    {
        Collider2D collider2D = GetComponent<Collider2D>();
        if (collider2D) collider2D.enabled = isTurnedOn;
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite) sprite.enabled = isTurnedOn;
        Animator animator = GetComponent<Animator>();
        if (animator) animator.enabled = isTurnedOn;
        isOptimized = !isTurnedOn;
    }
    public virtual void RemoveFromScene()
    {
        SetComponents(false);
        gameObject.SetActive(false);
        transform.eulerAngles = Vector3.zero;
        transform.parent = pool;
        transform.position = pool.position;
    }
    // Update is called once per frame
    protected virtual void Update()
    {
        if (isOptimized && transform.position.x < turnOnOnX) SetComponents(true);
        if (transform.position.x <= removeX) RemoveFromScene();
    }
}

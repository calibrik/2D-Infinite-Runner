using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndSectionTrigger : MovingObject
{
    //public float deleteCoordinateX = -10;
    // Start is called before the first frame update
    public override void RemoveFromScene()
    {
        Destroy(gameObject);
    }

   protected override void Update()
    {
        base.Update();
        if (transform.position.x <= 0)
        {
            Main.S.SectionEnded();
            RemoveFromScene();
        }
    }
}

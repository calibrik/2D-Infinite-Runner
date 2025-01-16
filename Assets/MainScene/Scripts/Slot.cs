using UnityEngine;


public class Slot : MonoBehaviour
{
    public Casino.Column column;
    [HideInInspector]
    public bool hasSpawned;

    private RectTransform rect;
    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!column.isMoving&&column.currSpeed<=0) return;

        //rect.Translate(0, -column.currSpeed * Time.deltaTime, 0,Space.World);
        rect.localPosition=new Vector2(rect.localPosition.x, rect.localPosition.y - column.currSpeed * Time.deltaTime);
        if (!hasSpawned && rect.localPosition.y <= column.spawnOnPastY)
        {
            hasSpawned = true;
            column.SpawnNewSlot(rect.localPosition.y + column.slotHeight);
        }
        if (rect.localPosition.y <= column.delOnY)
        {
            column.DeleteSlot();
            Destroy(gameObject);
        }
    }
}

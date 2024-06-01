using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDrag : MonoBehaviour
{
    private Canvas canvas;
    private float startY;
    private int index;

    private void Start()
    {
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        startY = transform.parent.parent.position.y;
    }

    public void Initialize(int index)
    {
        this.index = index;
    }

    public void DragBegin(BaseEventData data)
    {
        transform.SetParent(transform.parent.parent.transform);
        layoutgroupcontroller.RemoveCard(index);
    }

    public void DragHandler(BaseEventData data)
    {
        PointerEventData pointerEventData = (PointerEventData)data;

        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform,
            pointerEventData.position,
            canvas.worldCamera,
            out position
        );

        transform.position = canvas.transform.TransformPoint(position) + Vector3.left * 10;
    }

    public void DragEnd(BaseEventData data)
    {
        PointerEventData pointerEventData = (PointerEventData)data;

        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform,
            pointerEventData.position,
            canvas.worldCamera,
            out position
        );

        layoutgroupcontroller.Instance.InsertCard(canvas.transform.TransformPoint(position));
        Destroy(gameObject);
    }
}

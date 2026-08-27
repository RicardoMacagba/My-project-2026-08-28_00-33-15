using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    private bool isDragging;
    private Vector3 offset;
    private Vector3 startPosition;
    private Transform currentSlot;

    void Start()
    {
        startPosition = transform.position;
    }

    void OnMouseDown()
    {
        offset = transform.position - GetMouseWorldPos();
        isDragging = true;
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            transform.position = GetMouseWorldPos() + offset;
        }
    }

    void OnMouseUp()
    {
        isDragging = false;

        // If we are hovering over ANY slot, snap to it!
        if (currentSlot != null)
        {
            transform.position = currentSlot.position;
            this.enabled = false; // Lock it in place
            Debug.Log("Snapped successfully!");
            return;
        }

        // If we missed the slot, go back home
        transform.position = startPosition;
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = 0;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Detect if the object we touched is a slot
        if (collision.CompareTag("Respawn")) 
        {
            currentSlot = collision.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (currentSlot == collision.transform)
        {
            currentSlot = null;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideTile : Tile
{
    [SerializeField] private Color baseColor, offsetColor;

    public void BoxUpdate()
    {
        BlocksMove = false;
        foreach (GameObject box in GridManager.Instance.boxes) {
            if (GridManager.Instance.GetClosestCell(box.transform.position) == GridManager.Instance.GetClosestCell(transform.position)) {
                BlocksMove = true;
            }
        }
    }

    public void InitializeColor(bool isOffset)
    {
        GetComponent<Renderer>().material.color = isOffset ? offsetColor : baseColor;
    }
}

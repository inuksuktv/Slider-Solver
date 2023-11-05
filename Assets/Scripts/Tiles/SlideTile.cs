using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideTile : Tile
{
    [SerializeField] private Color baseColor, offsetColor;
    public override bool BlocksMove
    {
        get { return blocksMove; }
        set { blocksMove = value; }
    }

    public void BoxUpdate()
    {
        foreach (GameObject box in GridManager.Instance.boxes) {
            if (GridManager.Instance.GetClosestCell(box.transform.position) == GridManager.Instance.GetClosestCell(transform.position)) {
                BlocksMove = true;
            }
            else { BlocksMove = false; }
        }
    }

    public void InitializeColor(bool isOffset)
    {
        GetComponent<Renderer>().material.color = isOffset ? offsetColor : baseColor;
    }
}

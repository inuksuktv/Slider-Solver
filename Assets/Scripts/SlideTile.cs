using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideTile : Tile
{
    [SerializeField] private Color baseColor, offsetColor;
    private GameObject highlight;
    public override bool blocksMove => false;

    private void Start()
    {
        highlight = transform.GetChild(0).gameObject;
    }

    public void InitializeColor(bool isOffset)
    {
        GetComponent<Renderer>().material.color = isOffset ? offsetColor : baseColor;
    }

    private void OnMouseEnter()
    {
        highlight.SetActive(true);
    }

    private void OnMouseExit()
    {
        highlight.SetActive(false);
    }
}

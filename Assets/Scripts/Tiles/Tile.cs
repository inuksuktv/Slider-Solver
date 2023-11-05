using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    protected bool blocksMove = true;
    public virtual bool BlocksMove
    {
        get { return blocksMove; }
        set { blocksMove = value; }
    }
}

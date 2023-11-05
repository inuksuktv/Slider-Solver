using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalTile : Tile
{
    public override bool BlocksMove
    {
        get { return false; }
    }
}

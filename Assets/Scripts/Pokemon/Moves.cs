using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public MoveBase moveBase { get; set; }
    public int PP { get; set; }

    public Move(MoveBase pBase)
    {
        moveBase = pBase;
        PP = pBase.PP;
    }
}

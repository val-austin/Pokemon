using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialog
{
    [SerializeField] List<string> line;

    public List<string> Lines
    {
        get { return line; }
    }
    
}

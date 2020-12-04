using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Goap;

public interface IBrain
{
    void Init();
    void Tick(IGoap goap);
    void Release();
    Dictionary<string,bool> NextGoal();
}

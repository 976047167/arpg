using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tester : Labourer
{
    /**
     * Our only goal will ever be to make tools.
     * The ForgeTooldAction will be able to fulfill this goal.
     */
    public override Dictionary<string, bool> createGoalState()
    {
        return Brain.NextGoal();
    }

    public override void Tick()
    {
        base.Tick();

        if (NeedAbort)
        {
            Agent.AbortFsm();
            NeedAbort = false;
        }
    }

    public bool NeedAbort;
}

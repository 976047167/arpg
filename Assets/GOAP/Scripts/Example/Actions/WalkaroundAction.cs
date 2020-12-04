using UnityEngine;
using System.Collections;

namespace Goap.Action
{
    public class WalkaroundAction : GoapAction
    {
        private GameObject _walkTarget;
        private bool isReached = false;

        public WalkaroundAction()
        {
            addEffect("walkaround", true);
        }


        public override void reset()
        {
            isReached = false;
        }

        public override bool isDone()
        {
            return isReached;
        }

        public override bool requiresInRange()
        {
            return true;
        }

        public override bool checkProceduralPrecondition(GameObject agent, BlackBoard bb)
        {
            if (_walkTarget == null)
            {
                _walkTarget = new GameObject("walkTarget");
                RandomTarget(agent);
            }

            target = _walkTarget;

            return true;
        }

        void RandomTarget(GameObject agent)
        {
            _walkTarget.transform.position = agent.transform.position;
            _walkTarget.transform.position += new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));
        }

        public override bool perform(GameObject agent, BlackBoard bb)
        {
            RandomTarget(agent);

            isReached = true;

            return true;
        }

    }

}
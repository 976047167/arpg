using UnityEngine;
using System.Collections;

namespace Goap.Action
{
    public class EatAppleAction : GoapAction
    {
        private bool eated = false;
        private AppleTreeComponet targetAppleTree; // where we get the logs from

        private float startTime = 0;
        public float workDuration = 2; // seconds
        public EatAppleAction()
        {
            addEffect(Goals.FillHunger, true);
        }
        public override void reset()
        {
            eated = false;
            targetAppleTree = null;
            startTime = 0;
        }

        public override bool isDone()
        {
            return eated;
        }

        public override bool checkProceduralPrecondition(GameObject agent, BlackBoard bb)
        {
            // find the nearest tree that we can chop
            AppleTreeComponet[] trees = (AppleTreeComponet[])bb.GetData("appleTree");
            AppleTreeComponet closest = null;
            float closestDist = 0;

            foreach (AppleTreeComponet tree in trees)
            {
                if (closest == null)
                {
                    // first one, so choose it for now
                    closest = tree;
                    closestDist = (tree.gameObject.transform.position - agent.transform.position).magnitude;
                }
                else
                {
                    // is this one closer than the last?
                    float dist = (tree.gameObject.transform.position - agent.transform.position).magnitude;
                    if (dist < closestDist)
                    {
                        // we found a closer one, use it
                        closest = tree;
                        closestDist = dist;
                    }
                }
            }
            if (closest == null)
                return false;

            targetAppleTree = closest;
            target = targetAppleTree.gameObject;

            return closest != null && closest.AppleNum > 0;
        }

        public override bool perform(GameObject agent, BlackBoard bb)
        {
            if (startTime == 0)
                startTime = Time.time;

            if (Time.time - startTime > workDuration)
            {
                // finished chopping
                Brain brain = (Brain)agent.GetComponent(typeof(Brain));
                targetAppleTree.AppleNum--;
                brain.Hunger += 50;
                eated = true;
            }
            return true;
        }

        public override bool requiresInRange()
        {
            return true; // yes we need to be near a tree
        }
    }

}
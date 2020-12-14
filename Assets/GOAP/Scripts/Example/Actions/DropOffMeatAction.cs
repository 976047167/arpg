
using System;
using UnityEngine;

namespace Goap.Action
{
    public class DropOffMeatAction : GoapAction
    {
        private bool droppedOffMeat = false;
        private SupplyPileComponent targetSupplyPile; // where we drop off the  tools

        public DropOffMeatAction()
        {
            addPrecondition("hasMeat", true); // can't drop off tools if we don't already have some
            addEffect("hasMeat", false); // we now have no tools
            addEffect(Goals.FillOther, true); // we collected tools
        }


        public override void reset()
        {
            droppedOffMeat = false;
            targetSupplyPile = null;
        }

        public override bool isDone()
        {
            return droppedOffMeat;
        }

        public override bool requiresInRange()
        {
            return true; // yes we need to be near a supply pile so we can drop off the tools
        }

        public override bool checkProceduralPrecondition(GameObject agent, DataBag bb)
        {
            // find the nearest supply pile that has spare tools
            SupplyPileComponent[] supplyPiles = (SupplyPileComponent[])bb.GetData("supplyPiles");
            SupplyPileComponent closest = null;
            float closestDist = 0;

            foreach (SupplyPileComponent supply in supplyPiles)
            {
                if (closest == null)
                {
                    // first one, so choose it for now
                    closest = supply;
                    closestDist = (supply.gameObject.transform.position - agent.transform.position).magnitude;
                }
                else
                {
                    // is this one closer than the last?
                    float dist = (supply.gameObject.transform.position - agent.transform.position).magnitude;
                    if (dist < closestDist)
                    {
                        // we found a closer one, use it
                        closest = supply;
                        closestDist = dist;
                    }
                }
            }
            if (closest == null)
                return false;

            targetSupplyPile = closest;
            target = targetSupplyPile.gameObject;

            return closest != null;
        }

        public override bool perform(GameObject agent, DataBag bb)
        {
            BackpackComponent backpack = (BackpackComponent)agent.GetComponent(typeof(BackpackComponent));
            targetSupplyPile.numMeat += backpack.numMeat;
            backpack.numMeat = 0;
            droppedOffMeat = true;
            //TODO play effect, change actor icon

            return true;
        }
    }
}
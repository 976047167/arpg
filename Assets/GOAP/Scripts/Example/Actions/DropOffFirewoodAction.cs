
using System;
using UnityEngine;

namespace Goap.Action
{
public class DropOffFirewoodAction : GoapAction
{
	private bool droppedOffFirewood = false;
	private SupplyPileComponent targetSupplyPile; // where we drop off the firewood
	
	public DropOffFirewoodAction () {
		addPrecondition ("hasFirewood", true); // can't drop off firewood if we don't already have some
		addEffect ("hasFirewood", false); // we now have no firewood
        addEffect("collectFirewood", true); // we collected firewood
        addEffect(Goals.FillOther, true); // we collected ore
	}
	
	
	public override void reset ()
	{
		droppedOffFirewood = false;
		targetSupplyPile = null;
	}
	
	public override bool isDone ()
	{
		return droppedOffFirewood;
	}
	
	public override bool requiresInRange ()
	{
		return true; // yes we need to be near a supply pile so we can drop off the firewood
	}
	
	public override bool checkProceduralPrecondition (GameObject agent,DataBag bb)
	{
		// find the nearest supply pile that has spare firewood
        SupplyPileComponent[] supplyPiles = (SupplyPileComponent[])bb.GetData("supplyPiles");
		SupplyPileComponent closest = null;
		float closestDist = 0;
		
		foreach (SupplyPileComponent supply in supplyPiles) {
			if (closest == null) {
				// first one, so choose it for now
				closest = supply;
				closestDist = (supply.gameObject.transform.position - agent.transform.position).magnitude;
			} else {
				// is this one closer than the last?
				float dist = (supply.gameObject.transform.position - agent.transform.position).magnitude;
				if (dist < closestDist) {
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
		targetSupplyPile.numFirewood += backpack.numFirewood;
		droppedOffFirewood = true;
		backpack.numFirewood = 0;
		
		return true;
	}
}

}
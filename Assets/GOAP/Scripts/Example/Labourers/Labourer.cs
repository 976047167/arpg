using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Goap;
using Goap.Action;

/**
 * A general labourer class.
 * You should subclass this for specific Labourer classes and implement
 * the createGoalState() method that will populate the goal for the GOAP
 * planner.
 */
public abstract class Labourer : MonoBehaviour, IGoap
{
	public BackpackComponent backpack;
	public float moveSpeed = 1;
    public bool EnableLog = false;
	private DataBag bb = new DataBag();

	void Start ()
	{
        Init();
	}


	void Update ()
	{
        Tick();
	}

    Dictionary<string, bool> worldData = new Dictionary<string, bool>();
	/**
	 * Key-Value data that will feed the GOAP actions and system while planning.
	 */
    public Dictionary<string, bool> getWorldState()
    {
        worldData["hasOre"] = backpack.numOre > 0;
        worldData["hasLogs"] = backpack.numLogs > 0;
        worldData["hasFirewood"] = backpack.numFirewood > 0;
        worldData["hasTool"] = backpack.tool != null;
        worldData["hasMeat"] = backpack.numMeat > 0;

		return worldData;
	}


    public DataBag GetBlackBoard()
    {
        return bb;
    }

	/**
	 * Implement in subclasses
	 */
    public abstract Dictionary<string, bool> createGoalState();


    public void planFailed(Dictionary<string, bool> failedGoal)
	{
		// Not handling this here since we are making sure our goals will always succeed.
		// But normally you want to make sure the world state has changed before running
		// the same goal again, or else it will just fail.
	}

    public void planFound(KeyValuePair<string, bool> goal, Queue<GoapAction> actions)
	{
		// Yay we found a plan for our goal
        if (EnableLog)
		    Debug.Log ("<color=green>Plan found</color> "+GoapAgent.prettyPrint(actions));
	}

	public void actionsFinished ()
	{
		// Everything is done, we completed our actions for this gool. Hooray!
        if (EnableLog)
            Debug.Log("<color=blue>Actions completed</color>");
	}

	public void planAborted (GoapAction aborter)
	{
		// An action bailed out of the plan. State has been reset to plan again.
		// Take note of what happened and make sure if you run the same goal again
		// that it can succeed.
        if (EnableLog)
            Debug.Log("<color=red>Plan Aborted</color> " + GoapAgent.prettyPrint(aborter));
	}

	public bool moveAgent(GoapAction nextAction) {
		// move towards the NextAction's target
		float step = moveSpeed * Time.deltaTime;
		gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, nextAction.target.transform.position, step);
		
		if (gameObject.transform.position.Equals(nextAction.target.transform.position) ) {
			// we are at the target location, we are done
			nextAction.setInRange(true);
			return true;
		} else
			return false;
	}

    public virtual void Init()
    {
        if (backpack == null)
            backpack = gameObject.AddComponent<BackpackComponent>() as BackpackComponent;
        if (backpack.tool == null)
        {
            GameObject prefab = Resources.Load<GameObject>(backpack.toolType);
            GameObject tool = Instantiate(prefab, transform.position, transform.rotation) as GameObject;
            backpack.tool = tool;
            tool.transform.parent = transform; // attach the tool
        }

        if (Brain == null)
            Brain = gameObject.GetComponent<Brain>();
        Brain.Init();

        //init world data
        worldData.Add("hasOre", (backpack.numOre > 0));
        worldData.Add("hasLogs", (backpack.numLogs > 0));
        worldData.Add("hasFirewood", (backpack.numFirewood > 0));
        worldData.Add("hasTool", (backpack.tool != null));
        worldData.Add("hasMeat", (backpack.numMeat > 0));

        //init blackboard
        bb.AddData("backpack", backpack);
        bb.AddData("brain", Brain);
        bb.AddData("ironRock", FindObjectsOfType(typeof(IronRockComponent)));
        bb.AddData("appleTree", FindObjectsOfType(typeof(AppleTreeComponet)));
        bb.AddData("forge", FindObjectsOfType(typeof(ForgeComponent)));
        bb.AddData("tree", FindObjectsOfType(typeof(TreeComponent)));
        bb.AddData("wolfDen", FindObjectsOfType(typeof(WolfDen)));
        bb.AddData("choppingBlock", FindObjectsOfType(typeof(ChoppingBlockComponent)));
        bb.AddData("supplyPiles", FindObjectsOfType(typeof(SupplyPileComponent)));
        bb.AddData("camp", FindObjectsOfType(typeof(CampComponent)));
    }

    public virtual void Tick()
    {
        Brain.Tick(this);
    }

    public virtual void Release()
    {
        Brain.Release();
    }

    public IAgent Agent { get; set; }

    public IBrain Brain { get; set; }
}


using System.Collections.Generic;
using UnityEngine;

namespace Goap.Action
{
	[GoapActionType(ACTION_TYPE.MOVE)]
	public class MoveAction : GoapAction
	{
		public override void Reset(GoapAgent agent)
		{

			DataBag bb = agent.dataProvider.GetDataBag();
			Queue<GameObject> q = bb.GetData<Queue<GameObject>>("MoveTarget");
			if(q == null)
				bb.SetData("MoveTarget",new Queue<GameObject>());
			else
				q.Clear();
		}

		public override bool checkProceduralPrecondition(GoapAgent agent)
		{
			return agent.dataProvider.GetDataBag().GetData<bool>("CanMove");
		}

		public override IEnumerator<bool> createPerformance(GoapAgent agent)
		{
			// throw new NotImplementedException();
			DataBag bb = agent.dataProvider.GetDataBag();
			GameObject gameObject = agent.dataProvider.gameObject;
			bool ret = true;
			GameObject target =  bb.GetData<Queue<GameObject>>("MoveTarget").Peek();
			if (target == null)
			{
				yield return false;
			}
			while (ret)
			{

				if (gameObject.transform.position.Equals(target.transform.position))
				{
					yield break;
				}
				yield return true;
			}
		}
		
	}
}
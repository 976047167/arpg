using System;
using UnityEngine;
namespace GameAction
{
	[GameActionType(ACTION_TYPE.SetDefauletValue)]
	[StartType(START_TYPE.Automatic)]
	[AnimatorIndex(0)]
	public class SetDefaultValue : GameActionBase
	{
		public override bool IsConcurrent {get { return true; } }
		public override bool CanActivate()
		{
			return true;
		}
		public override void Activavte(){
			this.AnimatorInt = 1;
			base.Activavte();
		}
		public override bool CanDeactivate(bool force =false)
		{
			if(force)return true;
			return false;
		}

	}
}
using System;
namespace GameAction
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	internal class GameActionType : Attribute
	{
		public readonly ACTION_TYPE type;
		public GameActionType(ACTION_TYPE type)
		{
			this.type = type;
		}

	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	internal class AnimatorIndex : Attribute
	{
		public readonly int idx;
		public AnimatorIndex(int idx)
		{
			this.idx = idx;
		}

	}
}
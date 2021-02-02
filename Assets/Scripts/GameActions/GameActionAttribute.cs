using System;
namespace GameAction
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	internal class GameActionType : Attribute
	{
		public readonly ACTION_TYPE value;
		public GameActionType(ACTION_TYPE type)
		{
			this.value = type;
		}

	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	internal class AnimatorIndex : Attribute
	{
		public readonly int value;
		public AnimatorIndex(int idx)
		{
			this.value = idx;
		}

	}
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	internal class StartType : Attribute
	{
		public readonly START_TYPE value;
		public StartType (START_TYPE idx)
		{
			this.value = idx;
		}

	}
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	internal class StopType : Attribute
	{
		public readonly START_TYPE value;
		public StopType (START_TYPE idx)
		{
			this.value = idx;
		}

	}
}
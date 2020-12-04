using System;
namespace Goap.Action
{

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	internal class GoapActionType : Attribute
	{
		public readonly ACTION_TYPE type;
		public GoapActionType(ACTION_TYPE type)
		{
			this.type = type;
		}

	}
}
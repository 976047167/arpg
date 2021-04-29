namespace GameAction
{
	public class Jump:GameActionBase {
		public override void Initialize(CharacterLocomotion owner,int priority)
		{
			base.Initialize(owner,priority);
			Notification.CreateBinding<CharacterLocomotion,string>(GameEvent.AnimationEvent, this.OnAnimationEvent);
		}
		private void OnAnimationEvent(CharacterLocomotion locomotion,string evnet)
		{
			if(this.OwnerLocomotion != locomotion) return;
			if(evnet != "OnAnimatorJump")return;
		}
	}
}
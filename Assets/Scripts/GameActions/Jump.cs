using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAction;
namespace GameAction
{
	public class Jump:GameActionBase {
		private bool AnimationFinished ;
		private RaycastHit RaycastRet ;
		private float MinCeilingJumpHeight =0.05f;

		public override void Initialize(CharacterLocomotion owner,int priority)
		{
			base.Initialize(owner,priority);
			Notification.CreateBinding<CharacterLocomotion,string>(GameEvent.AnimationEvent, this.OnAnimationEvent);
		}
		private void OnAnimationEvent(CharacterLocomotion locomotion,string evnet)
		{
			if(this.OwnerLocomotion != locomotion) return;
			if(evnet != "OnAnimatorJump")return;
			this.AnimationFinished = true;
		}
		public override bool CanActivate(PlayerInput input)
		{
			if(! base.CanActivate(input)) return false;
			if(!input.GetJump())return false;
			var castLength = this.OwnerLocomotion.SkinWidth + Constants.ColliderSpacing +0.05f;
			if (this.OwnerLocomotion.SingleCast(this.OwnerLocomotion.transform.up *castLength,
													Vector3.zero,LayerMask.SolidObjectLayers, ref this.RaycastRet)) {
				return false;
			}
			return true;
		}
		public override void Activavte()
		{
			this.OwnerLocomotion.UseAnimatorRotation = false;
			this.OwnerLocomotion.UseAnimatorPosition = false;
			base.Activavte();
		}
	}
}
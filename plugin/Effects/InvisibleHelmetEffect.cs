using System;
using SideLoader;

namespace HideHelmet.Effects {
	
	public class InvisibleHelmetEffectTemplate : SL_Effect, ICustomModel {
		public Type SLTemplateModel => typeof(InvisibleHelmetEffectTemplate);
		public Type GameModel => typeof(InvisibleHelmetEffect);

		public override void ApplyToComponent<T>(T component) { }
		public override void SerializeEffect<T>(T effect) { }
	}


	public class InvisibleHelmetEffect : Effect, ICustomModel {
		public Type SLTemplateModel => typeof(InvisibleHelmetEffectTemplate);
		public Type GameModel => typeof(InvisibleHelmetEffect);
		
		private EquipmentSlot GetHelmetSlot(Character character) {
			return character.Inventory.Equipment.EquipmentSlots[(int)EquipmentSlot.EquipmentSlotIDs.Helmet];
		}

		public override void ActivateLocally(Character _affectedCharacter, object[] _infos) {
			VisualSlot helmetSlot = GetHelmetSlot(_affectedCharacter).VisualSlot;
			if (helmetSlot.CurrentVisual != null) {
				HideHelmet.Log.LogInfo("Hiding helmet");
				helmetSlot.PutBackVisuals();
			}
		}

		public override void StopAffectLocally(Character _affectedCharacter) {
			EquipmentSlot helmetSlot = GetHelmetSlot(_affectedCharacter);
			HideHelmet.Log.LogInfo("Showing helmet");
			if (helmetSlot.EquippedItem) {
				helmetSlot.VisualSlot.PositionVisuals(helmetSlot.EquippedItem);
			}
		}
	}
}
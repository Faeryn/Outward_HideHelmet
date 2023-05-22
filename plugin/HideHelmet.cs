using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using HideHelmet.Effects;
using SideLoader;

namespace HideHelmet {
	[BepInPlugin(GUID, NAME, VERSION)]
	public class HideHelmet : BaseUnityPlugin {
		public const string GUID = "faeryn.hidehelmet";
		public const string NAME = "HideHelmet";
		public const string VERSION = "0.1.0";
		public const string KEY_TOGGLE_HELMET = "Toggle Helmet Visibility";
		internal static ManualLogSource Log;
		
		public static ConfigEntry<bool> ShowHelmet;

		internal void Awake() {
			Log = this.Logger;
			Log.LogMessage($"Starting {NAME} {VERSION}");
			InitializeKeybindings();
			InitializeSL();
			new Harmony(GUID).PatchAll();
		}
		
		private void InitializeSL() {
			SL.BeforePacksLoaded += SL_BeforePacksLoaded;
		}

		private void SL_BeforePacksLoaded() {
			new SL_StatusEffect {
				TargetStatusIdentifier = "Stability Up",
				NewStatusID = -12080,
				StatusIdentifier = Constants.INVISIBLE_HELMET_STATUS_IDENTIFIER,
				Name = "Invisible Helmet",
				Description = "Your helmet is invisible",
				Purgeable = false,
				DisplayedInHUD = false,
				IsMalusEffect = false,
				Lifespan = -1f,
				RefreshRate = 1f,
				AmplifiedStatusIdentifier = string.Empty,
				FamilyMode = StatusEffect.FamilyModes.Bind,
				EffectBehaviour = EditBehaviours.Destroy,
				Effects = new SL_EffectTransform[] {
					new SL_EffectTransform {
						TransformName = "Effects",
						Effects = new SL_Effect[] {
							new InvisibleHelmetEffectTemplate()
						}
					}
				}
			}.ApplyTemplate();

			new SL_Skill {
				Target_ItemID = 8200010, // Reveal Soul
				New_ItemID = Constants.INVISIBLE_HELMET_SKILL_ID,
				Name = "Invisible Helmet",
				Description = "Hides your equipped helmet",
				Cooldown = 1.0f,
				StaminaCost = 10.0f,
				ManaCost = 0.0f,
				CastType = Character.SpellCastType.UseUp,
				EffectBehaviour = EditBehaviours.Destroy,
				EffectTransforms = new SL_EffectTransform[] {
					new SL_EffectTransform {
						TransformName = "Effects",
						EffectConditions = new SL_EffectCondition[] {
							new SL_StatusEffectCondition {
								StatusIdentifier = Constants.INVISIBLE_HELMET_STATUS_IDENTIFIER
							}	
						},
						Effects = new SL_Effect[] {
							new SL_RemoveStatusEffect {
								CleanseType = RemoveStatusEffect.RemoveTypes.StatusSpecific,
								SelectorValue = Constants.INVISIBLE_HELMET_STATUS_IDENTIFIER
							}
						}
					},
					new SL_EffectTransform {
						TransformName = "Effects",
						EffectConditions = new SL_EffectCondition[] {
							new SL_StatusEffectCondition {
								StatusIdentifier = Constants.INVISIBLE_HELMET_STATUS_IDENTIFIER,
								Invert = true
							}	
						},
						Effects = new SL_Effect[] {
							new SL_AddStatusEffect {
								StatusEffect = Constants.INVISIBLE_HELMET_STATUS_IDENTIFIER
							}
						}
					}
				}
			}.ApplyTemplate();

			SL_Item potion = new SL_Item {
				Target_ItemID = 4300130,
				New_ItemID = -12083,
				Name = "Potion of Invisible Helmet",
				Description = "Potion that makes you able to turn your helmet invisible at will",
				StatsHolder = new SL_ItemStats {
					BaseValue = 34,
					RawWeight = 1f
				},
				EffectBehaviour = EditBehaviours.Destroy,
				EffectTransforms = new SL_EffectTransform[] {
					new SL_EffectTransform {
						TransformName = "Effects",
						Effects = new SL_Effect[] {
							new SL_LearnSkillEffect {
								SkillID = Constants.INVISIBLE_HELMET_SKILL_ID
							}
						}
					}
				}
			};
			potion.ApplyTemplate();
			
			SL_Recipe potionRecipe = new SL_Recipe {
				UID = Constants.POTION_RECIPE_UID,
				StationType = Recipe.CraftingType.Alchemy,
				Ingredients = {
					new SL_Recipe.Ingredient {
						Type = RecipeIngredient.ActionTypes.AddSpecificIngredient,
						SelectorValue = "4400060" // Spiritual Varnish
					},
					new SL_Recipe.Ingredient {
						Type = RecipeIngredient.ActionTypes.AddSpecificIngredient,
						SelectorValue = "4300130" // Stealth Potion
					},
					new SL_Recipe.Ingredient {
						Type = RecipeIngredient.ActionTypes.AddSpecificIngredient,
						SelectorValue = "4300250" // Great Astral Potion
					}
				},
				Results = {
					new SL_Recipe.ItemQty {
						ItemID = potion.New_ItemID, 
						Quantity = 2
					}
				}
			};
			potionRecipe.ApplyTemplate();

			SL_RecipeItem potionRecipeItem = new SL_RecipeItem {
				RecipeUID = Constants.POTION_RECIPE_UID,
				Target_ItemID = 5700110,
				New_ItemID = -12084,
				Name = "Alchemy: Potion of Invisible Helmet"
			};
			potionRecipeItem.ApplyTemplate();

			SL_DropTable recipeDT = new SL_DropTable {
				UID = Constants.POTION_DROPTABLE_UID,
				RandomTables = {new SL_RandomDropGenerator {
					MinNumberOfDrops = 1,
					MaxNumberOfDrops = 2,
					NoDrop_DiceValue = 2,
					Drops = {
						new SL_ItemDropChance {
							DiceValue = 5,
							MinQty = 1,
							MaxQty = 1,
							DroppedItemID = potionRecipeItem.New_ItemID
						}
					}
				}}
			};
			recipeDT.ApplyTemplate();
			
			SL_DropTableAddition potionAndRecipeForMerchants = new SL_DropTableAddition {
				SelectorTargets = {"-MSrkT502k63y3CV2j98TQ", "G_GyAVjRWkq8e2L8WP4TgA"}, // Soroborean Caravanner
				DropTableUIDsToAdd = {recipeDT.UID}
				
			};
			potionAndRecipeForMerchants.ApplyTemplate();
		}

		public void InitializeKeybindings() {
			CustomKeybindings.AddAction(KEY_TOGGLE_HELMET, KeybindingsCategory.CustomKeybindings, ControlType.Both);
		}

		public void Update() {
			int playerID;
			if (CustomKeybindings.GetKeyDown(KEY_TOGGLE_HELMET, out playerID)) {
				ToggleHelmet(playerID);
			}
		}

		private void ToggleHelmet(int playerID) {
			Character character = GetLocalCharacter(playerID);
			Skill invisibleHelmetSkill = character.Inventory.SkillKnowledge.GetItemFromItemID(Constants.INVISIBLE_HELMET_SKILL_ID) as Skill;
			if (invisibleHelmetSkill) {
				invisibleHelmetSkill.TryUse(character);
			}
		}
		
		private Character GetLocalCharacter(int playerID) {
			return SplitScreenManager.Instance.LocalPlayers[playerID].AssignedCharacter;
		}
	}
}
using TMPro;
using UnityEngine;
using System;
using static UnityEngine.RuleTile.TilingRuleOutput;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;
using System.Collections;

public class Character_Core : MonoBehaviour
{
    #region variable declarations

	private GameHandler caller;
	private GameHandler.character_type this_characters_type;
	private Character_Core target_opponent;
	private int current_health;
	private int current_damage;
	private int max_health;
	private int end_of_turn_effect_counter = 0;
	private bool after_action = false;
	private bool end_turn = false;
	private bool end_of_turn_effects = false;
	private bool used_minor_agility = false;
	private bool used_major_agility = false;
	private float action_timer = 0;
	private float end_of_turn_timer = 0;
	private TextMeshProUGUI health_text;
	private TextMeshProUGUI damage_text;
	private TextMeshProUGUI armor_text;
	private TextMeshProUGUI fighting_text;

	#region creature effects

	private int armor = 0;
	private int minor_hemorrhage = 0;
	private int major_hemorrhage = 0;
	private int poison = 0;
	private int venom = 0;
	private int minor_frenzy = 0;
	private int major_frenzy = 0;
	private int regrowth = 0;

	private bool minor_derangement = false;
	private bool major_derangement = false;
	private bool minor_agility = false;
	private bool major_agility = false;
	private bool minor_resistance = false;
	private bool major_resistance = false;
	private bool minor_life_steal = false;
	private bool major_life_steal = false;
	private bool minor_luck = false;
	private bool major_luck = false;

	#endregion
	#region end of turn effects

	private List<GameHandler.EndOfTurnStackSubtraction> end_of_turn_stack_subtraction_list = new List<GameHandler.EndOfTurnStackSubtraction>();

	private int bleed_stacks = 0;
	private int poison_stacks = 0;
	private int minor_frenzy_stacks = 0;
	private int major_frenzy_stacks = 0;
	private int regrowth_stacks = 0;

	#endregion

	public void SetVariables (GameHandler.CardCreature creature)
	{
		this_characters_type = GameHandler.character_type.monster;
		max_health = creature.minion_health;
		current_damage = creature.minion_attack_dmg;
		foreach (GameHandler.CreatureEffect effect in creature.effects)
		{
			switch (effect.effect)
			{
				case GameHandler.creature_effects.armor:
				armor += effect.effect_strength;
				break;

				case GameHandler.creature_effects.minor_hemorrhage:
				minor_hemorrhage += effect.effect_strength;
				break;

				case GameHandler.creature_effects.major_hemorrhage:
				major_hemorrhage += effect.effect_strength;
				break;

				case GameHandler.creature_effects.poison:
				poison += effect.effect_strength;
				break;

				case GameHandler.creature_effects.venom:
				venom += effect.effect_strength;
				break;

				case GameHandler.creature_effects.minor_frenzy:
				minor_frenzy += effect.effect_strength;
				break;

				case GameHandler.creature_effects.major_frenzy:
				major_frenzy += effect.effect_strength;
				break;

				case GameHandler.creature_effects.regrowth:
				regrowth += effect.effect_strength;
				break;
			}
		}
		foreach (GameHandler.CreaturePassiveEffect effect in creature.passive_effects)
		{
			switch (effect.effect)
			{
				case GameHandler.creature_passive_effects.minor_derangement:
				minor_derangement = true;
				break;

				case GameHandler.creature_passive_effects.major_derangement:
				major_derangement = true;
				break;

				case GameHandler.creature_passive_effects.minor_agility:
				minor_agility = true;
				break;

				case GameHandler.creature_passive_effects.major_agility:
				major_agility = true;
				break;

				case GameHandler.creature_passive_effects.minor_resistance:
				minor_resistance = true;
				break;

				case GameHandler.creature_passive_effects.major_resistance:
				major_resistance = true;
				break;

				case GameHandler.creature_passive_effects.minor_life_steal:
				minor_life_steal = true;
				break;

				case GameHandler.creature_passive_effects.major_life_steal:
				major_life_steal = true;
				break;

				case GameHandler.creature_passive_effects.minor_luck:
				minor_luck = true;
				break;

				case GameHandler.creature_passive_effects.major_luck:
				major_luck = true;
				break;
			}
		}
	}

	public void SetVariables (GameHandler.character_type type)
	{
		max_health = caller.testing_options.player_starting_health;
		current_damage = caller.testing_options.player_starting_damage;
		this_characters_type = type;
	}

	#endregion

	private void Start()
	{
		caller = GameObject.Find ("Game Handler").GetComponent<GameHandler>();
		health_text = transform.GetChild(0).GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
		damage_text = transform.GetChild(0).GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
		armor_text = transform.GetChild(0).GetChild(2).GetComponentInChildren<TextMeshProUGUI>();
		fighting_text  = transform.GetChild(1).GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
		current_health = max_health;
		health_text.text = current_health.ToString();
		damage_text.text = current_damage.ToString();
		if (armor == 0)
		{
			armor_text.GetComponentInParent<SpriteRenderer>().gameObject.SetActive (false);
		}
		else
		{
			armor_text.text = armor.ToString();
		}
		fighting_text.text = "";
	}

	void Update()
    {
		if (caller.GetFighting() == true && caller.GetPlayerTurn() == true && after_action == false && end_turn == false)
		{
			DealDamage (target_opponent);
		}
		if (after_action == true)
		{
			action_timer += Time.deltaTime;
			if (action_timer > caller.gameplay_options.ui.action_time)
			{
				ClearFightingText();
				if (target_opponent.GetCurrentHealth() > 0)
				{
					target_opponent.ClearFightingText();
				}
				else
				{
					switch (target_opponent.this_characters_type)
					{
						case GameHandler.character_type.player:
						caller.GameOver();
						break;

						case GameHandler.character_type.ai:

						break;

						case GameHandler.character_type.monster:
						Destroy (target_opponent.gameObject);
						caller.RemoveEnemy (0);
						break;
					}
				}
				after_action = false;
				end_turn = true;
			}
		}
		if (end_of_turn_effects == true)
		{
			end_of_turn_timer += Time.deltaTime;
			if (end_of_turn_timer > caller.gameplay_options.ui.end_of_turn_effect_time)
			{
				ClearFightingText();
				if (end_of_turn_effect_counter > 2)
				{
					end_of_turn_effects = false;
					caller.SetFinishedEndOfTurnEffects (true);
				}
				switch (end_of_turn_effect_counter)
				{
					case 1:
					if (poison_stacks > 0)
					{
						ReceiveDamage (poison_stacks);
						fighting_text.text += System.Environment.NewLine + "poison";
						foreach (GameHandler.EndOfTurnStackSubtraction stack in end_of_turn_stack_subtraction_list)
						{
							if (stack.end_of_turn_stack == GameHandler.end_of_turn_stack.poison)
							{
								stack.time_to_subtraction -= 1;
								if (stack.time_to_subtraction == 0)
								{
									poison_stacks -= stack.amount_subtracted;
									end_of_turn_stack_subtraction_list.Remove (stack);
								}
							}
						}
						end_of_turn_timer = 0;
					}
					break;

					case 2:
					if (regrowth_stacks > 0)
					{
						ReceiveHeal (regrowth_stacks);
						fighting_text.text += System.Environment.NewLine + "regrowth";
						end_of_turn_timer = 0;
					}
					break;
				}
				end_of_turn_effect_counter++;
			}
		}
    }

	private void DealDamage (Character_Core target)
	{
		float damage_modifier = 1;
		if (minor_luck == true)
		{
			int luck_chance = caller.RandomInt (1, 100);
			if (luck_chance <= caller.testing_options.minor_luck_chance)
			{
				damage_modifier *= caller.testing_options.minor_luck_damage_modifier;
			}
		}
		if (major_luck == true)
		{
			int luck_chance = caller.RandomInt (1, 100);
			if (luck_chance <= caller.testing_options.major_luck_chance)
			{
				damage_modifier *= caller.testing_options.major_luck_damage_modifier;
			}
		}
		fighting_text.text = "attacking";
		int amount_of_attacks;
		switch (minor_derangement, major_derangement)
		{
			case (false, false):
			target.ReceiveDamage ((int) (current_damage * damage_modifier));
			break;

			case (true, false):
			amount_of_attacks = caller.RandomInt (caller.testing_options.minor_derangement_attack_amount_bottom_range,
			caller.testing_options.minor_derangement_attack_amount_top_range);
			for (int i = 0; i < amount_of_attacks; i++)
			{
				target.ReceiveDamage ((int) (caller.RandomInt (caller.testing_options.minor_derangement_attack_damage_bottom_range,
				caller.testing_options.minor_derangement_attack_damage_top_range) * damage_modifier));
				fighting_text.text += System.Environment.NewLine + "deranged attack";
			}
			break;

			case (true, true) or (false, true):
			amount_of_attacks = caller.RandomInt (caller.testing_options.major_derangement_attack_amount_bottom_range,
			caller.testing_options.major_derangement_attack_amount_top_range);
			for (int i = 0; i < amount_of_attacks; i++)
			{
				int self_hit_chance = caller.RandomInt (1, 100);
				bool self_hit = false;
				if (self_hit_chance <= caller.testing_options.major_derangement_self_hit_chance)
				{
					self_hit = true;
				}
				if (self_hit == false)
				{
					target.ReceiveDamage ((int) (caller.RandomInt (caller.testing_options.major_derangement_attack_damage_bottom_range,
					caller.testing_options.major_derangement_attack_damage_top_range) * damage_modifier));
					fighting_text.text += System.Environment.NewLine + "deranged attack";
				}
				else
				{
					ReceiveDamage ((int) (caller.RandomInt (caller.testing_options.major_derangement_attack_damage_bottom_range,
					caller.testing_options.major_derangement_attack_damage_top_range) * damage_modifier));
					fighting_text.text += System.Environment.NewLine + "deranged attack self hit";
				}
			}
			break;
		}
		if (minor_agility == true && used_minor_agility == false)
		{
			used_minor_agility = true;
			int agility_chance = caller.RandomInt (1, 100);
			if (agility_chance <= caller.testing_options.minor_agility_chance)
			{
				DealDamage (target, caller.testing_options.minor_agility_damage_modifier);
			}
			fighting_text.text += System.Environment.NewLine + "agility";
		}
		if (major_agility == true && used_major_agility == false)
		{
			used_major_agility = true;
			int agility_chance = caller.RandomInt (1, 100);
			if (agility_chance <= caller.testing_options.major_agility_chance)
			{
				int number_of_attacks = caller.RandomInt (1, 2);
				DealDamage (target, caller.testing_options.major_agility_damage_modifier);
				if (number_of_attacks == 2)
				{
					DealDamage (target, caller.testing_options.major_agility_damage_modifier);
				}
			}
			fighting_text.text += System.Environment.NewLine + "agility";
		}
		if (minor_life_steal == true)
		{
			fighting_text.text += System.Environment.NewLine + "lifesteal";
		}
		if (major_life_steal == true)
		{
			fighting_text.text += System.Environment.NewLine + "lifesteal";
		}
		after_action = true;
		action_timer = 0;
	}

	private void DealDamage (Character_Core target, float damage_modifier)
	{
		if (minor_luck == true)
		{
			int luck_chance = caller.RandomInt (1, 100);
			if (luck_chance <= caller.testing_options.minor_luck_chance)
			{
				damage_modifier *= caller.testing_options.minor_luck_damage_modifier;
			}
		}
		if (major_luck == true)
		{
			int luck_chance = caller.RandomInt (1, 100);
			if (luck_chance <= caller.testing_options.major_luck_chance)
			{
				damage_modifier *= caller.testing_options.major_luck_damage_modifier;
			}
		}
		target.ReceiveDamage ((int) (current_damage * damage_modifier));
		fighting_text.text += "attacking";
		after_action = true;
		action_timer = 0;
	}

	public void ReceiveDamage (int damage)
	{
		if (minor_frenzy > 0)
		{
			
		}
		if (major_frenzy > 0)
		{
			
		}
		if (minor_resistance == true)
		{
			
		}
		if (major_resistance == true)
		{
			
		}
		if (armor >= damage)
		{
			armor -= damage;
			armor_text.text = armor.ToString ();
		}
		else
		{
			current_health  -= damage - armor;
			armor = 0;
			armor_text.text = armor.ToString ();
			health_text.text = current_health.ToString ();
		}
		fighting_text.text += "hit - " + damage.ToString();
	}

	public void ReceiveDamage (int damage, Character_Core attacker)
	{
		if (minor_frenzy > 0)
		{
			
		}
		if (major_frenzy > 0)
		{
			
		}
		if (minor_resistance == true)
		{
			
		}
		if (major_resistance == true)
		{
			
		}
		if (armor >= damage)
		{
			armor -= damage;
			armor_text.text = armor.ToString ();
		}
		else
		{
			current_health  -= damage - armor;
			armor = 0;
			armor_text.text = armor.ToString ();
			health_text.text = current_health.ToString ();
		}
		fighting_text.text += "hit - " + damage.ToString();
		if (attacker.GetEffectStacks().minor_hemorrhage > 0)
		{
			AddBleedStacks (attacker.GetEffectStacks().minor_hemorrhage);
			if (caller.testing_options.minor_hemorrhage_duration > 0)
			{
				AddEndOfTurnStack (GameHandler.end_of_turn_stack.bleed, attacker.GetEffectStacks().minor_hemorrhage, caller.testing_options.minor_hemorrhage_duration);
			}
			fighting_text.text += System.Environment.NewLine + "bleed";
		}
		if (attacker.GetEffectStacks().major_hemorrhage > 0)
		{
			AddBleedStacks (attacker.GetEffectStacks().major_hemorrhage);
			if (caller.testing_options.major_hemorrhage_duration > 0)
			{
				AddEndOfTurnStack (GameHandler.end_of_turn_stack.bleed, attacker.GetEffectStacks().major_hemorrhage, caller.testing_options.major_hemorrhage_duration);
			}
			fighting_text.text += System.Environment.NewLine + "bleed";
		}
		if (attacker.GetEffectStacks().poison > 0)
		{
			AddPoisonStacks (attacker.GetEffectStacks().poison);
			if (caller.testing_options.poison_duration > 0)
			{
				AddEndOfTurnStack (GameHandler.end_of_turn_stack.poison, attacker.GetEffectStacks().poison, caller.testing_options.poison_duration);
			}
			fighting_text.text += System.Environment.NewLine + "poison";
		}
		if (attacker.GetEffectStacks().venom > 0)
		{
			AddPoisonStacks (attacker.GetEffectStacks().venom);
			if (caller.testing_options.venom_duration > 0)
			{
				AddEndOfTurnStack (GameHandler.end_of_turn_stack.poison, attacker.GetEffectStacks().venom, caller.testing_options.venom_duration);
			}
			fighting_text.text += System.Environment.NewLine + "poison";
		}
		if (attacker.GetEffectStacks().regrowth > 0 && attacker.GetEffectStacks().regrowth_stacks == 0)
		{
			attacker.RegrowthStackAddition (attacker.GetEffectStacks().regrowth);
			fighting_text.text += System.Environment.NewLine + "regrowth";
		}
	}

	public void ReceiveHeal (int heal)
	{
		if (current_health + heal <= max_health)
		{
			current_health += heal;
			fighting_text.text = "healed - " + heal.ToString();
		}
		else
		{
			heal = max_health - current_health;
			current_health += heal;
			fighting_text.text = "healed - " + heal.ToString();
		}
	}

	public void AddEndOfTurnStack (GameHandler.end_of_turn_stack stack_type, int stack_strength, int duration)
	{
		end_of_turn_stack_subtraction_list.Add (new GameHandler.EndOfTurnStackSubtraction(stack_type, stack_strength, duration));
	}

	public void EndOfTurnEffects ()
	{
		end_of_turn_effects = true;
		if (bleed_stacks > 0)
		{
			ReceiveDamage (bleed_stacks);
			fighting_text.text += System.Environment.NewLine + "bleed";
			foreach (GameHandler.EndOfTurnStackSubtraction stack in end_of_turn_stack_subtraction_list)
			{
				if (stack.end_of_turn_stack == GameHandler.end_of_turn_stack.bleed)
				{
					stack.time_to_subtraction -= 1;
					if (stack.time_to_subtraction == 0)
					{
						bleed_stacks -= stack.amount_subtracted;
						end_of_turn_stack_subtraction_list.Remove (stack);
					}
				}
			}
			end_of_turn_effect_counter = 0;
		}
		end_of_turn_effect_counter++;
	}

	public void ClearFightingText ()
	{
		fighting_text.text = "";
	}

	public int GetCurrentHealth ()
	{
		return current_health;
	}

	public bool GetEndTurn ()
	{
		return end_turn;
	}

	public void SetEndTurn (bool state)
	{
		end_turn = state;
	}

	public void AddBleedStacks (int amount_added)
	{
		bleed_stacks += amount_added;
	}

	public void AddPoisonStacks (int amount_added)
	{
		poison_stacks += amount_added;
	}

	public (int minor_hemorrhage, int major_hemorrhage, int poison, int venom, int regrowth, int regrowth_stacks) GetEffectStacks ()
	{
		return (minor_hemorrhage, major_hemorrhage, poison, venom, regrowth, regrowth_stacks);
	}

	public void RegrowthStackAddition (int amount_added)
	{
		regrowth_stacks += amount_added;
	}
}

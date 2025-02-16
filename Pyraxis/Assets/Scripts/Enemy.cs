using TMPro;
using UnityEngine;
using System;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    #region variable declarations

	Player player;
	GameHandler caller;
	public int max_health;
	public int current_health;
	int current_damage;
	public int gold;
	public int exp;
	public bool end_turn = false;
	public bool end_of_turn_effects = false;
	private TextMeshProUGUI health_text;
	private TextMeshProUGUI damage_text;
	private TextMeshProUGUI armor_text;
	private TextMeshProUGUI fighting_text;
	private float action_timer = 0;
	private float end_of_turn_timer = 0;
	private bool after_action = false;
	public bool my_turn = false;
	public int end_of_turn_effect_counter = 0;

	#region creature effects

	public int armor = 0;
	public int minor_hemorrhage = 0;
	public int major_hemorrhage = 0;
	public int poison = 0;
	public int venom = 0;
	public int minor_frenzy = 0;
	public int major_frenzy = 0;
	public int regrowth = 0;

	public bool minor_derangement = false;
	public bool major_derangement = false;
	public bool minor_agility = false;
	public bool major_agility = false;
	public bool minor_resistance = false;
	public bool major_resistance = false;
	public bool minor_life_steal = false;
	public bool major_life_steal = false;
	public bool minor_luck = false;
	public bool major_luck = false;

	#endregion
	#region end of turn effects

	private List<GameHandler.EndOfTurnStackSubtraction> end_of_turn_stack_subtraction_list = new List<GameHandler.EndOfTurnStackSubtraction>();

	public int bleed_stacks = 0;
	public int poison_stacks = 0;
	public int minor_frenzy_stacks = 0;
	public int major_frenzy_stacks = 0;
	public int regrowth_stacks = 0;

	#endregion

	public void SetVariables (GameHandler.CardCreature creature)
	{
		max_health = creature.minion_health;
		current_damage = creature.minion_attack_dmg;
		gold = creature.bounty_gold;
		exp = creature.bounty_exp;
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

	#endregion

	private void Start()
	{
		current_health = max_health;
		caller = GameObject.Find ("Game Handler").GetComponent<GameHandler>();
		health_text = transform.GetChild(0).GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
		damage_text = transform.GetChild(0).GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
		armor_text = transform.GetChild(0).GetChild(2).GetComponentInChildren<TextMeshProUGUI>();
		fighting_text  = transform.GetChild(1).GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
		player = GameObject.Find ("Test Player").GetComponent<Player>();
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
		if (caller.fighting == true && my_turn == true && after_action == false && end_turn == false)
		{
			if (minor_derangement == true)
			{
				
			}
			if (major_derangement == true)
			{
				
			}
			DealDamage ();
			after_action = true;
			action_timer = 0;
			if (minor_hemorrhage > 0)
			{
				player.bleed_stacks += minor_hemorrhage;
				if (caller.testing_options.minor_hemorrhage_duration > 0)
				{
					player.AddEndOfTurnStack (GameHandler.end_of_turn_stack.bleed, minor_hemorrhage, caller.testing_options.minor_hemorrhage_duration);
				}
				fighting_text.text += System.Environment.NewLine + "bleed";
			}
			if (major_hemorrhage > 0)
			{
				player.bleed_stacks += major_hemorrhage;
				if (caller.testing_options.major_hemorrhage_duration > 0)
				{
					player.AddEndOfTurnStack (GameHandler.end_of_turn_stack.bleed, major_hemorrhage, caller.testing_options.major_hemorrhage_duration);
				}
				fighting_text.text += System.Environment.NewLine + "bleed";
			}
			if (poison > 0)
			{
				player.poison_stacks += poison;
				if (caller.testing_options.poison_duration > 0)
				{
					player.AddEndOfTurnStack (GameHandler.end_of_turn_stack.poison, poison, caller.testing_options.poison_duration);
				}
				fighting_text.text += System.Environment.NewLine + "poison";
			}
			if (venom > 0)
			{
				player.poison_stacks += venom;
				if (caller.testing_options.venom_duration > 0)
				{
					player.AddEndOfTurnStack (GameHandler.end_of_turn_stack.poison, venom, caller.testing_options.venom_duration);
				}
				fighting_text.text += System.Environment.NewLine + "poison";
			}
			if (regrowth > 0 && regrowth_stacks == 0)
			{
				regrowth_stacks = regrowth;
				fighting_text.text += System.Environment.NewLine + "regrowth";
			}
			if (minor_agility == true)
			{
				int agility_chance = caller.RandomInt (1, 100);
				if (agility_chance <= caller.testing_options.minor_agility_chance)
				{
					DealDamage (caller.testing_options.minor_agility_damage_modifier);
				}
				fighting_text.text += System.Environment.NewLine + "agility";
			}
			if (major_agility == true)
			{
				int agility_chance = caller.RandomInt (1, 100);
				if (agility_chance <= caller.testing_options.major_agility_chance)
				{
					DealDamage (caller.testing_options.major_agility_damage_modifier);
				}
				agility_chance = caller.RandomInt (1, 100);
				if (agility_chance <= caller.testing_options.major_agility_chance)
				{
					DealDamage (caller.testing_options.major_agility_damage_modifier);
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
		}
		if (after_action == true)
		{
			action_timer += Time.deltaTime;
			if (action_timer > caller.gameplay_options.ui.action_time)
			{
				ClearFightingText();
				if (player.current_health > 0)
				{
					player.ClearFightingText();
					after_action = false;
					end_turn = true;
				}
				else
				{
					caller.GameOver();
				}
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
					caller.finished_end_of_turn_effect = true;
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

	private void DealDamage ()
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
		player.ReceiveDamage ((int) (current_damage * damage_modifier));
		fighting_text.text = "attacking";
	}

	private void DealDamage (float damage_modifier)
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
		player.ReceiveDamage ((int) (current_damage * damage_modifier));
		fighting_text.text += "attacking";
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
			end_of_turn_timer = 0;
		}
		end_of_turn_effect_counter++;
	}

	public void ClearFightingText ()
	{
		fighting_text.text = "";
	}
}

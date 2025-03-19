using TMPro;
using UnityEngine;
using System;
using static UnityEngine.RuleTile.TilingRuleOutput;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    #region variable declarations

	GameHandler caller;
	private bool initialization = false;
	private bool in_new_room = false;
	private float next_room_x;
	private int room_counter = 0;
	private int current_health;
	private int current_damage;
	private int max_health;
	private bool after_action = false;
	private bool end_turn = false;
	private bool end_of_turn_effects = false;
	private TextMeshProUGUI health_text;
	private TextMeshProUGUI damage_text;
	private TextMeshProUGUI armor_text;
	private TextMeshProUGUI fighting_text;
	private float action_timer = 0;
	private float end_of_turn_timer = 0;
	private int end_of_turn_effect_counter = 0;

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

	#endregion

	private void Start()
	{
		caller = GameObject.Find ("Game Handler").GetComponent<GameHandler>();
		health_text = transform.GetChild(0).GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
		damage_text = transform.GetChild(0).GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
		armor_text = transform.GetChild(0).GetChild(2).GetComponentInChildren<TextMeshProUGUI>();
		fighting_text  = transform.GetChild(1).GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
		current_health = caller.testing_options.player_starting_health;
		max_health = caller.testing_options.player_starting_health;
		current_damage = caller.testing_options.player_starting_damage;
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
		if (initialization == false)
		{
			transform.position = new Vector3 (-6f, -1.25f, 0f);
			next_room_x = caller.GetRoomEntrancePosition (0);
			initialization = true;
			room_counter = 0;
		}
        if (caller.GetActionPhase() == true)
		{
			if (in_new_room == false && caller.GetFighting() == false)
			{
				transform.position += new Vector3 (Time.deltaTime, 0f, 0f);
				if (transform.position.x >= next_room_x + 1f)
				{
					transform.position = new Vector3 (next_room_x + 1f, transform.position.y, transform.position.z);
					in_new_room = true;
					caller.SetNewRoom (true);
					room_counter++;
					if (room_counter < caller.GetAmountOfRooms())
					{
						next_room_x = caller.GetRoomEntrancePosition (room_counter);
					}
					else
					{
						next_room_x = caller.GetFloorExitPosition();
						if (room_counter > caller.GetAmountOfRooms())
						{
							in_new_room = false;
							caller.SetNewRoom (false);
							caller.SetFinishedFloor (true);
							caller.SetActionPhase (false);
						}
					}
				}
			}
			if (caller.GetFighting() == true && caller.GetPlayerTurn() == true && after_action == false && end_turn == false)
			{
				DealDamage (caller.GetEnemy (0));
				after_action = true;
				action_timer = 0;
			}
			if (after_action == true)
			{
				action_timer += Time.deltaTime;
				if (action_timer > caller.gameplay_options.ui.action_time)
				{
					ClearFightingText();
					if (caller.GetEnemy (0).GetCurrentHealth() > 0)
					{
						caller.GetEnemy (0).ClearFightingText();
					}
					else
					{
						Destroy (caller.GetEnemy (0).gameObject);
						caller.RemoveEnemy (0);
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
    }

	private void DealDamage (Enemy enemy)
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
		enemy.ReceiveDamage ((int) (current_damage * damage_modifier));
		fighting_text.text = "attacking";
	}

	private void DealDamage (Enemy enemy, float damage_modifier)
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
		enemy.ReceiveDamage ((int) (current_damage * damage_modifier));
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

	public void SetInitialization (bool state)
	{
		initialization = state;
	}

	public void SetInNewRoom (bool state)
	{
		in_new_room = state;
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
}

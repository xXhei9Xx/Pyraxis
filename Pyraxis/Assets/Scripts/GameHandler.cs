using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.IO;
using CodeMonkey.Utils;
using static CardHandler;

public class GameHandler : MonoBehaviour
{
	#region variable declarations

	private (float x, float y, float z) board_center_position_tuple;
	private GameObject game_over_text, end_turn_button, objects_attached_to_map, card_slots_object, cards_in_card_slots;
	private Player player;
	private List<float> card_slots_positions;
	private List<float> room_entrance_positions;
	private List<GameObject> card_slots;
	private List<int> room_sizes;
	private List<Enemy> enemies_list = new List<Enemy>();
	private camera_directions camera_direction = camera_directions.up;
	private float floor_exit_position;
	private Vector3 last_camera_position;
	#region references

	new GameObject camera;
	private GameObject object_under_cursor = null;
	private CardHandler card_handler;
	private MapGenerator map_generator;

	#endregion
	#region bool

	private bool draw_phase = false;
	private bool planning_phase = false;
	private bool action_phase = false;
	private bool new_room = false;
	private bool fighting = false;
	private bool finished_floor = false;
	private bool player_turn = false;
	private bool picked_next_fighter = false;
	private bool end_of_turn_effects = false;
	private bool finished_end_of_turn_effect = false;

	#endregion
	#region int

	private int amount_of_rooms = 0;
	private int current_floor = 0;
	private int room_counter = 0;
	private int enemy_counter = 0;
	private int card_slot_counter = 0;
	private int end_of_turn_counter = 0;

	#endregion
	#region serialized variables
	[SerializeField] public TestingOptions testing_options;
	[SerializeField] public GameplayOptions gameplay_options;
	[SerializeField] public DeckOptions deck_options;

	#region testing options
	[Serializable] public class TestingOptions
	{
		[SerializeField] public bool display_grid_text = true;
		[SerializeField] public int player_starting_health;
		[SerializeField] public int player_starting_damage;
		[SerializeField] public float time_scale;
		[SerializeField] public int minor_hemorrhage_duration;
		[SerializeField] public int major_hemorrhage_duration;
		[SerializeField] public int poison_duration;
		[SerializeField] public int venom_duration;
		[SerializeField] public int minor_frenzy_duration;
		[SerializeField] public int minor_frenzy_max_stacks;
		[SerializeField] public float minor_frenzy_incoming_damage_modifier;
		[SerializeField] public int major_frenzy_duration;
		[SerializeField] public int major_frenzy_max_stacks;
		[SerializeField] public float major_frenzy_incoming_damage_modifier;
		[SerializeField] public int regrowth_duration;
		[SerializeField] public int minor_agility_chance;
		[SerializeField] public float minor_agility_damage_modifier;
		[SerializeField] public int major_agility_chance;
		[SerializeField] public float major_agility_damage_modifier;
		[SerializeField] public int minor_luck_chance;
		[SerializeField] public float minor_luck_damage_modifier;
		[SerializeField] public int major_luck_chance;
		[SerializeField] public float major_luck_damage_modifier;

	}
	#endregion
	#region gameplay options
	[Serializable] public class GameplayOptions
	{
		[SerializeField] public MapOptions map;
		[SerializeField] public UIOptions ui;
		[SerializeField] public Controls controls;
	}

	[Serializable] public class MapOptions
	{
		[SerializeField] public List<FloorOptions> floor_room_ranges;
		[SerializeField] [Range (1, 6)] public int max_card_slots_per_room;
		[SerializeField] public List<float> room_sizes;
		[SerializeField] [Range (1, 9)] public int min_amount_of_floors;
	}

	[Serializable] public class UIOptions
	{
		[SerializeField] [Range (0.1f, 2)] public float card_movement_time = 0.5f;
		[SerializeField] [Range (0.5f, 4)] public float camera_movement_speed;
		[SerializeField] [Range (4f, 6f)] public float space_between_cards;
		[SerializeField] public float action_time;
		[SerializeField] public float end_of_turn_effect_time;
	}

	[Serializable] public class Controls
	{
		public int MouseButtonTranslator (MouseButton button)
		{
			switch (button)
			{
				case MouseButton.Left: return 0;

				case MouseButton.Right: return 1;

				case MouseButton.Middle: return 2;

				default: return 0;
			}
		}

		[SerializeField] public MouseButton drag_card;
		[SerializeField] public MouseButton cancel;
		[SerializeField] public KeyCode move_left;
		[SerializeField] public KeyCode move_right;
		[SerializeField] public KeyCode move_up;
		[SerializeField] public KeyCode move_down;
		[SerializeField] public KeyCode rotate_left;
		[SerializeField] public KeyCode rotate_right;
	}

	[Serializable] public class FloorOptions
	{
		[SerializeField] public int min_amount_of_rooms;
		[SerializeField] public int max_amount_of_rooms;
		[SerializeField] public int min_amount_of_card_slots;
		[SerializeField] public int max_amount_of_card_slots;
	}

	#endregion
	#region deck options
	[Serializable] public class DeckOptions
	{
		[SerializeField] public List<CardAbility> ability_cards_in_game;
		[SerializeField] public List<CardCreature> creature_cards_in_game;
		[SerializeField] public List<CardWeapon> weapon_cards_in_game;
		[SerializeField] public List<CardConsumable> consumable_cards_in_game;
		[SerializeField] [Range (1, 5)] public int starting_card_amount_in_hand = 5;
		[SerializeField] [Range (5, 10)] public int max_hand_size = 8;
		[SerializeField] [Range (2, 5)]public int amount_of_new_cards_to_choose = 3;
		[SerializeField] public Sprite blue_card_frame, purple_card_frame, red_card_frame, orange_card_frame, green_card_frame;
	}

	[Serializable] public class CardAbility
	{
		[SerializeField] public string name;
		[SerializeField] public Sprite miniature;
		[SerializeField] public int duration;
		[SerializeField] public int exp_cost;
		[SerializeField] public List<AbilityEffect> effects;
	}

	[Serializable] public class CardCreature
	{
		[SerializeField] public string name;
		[SerializeField] public Sprite miniature;
		[SerializeField] public int bounty_gold;
		[SerializeField] public int bounty_exp;
		[SerializeField] public int minion_attack_dmg;
		[SerializeField] public int minion_health;
		[SerializeField] public List<CreatureEffect> effects;
		[SerializeField] public List<CreaturePassiveEffect> passive_effects;
	}

	[Serializable] public class CardWeapon
	{
		[SerializeField] public string name;
		[SerializeField] public Sprite miniature;
		[SerializeField] public int gold_cost;
		[SerializeField] public int durability;
		[SerializeField] public List<AbilityEffect> effects;
	}

	[Serializable] public class CardConsumable
	{
		[SerializeField] public string name;
		[SerializeField] public Sprite miniature;
		[SerializeField] public int gold_cost;
		[SerializeField] public List<AbilityEffect> effects;
	}

	[Serializable] public class AbilityEffect
	{
		[SerializeField] public player_character_type target_character;
		[SerializeField] public ability_effects effect;
		[SerializeField] public int effect_strength;
	}

	[Serializable] public class CreatureEffect
	{
		[SerializeField] public creature_effects effect;
		[SerializeField] public int effect_strength;
	}
	[Serializable] public class CreaturePassiveEffect
	{
		[SerializeField] public creature_passive_effects effect;
	}
	#endregion
	#region enums

	public enum card_type
	{
		creature,
		weapon,
		ability,
		consumable
	}

	private enum camera_directions
	{
		up,
		down,
		left,
		right
	}

	public enum ability_effects
	{
		deal_damage,
		heal,
		draw,
		discard,
	}

	public enum creature_effects
	{
		armor,
		minor_hemorrhage,
		major_hemorrhage,
		poison,
		venom,
		minor_frenzy,
		major_frenzy,
		regrowth
	}

	public enum creature_passive_effects
	{
		minor_derangement,
		major_derangement,
		minor_agility,
		major_agility,
		minor_resistance,
		major_resistance,
		minor_life_steal,
		major_life_steal,
		minor_luck,
		major_luck
	}

	public enum player_character_type
	{
		player,
		enemy,
		another_enemy
	}

	public enum enemy_character_type
	{
		player,
		this_enemy,
		another_enemy
	}

	public enum end_of_turn_stack
	{
		bleed,
		poison
	}

	#endregion
	#endregion

	#endregion

	void Start()
    {
		#region references

		card_handler = GameObject.Find("Card Handler").GetComponent<CardHandler>();
		camera = GameObject.Find("Main Camera");
		map_generator = GameObject.Find ("Map Generator").GetComponent<MapGenerator>();
		player = GameObject.Find ("Test Player").GetComponent<Player>();
		game_over_text = GameObject.Find ("Game Over Text");
		end_turn_button = GameObject.Find ("End Turn Button");
		objects_attached_to_map = GameObject.Find ("Objects Attached To Map");
		card_slots_object = GameObject.Find ("Card Slots");
		cards_in_card_slots = GameObject.Find ("Cards In Card Slots");

		#endregion
		#region buttons

		end_turn_button.GetComponent<Button_UI>().ClickFunc = () => {camera.transform.position = player.transform.position; camera.transform.SetParent (player.transform);
		planning_phase = false;action_phase = true; end_turn_button.SetActive (false);};
		end_turn_button.SetActive (false);

		#endregion
		#region generating first floor

		map_generator.GenerateMap (this, RandomInt (gameplay_options.map.floor_room_ranges[0].min_amount_of_rooms, gameplay_options.map.floor_room_ranges[0].max_amount_of_rooms),
		RandomInt (gameplay_options.map.floor_room_ranges[0].min_amount_of_card_slots, gameplay_options.map.floor_room_ranges[0].max_amount_of_card_slots), out card_slots_positions,
		out room_entrance_positions, out room_sizes, out floor_exit_position, out amount_of_rooms);

		#endregion
		end_turn_button.SetActive (true);
		Time.timeScale = testing_options.time_scale;
		game_over_text.SetActive (false);
		card_handler.CardSlotsConstructor (card_slots_positions, out card_slots);
		last_camera_position = camera.transform.position;
	}

	private void Update()
	{
		if (last_camera_position != camera.transform.position)
		{
			objects_attached_to_map.transform.position = RectTransformUtility.WorldToScreenPoint (Camera.main, (camera.transform.position - last_camera_position));
			last_camera_position = camera.transform.position;
		}
		if (action_phase == true)
		{
			if (new_room == true && finished_floor == false)
			{
				enemy_counter = 0;
				int room_card_slot_counter = 0;
				while (room_card_slot_counter < room_sizes [room_counter] )
				{
					if (card_slots [card_slot_counter].GetComponent<CardSlot>().card_object_in_slot != null)
					{
						switch (card_slots [card_slot_counter].GetComponent<CardSlot>().card_object_in_slot.GetComponent<Card>().card_type)
						{
							case card_type.creature:
							GameObject enemy = Instantiate (GameObject.Find ("Test Enemy"));
							enemy.AddComponent<Enemy>().SetVariables (deck_options.creature_cards_in_game 
							[card_slots [card_slot_counter].GetComponent<CardSlot>().card_object_in_slot.GetComponent<Card>().card_list_index]);
							enemies_list.Add (enemy.GetComponent<Enemy>());
							enemy.transform.position = new Vector3 (room_entrance_positions [room_counter] + 3f + (3f * enemy_counter), -1.25f, 0f);
							enemy_counter++;
							break;

							case card_type.ability:

							break;
						}
						card_handler.MoveCardToGraveyard (card_slots [card_slot_counter].GetComponent<CardSlot>().card_object_in_slot);
					}
					card_slots [card_slot_counter].SetActive (false);
					card_slot_counter++;
					room_card_slot_counter++;
				}
				room_counter++;
				new_room = false;
				player.SetInNewRoom (false);
				if (enemy_counter > 0)
				{
					fighting = true;
					player_turn = true;
				}
			}
			if (fighting == true)
			{
				if (player_turn == true && player.GetEndTurn() == true)
				{
					player_turn = false;
					if (enemies_list.Count > 0)
					{
						enemy_counter = 0;
						enemies_list [enemy_counter].SetMyTurn (true);
					}
					else
					{
						fighting = false;
						action_phase = true;
						player.SetEndTurn (false);
					}
				}
				if (player_turn == false && enemy_counter < enemies_list.Count && end_of_turn_effects == false)
				{
					if (enemies_list [enemy_counter].GetEndTurn() == true)
					{
						enemies_list [enemy_counter].SetEndTurn (false);
						enemies_list [enemy_counter].SetMyTurn (false);
						enemy_counter++;
						if (enemy_counter < enemies_list.Count)
						{
							enemies_list [enemy_counter].SetMyTurn (true);
						}
						else
						{
							end_of_turn_effects = true;
							finished_end_of_turn_effect = true;
						}
					}
				}
				if (player_turn == false && end_of_turn_effects == true && finished_end_of_turn_effect == true)
				{
					finished_end_of_turn_effect = false;
					if (end_of_turn_counter == 0)
					{
						player.EndOfTurnEffects ();
						end_of_turn_counter++;
					}
					else
					{
						if (end_of_turn_counter <= enemies_list.Count)
						{
							enemies_list [end_of_turn_counter - 1].EndOfTurnEffects();
							end_of_turn_counter++;
						}
						else
						{
							player.SetEndTurn (false);
							player_turn = true;
							end_of_turn_counter = 0;
							finished_end_of_turn_effect = false;
							end_of_turn_effects = false;
						}
					}
				}
			}
		}
		if (finished_floor == true)
		{
			room_counter = 0;
			current_floor++;
			ClearCardSlots ();
			map_generator.DestroyFloor();
			map_generator.GenerateMap (this, RandomInt (gameplay_options.map.floor_room_ranges[current_floor].min_amount_of_rooms, gameplay_options.map.floor_room_ranges[current_floor].max_amount_of_rooms),
			RandomInt (gameplay_options.map.floor_room_ranges[current_floor].min_amount_of_card_slots, gameplay_options.map.floor_room_ranges[current_floor].max_amount_of_card_slots), out card_slots_positions,
			out room_entrance_positions, out room_sizes, out floor_exit_position, out amount_of_rooms);
			player.SetInitialization (false);
			finished_floor = false;
			camera.transform.SetParent (transform);
			camera.transform.position = new Vector3 (0, 0, 0);
			card_handler.CardSlotsConstructor (card_slots_positions, out card_slots);
			draw_phase = true;
		}
	}

	#region references: 



	#endregion
	#region random int

	public int RandomInt (int lower_limit, int upper_limit)
	{
		return (int) UnityEngine.Random.Range ((float) lower_limit, (float) upper_limit + 1);
	}

	#endregion
	#region Timer

	public class Timer
	{
		public static GameObject Create (Action action, float time, string timer_name, GameObject parent)
		{
			GameObject timer;
			new Timer (action, time, timer_name, parent, out timer);
			return timer;
		}

		public static GameObject Create (Action action, float time, string timer_name)
		{
			GameObject timer;
			new Timer (action, time, timer_name, out timer);
			return timer;
		}

		public static GameObject Create (float time, string timer_name, GameObject parent)
		{
			GameObject timer;
			new Timer (time, timer_name, parent, out timer);
			return timer;
		}

		public static GameObject Create (float time, string timer_name)
		{
			GameObject timer;
			new Timer (time, timer_name, out timer);
			return timer;
		}

		private Timer (Action action, float time, string timer_name, GameObject parent, out GameObject timer)
		{
			timer = new GameObject ("timer - " + timer_name);
			timer.transform.SetParent (parent.transform);
			timer.AddComponent<TimerComponent>().SetVariables (time);
			timer.GetComponent<TimerComponent>().SetAction (action);
		}

		private Timer (Action action, float time, string timer_name, out GameObject timer)
		{
			timer = new GameObject ("timer - " + timer_name);
			timer.AddComponent<TimerComponent>().SetVariables (time);
			timer.GetComponent<TimerComponent>().SetAction (action);
		}

		private Timer (float time, string timer_name, GameObject parent, out GameObject timer)
		{
			timer = new GameObject ("timer - " + timer_name);
			timer.transform.SetParent (parent.transform);
			timer.AddComponent<TimerComponent>().SetVariables (time);
		}

		private Timer (float time, string timer_name, out GameObject timer)
		{
			timer = new GameObject ("timer - " + timer_name);
			timer.AddComponent<TimerComponent>().SetVariables (time);
		}

		public class TimerComponent : MonoBehaviour
		{
			#region

			private float time;
			private Action action;

			#endregion

			public void SetVariables (float time)
			{
				this.time = time;
			}

			public void SetAction (Action action)
			{
				this.action = action;
			}

			private void FixedUpdate()
			{
				time -= Time.deltaTime;
				if (time < 0)
				{
					action ();
					Destroy (gameObject);
				}
			}
		}
	}

	#endregion

	private void ClearCardSlots ()
	{
		foreach (CardSlot card_slot in card_slots_object.transform.GetComponentsInChildren<CardSlot>())
		{
			Destroy (card_slot.gameObject);
		}
	}

	public void GameOver ()
	{
		game_over_text.SetActive (true);
		game_over_text.transform.position = new Vector3 (player.transform.position.x, -2f, 0);
	}

	public class EndOfTurnStackSubtraction
	{
		public end_of_turn_stack end_of_turn_stack;
		public int amount_subtracted;
		public int time_to_subtraction;

		public EndOfTurnStackSubtraction (end_of_turn_stack stack_type, int amount_subtracted, int time_to_subtraction)
		{
			end_of_turn_stack = stack_type;
			this.amount_subtracted = amount_subtracted;
			this.time_to_subtraction = time_to_subtraction;
		}
	}

	public float GetRoomEntrancePosition (int room_index)
	{
		 return room_entrance_positions [room_index];
	}

	public float GetFloorExitPosition ()
	{
		return floor_exit_position;
	}

	public int GetAmountOfRooms ()
	{
		return amount_of_rooms;
	}

	public Enemy GetEnemy (int enemy_index)
	{
		return enemies_list [enemy_index];
	}

	public void RemoveEnemy (int enemy_index)
	{
		enemies_list.RemoveAt (enemy_index);
	}

	public bool GetFighting ()
	{
		return fighting;
	}

	public void SetFinishedEndOfTurnEffects (bool state)
	{
		finished_end_of_turn_effect = state;
	}

	public bool GetActionPhase ()
	{
		return action_phase;
	}

	public void SetActionPhase (bool state)
	{
		action_phase = state;
	}

	public void SetNewRoom (bool state)
	{
		new_room = state;
	}

	public void SetFinishedFloor (bool state)
	{
		finished_floor = state;
	}

	public bool GetPlayerTurn ()
	{
		return player_turn;
	}
}

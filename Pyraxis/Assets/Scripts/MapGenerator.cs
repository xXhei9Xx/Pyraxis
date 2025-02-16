using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;

public class MapGenerator: MonoBehaviour
{
    public void GenerateMap (GameHandler caller, int amount_of_rooms, int amount_of_card_slots, out List<float> card_slots_positions, out List<float> room_entrance_positions,
	out List<int> room_sizes, out float floor_exit_position)
	{
		card_slots_positions = new List<float>();
		room_entrance_positions = new List<float>();
		room_sizes = new List<int>();
		if (amount_of_rooms > amount_of_card_slots)
		{
			amount_of_card_slots = amount_of_rooms;
		}
		if ((float) amount_of_card_slots / amount_of_rooms > caller.gameplay_options.map.max_card_slots_per_room)
		{
			amount_of_card_slots = caller.gameplay_options.map.max_card_slots_per_room * amount_of_rooms;
		}
		Debug.Log (amount_of_rooms.ToString() + "," + amount_of_card_slots.ToString());
		int extra_card_slots = amount_of_card_slots - amount_of_rooms;
		int spawned_rooms = 0;
		int spawned_card_slots = 0;
		float current_x = -7.5f;
		//SpawnRoom (-2, current_x, out current_x);
		SpawnRoom (caller, 0, current_x, out current_x, card_slots_positions, out card_slots_positions);
		while (true)
		{
			room_entrance_positions.Add (current_x);
			if (extra_card_slots > 0)
			{
				if (amount_of_rooms - spawned_rooms == 1)
				{
					SpawnRoom (caller, amount_of_card_slots - spawned_card_slots, current_x, out current_x, card_slots_positions, out card_slots_positions);
					room_sizes.Add (amount_of_card_slots - spawned_card_slots);
				}
				else
				{
					int lower_range = (amount_of_card_slots - spawned_card_slots) / (amount_of_rooms - spawned_rooms);
					int upper_range = extra_card_slots + 1;
					if (upper_range > caller.gameplay_options.map.max_card_slots_per_room)
					{
						upper_range = caller.gameplay_options.map.max_card_slots_per_room;
					}
					if (lower_range > upper_range)
					{
						lower_range = upper_range;
					}
					switch (caller.RandomInt (lower_range, upper_range))
					{
						case 1:
						SpawnRoom (caller, 1, current_x, out current_x, card_slots_positions, out card_slots_positions);
						spawned_card_slots++;
						room_sizes.Add (1);
						break;

						case 2:
						SpawnRoom (caller, 2, current_x, out current_x, card_slots_positions, out card_slots_positions);
						extra_card_slots--;
						spawned_card_slots += 2;
						room_sizes.Add (2);
						break;

						case 3:
						SpawnRoom (caller, 3, current_x, out current_x, card_slots_positions, out card_slots_positions);
						extra_card_slots -= 2;
						spawned_card_slots += 3;
						room_sizes.Add (3);
						break;

						case 4:
						SpawnRoom (caller, 4, current_x, out current_x, card_slots_positions, out card_slots_positions);
						extra_card_slots -= 3;
						spawned_card_slots += 4;
						room_sizes.Add (4);
						break;

						case 5:
						SpawnRoom (caller, 5, current_x, out current_x, card_slots_positions, out card_slots_positions);
						extra_card_slots -= 4;
						spawned_card_slots += 5;
						room_sizes.Add (5);
						break;

						case 6:
						SpawnRoom (caller, 6, current_x, out current_x, card_slots_positions, out card_slots_positions);
						extra_card_slots -= 5;
						spawned_card_slots += 6;
						room_sizes.Add (6);
						break;
					}
				}
			}
			else
			{
				SpawnRoom (caller, 1, current_x, out current_x, card_slots_positions, out card_slots_positions);
				room_sizes.Add (1);
			}

			spawned_rooms++;
			SpawnRoom (caller, 0, current_x, out current_x, card_slots_positions, out card_slots_positions);
			
			if (spawned_rooms == amount_of_rooms)
			{
				floor_exit_position = current_x;
				//SpawnRoom (-1, current_x, out current_x);
				break;
			}
		}
	}

	private void SpawnRoom (GameHandler caller, int card_slots, float current_x, out float cur_x, List<float> card_slots_positions, out List<float> card_slots_positions_out)
	{
		card_slots_positions_out = card_slots_positions;
		GameObject room;
		switch (card_slots)
		{
			default:
			
			cur_x = current_x;
			break;

			//case -2:

			//break;

			//case -1:

			//break;

			case 0:
			room = Instantiate (GameObject.Find ("Corridor Template"));
			room.transform.position = new Vector3 (current_x + caller.gameplay_options.map.room_sizes[0]/2, -0.625f, 0);
			room.name = "Corridor";
			room.transform.SetParent (GameObject.Find ("Map").transform.GetChild(1).transform);
			cur_x = current_x + caller.gameplay_options.map.room_sizes[0];
			break;

			case 1:
			card_slots_positions_out.Add (current_x + (caller.gameplay_options.map.room_sizes[1] / 2));
			room = Instantiate (GameObject.Find ("One Slot Room Template"));
			room.transform.position = new Vector3 (current_x + caller.gameplay_options.map.room_sizes[1]/2, 0, 0);
			room.name = "One Slot Room";
			room.transform.SetParent (GameObject.Find ("Map").transform.GetChild(1).transform);
			cur_x = current_x + caller.gameplay_options.map.room_sizes[1];
			break;

			case 2:
			card_slots_positions_out.Add (current_x + (caller.gameplay_options.map.room_sizes[2] / 3));
			card_slots_positions_out.Add (current_x + (caller.gameplay_options.map.room_sizes[2] * 2 / 3));
			room = Instantiate (GameObject.Find ("Two Slot Room Template"));
			room.transform.position = new Vector3 (current_x + caller.gameplay_options.map.room_sizes[2]/2, 0, 0);
			room.name = "Two Slot Room";
			room.transform.SetParent(GameObject.Find("Map").transform.GetChild(1).transform);
			cur_x = current_x + caller.gameplay_options.map.room_sizes[2];
			break;

			case 3:
			card_slots_positions_out.Add (current_x + (caller.gameplay_options.map.room_sizes[3] / 4));
			card_slots_positions_out.Add (current_x + (caller.gameplay_options.map.room_sizes[3] / 2));
			card_slots_positions_out.Add (current_x + (caller.gameplay_options.map.room_sizes[3] * 3 / 4));
			room = Instantiate (GameObject.Find ("Three Slot Room Template"));
			room.transform.position = new Vector3 (current_x + caller.gameplay_options.map.room_sizes[3]/2, 0, 0);
			room.name = "Three Slot Room";
			room.transform.SetParent(GameObject.Find("Map").transform.GetChild(1).transform);
			cur_x = current_x + caller.gameplay_options.map.room_sizes[3];
			break;

			case 4:
			card_slots_positions_out.Add (current_x + (caller.gameplay_options.map.room_sizes[4] / 5));
			card_slots_positions_out.Add (current_x + (caller.gameplay_options.map.room_sizes[4] * 2 / 5));
			card_slots_positions_out.Add (current_x + (caller.gameplay_options.map.room_sizes[4] * 3 / 5));
			card_slots_positions_out.Add (current_x + (caller.gameplay_options.map.room_sizes[4] * 4 / 5));
			room = Instantiate (GameObject.Find ("Four Slot Room Template"));
			room.transform.position = new Vector3 (current_x + caller.gameplay_options.map.room_sizes[4]/2, 0, 0);
			room.name = "Four Slot Room";
			room.transform.SetParent(GameObject.Find("Map").transform.GetChild(1).transform);
			cur_x = current_x + caller.gameplay_options.map.room_sizes[4];
			break;

			case 5:
			card_slots_positions_out.Add (current_x + (caller.gameplay_options.map.room_sizes[5] / 6));
			card_slots_positions_out.Add (current_x + (caller.gameplay_options.map.room_sizes[5] * 2 / 6));
			card_slots_positions_out.Add (current_x + (caller.gameplay_options.map.room_sizes[5] * 3 / 6));
			card_slots_positions_out.Add (current_x + (caller.gameplay_options.map.room_sizes[5] * 4 / 6));
			card_slots_positions_out.Add (current_x + (caller.gameplay_options.map.room_sizes[5] * 5 / 6));
			room = Instantiate (GameObject.Find ("Five Slot Room Template"));
			room.transform.position = new Vector3 (current_x + caller.gameplay_options.map.room_sizes[5]/2, 0, 0);
			room.name = "Five Slot Room";
			room.transform.SetParent(GameObject.Find("Map").transform.GetChild(1).transform);
			cur_x = current_x + caller.gameplay_options.map.room_sizes[5];
			break;

			case 6:
			card_slots_positions_out.Add (current_x + (caller.gameplay_options.map.room_sizes[6] / 7));
			card_slots_positions_out.Add (current_x + (caller.gameplay_options.map.room_sizes[6] * 2 / 7));
			card_slots_positions_out.Add (current_x + (caller.gameplay_options.map.room_sizes[6] * 3 / 7));
			card_slots_positions_out.Add (current_x + (caller.gameplay_options.map.room_sizes[6] * 4 / 7));
			card_slots_positions_out.Add (current_x + (caller.gameplay_options.map.room_sizes[6] * 5 / 7));
			card_slots_positions_out.Add (current_x + (caller.gameplay_options.map.room_sizes[6] * 6 / 7));
			room = Instantiate (GameObject.Find ("Six Slot Room Template"));
			room.transform.position = new Vector3 (current_x + caller.gameplay_options.map.room_sizes[6]/2, 0, 0);
			room.name = "Six Slot Room";
			room.transform.SetParent(GameObject.Find("Map").transform.GetChild(1).transform);
			cur_x = current_x + caller.gameplay_options.map.room_sizes[6];
			break;
		}
	}

	public void DestroyFloor ()
	{
		SpriteRenderer [] renderer_array = GameObject.Find("Map").transform.GetChild(1).transform.GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer child in renderer_array)
		{
			Destroy (child);
		}
	}
}

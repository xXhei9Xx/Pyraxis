using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEditor;
using Unity.VisualScripting;
using TMPro;
using static CardHandler;
using System.Linq;


public class CardHandler : MonoBehaviour
{
	#region variable declarations
	
	private float timer = 0, extra_time_between_actions = 0.05f;
	private int target_amount_of_cards, max_hand_size, deck_size, cards_in_deck;
	public bool drawing = true, cant_draw = false, finished_discarding = false, after_picking = false, picking = false, choosing_upgrade = false;
	private int cards_in_hand = 0;
	private GameHandler caller;
	private GameObject currently_picked_card = null;
	private GameObject game_handler, main_camera;
	private GameObject deck, hand, graveyard;
	public List<GameObject> card_objects_list = new List<GameObject>();
	private Vector3 card_draw_starting_position = new Vector3 (-1200, 0, 0), card_discard_target_position = new Vector3 (2400, -800, 0), hand_target_position = new Vector3 (0, -1400, 0);
	private List <GameObject> card_objects_in_deck_list = new List<GameObject> ();
	public List <GameObject> card_objects_in_graveyard_list = new List<GameObject> ();
	public List <GameObject> card_objects_in_slots_list = new List<GameObject> ();

	#endregion


    void Start()
    {
		game_handler = GameObject.Find("Game Handler");
		main_camera = GameObject.Find("Main Camera");
		deck = GameObject.Find("Deck");
		hand = GameObject.Find("Hand");
		graveyard = GameObject.Find("Graveyard");
		caller = game_handler.GetComponent<GameHandler>();
		max_hand_size = caller.deck_options.max_hand_size;
		target_amount_of_cards = caller.deck_options.starting_card_amount_in_hand;
		CardConstructor (caller.deck_options.creature_cards_in_game [0]);
		CardConstructor (caller.deck_options.creature_cards_in_game [0]);
		CardConstructor (caller.deck_options.creature_cards_in_game [0]);
		CardConstructor (caller.deck_options.creature_cards_in_game [0]);
		CardConstructor (caller.deck_options.creature_cards_in_game [0]);
		CardConstructor (caller.deck_options.creature_cards_in_game [0]);
    }

	public void CardConstructor (GameHandler.CardCreature card_id)
	{
		GameObject current_card = Instantiate (GameObject.Find("Creature Card Template"));
		current_card.AddComponent<Card>().target_position = hand_target_position;
		current_card.GetComponent<Card>().hand_position = hand_target_position;
		current_card.GetComponent<Card>().card_handler = this;
		current_card.GetComponent<Card>().caller = caller;
		current_card.GetComponent<Card>().card_movement_time = caller.gameplay_options.ui.card_movement_time;
		current_card.GetComponent<Card>().starting_position = card_draw_starting_position;
		current_card.GetComponent<Card>().card_type = GameHandler.card_type.creature;
		current_card.GetComponent<Card>().card_list_index = caller.deck_options.creature_cards_in_game.IndexOf (card_id);
		card_objects_in_graveyard_list.Add(current_card);
		current_card.name = card_id.name;
		current_card.transform.SetParent (graveyard.transform);
		current_card.transform.localPosition = card_draw_starting_position;
		current_card.transform.GetChild (0).GetChild (0).GetComponent<TextMeshProUGUI>().text = card_id.name;
		current_card.transform.GetChild (0).GetChild (1).GetComponent<TextMeshProUGUI>().text = card_id.minion_attack_dmg.ToString();
		current_card.transform.GetChild (0).GetChild (2).GetComponent<TextMeshProUGUI>().text = card_id.minion_health.ToString();
		foreach (GameHandler.CreatureEffect effect in card_id.effects)
		{
			current_card.transform.GetChild (0).GetChild (3).GetComponent<TextMeshProUGUI>().text += System.Environment.NewLine + effect.effect.ToString() + " " + effect.effect_strength.ToString();
		}
		foreach (GameHandler.CreaturePassiveEffect effect in card_id.passive_effects)
		{
			current_card.transform.GetChild (0).GetChild (3).GetComponent<TextMeshProUGUI>().text += System.Environment.NewLine + effect.effect.ToString();
		}
		current_card.transform.GetChild (0).GetComponent<UnityEngine.UI.Image>().overrideSprite = card_id.miniature;
	}

    void FixedUpdate()
    {
		//draw phase of the turn
		if ((cards_in_hand >= target_amount_of_cards && drawing == true && timer > (caller.gameplay_options.ui.card_movement_time + extra_time_between_actions)) ||
		cant_draw == true && timer > (caller.gameplay_options.ui.card_movement_time + extra_time_between_actions))
		{
			drawing = false;
		}
		if (drawing == true && cant_draw == false && timer > (caller.gameplay_options.ui.card_movement_time + extra_time_between_actions))
		{
			cards_in_hand++;
			DrawCard();
			timer = 0;
		}
		timer += Time.fixedDeltaTime;
	}

	public void DrawCard ()
	{
		//checking if drawing is possible
		if (card_objects_in_deck_list.Count == 0 && card_objects_in_graveyard_list.Count == 0)
		{
			cant_draw = true;
		}
		else
		{
			// shuffling graveyard back into the deck
			if (card_objects_in_deck_list.Count == 0)
			{
				for (int i = 0; i < card_objects_in_graveyard_list.Count;)
				{
					card_objects_in_deck_list.Add (card_objects_in_graveyard_list [i]);
					card_objects_in_graveyard_list [i].transform.SetParent (deck.transform);
					card_objects_in_graveyard_list.Remove (card_objects_in_graveyard_list [i]);
				}
			}
			//picking a random card from the deck
			int card_index = UnityEngine.Random.Range (0, card_objects_in_deck_list.Count);
			card_objects_list.Add (card_objects_in_deck_list [card_index]);
			card_objects_in_deck_list [card_index].name = "card " + card_objects_list.Count;
			card_objects_in_deck_list [card_index].GetComponent<Card>().card_number = card_objects_list.Count;
			card_objects_in_deck_list [card_index].transform.localPosition = card_draw_starting_position;
			card_objects_in_deck_list [card_index].GetComponent<Card>().auto_moving = true;
			card_objects_in_deck_list [card_index].transform.SetParent (hand.transform);
			card_objects_in_deck_list.RemoveAt (card_index);
			RecenterCards ();
		}
	}

	public void DiscardCard (int position)
	{
		card_objects_list [position - 1].GetComponent<Card>().discarding = true;
		card_objects_list [position - 1].transform.SetParent (graveyard.transform);
		card_objects_in_graveyard_list.Add (card_objects_list [position - 1]);
		card_objects_list.Remove (card_objects_list [position - 1]);
	}

	public void MoveCardToGraveyard (GameObject card_object)
	{
		card_object.transform.position = card_discard_target_position;
		Vector3 current_rotation = card_object.transform.rotation.eulerAngles;
		card_object.transform.Rotate (-current_rotation.x, -current_rotation.y, -current_rotation.z);
		card_object.transform.SetParent (graveyard.transform);
		card_objects_in_graveyard_list.Add (card_object);
		if (card_objects_list.Contains (card_object))
		{
			card_objects_list.Remove (card_object);
		}
		if (card_objects_in_slots_list.Contains (card_object))
		{
			card_objects_in_slots_list.Remove (card_object);
		}
	}

	public void RecenterCards ()
	{
		Vector3 first_card_rotation = new Vector3 (0, 0, - (card_objects_list.Count() - 1) *
		caller.gameplay_options.ui.space_between_cards);
		foreach (GameObject card in card_objects_list)
		{
			card.GetComponent<Card>().target_rotation = first_card_rotation +
			new Vector3 (0, 0, (card_objects_list.IndexOf (card) * caller.gameplay_options.ui.space_between_cards * 2));
			if (card.GetComponent<Card>().target_rotation.z > card.transform.rotation.eulerAngles.z)
			{
				card.GetComponent<Card>().rotation_gap_z = card.GetComponent<Card>().target_rotation.z - card.transform.rotation.eulerAngles.z;
			}
			else
			{
				card.GetComponent<Card>().rotation_gap_z = card.GetComponent<Card>().target_rotation.z - card.transform.rotation.eulerAngles.z;
			}
			switch (card.GetComponent<Card>().rotation_gap_z)
			{
				case < - 180:
				card.GetComponent<Card>().rotation_gap_z += 360;
				break;

				case > 180:
				card.GetComponent<Card>().rotation_gap_z -= 360;
				break;
			}
			card.GetComponent<Card>().rotating = true;
		}
	}

	public void RenameCards ()
	{
		//readjusting naming scheme after using or removing a card
		for (int i = 1; i <= card_objects_list.Count; i++)
		{
			card_objects_list [i - 1].name = "card " + i;
			card_objects_list [i - 1].GetComponent<Card>().card_number = i;
		}
	}

	//public void AddingNewCardToDeck ()
	//{
	//	picking = true;
	//	List<GameObject> cards_to_pick = new List<GameObject> ();
	//	List<int> cards_to_pick_indexes = new List<int> ();
	//	var cards_to_pick_indexes_var = Enum.GetValues(typeof(card_id));
	//	foreach (int i in cards_to_pick_indexes_var)
	//	{
	//		cards_to_pick_indexes.Add (i);
	//	}
	//	for (int i = 0; i < caller.deck_options.amount_of_new_cards_to_choose; i++)
	//	{
	//		int number = UnityEngine.Random.Range (0, cards_to_pick_indexes.Count);
	//		int index = cards_to_pick_indexes [number];
	//		GameObject card = Instantiate (GameObject.Find("Card Template"));
	//		card.transform.SetParent (GameObject.Find ("Camera Screen").transform);
	//		card.AddComponent<CardToPick>().SetVariables ((card_id)index, caller, this, cards_to_pick);
	//		cards_to_pick_indexes.Remove (index);
	//		cards_to_pick.Add(card);
	//	}
	//	foreach (GameObject card in cards_to_pick)
	//	{
	//		card.transform.localPosition = new Vector3 ((-500 + (cards_to_pick.IndexOf(card) * 500)), -1000, 0);
	//	}
	//}

	//public void AddCardToDeck ()
	//{
	//	deck_size++;
	//	CardConstructor ();
	//}

	public void CardSlotsConstructor (List<float> card_slots_positions, out List<GameObject> card_slots)
	{
		card_slots = new List<GameObject>();
		GameObject card_slot_template = GameObject.Find ("Card Slot Template");
		for (int counter = 0; counter < card_slots_positions.Count; counter++)
		{
			GameObject card_slot = Instantiate (card_slot_template);
			card_slot.AddComponent<CardSlot>().card_slot_number = counter + 1;
			card_slot.GetComponent<CardSlot>().card_handler = this;
			card_slot.transform.position = new Vector3 (card_slots_positions [counter] - GameObject.Find("Main Camera").transform.position.x, 0, 0);
			card_slot.transform.SetParent (transform.GetChild (1).GetChild(0));
			card_slot.transform.position =  RectTransformUtility.WorldToScreenPoint(Camera.main, card_slot.transform.position);
			card_slots.Add (card_slot);
		}
	}

	public class CardSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		#region variable declarations

		public int card_slot_number;
		public bool mouse_on_card_slot = false;
		public bool card_in_slot = false;
		public CardHandler card_handler;
		public GameObject card_object_in_slot;

		#endregion

		private void Update()
		{
			if (mouse_on_card_slot == true && card_handler.currently_picked_card != null && card_in_slot == false)
			{
				card_handler.currently_picked_card.GetComponent<Card>().in_slot_position = transform.position;
				card_handler.currently_picked_card.GetComponent<Card>().card_slot_number = card_slot_number;
				card_object_in_slot = card_handler.currently_picked_card;
				card_in_slot = true;
			}
			if (mouse_on_card_slot == false && card_handler.currently_picked_card == card_object_in_slot && card_in_slot == true)
			{
				card_object_in_slot = null;
				card_handler.currently_picked_card.GetComponent<Card>().card_slot_number = 0;
				card_in_slot = false;
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			mouse_on_card_slot = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			mouse_on_card_slot = false;
		}
	}

	public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		#region variable delcarations

		public int card_number;
		public float timer_moving = 0, timer_rotating = 0, timer_discarding = 0;
		public GameHandler caller;
		public CardHandler card_handler;
		public bool auto_moving = false, rotating = false, mouse_tracking = false, mouse_on_card = false, discarding = false, shifting_card = false;
		public float card_movement_time;
		public Vector3 target_position, starting_position;
		public float rotation_gap_z;
		public float movement_speed_multiplier = 1;
		public float rotation_speed_multiplier = 1;
		public Vector3 target_rotation;
		public int card_slot_number = 0;
		public Vector3 in_slot_position;
		private Vector3 pivot_shift;
		private Vector3 image_shift;
		public Vector3 hand_position;
		public GameHandler.card_type card_type;
		public int card_list_index;

		#endregion

		private void Start()
		{
			pivot_shift = new Vector3 (0, (0.5f - GetComponent<RectTransform>().pivot.y) * GetComponent<RectTransform>().sizeDelta.y * GetComponent<RectTransform>().localScale.y, 0);
		}

		private void Update()
		{
			//moving from starting position to the card hand position
			if (auto_moving == true)
			{
				timer_moving += Time.deltaTime * movement_speed_multiplier / card_movement_time;
				transform.localPosition = starting_position + ((target_position - starting_position) * timer_moving);
				if (timer_moving >= 1)
				{
					auto_moving = false;
					transform.localPosition = target_position; 
					timer_moving = 0;
				}
			}
			//rotating into target position
			if (rotating == true)
			{
				timer_rotating += Time.deltaTime * rotation_speed_multiplier / card_movement_time;
				gameObject.transform.Rotate(0, 0, rotation_gap_z * Time.deltaTime * rotation_speed_multiplier / card_movement_time);
				if (timer_rotating >= 1)
				{
					rotating = false;
					transform.Rotate (0, 0, target_rotation.z - transform.rotation.eulerAngles.z);
					timer_rotating = 0;
				}
			}
			//moving from hand to discard position
			if (discarding == true)
			{
				timer_discarding += Time.deltaTime / card_movement_time;
				transform.localPosition = target_position + ((card_handler.card_discard_target_position - target_position) * timer_discarding);
				if (timer_discarding >= 1)
				{
					transform.localPosition = card_handler.card_discard_target_position;
					target_rotation = transform.rotation.eulerAngles;
					gameObject.transform.Rotate(0, 0, -target_rotation.z);
					discarding = false;
					timer_discarding = 0;
				}
			}
			//resetting the movement and rotation speed and shifting boolean
			if (rotating == false && auto_moving == false && shifting_card == true)
			{
				rotation_speed_multiplier = 1;
				shifting_card = false;
			}
			//initializing moving the card along with the mouse
			if (Input.GetMouseButtonDown(caller.gameplay_options.controls.MouseButtonTranslator(caller.gameplay_options.controls.drag_card)) &&
			mouse_on_card == true && mouse_tracking == false && card_handler.drawing == false && rotating == false)
			{
				target_rotation = transform.rotation.eulerAngles;
				target_position = transform.localPosition;
				gameObject.transform.Rotate(0, 0, -target_rotation.z);
				mouse_tracking = true;
				card_handler.currently_picked_card = gameObject;
				transform.GetComponentInChildren<UnityEngine.UI.Image>().raycastTarget = false;
			}
			if (mouse_tracking == true)
			{
				transform.position = Input.mousePosition - pivot_shift;
				//shifting card positions between each other
				if (transform.GetChild(0).gameObject.activeSelf == true)
				{
					var pointerEventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
					var raycastResults = new List<RaycastResult>();
					EventSystem.current.RaycastAll(pointerEventData, raycastResults);
					if (raycastResults.Count > 0)
					{
						foreach (var result in raycastResults)
						{
							if (result.gameObject.tag == "card_image")
							{
								RectTransformUtility.ScreenPointToLocalPointInRectangle(result.gameObject.GetComponentInParent<RectTransform>(), Input.mousePosition, null, out Vector2 localPoint);
								if (result.gameObject.GetComponentInParent<Card>().shifting_card == false &&
								((result.gameObject.GetComponentInParent<Card>().card_number + 1 == card_number && localPoint.x > 0) ||
								(result.gameObject.GetComponentInParent<Card>().card_number - 1 == card_number && localPoint.x < 0) ||
								(result.gameObject.GetComponentInParent<Card>().card_number - 1 > card_number) ||
								(result.gameObject.GetComponentInParent<Card>().card_number + 1 < card_number)))
								{
									Vector3 temp_target_rotation = result.gameObject.GetComponentInParent<RectTransform>().rotation.eulerAngles;
									//card to the right
									if (result.gameObject.GetComponentInParent<Card>().card_number > card_number)
									{
										for (int i = result.gameObject.GetComponentInParent<Card>().card_number; i > card_number; i--)
										{
											if (card_handler.card_objects_list[i - 1].GetComponent<Card>().shifting_card == false)
											{
												card_handler.card_objects_list[i - 1].GetComponent<Card>().rotation_gap_z = -2 * caller.gameplay_options.ui.space_between_cards;
												card_handler.card_objects_list[i - 1].GetComponent<Card>().rotation_speed_multiplier = 2;
												card_handler.card_objects_list[i - 1].GetComponent<Card>().rotating = true;
												card_handler.card_objects_list[i - 1].GetComponent<Card>().shifting_card = true;
												card_handler.card_objects_list[i - 1].GetComponent<Card>().target_rotation = card_handler.card_objects_list[i - 2].GetComponent<Card>().target_rotation;
											}
										}
									}
									//cards to the left
									else
									{
										for (int i = result.gameObject.GetComponentInParent<Card>().card_number; i < card_number; i++)
										{
											if (card_handler.card_objects_list[i - 1].GetComponent<Card>().shifting_card == false)
											{
												card_handler.card_objects_list[i - 1].GetComponent<Card>().rotation_gap_z = 2 * caller.gameplay_options.ui.space_between_cards;
												card_handler.card_objects_list[i - 1].GetComponent<Card>().rotation_speed_multiplier = 2;
												card_handler.card_objects_list[i - 1].GetComponent<Card>().rotating = true;
												card_handler.card_objects_list[i - 1].GetComponent<Card>().shifting_card = true;
												card_handler.card_objects_list[i - 1].GetComponent<Card>().target_rotation = card_handler.card_objects_list[i].GetComponent<Card>().target_rotation;
											}
										}
									}
									target_rotation = temp_target_rotation;
									card_handler.card_objects_list.RemoveAt(card_number - 1);
									card_handler.card_objects_list.Insert(result.gameObject.GetComponentInParent<Card>().card_number - 1, gameObject);
									card_handler.RenameCards();
									transform.SetSiblingIndex(card_number - 1);
								}
							}
						}
					}
				}
				//cancel card placement
				if (Input.GetMouseButton(caller.gameplay_options.controls.MouseButtonTranslator(caller.gameplay_options.controls.cancel)) ||
				   !Input.GetMouseButton(caller.gameplay_options.controls.MouseButtonTranslator(caller.gameplay_options.controls.drag_card)))
				{
					if (card_slot_number == 0)
					{
						if (!card_handler.card_objects_list.Contains (gameObject))
						{
							card_handler.card_objects_list.Add (gameObject);
							card_handler.card_objects_in_slots_list.Remove (gameObject);
							card_handler.RecenterCards ();
							card_handler.RenameCards();
							transform.SetParent (GameObject.Find ("Hand").transform);
							gameObject.transform.Rotate(0, 0, target_rotation.z - transform.rotation.eulerAngles.z);
							rotating = false;
						}
						else
						{
							gameObject.transform.Rotate(0, 0, target_rotation.z);
						}
						transform.localPosition = hand_position;
					}
					else
					{
						transform.position = in_slot_position - pivot_shift;
						if (!card_handler.card_objects_in_slots_list.Contains (gameObject))
						{
							transform.SetParent (GameObject.Find ("Cards In Card Slots").transform);
							card_handler.card_objects_in_slots_list.Add (gameObject);
							card_handler.card_objects_list.Remove (gameObject);
							card_handler.RecenterCards ();
							card_handler.RenameCards();
						}
					}
					mouse_tracking = false;
					card_handler.currently_picked_card = null;
					transform.GetComponentInChildren<UnityEngine.UI.Image>().raycastTarget = true;
				}
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			mouse_on_card = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			mouse_on_card = false;
		}
	}

	public class CardToPick : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		#region variable declarations

		private bool mouse_on_card = false;
		private GameHandler caller;
		private CardHandler card_handler;
		private List<GameObject> cards_to_pick;

		#endregion

		private void FixedUpdate()
		{
			if (mouse_on_card == true && Input.GetMouseButtonDown (caller.gameplay_options.controls.MouseButtonTranslator (caller.gameplay_options.controls.drag_card)))
			{
				//card_handler.AddCardToDeck ();
				card_handler.picking = false;
				card_handler.after_picking = true;
				foreach (GameObject card in cards_to_pick)
				{
					Destroy (card);
				}
			}
		}

		public void SetVariables (GameHandler caller, CardHandler card_handler, List<GameObject> cards_to_pick)
		{
			this.caller = caller;
			this.card_handler = card_handler;
			this.cards_to_pick = cards_to_pick;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			mouse_on_card = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			mouse_on_card = false;
		}
	}
}

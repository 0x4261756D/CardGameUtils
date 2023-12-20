using System;
using System.Text;

namespace CardGameUtils.Structs;

public class CardStruct
{
	public string name, text;
	public GameConstants.CardType card_type;
	public GameConstants.PlayerClass card_class;
	public GameConstants.Location location;
	public int uid, life, power, cost, base_life, base_power, base_cost, position, controller, base_controller;
	public bool is_class_ability;
	public bool can_be_class_ability;
	public CardStruct(string name,
		string text,
		GameConstants.CardType card_type,
		GameConstants.PlayerClass card_class,
		int uid, int life, int power, int cost,
		int base_life, int base_power, int base_cost,
		GameConstants.Location location, int position,
		bool is_class_ability,
		bool can_be_class_ability,
		int controller,
		int base_controller)
	{
		this.name = name;
		this.text = text;
		this.card_type = card_type;
		this.card_class = card_class;
		this.uid = uid;
		this.life = life;
		this.power = power;
		this.cost = cost;
		this.base_life = base_life;
		this.base_power = base_power;
		this.base_cost = base_cost;
		this.location = location;
		this.position = position;
		this.is_class_ability = is_class_ability;
		this.can_be_class_ability = can_be_class_ability;
		this.controller = controller;
		this.base_controller = base_controller;
	}
	public CardStruct()
	{
		name = "UNKNOWN";
		text = "UNKNOWN";
	}

	public override string ToString()
	{
		return Format(separator: '|');
	}

	public override bool Equals(object? other)
	{
		if(other == null) return false;
		return uid == ((CardStruct)other).uid;
	}

	public override int GetHashCode()
	{
		return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
	}

	public string Format(bool inDeckEdit = false, char separator = '\n')
	{
		StringBuilder builder = new();
		if(!inDeckEdit)
		{
			builder.Append($"UID: {uid}{separator}");
		}
		builder.Append($"name: {name}{separator}");
		if(card_type == GameConstants.CardType.Quest)
		{
			builder.Append($"{separator}quest progress: {position}/{cost}");
		}
		else if(location == GameConstants.Location.Ability)
		{
			builder.Append($"{separator}cost: 1");
		}
		else
		{
			builder.Append($"{separator}cost: {cost}");
			if(!inDeckEdit)
			{
				builder.Append($"/{base_cost}");
			}
		}
		if(!inDeckEdit)
		{
			builder.Append($"{separator}controller: {controller}/{base_controller}");
		}
		builder.Append($"{separator}card_type: {card_type}");
		builder.Append($"{separator}class: {card_class}");
		if(!inDeckEdit)
		{
			builder.Append($"{separator}location: {location}");
		}
		if(card_type == GameConstants.CardType.Creature)
		{
			builder.AppendJoin(separator, $"{separator}power: {power}{(inDeckEdit ? "" : "/" + base_power)}", $"life: {life}{(inDeckEdit ? "" : "/" + base_life)}");
			if(location == GameConstants.Location.Field)
			{
				builder.Append($"{separator}position: {position}");
			}
		}
		else if(card_type == GameConstants.CardType.Spell && inDeckEdit)
		{
			builder.Append($"{separator}can_be_class_ability: {can_be_class_ability}");
		}
		builder.Append($"{separator}----------{separator}{text}");
		return builder.ToString();
	}
}


public class URL(string address, int port)
{
	public string address = address;
	public int port = port;
}

public class PlatformCoreConfig
{
	public CoreConfig? windows, linux;
}

public class CoreConfig(int port, CoreConfig.CoreMode mode, CoreConfig.DuelConfig? duel_config = null, CoreConfig.DeckConfig? deck_config = null)
{
	public enum CoreMode
	{
		Duel,
		Client,
	}
	public class DuelConfig(PlayerConfig[] players, bool noshuffle)
	{
		public PlayerConfig[] players = players;
		public bool noshuffle = noshuffle;
	}
	public class PlayerConfig(string name, string[] decklist, string id)
	{
		public string name = name;
		public string[] decklist = decklist;
		public string id = id;
	}
	public class DeckConfig(URL additional_cards_url, string deck_location, bool should_fetch_additional_cards)
	{
		public string deck_location = deck_location;
		public bool should_fetch_additional_cards = should_fetch_additional_cards;
		public URL additional_cards_url = additional_cards_url;
	}

	public int port = port;
	public CoreMode mode = mode;
	public DuelConfig? duel_config = duel_config;
	public DeckConfig? deck_config = deck_config;
}

public class PlatformClientConfig
{
	public ClientConfig? windows, linux;
}

public class ClientConfig(
	URL deck_edit_url, int width, int height, CoreInfo core_info, bool should_save_player_name, bool should_spawn_core, string server_address, int animation_delay_in_ms, ClientConfig.ThemeVariant? theme, string? picture_path)
{
	public enum ThemeVariant
	{
		Default,
		Dark,
		Light,
	}

	public URL deck_edit_url = deck_edit_url;
	public int width = width, height = height;
	public bool should_spawn_core = should_spawn_core;
	public CoreInfo core_info = core_info;
	public string? player_name;
	public bool should_save_player_name = should_save_player_name;
	public string server_address = server_address;
	public string? last_deck_name;
	public int animation_delay_in_ms = animation_delay_in_ms;
	public ThemeVariant? theme = theme;
	public string? picture_path = picture_path;
}
public struct CoreInfo
{
	public string FileName;
	public string Arguments;
	public bool CreateNoWindow;
	public string Domain;
	public bool ErrorDialog;
	public bool UseShellExecute;
	public string WorkingDirectory;
}

public struct PlatformServerConfig
{
	public ServerConfig windows, linux;
}
public class ServerConfig(string additional_cards_path, int port, int room_min_port, int room_max_port, CoreInfo core_info)
{
	public CoreInfo core_info = core_info;
	public int port = port;
	public int room_min_port = room_min_port, room_max_port = room_max_port;
	public string additional_cards_path = additional_cards_path;
}

public class PlayerInfo(string name, string id, string[] decklist)
{
	public string name = name;
	public string id = id;
	public string[] decklist = decklist;
}

public class NetworkingStructs
{
	// NOTE: The packet class exists for reference only,
	// 		 in practive it is unnecessary to actually serialize a packet,
	// 		 accessing it's type and then parsing it's content is enough.
	public class Packet
	{
		public int length;
		public byte type;
		public PacketContent content = new();
	}
	public class PacketContent { }
	public class DuelPackets
	{
		public class SurrenderRequest : PacketContent
		{
		}

		public class GameResultResponse : PacketContent
		{
			public GameConstants.GameResult result;
		}

		public class GetOptionsRequest : PacketContent
		{
			public GameConstants.Location location;
			public int uid;
		}
		public class GetOptionsResponse : PacketContent
		{
			public GameConstants.Location location;
			public int uid;
			// TODO: this might not be the way to do this
			public string[] options = [];
		}

		public class SelectOptionRequest : PacketContent
		{
			public string desc = "UNINITIALIZED";
			public GameConstants.Location location;
			public int uid;
		}

		public class YesNoRequest : PacketContent
		{
			public string question = "UNINITIALIZED";
		}
		public class YesNoResponse : PacketContent
		{
			public bool result;
		}

		public class SelectCardsRequest : PacketContent
		{
			public CardStruct[] cards = [];
			public int amount;
			public string? desc;
		}
		public class SelectCardsResponse : PacketContent
		{
			public int[] uids = [];
		}

		public class CustomSelectCardsRequest : PacketContent
		{
			public CardStruct[] cards = [];
			public string? desc;
			public bool initialState;
		}
		public class CustomSelectCardsResponse : PacketContent
		{
			public int[] uids = [];
		}

		public class CustomSelectCardsIntermediateRequest : PacketContent
		{
			public int[] uids = [];
		}
		public class CustomSelectCardsIntermediateResponse : PacketContent
		{
			public bool isValid;
		}

		public class FieldUpdateRequest : PacketContent
		{
			public struct Field
			{
				public int life, deckSize, graveSize, momentum;
				public CardStruct[] hand;
				public CardStruct?[] field;
				// TODO: Don't always send these
				public string name;
				public CardStruct ability, quest;
				public ShownInfo shownInfo;
				public class ShownInfo
				{
					public CardStruct? card;
					public string? description;
				}
			}


			public Field ownField, oppField;
			public int turn;
			public bool hasInitiative, battleDirectionLeftToRight;
			public int? markedZone;
		}

		public class SelectZoneRequest : PacketContent
		{
			public bool[] options = [];
		}
		public class SelectZoneResponse : PacketContent
		{
			public int zone;
		}

		internal class PassRequest : PacketContent
		{
		}

		internal class ViewGraveRequest : PacketContent
		{
			public bool opponent = false;
		}
		internal class ViewCardsResponse : PacketContent
		{
			public string? message = null;
			public CardStruct[] cards = [];
		}
	}

	public class DeckPackets
	{
		public struct Deck
		{
			public string name;
			public CardStruct[] cards;
			public GameConstants.PlayerClass player_class;
			public CardStruct? ability, quest;
			public override readonly string? ToString()
			{
				if(name == null) return null;
				StringBuilder builder = new();
				builder.Append(player_class);
				if(ability != null)
				{
					builder.Append("\n#");
					builder.Append(ability.name);
				}
				if(quest != null)
				{
					builder.Append("\n|");
					builder.Append(quest.name);
				}
				foreach(var card in cards)
				{
					builder.Append('\n');
					builder.Append(card.name);
				}
				return builder.ToString();
			}
		}

		public class NamesRequest : PacketContent
		{
		}

		public class NamesResponse : PacketContent
		{
			public string[] names = [];
		}

		public class ListRequest : PacketContent
		{
			public string name = "UNINITIALIZED";
		}
		public class ListResponse : PacketContent
		{
			public Deck deck;
		}

		public class SearchRequest : PacketContent
		{
			public string? filter;
			public GameConstants.PlayerClass playerClass;
			public bool includeGenericCards;
		}
		public class SearchResponse : PacketContent
		{
			public CardStruct[] cards = [];
		}

		public class ListUpdateRequest : PacketContent
		{
			public Deck deck;
		}
		public class ListUpdateResponse : PacketContent
		{
			public bool should_update;
		}
	}

	public class ServerPackets
	{
		public class AdditionalCardsRequest : PacketContent
		{
		}
		public class AdditionalCardsResponse : PacketContent
		{
			public DateTime time;
			public CardStruct[] cards = [];
		}

		public class CreateRequest : PacketContent
		{
			public string? name;
		}
		public class CreateResponse : PacketContent
		{
			public bool success;
			public string? reason;
		}

		public class JoinRequest : PacketContent
		{
			public string? name, targetName;
		}
		public class JoinResponse : PacketContent
		{
			public bool success;
			public string? reason;
		}

		public class LeaveRequest : PacketContent
		{
			public string? name;
		}
		public class LeaveResponse : PacketContent
		{
			public bool success;
			public string? reason;
		}

		public class RoomsRequest : PacketContent
		{ }
		public class RoomsResponse : PacketContent
		{
			public string[] rooms = [];
		}

		public class StartRequest : PacketContent
		{
			public string? name;
			public string[] decklist = [];
			public bool noshuffle;
		}
		public class StartResponse : PacketContent
		{
			public enum Result
			{
				Failure,
				Success,
				SuccessButWaiting,
			}
			public Result success;
			public string? id;
			public int port;
			public string? reason;
		}
	}
}

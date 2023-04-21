using System.Text;

namespace CardGameUtils.Structs;

public class CardStruct
{
	public string name, text;
	public GameConstants.CardType card_type;
	public GameConstants.PlayerClass card_class;
	public GameConstants.Location location;
	public int uid, life, power, cost, position, controller;
	public bool is_class_ability;
	public bool can_be_class_ability;
	public CardStruct(string name,
		string text,
		GameConstants.CardType card_type,
		GameConstants.PlayerClass card_class,
		int uid, int life, int power, int cost,
		GameConstants.Location location, int position,
		bool is_class_ability,
		bool can_be_class_ability,
		int controller)
	{
		this.name = name;
		this.text = text;
		this.card_type = card_type;
		this.card_class = card_class;
		this.uid = uid;
		this.life = life;
		this.power = power;
		this.cost = cost;
		this.location = location;
		this.position = position;
		this.is_class_ability = is_class_ability;
		this.can_be_class_ability = can_be_class_ability;
		this.controller = controller;
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

	public string Format(bool inDeckEdit = false, char separator = '\n')
	{
		StringBuilder builder = new StringBuilder();
		if(!inDeckEdit)
		{
			builder.Append($"UID: {uid}{separator}");
		}
		builder.Append($"name: {name}{separator}");
		if(card_type != GameConstants.CardType.Quest)
		{
			builder.Append($"{separator}cost: {cost}");
		}
		else
		{
			builder.Append($"{separator}quest progress: {position}/{cost}");
		}
		builder.Append($"{separator}card_type: {card_type}");
		builder.Append($"{separator}class: {card_class}");
		if(!inDeckEdit)
		{
			builder.Append($"{separator}location: {location}");
		}
		if(card_type == GameConstants.CardType.Creature)
		{
			builder.AppendJoin(separator, $"{separator}power: {power}", $"life: {life}");
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


public class URL
{
	public string address;
	public int port;

	public URL(string address, int port)
	{
		this.address = address;
		this.port = port;
	}
}

public class PlatformCoreConfig
{
	public CoreConfig? windows, linux;
}

public class CoreConfig
{
	public enum CoreMode
	{
		Duel,
		Client,
	}
	public class DuelConfig
	{
		public PlayerConfig[] players;
		public bool noshuffle;

		public DuelConfig(PlayerConfig[] players, bool noshuffle)
		{
			this.players = players;
			this.noshuffle = noshuffle;
		}
	}
	public class PlayerConfig
	{
		public string name;
		public string[] decklist;
		public string id;

		public PlayerConfig(string name, string[] decklist, string id)
		{
			this.name = name;
			this.decklist = decklist;
			this.id = id;
		}
	}
	public class DeckConfig
	{
		public string deck_location;
		public bool should_fetch_additional_cards;
		public URL additional_cards_url;

		public DeckConfig(URL additional_cards_url, string deck_location, bool should_fetch_additional_cards)
		{
			this.additional_cards_url = additional_cards_url;
			this.deck_location = deck_location;
			this.should_fetch_additional_cards = should_fetch_additional_cards;
		}
	}

	public int port;
	public CoreMode mode;
	public DuelConfig? duel_config;
	public DeckConfig? deck_config;

	public CoreConfig(int port, CoreMode mode, DuelConfig? duel_config = null, DeckConfig? deck_config = null)
	{
		this.port = port;
		this.mode = mode;
		this.duel_config = duel_config;
		this.deck_config = deck_config;
	}
}

public class PlatformClientConfig
{
	public ClientConfig? windows, linux;
}

public class ClientConfig
{
	public URL deck_edit_url;
	public int width, height;
	public bool should_spawn_core;
	public CoreInfo core_info;
	public string? player_name;
	public bool should_save_player_name;
	public string server_address;
	public string? last_deck_name;

	public ClientConfig(
		URL deck_edit_url, int width, int height, CoreInfo core_info, bool should_save_player_name, bool should_spawn_core, string server_address)
	{
		this.deck_edit_url = deck_edit_url;
		this.width = width;
		this.height = height;
		this.core_info = core_info;
		this.should_save_player_name = should_save_player_name;
		this.should_spawn_core = should_spawn_core;
		this.server_address = server_address;
	}
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
public class ServerConfig
{
	public CoreInfo core_info;
	public int port;
	public int room_min_port, room_max_port;
	public string additional_cards_path;

	public ServerConfig(string additional_cards_path, int port, int room_min_port, int room_max_port, CoreInfo core_info)
	{
		this.additional_cards_path = additional_cards_path;
		this.port = port;
		this.room_max_port = room_max_port;
		this.room_min_port = room_min_port;
		this.core_info = core_info;
	}
}

public class NetworkingStructs
{
	// NOTE: The packet class exists for reference only, 
	// 		 in practive it is unnecessary to actually serialize a packet, 
	// 		 accessing it's type and then parsing it's content is enough.
	public class Packet
	{
		public byte type;
		public PacketContent content = new PacketContent();
		public static byte[] ENDING = Encoding.ASCII.GetBytes("|END|");
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
			public string[] options = new string[0];
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
			public CardStruct[] cards = new CardStruct[0];
			public int amount;
			public string? desc;
		}
		public class SelectCardsResponse : PacketContent
		{
			public int[] uids = new int[0];
		}

		public class CustomSelectCardsRequest : PacketContent
		{
			public CardStruct[] cards = new CardStruct[0];
			public string? desc;
			public bool initialState;
		}
		public class CustomSelectCardsResponse : PacketContent
		{
			public int[] uids = new int[0];
		}

		public class CustomSelectCardsIntermediateRequest : PacketContent
		{
			public int[] uids = new int[0];
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
				public CardStruct? shownCard;
			}

			public Field ownField, oppField;
			public int turn;
			public bool hasInitiative, battleDirectionLeftToRight;
			public int? markedZone;
		}

		public class SelectZoneRequest : PacketContent
		{
			public bool[] options = new bool[0];
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
			public CardStruct[] cards = new CardStruct[0];
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
			public override string? ToString()
			{
				if(name == null) return null;
				StringBuilder builder = new StringBuilder();
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
					builder.Append("\n");
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
			public string[] names = new string[0];
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
		}
		public class SearchResponse : PacketContent
		{
			public CardStruct[] cards = new CardStruct[0];
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
			public CardStruct[] cards = new CardStruct[0];
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
			public string[] rooms = new string[0];
		}

		public class StartRequest : PacketContent
		{
			public string? name;
			public string[] decklist = new string[0];
			public bool noshuffle;
		}
		public class StartResponse : PacketContent
		{
			public bool success;
			public string? id;
			public int port;
			public string? reason;
		}
	}
}
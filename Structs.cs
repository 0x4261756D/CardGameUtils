namespace CardGameUtils.Structs;

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

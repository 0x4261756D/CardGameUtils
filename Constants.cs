using System.Text.Json;

namespace CardGameUtils;

public class NetworkingConstants
{
	private static int counter = -1;
	public static int iota()
	{
		if (counter == int.MaxValue)
		{
			throw new Exception("iota overflew");
		}
		int tmp = counter;
		counter = (counter == -1) ? 1 : (counter + 1);
		return tmp;
	}

	public static byte iotaByte()
	{
		if (counter == byte.MaxValue)
		{
			throw new Exception("iota_byte overflow");
		}
		return (byte)iota();
	}

	public static byte iotaByteReset()
	{
		return (byte)iotaReset();
	}
	public static int iotaReset()
	{
		int tmp = counter;
		counter = -1;
		return tmp;
	}
	public static readonly byte PACKET_UNKNOWN = iotaByte(),
								PACKET_DECK_NAMES_REQUEST = iotaByte(),
								PACKET_DECK_NAMES_RESPONSE = iotaByte(),
								PACKET_DECK_LIST_REQUEST = iotaByte(),
								PACKET_DECK_LIST_RESPONSE = iotaByte(),
								PACKET_DECK_SEARCH_REQUEST = iotaByte(),
								PACKET_DECK_SEARCH_RESPONSE = iotaByte(),
								PACKET_DECK_LIST_UPDATE_REQUEST = iotaByte(),
								PACKET_DECK_LIST_UPDATE_RESPONSE = iotaByte(),
								PACKET_SERVER_ADDITIONAL_CARDS_REQUEST = iotaByte(),
								PACKET_SERVER_ADDITIONAL_CARDS_RESPONSE = iotaByte(),
								PACKET_COUNT = iotaByteReset();
	public static readonly JsonSerializerOptions jsonIncludeOption = new JsonSerializerOptions { IncludeFields = true };
	public static readonly Dictionary<Type, byte> PacketDict = new Dictionary<Type, byte>
	{
		{typeof(Structs.NetworkingStructs.ServerPackets.AdditionalCardsRequest), PACKET_SERVER_ADDITIONAL_CARDS_REQUEST},
		{typeof(Structs.NetworkingStructs.ServerPackets.AdditionalCardsResponse), PACKET_SERVER_ADDITIONAL_CARDS_RESPONSE},
		{typeof(Structs.NetworkingStructs.DeckPackets.NamesRequest), PACKET_DECK_NAMES_REQUEST},
		{typeof(Structs.NetworkingStructs.DeckPackets.NamesResponse), PACKET_DECK_NAMES_RESPONSE},
	};
	internal static object PacketTypeToName(byte type)
	{
		return PacketDict.First(x => x.Value == type).Key;
	}
}

public class GameConstants
{
	public enum CardType
	{
		UNKNOWN,
		Creature,
		Spell,
	}

	public enum PlayerClass
	{
		UNKNOWN,
		All,
		Cultist,
		Pyromancer,
		Artificer,
	}

	[Flags]
	public enum Location
	{
		UNKNOWN,
		Deck = 1 << 0,
		Hand = 1 << 1,
		Field = 1 << 2,
		Grave = 1 << 3,
		ALL = Deck | Hand | Field | Grave,
	}
}
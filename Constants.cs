using System;
using System.Collections.Generic;
using System.Linq;
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

								PACKET_DUEL_SURRENDER_REQUEST = iotaByte(),

								PACKET_SERVER_ADDITIONAL_CARDS_REQUEST = iotaByte(),
								PACKET_SERVER_ADDITIONAL_CARDS_RESPONSE = iotaByte(),
								PACKET_SERVER_CREATE_REQUEST = iotaByte(),
								PACKET_SERVER_CREATE_RESPONSE = iotaByte(),
								PACKET_SERVER_JOIN_REQUEST = iotaByte(),
								PACKET_SERVER_JOIN_RESPONSE = iotaByte(),
								PACKET_SERVER_LEAVE_REQUEST = iotaByte(),
								PACKET_SERVER_LEAVE_RESPONSE = iotaByte(),
								PACKET_SERVER_ROOMS_REQUEST = iotaByte(),
								PACKET_SERVER_ROOMS_RESPONSE = iotaByte(),
								PACKET_SERVER_START_REQUEST = iotaByte(),
								PACKET_SERVER_START_RESPONSE = iotaByte(),
								PACKET_COUNT = iotaByteReset();
	public static readonly JsonSerializerOptions jsonIncludeOption = new JsonSerializerOptions { IncludeFields = true };
	public static readonly Dictionary<Type, byte> PacketDict = new Dictionary<Type, byte>
	{
		{typeof(Structs.NetworkingStructs.DeckPackets.NamesRequest), PACKET_DECK_NAMES_REQUEST},
		{typeof(Structs.NetworkingStructs.DeckPackets.NamesResponse), PACKET_DECK_NAMES_RESPONSE},
		{typeof(Structs.NetworkingStructs.DeckPackets.ListRequest), PACKET_DECK_LIST_REQUEST},
		{typeof(Structs.NetworkingStructs.DeckPackets.ListResponse), PACKET_DECK_LIST_RESPONSE},
		{typeof(Structs.NetworkingStructs.DeckPackets.SearchRequest), PACKET_DECK_SEARCH_REQUEST},
		{typeof(Structs.NetworkingStructs.DeckPackets.SearchResponse), PACKET_DECK_SEARCH_RESPONSE},
		{typeof(Structs.NetworkingStructs.DeckPackets.ListUpdateRequest), PACKET_DECK_LIST_UPDATE_REQUEST},
		{typeof(Structs.NetworkingStructs.DeckPackets.ListUpdateResponse), PACKET_DECK_LIST_UPDATE_RESPONSE},

		{typeof(Structs.NetworkingStructs.DuelPackets.SurrenderRequest), PACKET_DUEL_SURRENDER_REQUEST},

		{typeof(Structs.NetworkingStructs.ServerPackets.AdditionalCardsRequest), PACKET_SERVER_ADDITIONAL_CARDS_REQUEST},
		{typeof(Structs.NetworkingStructs.ServerPackets.AdditionalCardsResponse), PACKET_SERVER_ADDITIONAL_CARDS_RESPONSE},
		{typeof(Structs.NetworkingStructs.ServerPackets.CreateRequest), PACKET_SERVER_CREATE_REQUEST},
		{typeof(Structs.NetworkingStructs.ServerPackets.CreateResponse), PACKET_SERVER_CREATE_RESPONSE},
		{typeof(Structs.NetworkingStructs.ServerPackets.JoinRequest), PACKET_SERVER_JOIN_REQUEST},
		{typeof(Structs.NetworkingStructs.ServerPackets.JoinResponse), PACKET_SERVER_JOIN_RESPONSE},
		{typeof(Structs.NetworkingStructs.ServerPackets.LeaveRequest), PACKET_SERVER_LEAVE_REQUEST},
		{typeof(Structs.NetworkingStructs.ServerPackets.LeaveResponse), PACKET_SERVER_LEAVE_RESPONSE},
		{typeof(Structs.NetworkingStructs.ServerPackets.RoomsRequest), PACKET_SERVER_ROOMS_REQUEST},
		{typeof(Structs.NetworkingStructs.ServerPackets.RoomsResponse), PACKET_SERVER_ROOMS_RESPONSE},
		{typeof(Structs.NetworkingStructs.ServerPackets.StartRequest), PACKET_SERVER_START_REQUEST},
		{typeof(Structs.NetworkingStructs.ServerPackets.StartResponse), PACKET_SERVER_START_RESPONSE},
	};
	internal static object PacketTypeToName(byte type)
	{
		return PacketDict.First(x => x.Value == type).Key;
	}
}

public class GameConstants
{
	public const int MAX_CARD_MULTIPLICITY = 2;
	public const int DECK_SIZE = 40;
	public const int START_HAND_SIZE = 5;
	public const int FIELD_SIZE = 6;
	public const int START_LIFE = 40;

	public enum State
	{
		UNINITIALIZED,
		TurnStart,
		TurnInitGained,
		BattleStart,
		BattleZoneMarked,
		BattleInitGained,
		DamageCalc,
		TurnEnd
	}
	public enum CardType
	{
		UNKNOWN,
		Creature,
		Spell,
		Quest,
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
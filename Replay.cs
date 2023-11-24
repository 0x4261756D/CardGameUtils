using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace CardGameUtils;

class Replay(string[] cmdlineArgs, int seed)
{
	[method: JsonConstructor]
	public class GameAction(int player, byte packetType, string packetContent, bool clientToServer)
	{
		public int player = player;
		public byte packetType = packetType;
		public string packetContent = packetContent;
		public byte[] packetContentBytes()
		{
			return Convert.FromBase64String(packetContent);
		}
		public byte[] fullPacketBytes()
		{
			List<byte> packet = packetContentBytes().ToList();
			packet.Insert(0, packetType);
			packet.InsertRange(0, BitConverter.GetBytes(packet.Count));
			return [.. packet];
		}
		public bool clientToServer = clientToServer;

		public GameAction(int player, byte packetType, byte[]? packet, bool clientToServer) : this(player, packetType, Convert.ToBase64String(packet!), clientToServer)
		{
		}
	}
	public string[] cmdlineArgs = cmdlineArgs;
	public List<GameAction> actions = [];
	public int seed = seed;
}
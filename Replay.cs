using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CardGameUtils;

class Replay
{
	public class GameAction
	{
		public int player;
		public byte packetType;
		public string packetContent;
		public byte[] packetContentBytes()
		{
			return Convert.FromBase64String(packetContent);
		}
		public byte[] fullPacketBytes()
		{
			List<byte> packet = packetContentBytes().ToList();
			packet.Insert(0, packetType);
			packet.InsertRange(0, BitConverter.GetBytes(packet.Count));
			return packet.ToArray();
		}
		public bool clientToServer;

		public GameAction(int player, byte packetType, byte[]? packet, bool clientToServer) : this(player, packetType, Convert.ToBase64String(packet!), clientToServer)
		{
		}
		[JsonConstructorAttribute]
		public GameAction(int player, byte packetType, string packetContent, bool clientToServer)
		{
			this.player = player;
			this.packetType = packetType;
			this.packetContent = packetContent;
			this.clientToServer = clientToServer;
		}
	}
	public string[] cmdlineArgs;
	public List<GameAction> actions = new List<GameAction>();
	public int seed;
	public Replay(string[] cmdlineArgs, int seed)
	{
		this.seed = seed;
		this.cmdlineArgs = cmdlineArgs;
	}
}
using System.Collections.Generic;

namespace CardGameUtils;

class Replay
{
	public class GameAction
	{
		public int player;
		public byte[] packet;
		public bool clientToServer;

		public GameAction(int player, byte[] packet, bool clientToServer)
		{
			this.player = player;
			this.packet = packet;
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
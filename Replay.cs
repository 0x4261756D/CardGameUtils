namespace CardGameUtils;

class Replay
{
	public class GameAction
	{
		public int player;
		public List<byte> packet;
		public bool clientToServer;

		public GameAction(int player, List<byte> packet, bool clientToServer)
		{
			this.player = player;
			this.packet = packet;
			this.clientToServer = clientToServer;
		}
	}
	public string[] cmdlineArgs;
	public List<GameAction> actions = new List<GameAction>();
	public Replay(string[] cmdlineArgs)
	{
		this.cmdlineArgs = cmdlineArgs;
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using static CardGameUtils.Structs.NetworkingStructs;

namespace CardGameUtils;

class Functions
{
	public enum LogSeverity
	{
		Debug,
		Warning,
		Error,
	}
	public static void Log(string message, LogSeverity severity = LogSeverity.Debug, bool includeFullPath = false, [CallerFilePath] string propertyName = "")
	{
		ConsoleColor current = Console.ForegroundColor;
		if (severity == LogSeverity.Warning)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
		}
		else if (severity == LogSeverity.Error)
		{
			Console.ForegroundColor = ConsoleColor.Red;
		}
#if RELEASE
		if(severity != LogSeverity.Debug)
		{
#endif
		Console.WriteLine($"{severity.ToString().ToUpper()}: [{(includeFullPath ? propertyName : Path.GetFileNameWithoutExtension(propertyName))}]: {message}");
#if RELEASE
		}
#endif
		Console.ForegroundColor = current;
	}

	public static List<byte> GeneratePayload<T>(T response)
	{
		List<byte> ret = new List<byte>();
		ret.Add(NetworkingConstants.PacketDict[typeof(T)]);
		string json = JsonSerializer.Serialize(response, NetworkingConstants.jsonIncludeOption);
		ret.AddRange(Encoding.UTF8.GetBytes(json));
		ret.AddRange(Packet.ENDING);
		return ret;
	}

	public static T? ReceivePacket<T>(NetworkStream stream, int timeout = -1) where T : PacketContent
	{
		List<byte>? payload = ReceiveRawPacket(stream, timeout);
		if (payload == null)
		{
			return null;
		}
		while (payload![0] != NetworkingConstants.PacketDict[typeof(T)])
		{
			payload = ReceiveRawPacket(stream, timeout);
			Log($"{NetworkingConstants.PacketDict.Where(x => x.Value == payload![0])}");
		}
		return DeserializePayload<T>(payload);
	}
	public static List<byte>? ReceiveRawPacket(NetworkStream stream, int timeout = -1)
	{
		int matched = 0;
		List<byte> bytes = new List<byte>();
		int sleepTime = 10;
		while (matched < Packet.ENDING.Length)
		{
			int waited = 0;
			while (!stream.DataAvailable)
			{
				Thread.Sleep(sleepTime);
				if (timeout > 0)
				{
					if (waited >= timeout)
					{
						return null;
					}
					waited += sleepTime;
				}
			}
			int maybeByte = stream.ReadByte();
			if (maybeByte == -1)
			{
				throw new Exception("Didn't read data");
			}
			byte bt = (byte)maybeByte;
			bytes.Add(bt);
			if (bt == Packet.ENDING[matched])
			{
				matched++;
			}
			else if (bt == Packet.ENDING[0])
			{
				matched = 1;
			}
			else
			{
				matched = 0;
			}
		}
		return bytes.GetRange(0, bytes.Count - Packet.ENDING.Length);
	}

	public static T DeserializePayload<T>(List<byte> payload) where T : PacketContent
	{
		if (payload[0] != NetworkingConstants.PacketDict[typeof(T)])
		{
			Type? t = null;
			foreach (var typ in NetworkingConstants.PacketDict)
			{
				if (typ.Value == payload[0])
				{
					t = typ.Key;
					break;
				}
			}
			throw new Exception($"Expected a packet of type {typeof(T)}({NetworkingConstants.PacketDict[typeof(T)]}) but got {t}({payload[0]}) instead");
		}
		payload.RemoveAt(0);
		return DeserializeJson<T>(Encoding.UTF8.GetString(payload.ToArray()));
	}
	public static T DeserializeJson<T>(string data) where T : PacketContent
	{
		T? ret = JsonSerializer.Deserialize<T>(data, NetworkingConstants.jsonIncludeOption);
		if (ret == null)
		{
			throw new Exception($"{data} deserialized to null");
		}
		return ret;
	}
}

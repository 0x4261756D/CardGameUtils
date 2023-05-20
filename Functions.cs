using System;
using System.Collections.Generic;
using System.Diagnostics;
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
	public static void Log(string message, LogSeverity severity = LogSeverity.Debug, bool includeFullPath = false, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string propertyName = "")
	{
		ConsoleColor current = Console.ForegroundColor;
		if(severity == LogSeverity.Warning)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
		}
		else if(severity == LogSeverity.Error)
		{
			Console.ForegroundColor = ConsoleColor.Red;
		}
#if RELEASE
		if(severity != LogSeverity.Debug)
		{
#endif
		Console.WriteLine($"{severity.ToString().ToUpper()}: [{(includeFullPath ? propertyName : Path.GetFileNameWithoutExtension(propertyName))}:{lineNumber}]: {message}");
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

	public static List<byte>? ReceivePacket<T>(NetworkStream stream, int timeout = -1) where T : PacketContent
	{
		List<byte>? payload = ReceiveRawPacket(stream, timeout);
		if(payload == null)
		{
			return null;
		}
		while(payload![0] != NetworkingConstants.PacketDict[typeof(T)])
		{
			payload = ReceiveRawPacket(stream, timeout);
			Log($"Ignoring {NetworkingConstants.PacketDict.First(x => x.Value == payload![0]).Key}", severity: LogSeverity.Warning);
		}
		return payload;
	}

	public static List<byte> remaining = new List<byte>();
	public static List<byte>? ReceiveRawPacket(NetworkStream stream, long timeoutMs = -1)
	{
		byte[] b = new byte[1024];
		Span<byte> buffer = new Span<byte>(b);
		Stopwatch? watch = null;
		if(timeoutMs != -1) watch = Stopwatch.StartNew();
		while(timeoutMs == -1 || watch?.ElapsedMilliseconds < timeoutMs)
		{
			if(remaining.Count >= Packet.ENDING.Length + 3)
			{
				int index = remaining.IndexOf(Packet.ENDING[0]);
				while(index != -1 && index + Packet.ENDING.Length <= remaining.Count)
				{
					if(remaining.GetRange(index, Packet.ENDING.Length).SequenceEqual(Packet.ENDING))
					{
						List<byte> ret = remaining.Take(index).ToList();
						remaining.RemoveRange(0, index + Packet.ENDING.Length);
						return ret;
					}
					index = remaining.IndexOf(Packet.ENDING[0], index);
				}
			}
			int count = stream.ReadAtLeast(buffer, Packet.ENDING.Length + 3, throwOnEndOfStream: false);
			remaining.AddRange(buffer.Slice(0, count).ToArray());
		}
		return null;
	}

	public static T DeserializePayload<T>(List<byte> payload) where T : PacketContent
	{
		if(payload[0] != NetworkingConstants.PacketDict[typeof(T)])
		{
			Type? t = null;
			foreach(var typ in NetworkingConstants.PacketDict)
			{
				if(typ.Value == payload[0])
				{
					t = typ.Key;
					break;
				}
			}
			throw new Exception($"Expected a packet of type {typeof(T)}({NetworkingConstants.PacketDict[typeof(T)]}) but got {t}({payload[0]}) instead");
		}
		return DeserializeJson<T>(Encoding.UTF8.GetString(payload.GetRange(1, payload.Count - 1).ToArray()));
	}
	public static T DeserializeJson<T>(string data) where T : PacketContent
	{
		T? ret = JsonSerializer.Deserialize<T>(data, NetworkingConstants.jsonIncludeOption);
		if(ret == null)
		{
			throw new Exception($"{data} deserialized to null");
		}
		return ret;
	}
	public static List<byte> Request(PacketContent request, string address, int port, int timeout = -1)
	{
		using(TcpClient client = new TcpClient())
		{
			client.Connect(address, port);
			return Request(request, client, timeout);
		}
	}

	public static List<byte> Request(PacketContent request, TcpClient client, int timeout = -1)
	{
		using(NetworkStream stream = client.GetStream())
		{
			List<byte> payload = new List<byte>();
			payload.Add(NetworkingConstants.PacketDict[request.GetType()]);
			string json = JsonSerializer.Serialize(request, request.GetType(), NetworkingConstants.jsonIncludeOption);
			payload.AddRange(Encoding.UTF8.GetBytes(json));
			payload.AddRange(Packet.ENDING);
			stream.Write(payload.ToArray(), 0, payload.Count);
			// Reuse the payload list for the response
			payload = ReceiveRawPacket(stream)!;
			return payload;
		}
	}
}


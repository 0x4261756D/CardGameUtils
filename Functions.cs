using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
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
		ret.InsertRange(0, BitConverter.GetBytes(ret.Count));
		return ret;
	}

	public static (byte, byte[]?) ReceivePacket<T>(NetworkStream stream) where T : PacketContent
	{
		(byte type, byte[]? payload) = ReceiveRawPacket(stream);
		if(payload == null)
		{
			return (type, payload);
		}
		while(type != NetworkingConstants.PacketDict[typeof(T)])
		{
			(type, payload) = ReceiveRawPacket(stream);
			Log($"Ignoring {NetworkingConstants.PacketDict.First(x => x.Value == type).Key}", severity: LogSeverity.Warning);
		}
		return (type, payload);
	}
	public static (byte, byte[]?) ReceiveRawPacket(NetworkStream stream)
	{
		Stopwatch watch = Stopwatch.StartNew();
		byte[] sizeBuffer = new byte[4];
		stream.ReadExactly(sizeBuffer);
		uint size = BitConverter.ToUInt32(sizeBuffer);
		Span<byte> buffer = new byte[size];
		stream.ReadExactly(buffer);
		Log($"{watch.ElapsedMilliseconds}ms for raw receive of {size} bytes");
		return (buffer[0], buffer.Length > 0 ? buffer.Slice(1).ToArray() : null);
	}

	public static T DeserializePayload<T>((byte type, byte[]? payload) input) where T : PacketContent
	{
		return DeserializePayload<T>(input.type, input.payload);
	}
	public static T DeserializePayload<T>(byte type, byte[]? payload) where T : PacketContent
	{
		if(payload == null)
		{
			return (T)new PacketContent();
		}
		if(type != NetworkingConstants.PacketDict[typeof(T)])
		{
			Type? t = null;
			foreach(var typ in NetworkingConstants.PacketDict)
			{
				if(typ.Value == type)
				{
					t = typ.Key;
					break;
				}
			}
			throw new Exception($"Expected a packet of type {typeof(T)}({NetworkingConstants.PacketDict[typeof(T)]}) but got {t}({type}) instead");
		}
		return DeserializeJson<T>(Encoding.UTF8.GetString(payload));
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
	public static (byte, byte[]?) Request(PacketContent request, string address, int port)
	{
		using(TcpClient client = new TcpClient())
		{
			client.Connect(address, port);
			return Request(request, client);
		}
	}

	public static (byte, byte[]?) Request(PacketContent request, TcpClient client)
	{
		using(NetworkStream stream = client.GetStream())
		{
			List<byte> payload = new List<byte>();
			payload.Add(NetworkingConstants.PacketDict[request.GetType()]);
			string json = JsonSerializer.Serialize(request, request.GetType(), NetworkingConstants.jsonIncludeOption);
			payload.AddRange(Encoding.UTF8.GetBytes(json));
			payload.InsertRange(0, BitConverter.GetBytes(payload.Count));
			stream.Write(payload.ToArray(), 0, payload.Count);
			// Reuse the payload list for the response
			return ReceiveRawPacket(stream);
		}
	}
}


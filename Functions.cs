using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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

	public static byte[] GeneratePayload<T>(T response)
	{
		byte[] json = JsonSerializer.SerializeToUtf8Bytes(response, NetworkingConstants.jsonIncludeOption);
		return
		[
			.. BitConverter.GetBytes(json.Length + 1),
			NetworkingConstants.PacketDict[typeof(T)],
			.. json,
		];
	}

	public static byte[]? TryReceivePacket<T>(NetworkStream stream, long timeoutMs) where T : PacketContent
	{
		Stopwatch watch = Stopwatch.StartNew();
		if(!stream.CanRead)
		{
			return null;
		}
		while(!stream.DataAvailable)
		{
			Thread.Sleep(10);
			if(!stream.CanRead || (timeoutMs != -1 && watch.ElapsedMilliseconds > timeoutMs))
			{
				return null;
			}
		}
		return ReceivePacket<T>(stream);
	}
	public static byte[]? ReceivePacket<T>(NetworkStream stream) where T : PacketContent
	{
		(byte type, byte[]? payload) = ReceiveRawPacket(stream);
		while(type != NetworkingConstants.PacketDict[typeof(T)])
		{
			(type, payload) = ReceiveRawPacket(stream);
			foreach(Type? key in NetworkingConstants.PacketDict.Keys)
			{
				if(NetworkingConstants.PacketDict[key] == type)
				{
					Log($"Ignoring {key}", severity: LogSeverity.Warning);
					break;
				}
			}
		}
		return payload;
	}

	public static (byte, byte[]?)? TryReceiveRawPacket(NetworkStream stream, long timeoutMs)
	{
		Stopwatch watch = Stopwatch.StartNew();
		if(!stream.CanRead)
		{
			return null;
		}
		while(!stream.DataAvailable)
		{
			Thread.Sleep(10);
			if(!stream.CanRead || (timeoutMs != -1 && watch.ElapsedMilliseconds > timeoutMs))
			{
				return null;
			}
		}
		return ReceiveRawPacket(stream);
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
		return (buffer[0], buffer.Length > 0 ? buffer[1..].ToArray() : null);
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
		return DeserializeJson<T>(payload);
	}
	public static T DeserializeJson<T>(byte[] data) where T : PacketContent
	{
		return JsonSerializer.Deserialize<T>(data, NetworkingConstants.jsonIncludeOption) ?? throw new Exception($"{data} deserialized to null");
	}
	public static (byte, byte[]?) Request(PacketContent request, string address, int port)
	{
		using TcpClient client = new();
		client.Connect(address, port);
		return Request(request, client);
	}

	public static (byte, byte[]?) Request(PacketContent request, TcpClient client)
	{
		using NetworkStream stream = client.GetStream();
		byte[] json = JsonSerializer.SerializeToUtf8Bytes(request, request.GetType(), NetworkingConstants.jsonIncludeOption);
		byte[] payload =
		[
			.. BitConverter.GetBytes(json.Length + 1),
			NetworkingConstants.PacketDict[request.GetType()],
			.. json,
		];
		stream.Write(payload);
		// Reuse the payload list for the response
		return ReceiveRawPacket(stream);
	}
}


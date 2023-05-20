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
using System.Threading.Tasks;
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

	public static ReadOnlySpan<byte> ReceivePacket<T>(NetworkStream stream) where T : PacketContent
	{
		ReadOnlySpan<byte> payload = ReceiveRawPacket(stream);
		if(payload == null)
		{
			return null;
		}
		while(payload![0] != NetworkingConstants.PacketDict[typeof(T)])
		{
			payload = ReceiveRawPacket(stream);
			byte type = payload[0];
			Log($"Ignoring {NetworkingConstants.PacketDict.First(x => x.Value == type).Key}", severity: LogSeverity.Warning);
		}
		return payload;
	}
	public static ReadOnlySpan<byte> ReceiveRawPacket(NetworkStream stream)
	{
		byte[] sizeBuffer = new byte[4];
		stream.ReadExactly(sizeBuffer);
		uint size = BitConverter.ToUInt32(sizeBuffer);
		byte[] buffer = new byte[size];
		stream.ReadExactly(buffer);
		return buffer;
	}

	public static async Task<ReadOnlyMemory<byte>> ReceiveRawPacketAsync(NetworkStream stream)
	{
		byte[] sizeBuffer = new byte[4];
		await stream.ReadExactlyAsync(sizeBuffer);
		uint size = BitConverter.ToUInt32(sizeBuffer);
		byte[] buffer = new byte[size];
		await stream.ReadExactlyAsync(buffer);
		return buffer;
	}

	public static T DeserializePayload<T>(ReadOnlySpan<byte> payload) where T : PacketContent
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
		return DeserializeJson<T>(Encoding.UTF8.GetString(payload.Slice(1)));
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
	public static ReadOnlySpan<byte> Request(PacketContent request, string address, int port)
	{
		using(TcpClient client = new TcpClient())
		{
			client.Connect(address, port);
			return Request(request, client);
		}
	}

	public static async Task<ReadOnlyMemory<byte>> RequestAsync(PacketContent request, string address, int port)
	{
		using TcpClient client = new TcpClient();
		await client.ConnectAsync(address, port);
		return await RequestAsync(request, client);
	}

	public static async Task<ReadOnlyMemory<byte>> RequestAsync(PacketContent request, TcpClient client)
	{
		using NetworkStream stream = client.GetStream();
		List<byte> payload = new List<byte>();
		payload.Add(NetworkingConstants.PacketDict[request.GetType()]);
		string json = JsonSerializer.Serialize(request, request.GetType(), NetworkingConstants.jsonIncludeOption);
		payload.AddRange(Encoding.UTF8.GetBytes(json));
		payload.InsertRange(0, BitConverter.GetBytes(payload.Count));
		await stream.WriteAsync(payload.ToArray(), 0, payload.Count);
		return await ReceiveRawPacketAsync(stream);
	}

	public static ReadOnlySpan<byte> Request(PacketContent request, TcpClient client)
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


using System.Text.Json;

namespace CardGameUtils;

public class NetworkingConstants
{
	public static readonly JsonSerializerOptions jsonIncludeOption = new JsonSerializerOptions { IncludeFields = true };
}
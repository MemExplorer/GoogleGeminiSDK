
namespace GoogleGeminiSDK;
internal class FileHelpers
{
	private static readonly (string MimeType, byte[] HeaderBytes)[] _fileHeaderDict = new[]
	{
		("application/pdf", new byte[]{ 0x25, 0x50, 0x44, 0x46 }),
		("image/bmp", new byte[]{ 0x42, 0x4D }),
		("image/gif", new byte[]{ 0x47, 0x49, 0x46 }),
		("image/jpeg", new byte[]{ 0xFF, 0xD8 }),
		("image/png", new byte[]{ 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })
	};

	public static string? GetMimeType(ReadOnlySpan<byte> data)
	{
		foreach (var entry in _fileHeaderDict)
		{
			if (data.Length < entry.HeaderBytes.Length)
				continue;

			var arrSlice = data.Slice(0, entry.HeaderBytes.Length);
			if (arrSlice.SequenceEqual(entry.HeaderBytes))
				return entry.MimeType;
		}

		return null;
	}
}

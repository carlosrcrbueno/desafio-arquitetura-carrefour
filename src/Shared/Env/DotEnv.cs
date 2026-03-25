namespace Shared.Env
{

	using System;
	using System.IO;

	public static class DotEnv
	{
		public static void Load(string filePath)
		{
			if (!File.Exists(filePath))
				return;

			foreach (var line in File.ReadAllLines(filePath))
			{
				var trimmed = line.Trim();
				if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
					continue;

				var index = trimmed.IndexOf('=', StringComparison.Ordinal);
				if (index < 0)
					continue;

				var key = trimmed[..index].Trim();
				var value = trimmed[(index + 1)..].Trim();

				if (string.IsNullOrEmpty(key))
					continue;

				Environment.SetEnvironmentVariable(key, value);
			}
		}

		public static void Load()
		{
			var appRoot = Directory.GetCurrentDirectory();
			var dotEnv = Path.Combine(appRoot, ".env");

			Load(dotEnv);
		}
	}


}

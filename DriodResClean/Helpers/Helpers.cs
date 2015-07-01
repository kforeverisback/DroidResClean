using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriodResClean.Helper
{
	public static class Helpers
	{
		public static class Functions
		{
			static readonly string[] SizeSuffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
			/// <summary>
			/// See this: http://stackoverflow.com/questions/14488796/does-net-provide-an-easy-way-convert-bytes-to-kb-mb-gb-etc
			/// </summary>
			/// <param name="value"></param>
			/// <returns></returns>
			public static string GetFileSizeString(Int64 value)
			{
				if (value < 0) { return "-" + GetFileSizeString(-value); }
				if (value == 0) { return "0.0 B"; }

				int mag = (int)Math.Log(value, 1024);
				decimal adjustedSize = (decimal)value / (1L << (mag * 10));

				return string.Format("{0:n1} {1}", adjustedSize, SizeSuffixes[mag]);
			}

#if DEBUG
			public static string ListStr(List<string> list)
			{
				StringBuilder sb = new StringBuilder("");
				foreach (var item in list)
				{
					sb.AppendLine(item);

				}

				return sb.ToString();

			}
#else
			public static string ListStr(List<string> list)
		{
			return list.ToString();
		}
#endif
		}
	}
}

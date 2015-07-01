using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

using HelperFunctions = DriodResClean.Helper.Helpers.Functions;
using TraceDebug = System.Diagnostics.Debug;

namespace DriodResClean
{
	public class ProjectProcessMan
	{
		public ProjectProcessMan()
		{
			
		}

		public static string HTMLFile_DefaultName
		{
			get { return "Unused_Files.html"; }
		}

		private string GetUncommentedSource(string filePath)
		{
			string source;
			string extension = Path.GetExtension(filePath);
			using (var fileStream = File.OpenRead(filePath))
			{
				var strReader = new StreamReader(fileStream, Encoding.UTF8, true, 512);
				string input = strReader.ReadToEnd();

				if (extension.ToLower().Contains("xml"))
				{
					source = StripCommentsFromXml(input);
				}
				else
				{
					source = StripCommentsFromJava(input);
				}
			}

			return source;
		}

		string StripCommentsFromXml(string input)
		{
			string xmlComment = "<!--.*?-->";

			string noComments = Regex.Replace(input,
				xmlComment, String.Empty, RegexOptions.Singleline);

			return noComments;
		}

		string StripCommentsFromJava(string input)
		{
			const string blockComments = @"/\*(.*?)\*/";
			const string lineComments = @"//(.*?)\r?\n";
			const string strings = @"""((\\[^\n]|[^""\n])*)""";
			const string verbatimStrings = @"@(""[^""]*"")+";

			string noComments = Regex.Replace(input,
				blockComments + "|"
				+ lineComments + "|"
				+ strings + "|"
				+ verbatimStrings, me =>
				{
					if (me.Value.StartsWith("/*") || me.Value.StartsWith("//"))
						return me.Value.StartsWith("//") ? Environment.NewLine : "";
					// Keep the literal strings
					return me.Value;
				}, RegexOptions.Singleline);

			return noComments;
		}

		Dictionary<string, List<string>> _unusedFiles;
		public async Task<Dictionary<string, List<string>>> Process(string projPath)
		{
			string pathToRes = Path.Combine(projPath, "res");
			string pathToSrc = Path.Combine(projPath, "src");

			_unusedFiles = new Dictionary<string, List<string>>();
			Dictionary<string, List<string>> drawableFiles = await Task.Run<Dictionary<string, List<string>>>(() =>
			{
				return EnumerateResDirForDrawableFiles(new DirectoryInfo(pathToRes));
			});

			HashSet<string> usedDrawableIDs = await Task.Run<HashSet<string>>(() => 
			{
				HashSet<string> ids = new HashSet<string>();

				DirectoryInfo dirInfo = new DirectoryInfo(pathToSrc);
				EnumerateSrcDirForIDs(dirInfo, ref ids);
				dirInfo = new DirectoryInfo(pathToRes);
				EnumerateResDirForIDs(dirInfo, ref ids);

				EnumerateAndroidManifestForIDs(new FileInfo(Path.Combine(projPath, "AndroidManifest.xml")), ref ids);

				return ids;
			});


			foreach (var item in drawableFiles)
			{
				if (!usedDrawableIDs.Contains(item.Key))
				{
					//Debug.WriteLine("~~~{0}@{1}", item.Key, ListStr(item.Value));
					_unusedFiles.Add(item.Key, item.Value);
				}
			}

			return _unusedFiles;

		}

		static readonly string html_prefix = "<style type=\"text/css\">\n.tg  {border-collapse:collapse;border-spacing:0;}\n.tg td{font-family:Arial, sans-serif;font-size:14px;padding:10px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;}\n.tg th{font-family:Arial, sans-serif;font-size:14px;font-weight:normal;padding:10px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;}\n.tg .tg-e3zv{font-weight:bold}\n.tg .tg-huad{background-color:#ffccc9}\n.tg .tg-u0wu{background-color:#9aff99;text-align:right}\n</style>\n<h3>Total files: __##__</h2><h3>Total Size : ##__##</h2>\n<table class=\"tg\">\n  <tr><th class=\"tg-e3zv\">Thumb<br></th><th class=\"tg-huad\">Deleted?</th><th class=\"tg-e3zv\">File Name</th><th class=\"tg-e3zv\">Relative Path</th><th class=\"tg-u0wu\">Size</th><th class=\"tg-e3zv\">Full Path<br></th>";
		static readonly string html_row = "</tr><td class=\"tg-031e\"><img src=\"{0}\" height=\"42\" width=\"42\"></td><td class=\"tg-huad\">{5}</td><td class=\"tg-031e\">{1}</td><td class=\"tg-031e\">{2}</td><td class=\"tg-u0wu\">{3}</td><td class=\"tg-031e\">{4}</td></tr>";
		public void generateHtml(List<Model.MainModel> models, string projPath)
		{
			try
			{
				string pathToHtml = Path.Combine(projPath, HTMLFile_DefaultName);
				using(StreamWriter strW = File.CreateText(pathToHtml))
				{
					
					int totalFiles = models.Count;
					long totalSize = 0;
					StringBuilder sb = new StringBuilder();
					foreach (var m in models)
					{

						//foreach (var path in item)
						{
							try
							{
								sb.AppendFormat(html_row, m.FileFullPath, m.FileName, m.FileRelPath, m.FileSize, m.FileFullPath, File.Exists(m.FileFullPath.AbsolutePath)?"No":"Yes");

								totalSize += m.FileSizeByte;
							}
							catch (System.IO.IOException ioex)
							{
								TraceDebug.WriteLine("==> Exception:" + ioex.Message);
#if DEBUG
#endif
							}


						}
					}

					strW.WriteLine(html_prefix.Replace("__##__", totalFiles.ToString()).Replace("##__##", HelperFunctions.GetFileSizeString(totalSize)));
					strW.WriteLine(sb.ToString());

					strW.Flush();
				}
			}
			catch (IOException ioex)
			{
				TraceDebug.WriteLine("==> IOEx: " + ioex.Message);
#if DEBUG
#endif
			}
		}

		private Dictionary<string, List<string>> EnumerateResDirForDrawableFiles(DirectoryInfo dirInfo)
		{
			Dictionary<string, List<string>> files = new Dictionary<string, List<string>>();
			foreach (var di in dirInfo.GetDirectories("drawable*"))
			{
				foreach (var fi in di.GetFiles("*.png"))
				{
					string key = Path.GetFileNameWithoutExtension(fi.Name);

					//FOR .9Patch and theme based files
					int indx = key.IndexOf('.');
					if(indx != -1)
					{
						key = key.Substring(0, indx);
					}
					//FOR .9Patch files
#if DEBUG
					TraceDebug.WriteLine("{2}::{0}@{1}", Path.GetFileNameWithoutExtension(fi.Name), fi.DirectoryName, key);
#endif
					if (files.ContainsKey(key))
					{
						files[key].Add(fi.FullName);
					}
					else
					{
						var list = new List<string>();
						list.Add(fi.FullName);
						files.Add(key, list);
					}
				}
			}

			return files;

		}

		private void EnumerateResDirForIDs(DirectoryInfo di, ref HashSet<string> drawableIds)
		{
			const string regexPattern = @"(@drawable/)(\w*)";
			foreach (var fi in di.GetFiles("*.xml"))
			{
#if DEBUG
				TraceDebug.WriteLine("{0}@{1}", Path.GetFileNameWithoutExtension(fi.Name), fi.DirectoryName);
#endif
				string uncommentedSrc = GetUncommentedSource(fi.FullName);

				MatchCollection mc = Regex.Matches(uncommentedSrc, regexPattern, RegexOptions.Singleline);
				for (int i = 0; i < mc.Count; i++)
				{
					Match m = mc[i];
#if DEBUG
					TraceDebug.WriteLine("==>" + m.Value.Substring(5));
#endif
					if (m.Value.StartsWith("@drawable/"))
						drawableIds.Add(m.Value.Substring("@drawable/".Length));
				}
			}

			foreach (var di2 in di.GetDirectories())
			{
				EnumerateResDirForIDs(di2, ref drawableIds);
			}

		}

		private void EnumerateAndroidManifestForIDs(FileInfo fi, ref HashSet<string> drawableIds)
		{
			const string regexPattern = @"(@drawable/)(\w*)";
			//foreach (var fi in di.GetFiles("*.xml"))
			{
#if DEBUG
				TraceDebug.WriteLine("{0}@{1}", Path.GetFileNameWithoutExtension(fi.Name), fi.DirectoryName);
#endif
				string uncommentedSrc = GetUncommentedSource(fi.FullName);

				MatchCollection mc = Regex.Matches(uncommentedSrc, regexPattern, RegexOptions.Singleline);
				for (int i = 0; i < mc.Count; i++)
				{
					Match m = mc[i];
#if DEBUG
					TraceDebug.WriteLine("==>" + m.Value.Substring(5));
#endif
					if (m.Value.StartsWith("@drawable/"))
						drawableIds.Add(m.Value.Substring("@drawable/".Length));
				}
			}
		}

		private void EnumerateSrcDirForIDs(DirectoryInfo di, ref HashSet<string> drawableIds)
		{
			foreach (var fi in di.GetFiles("*.java"))
			{
#if DEBUG
				TraceDebug.WriteLine("{0}@{1}", Path.GetFileNameWithoutExtension(fi.Name), fi.DirectoryName);
#endif
				string uncommentedSrc = GetUncommentedSource(fi.FullName);
				const string regexPattern = @"R.drawable.(\w+)";

				MatchCollection mc = Regex.Matches(uncommentedSrc, regexPattern, RegexOptions.Singleline);
				for (int i = 0; i < mc.Count; i++ )
				{
					Match m = mc[i];
#if DEBUG
					TraceDebug.WriteLine("==>" + m.Value.Substring(5));
#endif
					if (m.Value.StartsWith("R.drawable."))
						drawableIds.Add(m.Value.Substring("R.drawable.".Length));
				}
			}

			foreach (var di2 in di.GetDirectories())
			{
				EnumerateSrcDirForIDs(di2, ref drawableIds);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace DriodResClean
{
	public class ProjectProcessMan
	{
		string _projectPath = "";
		public ProjectProcessMan(string projectPath)
		{
			_projectPath = projectPath;
		}

		private string GetUncommentedSource(string filePath)
		{
			string source;
			string extension = Path.GetExtension(filePath);
			using (var fileStream = File.OpenRead(filePath))
			using (var strReader = new StreamReader(fileStream, Encoding.UTF8, true, 512))
			{
				string input = strReader.ReadToEnd();

				if (extension.ToLower().Contains("xml"))
				{
					source = StripCommentsFromXml(input);
				}
				else
				{
					source = StripCommentsFromJavaOrCs(input);
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

		string StripCommentsFromJavaOrCs(string input)
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

		private HashSet<string> _files = new HashSet<string>();
		public async void Process()
		{
			_files = await Task.Run < HashSet<string>>(() => 
			{
				HashSet<string> files = new HashSet<string>();
				string pathToRes = Path.Combine(_projectPath, "res");
				foreach (string path in Directory.EnumerateDirectories(pathToRes, "drawable*"))
				{
					System.Diagnostics.Debug.WriteLine(path);
					foreach (string file in Directory.EnumerateFiles(path, "*.png"))
					{
						System.Diagnostics.Debug.WriteLine(Path.GetFileName(file));
						files.Add(Path.GetFileName(file));
					}
				}

				return files;
			});
		}
	}
}

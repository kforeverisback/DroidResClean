using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GalaSoft.MvvmLight;
using DriodResClean.Helper;

namespace DriodResClean.Model
{
	public class MainModel : ObservableObject
	{
		private bool _IsSelected = true;
		public bool IsSelected
		{
			get
			{
				return _IsSelected;
			}
			set
			{
				_IsSelected = value;
				RaisePropertyChanged("IsSelected");
			}
		}

		private string _fileName;
		public string FileName
		{
			get
			{
				return _fileName;
			}
			set
			{
				_fileName = value;
				RaisePropertyChanged("FileName");
			}
		}

		private string _fileRelPath;
		public string FileRelPath
		{
			get
			{
				return _fileRelPath;
			}
			set
			{
				_fileRelPath = value;
				RaisePropertyChanged("FileRelPath");
			}
		}

		private string _fileSize;
		public string FileSize
		{
			get
			{
				return _fileSize;
			}
			set
			{
				_fileSize = value;
				RaisePropertyChanged("FileSize");
			}
		}

		public long FileSizeByte
		{
			get;
			private set;
		}

		private string _fileFullPath;
		public Uri FileFullPath
		{
			get
			{
				return new Uri(_fileFullPath, UriKind.Absolute);
			}
			set
			{
				_fileFullPath = value.AbsolutePath;
				RaisePropertyChanged("FileFullPath");
				FileName = Path.GetFileName(_fileFullPath);
				int indx = _fileFullPath.IndexOf("res");
				if (indx != -1)
				{
					string path = _fileFullPath.Substring(indx);
					FileRelPath = Path.GetDirectoryName(path);
				}

				FileSizeByte = new FileInfo(_fileFullPath).Length;
				FileSize = Helpers.Functions.GetFileSizeString(FileSizeByte);
			}
		}

		
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;

namespace DriodResClean.ViewModel
{
	public class MainViewModel : ViewModelBase
	{
		#region Bound Properties
		List<Model.MainModel> _mainList = new List<Model.MainModel>();
		public List<Model.MainModel> DataItems
		{
			get
			{
				return _mainList;
			}
		}

		bool _headerSelectAll = false;
		public bool HeaderSelectAll
		{
			get { return _headerSelectAll; }
			set 
			{
				_headerSelectAll = value;
				RaisePropertyChanged("HeaderSelectAll");
				System.Diagnostics.Debug.WriteLine("==>>ETSTSETESTSE");
			}
		}

		bool? _isAndroidProject = null;
		public bool? IsAndroidProject
		{
			get { return _isAndroidProject; }
			set
			{
				_isAndroidProject = value;
				RaisePropertyChanged("IsAndroidProject");
			}
		}

		string _projectPathText = "";
		public string ProjectPathText
		{
			get { return _projectPathText; }
			set
			{
				_projectPathText = value;
				RaisePropertyChanged("ProjectPathText");
			}
		}
		#endregion

		public MainViewModel()
		{
			
		}

		RelayCommand _browseCommand;
		public ICommand BrowseCommand
		{
			get
			{
				if (_browseCommand == null)
				{
					_browseCommand = new RelayCommand( ()=>
					{
						System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
						fbd.RootFolder = Environment.SpecialFolder.MyComputer;
						System.Windows.Forms.DialogResult result = fbd.ShowDialog();
						if (result == System.Windows.Forms.DialogResult.OK)
						{
							ProjectPathText = fbd.SelectedPath;
						}
					},
						() => true);
				}
				return _browseCommand;
			}
		}

		RelayCommand _processCommand;
		public ICommand ProcessCommand
		{
			get
			{
				if (_processCommand == null)
				{
					_processCommand = new RelayCommand(() =>
					{
						System.Diagnostics.Debug.WriteLine("==>> Process!!");
						ProjectProcessMan pp = new ProjectProcessMan(this.ProjectPathText);
						pp.Process();
					},
					() => IsAndroidProject == true);
				}
				return _processCommand;
			}
		}
	}
}

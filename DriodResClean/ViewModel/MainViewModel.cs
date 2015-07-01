using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using System.IO;
using TraceDebug = System.Diagnostics.Debug;
using HelperFunctions = DriodResClean.Helper.Helpers.Functions;

namespace DriodResClean.ViewModel
{
	public enum StatusEnum
	{
		ValidDroidProject,
		InvalidDroidProject,
		ProcessSuccess,
		Processing,
		EmptyProjectPath,
		DeleteSuccess
	}
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

		bool _headerSelectAll = true;
		public bool HeaderSelectAll
		{
			get { return _headerSelectAll; }
			set 
			{
				_headerSelectAll = value;
				RaisePropertyChanged("HeaderSelectAll");
				foreach (var item in DataItems)
				{
					item.IsSelected = value;
				}
			}
		}

		StatusEnum? _status = StatusEnum.EmptyProjectPath;
		public StatusEnum? Status
		{
			get { return _status; }
			set 
			{
				_status = value;
				RaisePropertyChanged("Status"); 
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

		public bool IsDataGridEnabled
		{
			get { return _status == StatusEnum.ProcessSuccess && DataItems.Count != 0; }
			set { RaisePropertyChanged("IsDataGridEnabled");}
		}
		#endregion

		#region Data Members
		Dictionary<string, List<string>> _unusedFiles = null;
		ProjectProcessMan _projectProcessor = new ProjectProcessMan();
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
					() => Status != StatusEnum.Processing);
				}
				return _browseCommand;
			}
		}

		RelayCommand _deleteCommand;
		public ICommand DeleteCommand
		{
			get
			{
				if (_deleteCommand == null)
				{
					_deleteCommand = new RelayCommand(async () =>
					{
						Status = StatusEnum.Processing;
						int deletedFilesCount = await Task.Run<int>(() => 
						{
							int count = 0;
							TraceDebug.WriteLine("Started Deleting...");
							Dictionary<string, List<string>> copied = new Dictionary<string, List<string>>(_unusedFiles);
							foreach (var item in copied.Values)
							{
								
								//foreach (var path in item)
								{
									try
									{
										foreach (var file in item)
										{
											File.SetAttributes(file, FileAttributes.Normal);
											File.Delete(file);
											count++;
										}
									}
									catch (Exception ioex)
									{
										TraceDebug.WriteLine("==> Exception:" + ioex.Message);
#if DEBUG
#endif
									}
								}
							}

							_projectProcessor.generateHtml(DataItems, ProjectPathText);
							System.Diagnostics.Process.Start(Path.Combine(ProjectPathText, ProjectProcessMan.HTMLFile_DefaultName));

							TraceDebug.WriteLine("Finished Deleting {0} files", count);
							return count;
						});
						System.Windows.MessageBox.Show(string.Format("Total files: {0}, deleted:{1}", DataItems.Count, deletedFilesCount), "Unused files deletion", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
						Status = StatusEnum.ProcessSuccess;
						_unusedFiles.Clear();
						DataItems.Clear();
						RaisePropertyChanged("DataItems");
						RaisePropertyChanged("IsDataGridEnabled");
					},
					() => Status == StatusEnum.ProcessSuccess && Status != StatusEnum.Processing && _mainList.Count != 0);
				}
				return _deleteCommand;
			}
		}


		RelayCommand _viewHTMLCommand;
		public ICommand ViewHTMLCommand
				{
					get
					{
						if (_viewHTMLCommand == null)
						{
							_viewHTMLCommand = new RelayCommand(async () =>
							{
								this.Status = StatusEnum.Processing;
								TraceDebug.WriteLine("Started Generating HTML");
								await Task.Run(() => 
								{
									_projectProcessor.generateHtml(DataItems, ProjectPathText);
									System.Diagnostics.Process.Start(Path.Combine(ProjectPathText, ProjectProcessMan.HTMLFile_DefaultName));
								});
								TraceDebug.WriteLine("Finished Generating HTML");
								this.Status = StatusEnum.ProcessSuccess;
							},
							() => Status == StatusEnum.ProcessSuccess && Status != StatusEnum.Processing && _mainList.Count != 0);
						}
						return _viewHTMLCommand;
					}
				}
		RelayCommand _processCommand;
		public ICommand ProcessCommand
		{
			get
			{
				if (_processCommand == null)
				{
					_processCommand = new RelayCommand(async () =>
					{
						Status = StatusEnum.Processing;
						IsDataGridEnabled = false;
						DataItems.Clear();
						TraceDebug.WriteLine("==>> Processing!!");

						//await Task.Run(() => 
						//{
						//	TraceDebug.WriteLine("==>> Waiting 10 sec!");
						//	System.Threading.Thread.Sleep(10000);
						//});
						//TraceDebug.WriteLine("==>> 10 sec finished!!");
						_unusedFiles = await _projectProcessor.Process(ProjectPathText);

						await Task.Run(() => 
						{
							if (_unusedFiles == null)
								return;
							int prevCount = DataItems.Count;
							foreach (var item in _unusedFiles)
							{
								foreach (var item2 in item.Value)
								{
									DataItems.Add(new Model.MainModel() { FileFullPath = new Uri(item2) });
								}
							}

							this.HeaderSelectAll = true;
							this.IsDataGridEnabled = true;

							if (DataItems.Count != prevCount)
							{
								RaisePropertyChanged("DataItems");
							}
							this.Status = StatusEnum.ProcessSuccess;
						});
						
					},
					() => Status != StatusEnum.EmptyProjectPath && Status != StatusEnum.InvalidDroidProject && Status != StatusEnum.Processing);
				}
				return _processCommand;
			}
		}



	}
}

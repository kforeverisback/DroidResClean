using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;

namespace DriodResClean
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		ViewModel.MainViewModel _mvm = new ViewModel.MainViewModel();
		public MainWindow()
		{
			InitializeComponent();
			this.DataContext = _mvm;

			_mvm.DataItems.Add(new Model.MainModel() { FileFullPath = @"D:\SCMWorkspace2\SamsungCameraManager\res\drawable\manager_divider_ab.9.png" });
			_mvm.DataItems.Add(new Model.MainModel() { FileFullPath = @"D:\SCMWorkspace2\SamsungCameraManager\res\drawable\list_divider_holo_dark.9.png" });
			_mvm.DataItems.Add(new Model.MainModel() { FileFullPath = @"D:\SCMWorkspace2\SamsungCameraManager\res\drawable\mobilelink_link_icon.png" });
			_mvm.DataItems.Add(new Model.MainModel() { FileFullPath = @"D:\SCMWorkspace2\SamsungCameraManager\res\drawable\rvf_link_icon.png" });	
		}
		
		private async void textBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			string path = (sender as TextBox).Text;

			if (string.IsNullOrEmpty(path))
				_mvm.IsAndroidProject = null;
			else
				_mvm.IsAndroidProject = File.Exists(path + "\\AndroidManifest.xml") && Directory.Exists(path + "\\src") && Directory.Exists(path + "\\res");

		}
	}
}

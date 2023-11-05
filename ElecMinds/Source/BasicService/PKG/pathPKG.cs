using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
/*
 * Filter format is "Display Text|File Extension",
 * and multiple filters are separated by a vertical bar "|".
 */
/*
GetFrame(0): 
	Retrieves the current stack frame, 
	which is the frame where the GetFrame() method is called.
GetFrame(1): 
	Retrieves the previous stack frame, 
	which is the frame that called the method 
	where the GetFrame() method is called.
GetFrame(n): 
	Retrieves the stack frame at the specified index n, 
	where n is a positive integer greater than or equal to 0. 
	The index indicates how many frames back in the call stack 
	you want to retrieve.
 */



namespace PKG
{
	public static class pathPKG
	{
		/// <summary>
		/// Get directory from current file going back some level
		/// </summary>

		public static string GetMainDir()
		{
			string mainFilePath = null;
			StackTrace stackTrace = new StackTrace(true);
			StackFrame[] frames = stackTrace.GetFrames();

			for (int i = 0; i < frames.Length; i++)
			{
				Debug.WriteLine("i: " + i);

				if (frames[i].GetFileName() == null)
				{
					break;
				}
				else
				{
					mainFilePath = frames[i].GetFileName();
					Debug.WriteLine("file name: {" + frames[i].GetFileName() + "}");
				}
			}
			string directory = Path.GetDirectoryName(mainFilePath);
			
			Debug.WriteLine("Base path: {" + mainFilePath + "}");
			Debug.WriteLine("Base Directory: {" + directory + "}");
			return directory;
		}

		public static string GetDirWithBackstep(int backSteps)
		{
			StackFrame frame = new StackTrace(true).GetFrame(backSteps);
			string fileName = frame.GetFileName();
			string location = Path.GetDirectoryName(fileName);
			
			Logger.WriteLine("File name: " + fileName);
			Logger.WriteLine("File location: " + location);
			return location;
		}
		/// <summary>
		/// Get the directory of current file
		/// </summary>
		public static string getCallerDir() 
		{
			return GetDirWithBackstep(1);
		}
		
		const string NULL_PATH = "NULL";
		public static string? SelectDirectoryByDialog(string title = "Select a folder")
		{
			// 創建 FolderBrowserDialog 物件
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

			// 設置對話框的標題和描述
			folderBrowserDialog.Description = title;
			folderBrowserDialog.ShowNewFolderButton = true;

			// 顯示彈出式對話框，並檢查使用者是否選擇了目錄
			if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				// 使用者選擇的目錄路徑
				string selectedPath = folderBrowserDialog.SelectedPath;

				// 在這裡可以使用選擇的目錄路徑進行後續處理
				Logger.WriteLine("Selected Directory： " + selectedPath);
				return selectedPath;
			}
			else
			{
				return null;
			}
		}
		public static string? SelectFileByDialog(
			string title = "Select a text file",
			string filter = "Text file (*.txt)|*.txt|All file (*.*)|*.*",
			string initDir = @"C:\")
		{
			// 创建 OpenFileDialog 对象
			OpenFileDialog openFileDialog = new OpenFileDialog();

			// 设置对话框的标题和初始目录
			openFileDialog.Title = title;
			openFileDialog.InitialDirectory = initDir;

			// 设置对话框的过滤条件
			openFileDialog.Filter = filter;

			// 打开对话框并检查用户是否选择了文件
			DialogResult result = openFileDialog.ShowDialog();

			if (result == DialogResult.OK)
			{
				// 获取用户选择的文件路径
				string filePath = openFileDialog.FileName;

				// 处理选中的文件路径
				Logger.WriteLine("Selected file path：" + filePath);
				return filePath;

			}
			else
			{
				// 用户取消了选择文件
				Logger.WriteLine("Selecting cancelled");
				return null;
			}
		}


		public static string GetComputerName()
		{
			string computerName = Environment.MachineName;
			Logger.WriteLine($"{computerName}");
			return computerName;
		}
	}

}

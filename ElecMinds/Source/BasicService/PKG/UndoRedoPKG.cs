using System.Collections.Generic;
namespace PKG
{
	/// <summary>
	/// Undo-able command interface
	/// </summary>
	public interface ICommand
	{
		void Execute();
		void Undo();
	}

	/// <summary>
	/// Record commands in Stacks for Redo & Undo
	/// </summary>
	public class UndoRedoManager
	{
		protected Stack<ICommand> _undoStack = new Stack<ICommand>();
		protected Stack<ICommand> _redoStack = new Stack<ICommand>();

		public void Execute(ICommand command)
		{
			command.Execute();
			_undoStack.Push(command);
			_redoStack.Clear(); // Clear redoStack when a new command is executed
		}

		public void Undo()
		{
			if (_undoStack.Count > 0)
			{
				ICommand command = _undoStack.Pop();
				command.Undo();
				_redoStack.Push(command);
			}
		}

		public void Redo()
		{
			if (_redoStack.Count > 0)
			{
				ICommand command = _redoStack.Pop();
				command.Execute();
				_undoStack.Push(command);
			}
		}
	}

	#region Demo
	/// <summary>
	/// Demo
	/// </summary>
	/// 定义一个继承 ICommand 的示例类 MyCommand
	public class MyCommand : ICommand
	{
		private string _text;

		public MyCommand(string text)
		{
			_text = text;
		}

		public void Execute() => Logger.WriteLine("Executing command: " + _text);

		public void Undo() => Logger.WriteLine("Undoing command: " + _text);
	}

	// 示例函数演示 UndoRedoManager 的使用
	public static class DemoUndoRedo
	{
		public static void Run()
		{
			UndoRedoManager undoRedoManager = new UndoRedoManager();

			// 创建一些示例命令
			ICommand command1 = new MyCommand("Command 1");
			ICommand command2 = new MyCommand("Command 2");
			ICommand command3 = new MyCommand("Command 3");

			// 执行命令并演示撤销和重做操作
			undoRedoManager.Execute(command1);
			undoRedoManager.Execute(command2);
			undoRedoManager.Undo();
			undoRedoManager.Redo();
			undoRedoManager.Execute(command3);
			undoRedoManager.Undo();
		}
	}
	#endregion
}

using System.Collections.Generic;

#region Dependency
using InteractionDirecting.API;
#endregion

namespace InteractionDirecting.Application
{


	/// <summary>
	/// Record commands in Stacks for Redo & Undo
	/// </summary>
	
	
	public class UndoRedoDirector
	{
		protected Stack<ICommandWithUndo> _undoStack = new Stack<ICommandWithUndo>();
		protected Stack<ICommandWithUndo> _redoStack = new Stack<ICommandWithUndo>();

		public void Execute(ICommandWithUndo command)
		{
			command.Execute();
			_undoStack.Push(command);
			_redoStack.Clear(); // Clear redoStack when a new command is executed
		}

		public void Undo()
		{
			if (_undoStack.Count > 0)
			{
				ICommandWithUndo command = _undoStack.Pop();
				command.Undo();
				_redoStack.Push(command);
			}
		}

		public void Redo()
		{
			if (_redoStack.Count > 0)
			{
				ICommandWithUndo command = _redoStack.Pop();
				command.Execute();
				_undoStack.Push(command);
			}
		}
	}

}

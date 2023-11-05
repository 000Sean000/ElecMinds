using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

#region Dependency
//using BasicService.API;
using InteractionDirecting.Application;

#endregion

namespace InteractionDirecting.API
{
	// OHS: Open Host Service,
	// let API interface lay in application layer,
	// OHS implementation can be in infrastructure layer
	public interface IInteractionAPI
	{
		#region Event Bus
		public void EBusSubscribe<TEvent>(Action<TEvent> handler);
		public void EBusUnsubscribe<TEvent>(Action<TEvent> handler);
		public void EBusPublish<TEvent>(TEvent eventToPublish);
		#endregion
		#region CQRS

		#endregion
		#region Undo-Redo-Director
		public void Execute(ICommandWithUndo command);
		public void Undo();
		public void Redo();
		#endregion
	}
	#region CQRS
	public interface ICommand { }

	public interface IQuery<TResult> { }

	public interface ICommandHandler<TCommand> where TCommand : ICommand
	{
		void Handle(TCommand command);
	}

	public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
	{
		TResult Handle(TQuery query);
	}
	#endregion
	#region Undo/Redo Director
	public interface ICommandWithUndo // Undo-able Action
	{
		void Execute();
		void Undo();
	}
	#endregion

	public class InteractionAPI : IInteractionAPI
	{
		protected EventBus EBus { get; set; }
		protected CommandQueryBus CQBus { get; set; }
		protected UndoRedoDirector Director { get; set; }

		public InteractionAPI(IServiceProvider serviceProvider)
		{

			EBus = (EventBus)serviceProvider.GetServices<EventBus>();
			CQBus = (CommandQueryBus)serviceProvider.GetServices<CommandQueryBus>(); ;
			Director = (UndoRedoDirector)serviceProvider.GetServices<UndoRedoDirector>(); ;
			///Director = new UndoRedoDirector(); // multi-director for separate field/vault/scope


		}
		#region Event Bus
		public void EBusSubscribe<TEvent>(Action<TEvent> handler)
		{
			EBus.Subscribe<TEvent>(handler);
		}
		public void EBusUnsubscribe<TEvent>(Action<TEvent> handler)
		{
			EBus.Unsubscribe<TEvent>(handler);
		}
		public void EBusPublish<TEvent>(TEvent eventToPublish)
		{
			EBus.Publish<TEvent>(eventToPublish);
		}
		#endregion
		#region CQRS

		#endregion
		#region Undo-Redo-Director
		public void Execute(ICommandWithUndo command)
		{
			Director.Execute(command);
		}
		public void Undo()
		{
			Director.Undo();
		}
		public void Redo()
		{
			Director.Redo();
		}
		#endregion



	}

}

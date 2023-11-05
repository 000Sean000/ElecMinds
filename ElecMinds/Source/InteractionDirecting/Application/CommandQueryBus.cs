
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InteractionDirecting;
#region Dependency

using InteractionDirecting.API;
#endregion
namespace InteractionDirecting.Application
{
	public class CommandQueryBus
	{
		private readonly IServiceProvider _serviceProvider;

		public CommandQueryBus(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public void Send<TCommand>(TCommand command) where TCommand : ICommandWithUndo
		{
			var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
			var handler = _serviceProvider.GetService(handlerType);
			if (handler != null)
			{
				var handleMethod = handlerType.GetMethod("Handle");
				handleMethod.Invoke(handler, new object[] { command });
			}
			else
			{
				throw new InvalidOperationException($"Handler for {command.GetType()} not found.");
			}
		}

		public TResult Send<TQuery, TResult>(TQuery query) where TQuery : IQuery<TResult>
		{
			var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
			var handler = _serviceProvider.GetService(handlerType);
			if (handler != null)
			{
				var handleMethod = handlerType.GetMethod("Handle");
				return (TResult)handleMethod.Invoke(handler, new object[] { query });
			}
			else
			{
				throw new InvalidOperationException($"Handler for {query.GetType()} not found.");
			}
		}
	}


}

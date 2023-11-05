using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

#region Dependency
using BasicService.API;
using NoteTaking.API;
using NoteTaking.Domain;
using NoteTaking.Application;
using NoteTaking.Infrastructure;
using InteractionDirecting.API;
using InteractionDirecting.Application;

#endregion

namespace ServiceOrchestrating.Application
{
	public class ServiceRegistrar
	{
		IServiceCollection ServiceCollection { get; set; }
		IServiceProvider ServiceProvider { get; set; }
		public ServiceRegistrar()
		{
			ServiceCollection = BasicAPI.ServiceCollection;
			ServiceProvider = BasicAPI.ServiceProvider;

			#region Interactoin Directing
			ServiceCollection.AddSingleton<IInteractionAPI,InteractionAPI>();
			ServiceCollection.AddSingleton<EventBus>();
			ServiceCollection.AddSingleton<CommandQueryBus>();
			ServiceCollection.AddSingleton<UndoRedoDirector>();
			#endregion

			#region Note Taking
			ServiceCollection.AddSingleton<INodeEditorAPI, NodeEditorAPI>();
			ServiceCollection.AddSingleton<INodeRepository, NodeRepository>();
			ServiceCollection.AddSingleton<NodeDomainService>();
			ServiceCollection.AddSingleton<NodeApplicationService>();

			#endregion

			#region

			#endregion

			#region
			#endregion

			#region
			#endregion

			#region
			#endregion
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

#region Dependency

using Config;
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
			ServiceCollection.AddSingleton<IInteractionAPI, InteractionAPI>();
			ServiceCollection.AddSingleton<EventBus>();
			ServiceCollection.AddSingleton<CommandQueryBus>();
			ServiceCollection.AddSingleton<UndoRedoDirector>();
			#endregion

			#region Note Taking
			ServiceCollection.AddSingleton<INeuronEditorAPI, NeuronEditorAPI>();
			ServiceCollection.AddSingleton<INeuronRepository, NeuronRepository>();
			///ServiceCollection.AddDbContext<SQLiteNeuronDbContext>(serviceProvider =>{return new SQLiteNeuronDbContext(""); });
			ServiceCollection.AddSingleton<NeuronDomainService>();
			ServiceCollection.AddSingleton<NeuronApplicationService>();

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

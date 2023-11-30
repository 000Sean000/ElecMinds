using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.Extensions.DependencyInjection;



#region Dependency

using Config;
using NoteTaking.Domain;

#endregion

namespace NoteTaking.Infrastructure
{
	public class NeuronDbContext : DbContext
	{
		public DbSet<NeuronData> NeuronData { get; set; }
		public DbSet<LinkData> LinkData { get; set; }
		public DbSet<ReferenceData> ReferenceData { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite("Data Source=NeuronRepo.db;");
		}
	}

	public class SQLiteNeuronRepository: INeuronRepository
	{
		protected readonly IServiceProvider _serviceProvider;
		protected readonly NeuronDbContext _dbContext = new NeuronDbContext();
		public SQLiteNeuronRepository(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;

		}
		public Neuron CreateNeuron()
		{			
			return new Neuron(new NeuronData());////
		}
		public TID CreateLinkInNeuron(TID neuronId)
		{
			Neuron neuron = FetchNeuron(neuronId);
			
			return (TID)new Link(new LinkData()).Id;////
		}
		public TID CreateReferenceInNeuron(TID neuronId)
		{
			return (TID)new Reference(new ReferenceData()).Id;////
		}
		public Neuron FetchNeuron(TID neuronId)
		{
			return new Neuron(new NeuronData());////
		}
		public void DeleteNeuron(TID neuronId)
		{

		}
		public void RecoverNeuron(NeuronData neuronData)
		{

		}
		protected Neuron LoadNeuron(TID neuronId)
		{
			return new Neuron(new NeuronData());////
		}
		protected void SaveNeuron(TID neuronId)
		{

		}
	}




}

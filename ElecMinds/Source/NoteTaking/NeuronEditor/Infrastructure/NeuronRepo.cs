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
	public class SQLiteNeuronDbContext : DbContext
	{
		protected readonly string _connectionString;
		public DbSet<NodeData> NodeData { get; set; }
		public DbSet<LinkData> LinkData { get; set; }
		public DbSet<ReferenceData> ReferenceData { get; set; }
		public SQLiteNeuronDbContext(string connectionString)
		{
			_connectionString = connectionString;
		}
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite($"Data Source={_connectionString}");
		}
	}

	public class NeuronRepository: INeuronRepository 
	{
		protected readonly IServiceProvider _serviceProvider;
		protected TDbContext? _context;
		
		public NeuronRepository(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;

		}
		public void ConnectStudioDB(string studioDBPath)
		{
			string connectionString = $"{studioDBPath}";
			_context = new TDbContext(connectionString);
		}

		public Neuron CreateNeuron()
		{			
			NodeData nodeData = new NodeData();
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
		public void SaveChanges()
		{

		}
	}




}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Proxies;
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

		public DbSet<IdPool> IDPools { get; set; }
		public DbSet<UnusedNeuronId> UnusedNeuronIds { get; set; }
		public DbSet<UnusedLinkId> UnusedLinkIds { get; set; }
		public DbSet<UnusedReferenceId> UnusedReferenceIds { get; set; }

		public DbSet<Neuron> Neurons { get; set; }
		public DbSet<LinkData> LinkData { get; set; }
		public DbSet<ReferenceData> ReferenceData { get; set; }
		public SQLiteNeuronDbContext(string connectionString)
		{
			_connectionString = connectionString;
		}
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder
				.UseLazyLoadingProxies() // Enable lazy loading proxies
				.UseSqlite($"Data Source={_connectionString}");
		}
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// Specifying table names
			modelBuilder.Entity<UnusedNeuronId>().ToTable("UnusedNeuronIds");
			modelBuilder.Entity<UnusedLinkId>().ToTable("UnusedLinkIds");
			modelBuilder.Entity<UnusedReferenceId>().ToTable("UnusedReferenceIds");

			modelBuilder.Entity<Neuron>()
				.ToTable("Neurons")
				.Ignore(n => n.IsLoadingDB)
				.HasKey(n => n.Id);
			modelBuilder.Entity<Neuron>()
				.HasMany(n => n.OutLinkData)
				.WithOne()
				.HasForeignKey(l => l.SourceNeuronId)
				.HasForeignKey(l => l.TargetNeuronId);
			modelBuilder.Entity<Neuron>()
				.HasMany(n => n.OutReferenceData)
				.WithOne()
				.HasForeignKey(r => r.SourceNeuronId);

			modelBuilder.Entity<LinkData>()
				.ToTable("Links")
				.HasKey(x => x.Id);
			
			modelBuilder.Entity<ReferenceData>()
				.ToTable("References")
				.HasKey(x => x.Id);

			// ... Other configurations ...
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
			
		}
		public TID CreateLinkInNeuron(TID neuronId)
		{
			Neuron neuron = FetchNeuron(neuronId);
			
			
		}
		public TID CreateReferenceInNeuron(TID neuronId)
		{
			
		}
		public Neuron FetchNeuron(TID neuronId)
		{
			
		}
		public Neuron FetchSourceNeuronOfLink(TID LinkId)
		{

		}
		public Neuron FetchSourceNeuronOfReference(TID ReferenceId)
		{

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
	public class IdPool
	{
		public TID Id { get; set; } // Primary key for the IDPool table

		// Last used IDs
		public TID LastNeuronId { get; set; }
		public TID LastLinkId { get; set; }
		public TID LastReferenceId { get; set; }

		// Relationships to Unused IDs using List<T>
		public List<UnusedNeuronId> UnusedNeuronIds { get; set; }
		public List<UnusedLinkId> UnusedLinkIds { get; set; }
		public List<UnusedReferenceId> UnusedReferenceIds { get; set; }

		public IdPool()
		{
			UnusedNeuronIds = new List<UnusedNeuronId>();
			UnusedLinkIds = new List<UnusedLinkId>();
			UnusedReferenceIds = new List<UnusedReferenceId>();
		}
	}
	public class UnusedId
	{
		public TID? Id { get; set; } // Primary key
		public TID? IdValue { get; set; } // unused id value
		public TID? IDPoolId { get; set; } // Foreign key to IDPool
		// Navigation property back to IDPool
		public IdPool? IdPool { get; set; }
	}
	public class UnusedNeuronId: UnusedId { }
	public class UnusedLinkId: UnusedId { }
	public class UnusedReferenceId: UnusedId { }



}

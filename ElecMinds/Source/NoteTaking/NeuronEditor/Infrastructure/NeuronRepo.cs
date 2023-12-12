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

		public DbSet<IdPool> IdPools { get; set; }
		public DbSet<TrashCan> TrashCans { get; set; }
		public DbSet<UnusedNeuronId> UnusedNeuronIds { get; set; }
		public DbSet<UnusedLinkId> UnusedLinkIds { get; set; }
		public DbSet<UnusedReferenceId> UnusedReferenceIds { get; set; }

		public DbSet<Neuron> Neurons { get; set; }
		public DbSet<LinkData> LinkDatas { get; set; }
		public DbSet<ReferenceData> ReferenceDatas { get; set; }
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

			modelBuilder.Entity<Neuron>().ToTable("Neurons")
				.Ignore(n => n.IsLoadingDB)
				.HasKey(n => n.Id);
			modelBuilder.Entity<Neuron>()
				.HasMany(n => n.OutLinkDatas)
				.WithOne()
				.HasForeignKey(l => l.SourceNeuronId)
				.OnDelete(DeleteBehavior.Restrict)
				.HasForeignKey(l => l.TargetNeuronId)
				.OnDelete(DeleteBehavior.Restrict);
			modelBuilder.Entity<Neuron>()
				.HasMany(n => n.OutReferenceDatas)
				.WithOne()
				.HasForeignKey(r => r.SourceNeuronId)
				.HasForeignKey(r => r.TargetNeuronId);

			modelBuilder.Entity<LinkData>().ToTable("Links")
				.HasKey(x => x.Id);
			
			modelBuilder.Entity<ReferenceData>().ToTable("References")
				.HasKey(x => x.Id);

			// ... Other configurations ...
		}

	}
	

	public class NeuronRepository: INeuronRepository 
	{
		protected readonly IServiceProvider _serviceProvider;
		protected TDbContext? _context;
		protected IdPool _idPool;
		protected TrashCan _trashCan;
		
		public NeuronRepository(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;

		}
		public void ConnectStudioDB(string studioDBPath)
		{
			string connectionString = $"{studioDBPath}";
			_context = new TDbContext(connectionString);
			_idPool = _context.IdPools.First();
			_trashCan = _context.TrashCans.First();
		}

		public Neuron CreateNeuron()
		{
			TID newNeuronId = _idPool.AcquireNeuronId();
			Neuron neuron = new Neuron { Id = newNeuronId };
			_context.Neurons.Add(neuron);
			return neuron;
		}
		public LinkData CreateLinkInNeuron(TID neuronId)
		{
			Neuron neuron = FetchNeuron(neuronId);
			TID linkId = _idPool.AcquireLinkId();
			LinkData linkData = new LinkData() { Id = linkId, SourceNeuronId = neuronId};

			return linkData;
		}
		public ReferenceData CreateReferenceInNeuron(TID neuronId)
		{
			Neuron neuron = FetchNeuron(neuronId) ;
			TID referenceId = _idPool.AcquireReferenceId() ;
			ReferenceData referenceData = new ReferenceData() { Id = referenceId, SourceNeuronId = neuronId };
			
			return referenceData;
		}
		public Neuron FetchNeuron(TID neuronId)
		{
			// Check if the Neuron is already being tracked
			var neuron = _context.Neurons.Local.FirstOrDefault(n => n.Id == neuronId);

			if (neuron == null)
			{
				// Neuron not in local cache, fetch from database
				
				neuron = _context.Neurons.FirstOrDefault(n => n.Id == neuronId);
			}
			return neuron;
		}
		protected LinkData FetchLink(TID linkId)
		{
			return _context.LinkDatas.Local.FirstOrDefault(l => l.Id == linkId); 
		}
		protected ReferenceData FetchReference(TID referenceId)
		{
			return _context.ReferenceDatas.Local.FirstOrDefault(r => r.Id == referenceId);
		}
		public Neuron FetchSourceNeuronOfLink(TID linkId)
		{
			var linkData = FetchLink(linkId) ;
			if (linkData == null)
			{
				linkData = _context.LinkDatas.FirstOrDefault(l => l.Id == linkId);
			}
			return FetchNeuron((TID)linkData.SourceNeuronId);
		}
		public Neuron FetchSourceNeuronOfReference(TID referenceId)
		{
			var referenceData = FetchReference(referenceId) ;
			if (referenceData == null)
			{
				referenceData = _context.ReferenceDatas.FirstOrDefault(r => r.Id == referenceId);
			}
			return FetchNeuron((TID)referenceData.SourceNeuronId);
		}
		public void DeleteNeuron(TID neuronId) 
		{
			Neuron neuron = FetchNeuron(neuronId);
			_trashCan.Feed<Neuron>(neuron, _context.Neurons, _trashCan.Neurons);
			// need cascading delete
		}
		public void DeleteLink(TID linkId)
		{
			LinkData link = FetchLink(linkId) ;
			_trashCan.Feed<LinkData>(link, _context.LinkDatas, _trashCan.LinkDatas);
		}
		public void DeleteReference(TID referenceId)
		{
			ReferenceData reference = FetchReference(referenceId) ;
			_trashCan.Feed<ReferenceData>(reference, _context.ReferenceDatas, _trashCan.ReferenceDatas);
		}
		public Neuron RecoverNeuron(TID neuronId)
		{
			Neuron neuron = _trashCan.Neurons.FirstOrDefault(entity => entity.Id == neuronId);	
			_trashCan.Recover(neuron, _context.Neurons, _trashCan.Neurons);
			return neuron;
		}
		public void RecoverLink(TID linkId)
		{
			LinkData link = _trashCan.LinkDatas.FirstOrDefault(entity => entity.Id == linkId);
			_trashCan.Recover(link, _context.LinkDatas, _trashCan.LinkDatas);
		}
		public void RecoverReference(TID referenceId)
		{
			ReferenceData reference = _trashCan.ReferenceDatas.FirstOrDefault(entity => entity.Id == referenceId);
			_trashCan.Recover(reference, _context.ReferenceDatas, _trashCan.ReferenceDatas);
		}
		public void ClearTrashCan()
		{
			foreach(var neuron in _trashCan.Neurons)
			{
				_idPool.ReleaseNeuronId((TID)neuron.Id);
				_trashCan.Neurons.Remove(neuron);
			}
			foreach(var link in _trashCan.LinkDatas)
			{
				_idPool.ReleaseLinkId((TID)link.Id);
				_trashCan.LinkDatas.Remove(link);
			}
			foreach(var reference in _trashCan.ReferenceDatas)
			{
				_idPool.ReleaseReferenceId((TID)reference.Id);
				_trashCan.ReferenceDatas.Remove(reference);
			}
		}
		public void SaveChanges()
		{
			_context.SaveChanges();
		}
	}
	public class TrashCan
	{
		public List<Neuron> Neurons { get; set; }
		public List<LinkData> LinkDatas { get; set; }
		public List<ReferenceData> ReferenceDatas { get; set; }
		public void Feed<TEntity>(TEntity entity, DbSet<TEntity> entities, List<TEntity> trashCanEntities) where TEntity : class
		{
			entities.Remove(entity);
			trashCanEntities.Add(entity);
		}
		public void Recover<TEntity>(TEntity entity, DbSet<TEntity> entities, List<TEntity> trashCanEntities) where TEntity : class
		{
			trashCanEntities.Remove(entity);
			entities.Add(entity);
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
		public TID AcquireNeuronId()
		{
			TID id;
			if (UnusedNeuronIds.Any())
			{
				id = UnusedNeuronIds.First().IdValue;
				UnusedNeuronIds.RemoveAt(0);
			}
			else
			{
				id = ++LastNeuronId;
			}
			return id;
		}
		public TID AcquireLinkId()
		{
			TID id;
			if (UnusedLinkIds.Any())
			{
				id = UnusedLinkIds.First().IdValue;
				UnusedLinkIds.RemoveAt(0);
			}
			else
			{
				id = ++LastLinkId;
			}
			return id;
		}
		public TID AcquireReferenceId()
		{
			TID id;
			if (UnusedReferenceIds.Any())
			{
				id = UnusedReferenceIds.First().IdValue;
				UnusedReferenceIds.RemoveAt(0);
			}
			else
			{
				id = ++LastReferenceId;
			}
			return id;
		}
		public void ReleaseNeuronId(TID id)
		{
			UnusedNeuronIds.Add(new UnusedNeuronId() { IdValue = id, IdPoolId = Id, IdPool = this});
		}
		public void ReleaseLinkId(TID id)
		{
			UnusedLinkIds.Add(new UnusedLinkId() { IdValue = id, IdPoolId = Id, IdPool = this });
		}
		public void ReleaseReferenceId(TID id)
		{
			UnusedReferenceIds.Add(new UnusedReferenceId() { IdValue = id, IdPoolId = Id, IdPool = this });
		}
	}
	public class UnusedId
	{
		public TID? Id { get; set; } // Primary key
		public TID IdValue { get; set; } // unused id value
		public TID? IdPoolId { get; set; } // Foreign key to IDPool
		// Navigation property back to IDPool
		public virtual IdPool? IdPool { get; set; }
	}
	public class UnusedNeuronId: UnusedId { }
	public class UnusedLinkId: UnusedId { }
	public class UnusedReferenceId: UnusedId { }



}

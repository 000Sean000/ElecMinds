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
using DTOs;
using Newtonsoft.Json;
using Enums;

#endregion

namespace NoteTaking.Infrastructure
{
	// properties which cannot be mapped will be ignore and have no warning!
	// List<PrimitiveType> => need conversion for serialization/deserialization with json string
	// List<ComplexType> => use OwnsMany() method to give it a respective table
	// ComplexType (Object) => use OwnsOne() mehtod to flatten to columns
	// Enum => int by default
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

			modelBuilder.Entity<Neuron>().ToTable("Neurons");
			modelBuilder.Entity<LinkData>().ToTable("Links");
			modelBuilder.Entity<ReferenceData>().ToTable("References");

			#region Neuron
			modelBuilder.Entity<Neuron>()
				.Ignore(n => n.IsLoadingDB)
				.HasKey(n => n.Id);
			modelBuilder.Entity<Neuron>()
				.OwnsOne(n => n.NoteData, noteData =>
				{
					noteData.OwnsMany(n => n.Segments);
				});
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
			modelBuilder.Entity<Neuron>()
				.Property(n => n.InLinkIds)
				.HasConversion(
					v => JsonConvert.SerializeObject(v),
					v => JsonConvert.DeserializeObject<List<TID>>(v) ?? new List<TID>()
				);
			modelBuilder.Entity<Neuron>()
				.Property(n => n.InReferenceIds)
				.HasConversion(
					v => JsonConvert.SerializeObject(v),
					v => JsonConvert.DeserializeObject<List<TID>>(v) ?? new List<TID>()
				);

			#endregion
			#region Link
			modelBuilder.Entity<LinkData>()
				.HasKey(l => l.Id);
			modelBuilder.Entity<LinkData>()
				.Property(l => l.LinkInfo)
				.HasConversion(
					v => JsonConvert.SerializeObject(v),
					v => JsonConvert.DeserializeObject<Dictionary<ELinkInfoIndex, string>>(v) ?? new Dictionary<ELinkInfoIndex, string>()
					);
			#endregion
			#region Reference
			modelBuilder.Entity<ReferenceData>()
				.HasKey(r => r.Id);
			#endregion
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
		
		public LinkData CreateLink()
		{
			TID linkId = _idPool.AcquireLinkId();
			LinkData linkData = new LinkData() { Id = linkId };
			_context.LinkDatas.Add(linkData);
			return linkData;
		}
		public LinkData CreateLinkInNeuron(TID neuronId)
		{
			Neuron neuron = FetchNeuron(neuronId);
			TID linkId = _idPool.AcquireLinkId();
			LinkData linkData = new LinkData() { Id = linkId, SourceNeuronId = neuronId};
			_context.LinkDatas.Add(linkData);
			return linkData;
		}
		public ReferenceData CreateReference()
		{
			TID referenceId = _idPool.AcquireReferenceId() ;
			ReferenceData referenceData = new ReferenceData() { Id = referenceId };
			_context.ReferenceDatas.Add(referenceData);
			return referenceData;
		}
		public ReferenceData CreateReferenceInNeuron(TID neuronId)
		{
			Neuron neuron = FetchNeuron(neuronId);
			TID referenceId = _idPool.AcquireReferenceId();
			ReferenceData referenceData = new ReferenceData() { Id = referenceId, SourceNeuronId = neuronId };
			_context.ReferenceDatas.Add(referenceData);
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
		public LinkData FetchLink(TID linkId)
		{
			return _context.LinkDatas.Local.FirstOrDefault(l => l.Id == linkId);
		}
		public ReferenceData FetchReference(TID referenceId)
		{
			return _context.ReferenceDatas.Local.FirstOrDefault(r => r.Id == referenceId);
		}
		public void DeleteNeuron(TID neuronId) 
		{
			Neuron neuron = FetchNeuron(neuronId);
			_trashCan.Feed(neuron); 
			// need cascading delete
		}
		public void DeleteLink(TID linkId)
		{
			LinkData link = FetchLink(linkId) ;
			_trashCan.Feed(link);
		}
		public void DeleteReference(TID referenceId)
		{
			ReferenceData reference = FetchReference(referenceId) ;
			_trashCan.Feed(reference);
		}
		public Neuron RecoverNeuron(TID neuronId)
		{
			Neuron neuron = _trashCan.TrashNeurons.FirstOrDefault(entity => entity.Id == neuronId);	
			_trashCan.Recover(neuron);
			return neuron;
		}
		public void RecoverLink(TID linkId)
		{
			LinkData linkData = _trashCan.TrashLinkDatas.FirstOrDefault(entity => entity.Id == linkId);
			_trashCan.Recover(linkData);
			Neuron neuron = FetchSourceNeuronOfLink(linkId) ;
			neuron.AddLink(linkId, linkData);
		}
		public void RecoverReference(TID referenceId)
		{
			ReferenceData referenceData = _trashCan.TrashReferenceDatas.FirstOrDefault(entity => entity.Id == referenceId);
			_trashCan.Recover(referenceData);
			Neuron neuron = FetchSourceNeuronOfReference(referenceId);
			neuron.AddReference(referenceId, referenceData);
		}
		public void ClearTrashCan()
		{
			foreach(var neuron in _trashCan.TrashNeurons)
			{
				_idPool.ReleaseNeuronId((TID)neuron.Id);
				_trashCan.TrashNeurons.Remove(neuron);
			}
			foreach(var link in _trashCan.TrashLinkDatas)
			{
				_idPool.ReleaseLinkId((TID)link.Id);
				_trashCan.TrashLinkDatas.Remove(link);
			}
			foreach(var reference in _trashCan.TrashReferenceDatas)
			{
				_idPool.ReleaseReferenceId((TID)reference.Id);
				_trashCan.TrashReferenceDatas.Remove(reference);
			}
		}
		public void SaveChanges()
		{
			_context.SaveChanges();
		}
	}
	public class TrashCan
	{
		protected DbSet<Neuron> _repoNeurons { get; set; }
		protected DbSet<LinkData> _repoLinkDatas { get; set; }
		protected DbSet<ReferenceData> _repoReferenceDatas { get; set; }
		public List<Neuron> TrashNeurons { get; set; }
		public List<LinkData> TrashLinkDatas { get; set; }
		public List<ReferenceData> TrashReferenceDatas { get; set; }
		public TrashCan(DbSet<Neuron> repoNeurons, DbSet<LinkData> repoLinkDatas, DbSet<ReferenceData> repoReferenceDatas)
		{
			_repoNeurons = repoNeurons;
			_repoLinkDatas = repoLinkDatas;
			_repoReferenceDatas = repoReferenceDatas;
		}
		public void Feed(Neuron neuron)
		{
			_repoNeurons.Remove(neuron);
			TrashNeurons.Add(neuron);

		}
		public void Feed(LinkData linkData)
		{
			_repoLinkDatas.Remove(linkData);
			TrashLinkDatas.Add(linkData);
		}
		public void Feed(ReferenceData referenceData)
		{
			_repoReferenceDatas.Remove(referenceData);
			TrashReferenceDatas.Add(referenceData);
		}
		public void Recover(Neuron neuron)
		{
			TrashNeurons.Remove(neuron);
			_repoNeurons.Add(neuron);
		}
		public void Recover(LinkData linkData)
		{
			TrashLinkDatas.Remove(linkData);
			_repoLinkDatas.Add(linkData);
		}
		public void Recover(ReferenceData referenceData)
		{
			TrashReferenceDatas.Remove(referenceData);
			_repoReferenceDatas.Add(referenceData);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#region Type Config
using TypeID = System.Int64;
using TypeDbContext = NoteTaking.Infrastructure.SQLiteNeuronDbContext;
#endregion
namespace Config
{
	public class TDbContext:TypeDbContext
	{
		public TDbContext(string connectionString) : base(connectionString) { }
	}
	public struct TID
	{
		private TypeID _value;

		public TID(TypeID value)
		{
			_value = value;
		}

		public TypeID Value => _value;

		// Implicit conversion operator from GenericWrapper to TypeID
		public static implicit operator TypeID(TID wrapper) => wrapper._value;

		// Implicit conversion operator from TypeID to GenericWrapper
		public static implicit operator TID(TypeID value) => new TID(value);

		// Override ToString() method
		public override string ToString() => _value.ToString();

		// Override Equals method
		public override bool Equals(object obj)
		{
			if (obj is TID wrapper)
			{
				return EqualityComparer<TypeID>.Default.Equals(_value, wrapper._value);
			}
			return false;
		}

		

		// Equality operators
		public static bool operator ==(TID left, TID right) => left.Equals(right);
		public static bool operator !=(TID left, TID right) => !(left == right);
	}
}

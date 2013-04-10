namespace Elevated.Data
{
	using System;
	using System.Data.Common;

	public interface IEntity
	{
		void PopulateFromReader(DbDataReader reader);
	}
}

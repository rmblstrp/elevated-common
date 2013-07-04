namespace NHibernate.Transform
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public static class TransformersContrib
	{
		private static Dictionary<Type, AliasToObjectResultTransformer> aliasToObjectInstances = new Dictionary<Type, AliasToObjectResultTransformer>();

		public static IResultTransformer AliasToObject<T>()
		{
			return AliasToObject(typeof(T));
		}

		public static IResultTransformer AliasToObject(Type type)
		{
			if (aliasToObjectInstances.ContainsKey(type))
			{
				return aliasToObjectInstances[type];
			}

			var transformer = new AliasToObjectResultTransformer(type);

			aliasToObjectInstances.Add(type, transformer);

			return transformer;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Exceptions;
using NHibernate.Transform;

public static class SessionExtensions
{
	public static void Delete<T>(this ISession session, object id)
	{
		session.Delete(session.Load<T>(id));
	}

	public static IList<T> GetColumns<T>(this ISession session, string[] columns, params ICriterion[] restrictions) where T : class
	{
		return GetColumns<T, T>(session, columns, restrictions);
	}

	public static IList<TResult> GetColumns<TEntity, TResult>(this ISession session, string[] columns, params ICriterion[] restrictions)
		where TEntity : class
		where TResult : class
	{
		var dictionary = new Dictionary<string, string>(columns.Length);

		foreach (var item in columns)
		{
			dictionary.Add(item, item);
		}

		return GetColumns<TEntity, TResult>(session, dictionary, restrictions);
	}

	public static IList<T> GetColumns<T>(this ISession session, Dictionary<string, string> columns, params ICriterion[] restrictions) where T : class
	{
		return GetColumns<T, T>(session, columns, restrictions);
	}

	public static IList<TResult> GetColumns<TEntity, TResult>(this ISession session, Dictionary<string, string> columns, params ICriterion[] restrictions)
		where TEntity : class
		where TResult : class
	{
		var type = typeof(TResult);

		var criteria = session.CreateCriteria<TEntity>();

		for (int index = 0; index < restrictions.Length; index++)
		{
			criteria.Add(restrictions[index]);
		}

		var list = Projections.ProjectionList();

		foreach (var kv in columns)
		{
			list.Add(Projections.Property(kv.Key), kv.Value);
		}

		criteria.SetProjection(list);
		
		criteria.SetResultTransformer(TransformersContrib.AliasToObject<TResult>());

		return criteria.List<TResult>();
	}
}

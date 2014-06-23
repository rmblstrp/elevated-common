namespace Elevated.Web
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Routing;

	public static class ViewUtility
	{
		private class UtilityController : Controller
		{
		}

		public static T CreateController<T>(RouteData routeData = null) where T : Controller, new()
		{
			// create a disconnected controller instance
			T controller = new T();

			// get context wrapper from HttpContext if available
			HttpContextBase wrapper;
			if (System.Web.HttpContext.Current != null)
			{
				wrapper = new HttpContextWrapper(System.Web.HttpContext.Current);
			}
			else
			{
				throw new InvalidOperationException("Can't create Controller Context if no active HttpContext instance is available.");
			}

			if (routeData == null)
			{
				routeData = new RouteData();
			}

			// add the controller routing if not existing
			if (!routeData.Values.ContainsKey("controller") && !routeData.Values.ContainsKey("Controller"))
			{
				routeData.Values.Add("controller", controller.GetType().Name.ToLower().Replace("controller", ""));
			}

			controller.ControllerContext = new ControllerContext(wrapper, routeData, controller);
			return controller;
		}

		public static string RenderView(string view, object model, bool partial = false)
		{
			return RenderView(CreateController<UtilityController>(), view, model, partial);
		}

		public static string RenderView(Controller controller, string view, object model, bool partial = false)
		{
			using (var stream = new StringWriter())
			{
				ViewEngineResult viewResult = null;

				if (partial)
				{
					viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, view);
				}
				else
				{
					viewResult = ViewEngines.Engines.FindView(controller.ControllerContext, view, null);
				}

				if (viewResult.View == null)
				{
					throw new FileNotFoundException("View cannot be found");
				}

				controller.ViewData.Model = model;

				var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, stream);
				
				viewResult.View.Render(viewContext, stream);

				return stream.GetStringBuilder().ToString();
			}
		}
	}
}

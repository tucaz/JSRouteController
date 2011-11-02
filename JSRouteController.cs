using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;

namespace JSUtils
{
    public class JSRouteController : Controller
    {
        [HttpGet]
        public ActionResult GetAll(string @namespace, string sufix)
        {
            var script = new StringBuilder();

            var routesPropertyName = @namespace;
            var actionFilterSufix = sufix;
            Func<string, string> transformActionName = (string name) => name.Replace(actionFilterSufix, String.Empty);
            Func<ActionDescriptor, string> wrapInFunctionCallWithParameters = (ActionDescriptor action) =>
            {
                var parameters = action.GetParameters().Select(x => x.ParameterName).ToList();
                var shouldAddParameters = parameters.Count > 0 && (action.GetCustomAttributes(true).Count() == 0 || action.GetCustomAttributes(true).Where(attr => attr.GetType() == typeof(HttpGetAttribute)).Count() > 0);
                var functionToWrap = "function({0}) {{ return '{1}'{2}; }}";

                return String.Format(functionToWrap,
                    shouldAddParameters ? String.Join(",", parameters) : String.Empty,
                    Url.Action(action.ActionName, action.ControllerDescriptor.ControllerName),
                    shouldAddParameters ? parameters.Aggregate("+'?", (acc, pmt) => String.Concat(acc, pmt, "=' + ", pmt)) : String.Empty
                    );
            };

            //Get all controller types from the executing assembly
            var allControllers = Assembly.GetExecutingAssembly().GetTypes().Where(x => typeof(ControllerBase).IsAssignableFrom(x));

            //For each one of them, grab all the actions that has the specified sufix and return their complete Url as a Javascript property
            var allActions = allControllers
                .Select(controller => new ReflectedControllerDescriptor(controller))
                .SelectMany(controller => controller.GetCanonicalActions()
                    .Where(action => action.ActionName.EndsWith(actionFilterSufix)))
                .GroupBy(action => action.ControllerDescriptor.ControllerName)
                .ToList();

            /* Put together all information from the matched controller actions and assembly a JS object like the below */
            /*                         
            namespace = {
                Controller1: {
                    Action1: function() { return '/Controller1/Action1'; }
                },
                Controller2: {
                    Action1: function() { return '/Controller2/Action1'; },
                    Action2WithPmts: function(pmt1) { return '/Controller2/Action2WithPmts' + '?pmt1=' + pmt1; }
                }
            }
            */

            script.AppendFormat("{0} = {{{1}}};",
                routesPropertyName,
                allActions.Aggregate(String.Empty, (controllers, group) =>
                    String.Concat(controllers, String.Format("{0}{1}: {{{2}}}",
                        controllers == String.Empty ? String.Empty : ",",
                        group.Key,
                        group.Aggregate(String.Empty, (actions, action) =>
                            String.Concat(actions, String.Format("{0}{1}: {2}",
                                actions == String.Empty ? String.Empty : ",",
                                transformActionName(action.ActionName),
                                wrapInFunctionCallWithParameters(action)))
                )))));

            return new JavaScriptResult { Script = script.ToString() };
        }

    }
}

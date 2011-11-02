Overview
========

A very simple plugin to generate routes dynamically for client side script (javascript) calls in ASP.NET MVC.

It scans all existing controllers in the executing assembly looking for actions that match the specified sufix and generates a javascript file to be included by calling client script.

To get started
==============

To get started simply drop the JSRouteController.cs into you Controllers folder in your ASP.NET MVC project and configure the desired route in global.asax.cs as follows:

	public static void RegisterRoutes(RouteCollection routes)
	{	
		routes.MapRoute(
			"JavascriptRoutes", //Route name
			"Scripts/Routes.js", //The desired URL to access the generated routes
			new { 
				controller = "JSRoute", 
				action = "GetAll", 
				@namespace = "MyAppRoutes", //Desired namespace to hold the routes
				sufix = "JSON"  //Sufix used to identify actions that should be included in the output script
			}
		);
		
		/* Other mapped routes */
	}
	
From this on all actions ending with "JSON" will be parsed and added to the output javascript

	public ActionResult MyActionJSON() { /* some code */ } 

Now you can include it in your view as you would with any other javascript library

	<script src="@Url.Content("~/Scripts/Routes.js")" type="text/javascript"></script>
	
To get a Javascript like this one

	MyAppRoutes = {
					Controller1: {
						Action1: function() { return '/Controller1/Action1'; }
					},
					Controller2: {
						Action1: function() { return '/Controller2/Action1'; },
						Action2WithPmts: function(pmt1) { return '/Controller2/Action2WithPmts' + '?pmt1=' + pmt1; }
					}
				}

And call it from the client side script

	MyAppRoutes.SomeController.SomeAction() //returns /SomeController/SomeAction

If the action responds to a GET verb and has parameters you can also pass them when using the route

	MyAppRoutes.SomeController.ActionWithParameter(pmt1value, pmt2value) //returns /SomeController/ActionWithParameter?pmt1=pmt1value&pmt2value
	
All other actions with parameters, but without the HttpGet attribute have the parameters ommited in the resulting javascript

Disclaimer
==========

* Tested with ASP.NET MVC 3 only
* Currently it does not add any kind of caching (which is a smart thing to do) besides the one IIS already uses with JS files (which is client side using expiration headers)
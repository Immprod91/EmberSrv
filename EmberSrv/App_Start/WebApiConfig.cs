using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using TestApp.Models;

namespace TestApp
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.Namespace = "Default";
            builder.EntitySet<Department>("Departments").
                    EntityType.Collection.Function("GetLogo").
                    Returns<System.Net.Http.HttpResponseMessage>().
                    Parameter<int>("id");
            builder.EntitySet<Bicycle>("Bicycles").
                    EntityType.Collection.Function("GetByDep").
                    ReturnsFromEntitySet<Bicycle>("Bicycles").
                    Parameter<int>("id");
            builder.EntitySet<Bicycle>("Bicycles").
                    EntityType.Collection.Function("GetLogo").
                    Returns<System.Net.Http.HttpResponseMessage>().
                    Parameter<int>("id");
            builder.EntitySet<History>("Histories");

            config.MapODataServiceRoute(
                routeName: "ODataRoute",
                routePrefix: null,
                model: builder.GetEdmModel());

            var cors = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(cors);
        }
    }
}

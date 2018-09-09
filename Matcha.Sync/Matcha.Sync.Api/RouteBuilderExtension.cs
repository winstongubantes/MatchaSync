using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;

namespace Matcha.Sync.Api
{
    public static class RouteBuilderExtension
    {
        public static void MapODataServiceRouteBase(
            this IRouteBuilder routeBuilder, 
            string routeName, 
            string routePrefix,
            Action<ODataConventionModelBuilder> modelBuilder = null)
        {
            var asm = Assembly.GetCallingAssembly();

            //TODO: add action for OData ModelBuilder
            routeBuilder.MapODataServiceRoute("api", "api", GetEdmModel(asm, modelBuilder));

            routeBuilder.Select()
                .MaxTop(100)
                .OrderBy()
                .Filter()
                .Count();
        }

        private static IEdmModel GetEdmModel(Assembly asm, Action<ODataConventionModelBuilder> modelBuilder = null)
        {
            var builder = new ODataConventionModelBuilder();
            
            var classes = asm.DefinedTypes.Where(t => t.BaseType?.Name == typeof(BaseController<>).Name);

            foreach (var @class in classes)
            {
                var type = @class.BaseType?.GenericTypeArguments[0];
                builder.AddEntitySet(GetControllerNameFromType(@class.Name), builder.AddEntityType(type));
            }

            modelBuilder?.Invoke(builder);

            return builder.GetEdmModel();
        }

        private static string GetControllerNameFromType(string typeName)
        {
            return typeName.Replace("Controller", string.Empty);
        }
    }
}

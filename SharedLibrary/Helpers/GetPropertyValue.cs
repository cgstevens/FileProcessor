using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;

namespace Shared.Helpers
{
    public static class GetProperty
    {
        public static object GetPropertyValue(this object reference, string propertyPath)
        {
            var properties = propertyPath.Split('.');
            object currentObject = reference;

            foreach(var property in properties)
            {
                currentObject = currentObject.GetType().GetProperties()
                    .Single(pi => pi.Name == property)
                    .GetValue(currentObject, null);
            }

            return currentObject;
        }


    }
}

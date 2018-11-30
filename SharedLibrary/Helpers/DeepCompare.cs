namespace Shared.Helpers
{
    public static class CompareHelpers
    {
        public static bool DeepCompare(this object obj, object another)
        {
            if (ReferenceEquals(obj, another)) return true;
            if ((obj == null) || (another == null)) return false;
            //Compare two object's class, return false if they are difference
            if (obj.GetType() != another.GetType()) return false;

            //Get all properties of obj
            //And compare each other
            foreach (var property in obj.GetType().GetProperties())
            {
                var objValue = property.GetValue(obj);
                var anotherValue = property.GetValue(another);
                if ((objValue == null) || (anotherValue == null)) return false;
                if (objValue.ToString() != anotherValue.ToString()) return false;
            }

            return true;
        }

        //public static bool DeepCompare(this object obj, object another)
        //{
        //    if (ReferenceEquals(obj, another)) return true;
        //    if ((obj == null) || (another == null)) return false;
        //    //Compare two object's class, return false if they are difference
        //    if (obj.GetType() != another.GetType()) return false;

        //    var result = true;
        //    //Get all properties of obj
        //    //And compare each other
        //    foreach (var property in obj.GetType().GetProperties())
        //    {
        //        var objValue = property.GetValue(obj);
        //        var anotherValue = property.GetValue(another);
        //        if ((objValue == null) || (anotherValue == null)) return false;
        //        if (objValue.ToString() != anotherValue.ToString()) return false;
        //    }

        //    return result;
        //}

    }
}

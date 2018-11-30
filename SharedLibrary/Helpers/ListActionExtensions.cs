using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;

namespace SharedLibrary.Helpers
{
    public static class ListActionExtensions
    {

        public static IList<Action> AddAction(this IList<Action> list, Action item)
        {
            list.Add(item);
            return list;
        }

        public static void ExecuteAndAggregateExceptions(this IList<Action> list)
        {
            var exceptions = new List<Exception>();

            foreach (var item in list)
            {
                try
                {
                    item();
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }

            if (1 == exceptions.Count)
            {
                Rethrow(exceptions.First());
            }
            else if (1 < exceptions.Count)
            {
                throw new AggregateException(exceptions);
            }
        }

        private static void Rethrow(this Exception e)
        {
            var eInfo = ExceptionDispatchInfo.Capture(e);
            eInfo.Throw();
        }
    }
}

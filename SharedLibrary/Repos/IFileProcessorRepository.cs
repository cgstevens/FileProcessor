using System;
using System.Collections.Generic;
using Shared.Models;

namespace Shared.Repos
{
    public interface IFileProcessorRepository
    {
        IEnumerable<LocationModel> GetLocationsWithFileSettings();
        void LongRunningProcess(string adUserName, int someId, Action<string> callback);
    }
}

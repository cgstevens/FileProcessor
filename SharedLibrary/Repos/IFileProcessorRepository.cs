using System;
using System.Collections.Generic;
using SharedLibrary.Models;

namespace SharedLibrary.Repos
{
    public interface IFileProcessorRepository
    {
        IEnumerable<LocationModel> GetLocationsWithFileSettings();
        void LongRunningProcess(string adUserName, int someId, Action<string> callback);
    }
}

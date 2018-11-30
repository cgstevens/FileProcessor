using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Shared.Models;

namespace Shared.Repos
{
    public class FileProcessorRepository : IFileProcessorRepository
    {

        public IEnumerable<LocationModel> GetLocationsWithFileSettings()
        {
            var locations = new List<LocationModel>();
            
            using (SqlConnection con = new SqlConnection("Server=.;Database=AkkaFileProcessor;Integrated Security=SSPI;"))
            {
                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("exec spLocationsWithFileSettings_Get", con))
                    {
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                locations.Add(ConvertToLocation(reader));
                            }
                        }
                        else
                        {
                            //Console.WriteLine("No rows found.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong. {ex.Message}");
                }
            }

            return locations;
        }
        

        public void LongRunningProcess(string adUserName, int someRandomNumber, Action<string> callback)
        {
            var settings = new List<FileSettingsModel>();
            using (SqlConnection con = new SqlConnection("Server=.;Database=AkkaFileProcessor;Integrated Security=SSPI;"))
            {
                if (callback != null)
                {
                    con.FireInfoMessageEventOnUserErrors = true;
                    con.InfoMessage += delegate (object sender, SqlInfoMessageEventArgs e)
                    {
                        callback.Invoke(e.Message);
                    };
                }

                con.Open();
                try
                {
                    using (SqlCommand command = new SqlCommand("exec spLongRunningProcess_ProcessAllThingsMagically @userName, @rnd", con))
                    {
                        command.Parameters.Add("@userName", SqlDbType.VarChar);
                        command.Parameters["@userName"].Value = adUserName;

                        command.Parameters.Add("@rnd", SqlDbType.Int);
                        command.Parameters["@rnd"].Value = someRandomNumber;

                        command.CommandTimeout = 30;

                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var test = "Nothing to do";
                            }
                        }
                        else
                        {
                            //Console.WriteLine("No rows found.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong. {ex.Message}");
                    throw;
                }
            }
            
        }

        

        #region Converters

        private static LocationModel ConvertToLocation(IDataRecord reader)
        {
            var id = int.Parse(reader.GetValue(reader.GetOrdinal("LocationID")).ToString());
            var name = reader.GetValue(reader.GetOrdinal("Name")).ToString();
            var fileSettings = ConvertToLocationFileSettings(reader);

            return new LocationModel(id, name, fileSettings);
        }

        private static FileSettingsModel ConvertToLocationFileSettings(IDataRecord reader)
        {
            var errorFolder = reader.GetValue(reader.GetOrdinal("ErrorFolder")).ToString(); 
            var inboundFolder = reader.GetValue(reader.GetOrdinal("InboundFolder")).ToString();
            var processedFolder = reader.GetValue(reader.GetOrdinal("ProcessedFolder")).ToString();
            var teamsetting = new FileSettingsModel(errorFolder, inboundFolder, processedFolder);
            return teamsetting;
        }

        #endregion

    }
}
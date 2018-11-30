using System.Data;
using FileHelpers;
using Shared.Enums;

namespace Shared.Models
{
    public class UserRecord
    {

        public string AdUserName { get; }
        public int Row { get; }
        public bool Processed { get; set; }

        public UserRecord()
        {
        }

        public UserRecord(string adUserName, int row)
        {
            AdUserName = adUserName;
            Row = row;
            Processed = false;
        }

        public UserRecord Copy()
        {
            return new UserRecord(AdUserName, Row);
        }
        
    }


}

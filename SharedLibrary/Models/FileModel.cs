using FileHelpers;
using Shared.Enums;

namespace Shared.Models
{
    [DelimitedRecord("|")]
    [IgnoreFirst(1)]
    [IgnoreEmptyLines]
    public class FileModel
    {
        [FieldQuoted('"', QuoteMode.AlwaysQuoted, MultilineMode.NotAllow)]
        public string AdUserName;
        
    }


}

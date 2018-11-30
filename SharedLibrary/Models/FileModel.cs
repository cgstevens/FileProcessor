using FileHelpers;

namespace SharedLibrary.Models
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

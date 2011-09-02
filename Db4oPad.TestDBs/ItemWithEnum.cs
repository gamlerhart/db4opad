using System;

namespace Db4oPad.TestDBs
{
    public class ItemWithEnum
    {
        AEnum enumField = AEnum.ValueTwo;      
    }

    enum AEnum  
    {
        ValueOne,
        ValueTwo    
    }

}
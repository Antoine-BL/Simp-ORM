using SimpORM;

namespace SimpORM_Test.Utils
{
    public class ColumnMappingExample
    {
        [ColumnMapping]
        public string id
        {
            get;
            set;
        }

        [ColumnMapping]
        public string nom
        {
            get;
            set;
        }

        [ColumnMapping("ID_CUST")]
        public string customer
        {
            get;
            set;
        }

        [ColumnMapping("ID_CLIENT")]
        public string client
        {
            get;
            set;
        }
    }
}

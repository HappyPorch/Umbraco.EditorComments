using NPoco;
using Umbraco.Core;

namespace Our.Umbraco.EditorComments.Core.Models
{
    [TableName(TableName)]
    public class User
    {
        public const string TableName = Constants.DatabaseSchema.Tables.User;

        [Column("id")]
        public int Id { get; set; }

        [Column("userName")]
        public string UserName { get; set; }
    }
}

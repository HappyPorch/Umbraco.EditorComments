using NPoco;
using Umbraco.Core;

namespace Our.Umbraco.EditorComments.Core.Models
{
    [TableName(TableName)]
    public class PropertyType
    {
        public const string TableName = Constants.DatabaseSchema.Tables.PropertyType;

        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }
    }
}

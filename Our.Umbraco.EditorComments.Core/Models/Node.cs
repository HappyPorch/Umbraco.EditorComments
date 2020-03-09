using NPoco;
using Umbraco.Core;

namespace Our.Umbraco.EditorComments.Core.Models
{
    [TableName(TableName)]
    public class Node
    {
        public const string TableName = Constants.DatabaseSchema.Tables.Node;

        [Column("id")]
        public int Id { get; set; }

        [Column("text")]
        public string Text { get; set; }
    }
}

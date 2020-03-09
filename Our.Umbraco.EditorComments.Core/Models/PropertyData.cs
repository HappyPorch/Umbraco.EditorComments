using NPoco;
using Umbraco.Core;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Our.Umbraco.EditorComments.Core.Models
{
    [TableName(TableName)]
    public class PropertyData
    {
        public const string TableName = Constants.DatabaseSchema.Tables.PropertyData;

        [Column("id")]
        public int Id { get; set; }

        [Column("propertyTypeId")]
        [ForeignKey(typeof(PropertyType), Column = "id")]
        public int PropertyTypeId { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.OneToOne, ColumnName = "PropertyTypeId")]
        public PropertyType PropertyType { get; set; }
    }
}

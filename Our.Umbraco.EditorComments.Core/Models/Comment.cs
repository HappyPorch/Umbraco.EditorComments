using NPoco;
using System;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Our.Umbraco.EditorComments.Core.Models
{
    [TableName(TableName)]
    [PrimaryKey("Id", AutoIncrement = true)]
    [ExplicitColumns]
    internal class Comment
    {
        public const string TableName = "EditorComments_Comments";

        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_Id")]
        public int Id { get; set; }

        [Column("parentCommentId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_ParentCommentId")]
        public int? ParentCommentId { get; set; }

        [Column("umbracoNodeId")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_UmbracoNodeId")]
        [ForeignKey(typeof(Node), Column = "id")]
        public int UmbracoNodeId { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.OneToOne, ColumnName = "UmbracoNodeId")]
        public Node UmbracoNode { get; set; }

        [Column("umbracoNodePropertyId")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_UmbracoNodePropertyId")]
        [ForeignKey(typeof(PropertyType), Column = "id")]
        public int UmbracoNodePropertyId { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.OneToOne, ColumnName = "UmbracoNodePropertyId")]
        public PropertyData UmbracoNodeProperty { get; set; }

        [Column("userId")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_UserId")]
        [ForeignKey(typeof(User), Column = "id")]
        public int UserId { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.OneToOne, ColumnName = "UserId")]
        public User User { get; set; }

        [Column("message")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [SpecialDbType(SpecialDbTypes.NTEXT)]
        public string Message { get; set; }

        [Column("status")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Constraint(Default = (int)CommentStatus.Pending)]
        public int Status { get; set; }

        [Column("createDate")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Constraint(Default = SystemMethods.CurrentDateTime)]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_CreateDate")]
        public DateTime CreateDate { get; set; }

        [Column("updateDate")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Constraint(Default = SystemMethods.CurrentDateTime)]
        public DateTime UpdateDate { get; set; }
    }
}

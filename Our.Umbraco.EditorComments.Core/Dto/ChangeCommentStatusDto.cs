using Our.Umbraco.EditorComments.Core.Models;

namespace Our.Umbraco.EditorComments.Core.Dto
{
    public class ChangeCommentStatusDto
    {
        public int CommentId { get; set; }

        public CommentStatus Status { get; set; }
    }
}

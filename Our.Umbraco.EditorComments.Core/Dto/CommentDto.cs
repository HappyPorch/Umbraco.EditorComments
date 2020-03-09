using Our.Umbraco.EditorComments.Core.Models;
using System;
using System.Collections.Generic;

namespace Our.Umbraco.EditorComments.Core.Dto
{
    public class CommentDto
    {
        public int Id { get; set; }

        public int? ParentCommentId { get; set; }

        public int UmbracoNodeId { get; set; }

        public string UmbracoNodeName { get; set; }

        public int UmbracoNodePropertyId { get; set; }

        public string UmbracoNodePropertyName { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }

        public string Message { get; set; }

        public CommentStatus Status { get; set; }

        public int? NotifyUser { get; set; }

        public int? NotifyUserGroup { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }

        public IList<CommentDto> Replies { get; set; }

        public string CreateDateFormatted
        {
            get
            {
                return $"{CreateDate.ToString("M")}, {CreateDate.ToString("yyyy")} {CreateDate.ToString("t")}";
            }
        }

        public string StatusColour
        {
            get
            {
                switch (Status)
                {
                    case CommentStatus.Pending:
                        return "warning";

                    case CommentStatus.Completed:
                    default:
                        return "gray";
                }
            }
        }

        public string StatusName
        {
            get
            {
                switch (Status)
                {
                    case CommentStatus.Pending:
                        return "Pending";

                    case CommentStatus.Completed:
                        return "Completed";

                    default:
                        return string.Empty;
                }
            }
        }

        public bool ShowStatus
        {
            get
            {
                return (Status != CommentStatus.NotApplicable);
            }
        }

        public bool IsCompleted
        {
            get
            {
                return (Status == CommentStatus.Completed);
            }
        }

    }
}

using System;
using Frapid.DataAccess;
using Frapid.Mapper.Decorators;

namespace Frapid.WebsiteBuilder.DTO
{
    [TableName("website.contents")]
    [PrimaryKey("content_id", AutoIncrement = true)]
    public sealed class Content : IPoco
    {
        public int ContentId { get; set; }
        public int CategoryId { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public int? AuthorId { get; set; }
        public DateTimeOffset PublishOn { get; set; }
        public string Markdown { get; set; }
        public string Contents { get; set; }
        public string Tags { get; set; }
        public bool IsDraft { get; set; }
        public string SeoDescription { get; set; }
        public bool IsHomepage { get; set; }
        public int? AuditUserId { get; set; }
        public DateTimeOffset? AuditTs { get; set; }
    }
}
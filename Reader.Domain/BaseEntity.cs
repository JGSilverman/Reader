namespace Reader.Domain
{
    public abstract class BaseEntity
    {
        public string CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }

        public string UpdatedById { get; set; }
        public DateTime UpdatedOn { get; set;}
    }
}

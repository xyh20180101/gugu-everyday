using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;

namespace GuguEveryday.Models
{
    public abstract class BaseModel : Entity<long>, IHasCreationTime, IHasModificationTime, IHasDeletionTime, ISoftDelete
    {
        public long CreatedBy { get; set; }

        public DateTime CreationTime { get; set; } = DateTime.UtcNow;

        public long? UpdatedBy { get; set; }

        public DateTime? LastModificationTime { get; set; }

        public bool IsDeleted { get; set; }

        public long? DeletedBy { get; set; }

        public DateTime? DeletionTime { get; set; }
    }
}

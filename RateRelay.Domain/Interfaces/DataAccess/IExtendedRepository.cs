using RateRelay.Domain.Entities;

namespace RateRelay.Domain.Interfaces.DataAccess;

public interface IEntityExtendedRepository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity;
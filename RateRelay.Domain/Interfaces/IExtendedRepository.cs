using RateRelay.Domain.Entities;

namespace RateRelay.Domain.Interfaces;

public interface IEntityExtendedRepository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity;
﻿using SubSonic.Infrastructure;
using SubSonic.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SubSonic.Data.Caching
{
    public class EntityCacheElementCollection<TEntity>
        : EntityCacheElement, IEnumerable<TEntity>
    {
        public EntityCacheElementCollection()
            : base(typeof(TEntity)) 
        {
            Cache = new ObservableCollection<IEntityProxy<TEntity>>();
        }

        public ICollection<IEntityProxy<TEntity>> Entities { get; }

        public new IEnumerator<TEntity> GetEnumerator()
        {
            return ((ICollection<IEntityProxy<TEntity>>)Cache).Select(x => x.Data).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Cache.GetEnumerator();
        }
    }

    public class EntityCacheElement
        : IEnumerable
    {
        protected EntityCacheElement(Type key)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
        }

        public Type Key { get; }

        public ICollection Cache { get; protected set; }

        public IEnumerator GetEnumerator()
        {
            return Cache.GetEnumerator();
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Schema
{
    public interface IDbRelationshipMap
    {
        DbRelationshipType RelationshipType { get; }

        string PropertyName { get; }

        IDbEntityModel ForeignModel { get; }
        IDbEntityModel LookupModel { get; }
        bool IsLookupMapping { get; }
        bool IsReciprocated { get; }

        IEnumerable<string> GetForeignKeys<TEntity>();
        IEnumerable<string> GetForeignKeys(IDbEntityModel entityModel);
    }
}

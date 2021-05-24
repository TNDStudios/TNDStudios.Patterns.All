﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TNDStudios.Repository
{
    public class MemoryRepository<TDomain, TDocument> : RepositoryBase<TDomain, TDocument>
        where TDocument : RepositoryDocument
        where TDomain : RepositoryDomainObject
    {
        private readonly Dictionary<String, TDocument> _values;

        public MemoryRepository(
            Func<TDomain, TDocument> toDocument,
            Func<TDocument, TDomain> toDomain) : base(toDocument, toDomain)
        {
            _values = new Dictionary<String, TDocument>();
        }

        public override bool Delete(String id) 
            => _values.Remove(id);

        public override TDomain Get(String id)
        {
            if (_values.ContainsKey(id))
            {
                return ToDomain(_values[id]);
            }

            return null;
        }

        public override IEnumerable<TDomain> Query(Expression<Func<TDocument, Boolean>> query)
        {
            var filtered = _values.Select(x => x.Value).AsQueryable<TDocument>().Where(query);
            return filtered.Select(x => ToDomain(x)).ToList();
        }

        public override TDomain ToDomain(TDocument document) 
            => _toDomain(document);

        public override TDocument ToDocument(TDomain domain) 
            => _toDocument(domain);

        public override bool Upsert(TDomain item)
        {
            TDocument document = ToDocument(item);
            if (document != null)
            {
                item.Id = document.Id = document.Id ?? Guid.NewGuid().ToString();
                _values[document.Id] = document;
                return true;
            }

            return false;
        }

        public override bool WithData(List<TDomain> data)
        {
            Int32 insertCount = 0;

            data.ForEach(item =>
                insertCount += (Upsert(item) ? 1 : 0));

            return insertCount == data.Count;
        }
    }
}

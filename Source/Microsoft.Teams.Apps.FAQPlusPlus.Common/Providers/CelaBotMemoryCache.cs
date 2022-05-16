using Microsoft.Extensions.Caching.Distributed;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;


namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    public class CelaBotMemoryCache : ICelaBotMemoryCache
    {
        public IDistributedCache _cache;
        public BinaryFormatter binFormatter;

        public CelaBotMemoryCache(IDistributedCache _cache)
        {
            this.binFormatter = new BinaryFormatter();
            this._cache = _cache;
        }

        public T GetCache<T>(string key) where T : class
        {
            try
            {
                byte[] values = _cache.Get(key);

                if (values == null) return null;

                BinaryFormatter binaryFormatter = new BinaryFormatter();
                using (MemoryStream memoryStream = new MemoryStream(values))
                {
                    return binaryFormatter.Deserialize(memoryStream) as T;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SetCache<T>(T values, string key)
        {
            try
            {
                MemoryStream mStream = new MemoryStream();

                binFormatter.Serialize(mStream, values);

                mStream.ToArray();

                DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions()
                {
                    AbsoluteExpiration = DateTime.Now.AddDays(7),
                    SlidingExpiration = TimeSpan.FromSeconds(30)
                };

                _cache.Set(key, mStream.ToArray());

                mStream.Close();



            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void RemoveCache(string key)
        {
            try
            {
                _cache.Remove(key);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        
    }
}

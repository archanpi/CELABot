using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    public interface ICelaBotMemoryCache
    {        
        void SetCache<T>(T values, string key);
        T GetCache<T>(string key) where T : class;
        void RemoveCache(string key);        
    }
}

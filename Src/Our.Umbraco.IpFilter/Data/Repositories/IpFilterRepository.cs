using System.Collections.Generic;
using System.Linq;
using Our.Umbraco.IpFilter.Models;

namespace Our.Umbraco.IpFilter.Data.Repositories
{
    internal class IpFilterRepository : AbstractRepository<IpFilterEntry>
    {
        public IpFilterEntry GetByNodeId(int id)
        {
            return All().SingleOrDefault(x => x.NodeId == id);
        }
    }
}

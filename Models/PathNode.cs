using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tasklist.Models
{
    public class PathNode
    {
        public PathProcess Self { get; set; }
        public List<PathNode> Children { get; set; }

        public PathNode(PathProcess self, List<PathNode> childs)
        {
            Self = self;
            Children = childs;
        }
    }
}

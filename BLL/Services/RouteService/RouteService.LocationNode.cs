namespace BLL.Services;

public partial class RouteService
{
    private class LocationNode
    {
        public LocationNode(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public ICollection<LegEdge> Outgoing { get; set; } = new List<LegEdge>();
        public ICollection<LegEdge> Incoming { get; set; } = new List<LegEdge>();

        public List<Path> FindOutgoingPaths(
            string target,
            Action<Path> relevantPathSegmentFoundCallback,
            LocationNode? from = null,
            Stack<LocationNode>? ignore = null)
        {
            from ??= this;
            ignore ??= new Stack<LocationNode>();
            ignore.Push(this);
            var result = new List<Path>();

            foreach (var edge in Outgoing)
            {
                if (ignore.Contains(edge.To))
                {
                    continue;
                }

                if (edge.To.Name.Contains(target, StringComparison.OrdinalIgnoreCase))
                {
                    var path = new Path(edge.From, edge.To, edge);
                    relevantPathSegmentFoundCallback(path);
                    result.Add(path);
                }

                foreach (var nextPath in edge.To.FindOutgoingPaths(target, relevantPathSegmentFoundCallback, from, ignore))
                {
                    var path = new Path(this, nextPath.To, edge, nextPath);
                    relevantPathSegmentFoundCallback(path);
                    result.Add(path);
                }
            }

            ignore.Pop();

            return result;
        }
    }
}
namespace BLL.Services;

public partial class RouteService
{
    private class RouteSearchState
    {
        public readonly Dictionary<string, LocationNode> AllNodes = new();
        public readonly Dictionary<LocationNode, List<Path>?> StartingNodes = new();
        public readonly Dictionary<Guid, LegEdge> AllEdges = new();

        public readonly RouteSearchDirectionSpecificState Forward = new();
        public readonly RouteSearchDirectionSpecificState Backward = new();

        internal class RouteSearchDirectionSpecificState
        {
            public readonly HashSet<Guid> SeenLegIds = new();
            public readonly HashSet<string> SeenNodeNames = new();
            public readonly Dictionary<string, LocationNode> CurrentNodes = new();
        }
    }
}
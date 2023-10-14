namespace BLL.Services;

public partial class RouteService
{
    private class Path
    {
        public Path(LocationNode from, LocationNode to, LegEdge edge, Path? next = null)
        {
            if (to.Name != edge.To.Name)
            {
                if (next == null)
                {
                    throw new ArgumentException(
                        $"Path leading to '{to.Name}' can't end with edge leading to '{edge.To.Name}'");
                }

                if (next.From.Name != edge.To.Name)
                {
                    throw new ArgumentException(
                        $"Path's edge's target '{edge.To.Name}' doesn't match next path source '{next.From.Name}'");
                }
            }

            From = from;
            To = to;
            Edge = edge;
            Next = next;
        }

        public LocationNode From { get; }
        public LocationNode To { get; }
        public LegEdge Edge { get; set; }
        public Path? Next { get; set; }
    }
}
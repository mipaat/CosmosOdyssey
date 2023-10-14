using Domain.Entities;

namespace BLL.Services;

public partial class RouteService
{
    private class LegEdge
    {
        public LocationNode From { get; set; }
        public LocationNode To { get; set; }
        public Leg Leg { get; set; }

        public LegEdge(Leg leg, LocationNode from, LocationNode to)
        {
            if (leg.StartLocation!.Name != from.Name)
            {
                throw new ArgumentException(
                    $"Start node name '{from.Name}' didn't match leg start '{leg.StartLocation.Name}'", nameof(from));
            }

            if (leg.EndLocation!.Name != to.Name)
            {
                throw new ArgumentException(
                    $"End node name '{to.Name}' didn't match leg start '{leg.EndLocation.Name}'", nameof(to));
            }

            Leg = leg;
            From = from;
            To = to;
            from.Outgoing.Add(this);
            to.Incoming.Add(this);
        }
    }
}
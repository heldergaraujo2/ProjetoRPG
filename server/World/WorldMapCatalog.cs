using System.Text.Json;
using ProjetoRpg.Shared.World;
using ProjetoRpg.Server.Content;

namespace ProjetoRpg.Server.World;

public sealed record WorldSpawn(string Id, WorldPosition Position);

public sealed class WorldMapCatalog
{
    private const double GridCellSize = 2.0;
    private const double SurfaceTolerance = 0.55;
    private const double CollisionRadius = 0.32;
    private const int CollisionSamples = 8;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly Dictionary<string, WorldMapDefinition> _maps;

    private WorldMapCatalog(Dictionary<string, WorldMapDefinition> maps) => _maps = maps;

    public static WorldMapCatalog Load(ContentCatalog content)
    {
        var navigation = new Dictionary<string, NavigationMesh>(StringComparer.Ordinal);
        foreach (var entry in content.Entries.Where(entry => entry.Kind == "world_navigation"))
        {
            var path = Path.Combine(content.RootDirectory, entry.RelativePath);
            var document = JsonSerializer.Deserialize<NavigationDocument>(File.ReadAllBytes(path), JsonOptions)
                ?? throw new InvalidDataException($"World navigation document is empty: {entry.Id}");
            if (!string.Equals(document.Id, entry.Id, StringComparison.Ordinal) || document.Triangles.Count == 0)
                throw new InvalidDataException($"World navigation document is invalid: {entry.Id}");
            navigation.Add(entry.Id, new NavigationMesh(document.Triangles));
        }

        var maps = new Dictionary<string, WorldMapDefinition>(StringComparer.Ordinal);
        foreach (var entry in content.Entries.Where(entry => entry.Kind == "world_map"))
        {
            var path = Path.Combine(content.RootDirectory, entry.RelativePath);
            var document = JsonSerializer.Deserialize<MapDocument>(File.ReadAllBytes(path), JsonOptions)
                ?? throw new InvalidDataException($"World map document is empty: {entry.Id}");
            if (!string.Equals(document.Id, entry.Id, StringComparison.Ordinal) ||
                string.IsNullOrWhiteSpace(document.DisplayName) ||
                !navigation.TryGetValue(document.NavigationId, out var mesh) ||
                document.WalkSpeed is <= 0 or > 10 || document.AgentRadius is <= 0 or > 1 ||
                document.Spawns.Count == 0)
                throw new InvalidDataException($"World map document is invalid or references unavailable navigation: {entry.Id}");

            var spawnIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var spawn in document.Spawns)
            {
                if (!spawnIds.Add(spawn.Id) || !mesh.IsWalkable(spawn.Position, document.AgentRadius))
                    throw new InvalidDataException($"World map spawn is duplicated or outside navigable Coast: {entry.Id}/{spawn.Id}");
            }

            maps.Add(entry.Id, new WorldMapDefinition(
                entry.Id, document.Enabled, document.DisplayName, document.NavigationId,
                document.Spawns, document.WalkSpeed, Math.Max(document.AgentRadius, CollisionRadius), mesh));
        }
        return new WorldMapCatalog(maps);
    }

    public bool TryGetAvailableMap(string mapId, out WorldMapDefinition? map) =>
        _maps.TryGetValue(mapId, out map) && map is { Enabled: true };

    public sealed class WorldMapDefinition
    {
        private readonly NavigationMesh _navigation;

        internal WorldMapDefinition(string id, bool enabled, string displayName, string navigationId,
            IReadOnlyList<WorldSpawn> spawns, double walkSpeed, double agentRadius, NavigationMesh navigation)
        {
            Id = id;
            Enabled = enabled;
            DisplayName = displayName;
            NavigationId = navigationId;
            Spawns = spawns;
            WalkSpeed = (float)walkSpeed;
            AgentRadius = agentRadius;
            _navigation = navigation;
        }

        public string Id { get; }
        public bool Enabled { get; }
        public string DisplayName { get; }
        public string NavigationId { get; }
        public IReadOnlyList<WorldSpawn> Spawns { get; }
        public float WalkSpeed { get; }
        public double AgentRadius { get; }
        public WorldSpawn GetSpawn(string spawnId) => Spawns.First(spawn => string.Equals(spawn.Id, spawnId, StringComparison.Ordinal));

        public bool TryMove(WorldPosition current, float directionX, float directionZ, int elapsedTicks,
            out WorldPosition position, out WorldPosition velocity)
        {
            var magnitude = Math.Sqrt(directionX * directionX + directionZ * directionZ);
            if (magnitude > 1)
            {
                directionX = (float)(directionX / magnitude);
                directionZ = (float)(directionZ / magnitude);
            }

            var deltaSeconds = Math.Clamp(elapsedTicks, 1, 2) / 20.0;
            var distance = WalkSpeed * deltaSeconds;
            var steps = Math.Max(1, (int)Math.Ceiling(distance / 0.05));
            var stepX = directionX * distance / steps;
            var stepZ = directionZ * distance / steps;
            var accepted = current;
            for (var index = 0; index < steps; index++)
            {
                var candidateX = accepted.X + stepX;
                var candidateZ = accepted.Z + stepZ;
                if (!_navigation.TryGetWalkableHeight(candidateX, candidateZ, accepted.Y, SurfaceTolerance, out var surfaceY) ||
                    !_navigation.IsWalkable(new WorldPosition(candidateX, surfaceY, candidateZ), AgentRadius))
                    break;
                accepted = new WorldPosition(candidateX, surfaceY, candidateZ);
            }

            position = accepted;
            var moved = Math.Abs(position.X - current.X) + Math.Abs(position.Z - current.Z) > 0.00001;
            velocity = moved
                ? new WorldPosition((position.X - current.X) / deltaSeconds, 0, (position.Z - current.Z) / deltaSeconds)
                : new WorldPosition(0, 0, 0);
            return moved;
        }
    }

    private sealed record MapDocument(
        string Id,
        bool Enabled,
        string DisplayName,
        string NavigationId,
        List<WorldSpawn> Spawns,
        double WalkSpeed,
        double AgentRadius);

    private sealed record NavigationDocument(string Id, List<NavigationTriangle> Triangles);
    internal sealed record NavigationTriangle(double[] A, double[] B, double[] C);

    internal sealed class NavigationMesh
    {
        private readonly Dictionary<(int X, int Z), List<Triangle>> _grid = new();

        public NavigationMesh(IReadOnlyList<NavigationTriangle> input)
        {
            foreach (var triangle in input)
            {
                if (!IsPoint(triangle.A) || !IsPoint(triangle.B) || !IsPoint(triangle.C))
                    throw new InvalidDataException("Navigation triangles must contain finite three-dimensional points.");
                var converted = new Triangle(triangle.A, triangle.B, triangle.C);
                for (var x = Cell(converted.MinX); x <= Cell(converted.MaxX); x++)
                for (var z = Cell(converted.MinZ); z <= Cell(converted.MaxZ); z++)
                {
                    if (!_grid.TryGetValue((x, z), out var bucket))
                    {
                        bucket = [];
                        _grid.Add((x, z), bucket);
                    }
                    bucket.Add(converted);
                }
            }
        }

        public bool IsWalkable(WorldPosition position, double radius)
        {
            if (!TryGetWalkableHeight(position.X, position.Z, position.Y, SurfaceTolerance, out var centerY))
                return false;
            for (var index = 0; index < CollisionSamples; index++)
            {
                var angle = index * Math.PI * 2 / CollisionSamples;
                var x = position.X + Math.Cos(angle) * radius;
                var z = position.Z + Math.Sin(angle) * radius;
                if (!TryGetWalkableHeight(x, z, centerY, 0.4, out var edgeY) || Math.Abs(edgeY - centerY) > 0.4)
                    return false;
            }
            return true;
        }

        public bool TryGetWalkableHeight(double x, double z, double referenceY, double maxHeightDifference, out double height)
        {
            height = 0;
            if (!_grid.TryGetValue((Cell(x), Cell(z)), out var candidates)) return false;
            var found = false;
            var bestDifference = double.MaxValue;
            foreach (var triangle in candidates)
            {
                if (!triangle.TryGetHeight(x, z, out var candidateHeight)) continue;
                var difference = Math.Abs(candidateHeight - referenceY);
                if (difference > maxHeightDifference || difference >= bestDifference) continue;
                height = candidateHeight;
                bestDifference = difference;
                found = true;
            }
            return found;
        }

        private static bool IsPoint(double[] point) => point.Length == 3 && point.All(double.IsFinite);
        private static int Cell(double coordinate) => (int)Math.Floor(coordinate / GridCellSize);

        private sealed class Triangle
        {
            private readonly double[] _a;
            private readonly double[] _b;
            private readonly double[] _c;
            private readonly double _denominator;

            public Triangle(double[] a, double[] b, double[] c)
            {
                _a = a;
                _b = b;
                _c = c;
                MinX = Math.Min(a[0], Math.Min(b[0], c[0]));
                MaxX = Math.Max(a[0], Math.Max(b[0], c[0]));
                MinZ = Math.Min(a[2], Math.Min(b[2], c[2]));
                MaxZ = Math.Max(a[2], Math.Max(b[2], c[2]));
                _denominator = (b[2] - c[2]) * (a[0] - c[0]) + (c[0] - b[0]) * (a[2] - c[2]);
            }

            public double MinX { get; }
            public double MaxX { get; }
            public double MinZ { get; }
            public double MaxZ { get; }

            public bool TryGetHeight(double x, double z, out double height)
            {
                height = 0;
                if (Math.Abs(_denominator) < 1e-10) return false;
                var u = (( _b[2] - _c[2]) * (x - _c[0]) + (_c[0] - _b[0]) * (z - _c[2])) / _denominator;
                var v = ((_c[2] - _a[2]) * (x - _c[0]) + (_a[0] - _c[0]) * (z - _c[2])) / _denominator;
                var w = 1 - u - v;
                if (u < -0.001 || v < -0.001 || w < -0.001) return false;
                height = u * _a[1] + v * _b[1] + w * _c[1];
                return double.IsFinite(height);
            }
        }
    }
}

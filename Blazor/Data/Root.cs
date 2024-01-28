namespace Blazor.Data;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Node
{
    public int id { get; set; }
    public List<string> labels { get; set; }
    public Properties properties { get; set; }
}

public class Path
{
    public Node start { get; set; }
    public Node end { get; set; }
    public List<Node> nodes { get; set; }
    public List<Relationship> relationships { get; set; }
}

public class Properties
{
    public object cityName { get; set; }
    public int cityId { get; set; }
    public int lineId { get; set; }
    public int mileage { get; set; }
}

public class Relationship
{
    public int id { get; set; }
    public string type { get; set; }
    public int startNodeId { get; set; }
    public int endNodeId { get; set; }
    public Properties properties { get; set; }
}

public class Root
{
    public Path path { get; set; }
    public List<Node> nodes { get; set; }
    public List<Relationship> relationships { get; set; }
}
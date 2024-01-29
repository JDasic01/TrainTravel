namespace Blazor.Data;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Node
{
    public int Id { get; set; }
    public List<string> Labels { get; set; }
    public Properties Properties { get; set; }
}

public class Properties
{
    public string city_name { get; set; }
    public int city_id { get; set; }
    public int line_id { get; set; }
    public int mileage { get; set; }
}

public class Relationship
{
    public int Id { get; set; }
    public string Type { get; set; }
    public int StartNodeId { get; set; }
    public int EndNodeId { get; set; }
    public Properties Properties { get; set; }
}

public class Root
{
    public Node Start { get; set; }
    public Node End { get; set; }
    public List<Node> Nodes { get; set; }
    public List<Relationship> Relationships { get; set; }
}
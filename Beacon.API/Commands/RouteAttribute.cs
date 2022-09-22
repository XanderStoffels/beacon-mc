namespace Beacon.API.Commands;

[AttributeUsage(AttributeTargets.Class)]
public class RouteAttribute : Attribute
{
    public string Template { get; set; }

    public RouteAttribute(string template)
    {
        Template = template;
    }
}

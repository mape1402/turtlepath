namespace TurtlePath.Automations.AspNetCore;

/// <summary>
/// Configures an ASP.NET Core endpoint generated from an automation descriptor.
/// </summary>
public sealed record AutomationEndpointOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AutomationEndpointOptions"/> class.
    /// </summary>
    /// <param name="route">The endpoint route template.</param>
    /// <param name="method">The HTTP method. When omitted, the method is inferred from the operation kind.</param>
    /// <param name="name">The optional endpoint name.</param>
    /// <param name="bindingFactory">The optional factory used to project endpoint values into request values.</param>
    /// <param name="requestBinder">The optional binder used to complete the created request.</param>
    public AutomationEndpointOptions(
        string route,
        string method = null,
        string name = null,
        Func<AutomationEndpointBindingContext, object> bindingFactory = null,
        Action<object, AutomationEndpointBindingContext> requestBinder = null)
    {
        if (string.IsNullOrWhiteSpace(route))
            throw new ArgumentException("Route cannot be null or empty.", nameof(route));

        Route = route;
        Method = method;
        Name = name;
        BindingFactory = bindingFactory;
        RequestBinder = requestBinder;
    }

    /// <summary>
    /// Gets the endpoint route template.
    /// </summary>
    public string Route { get; }

    /// <summary>
    /// Gets the HTTP method.
    /// </summary>
    public string Method { get; }

    /// <summary>
    /// Gets the optional endpoint name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the optional factory used to project endpoint values into request values.
    /// </summary>
    public Func<AutomationEndpointBindingContext, object> BindingFactory { get; }

    /// <summary>
    /// Gets the optional binder used to complete the created request.
    /// </summary>
    public Action<object, AutomationEndpointBindingContext> RequestBinder { get; }
}

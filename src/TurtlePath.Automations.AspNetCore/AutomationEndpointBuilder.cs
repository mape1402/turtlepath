namespace TurtlePath.Automations.AspNetCore;

/// <summary>
/// Configures ASP.NET Core endpoint metadata for a TurtlePath automation request.
/// </summary>
/// <typeparam name="TRequest">The request type dispatched by the automation endpoint.</typeparam>
public sealed class AutomationEndpointBuilder<TRequest>
{
    private readonly string route;
    private string method;
    private string name;
    private Func<AutomationEndpointBindingContext, object> bindingFactory;
    private Action<object, AutomationEndpointBindingContext> requestBinder;

    internal AutomationEndpointBuilder(string route)
    {
        this.route = route;
    }

    /// <summary>
    /// Sets the HTTP method. When omitted, TurtlePath infers the method from the automation operation kind.
    /// </summary>
    public AutomationEndpointBuilder<TRequest> Method(string method)
    {
        this.method = method;
        return this;
    }

    /// <summary>
    /// Sets the endpoint name.
    /// </summary>
    public AutomationEndpointBuilder<TRequest> Name(string name)
    {
        this.name = name;
        return this;
    }

    /// <summary>
    /// Projects endpoint values into constructor or property values used to create the request.
    /// </summary>
    public AutomationEndpointBuilder<TRequest> Bind(Func<AutomationEndpointBindingContext, object> bindingFactory)
    {
        ArgumentNullException.ThrowIfNull(bindingFactory);

        this.bindingFactory = bindingFactory;
        return this;
    }

    /// <summary>
    /// Completes a request after TurtlePath creates it from the request body or default query conventions.
    /// </summary>
    public AutomationEndpointBuilder<TRequest> Bind(Action<TRequest, AutomationEndpointBindingContext> requestBinder)
    {
        ArgumentNullException.ThrowIfNull(requestBinder);

        this.requestBinder = (request, context) => requestBinder((TRequest)request, context);
        return this;
    }

    internal AutomationEndpointOptions Build()
        => new(route, method, name, bindingFactory, requestBinder);
}

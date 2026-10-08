namespace TurtlePath.Automations.AspNetCore;

using Microsoft.AspNetCore.Authorization;

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
    private readonly List<object> metadata = [];

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

    /// <summary>
    /// Adds an attribute to the generated endpoint.
    /// </summary>
    public AutomationEndpointBuilder<TRequest> UseAttribute<TAttribute>()
        where TAttribute : Attribute, new()
        => UseAttribute(new TAttribute());

    /// <summary>
    /// Adds an attribute to the generated endpoint.
    /// </summary>
    public AutomationEndpointBuilder<TRequest> UseAttribute<TAttribute>(Func<TAttribute> attributeFactory)
        where TAttribute : Attribute
    {
        ArgumentNullException.ThrowIfNull(attributeFactory);

        return UseAttribute(attributeFactory());
    }

    /// <summary>
    /// Adds an attribute to the generated endpoint.
    /// </summary>
    public AutomationEndpointBuilder<TRequest> UseAttribute(Attribute attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        metadata.Add(attribute);
        return this;
    }

    /// <summary>
    /// Requires authorization for the generated endpoint.
    /// </summary>
    public AutomationEndpointBuilder<TRequest> Authorize(params string[] policies)
    {
        if (policies is null || policies.Length == 0)
        {
            metadata.Add(new AuthorizeAttribute());
            return this;
        }

        foreach (var policy in policies.Where(item => !string.IsNullOrWhiteSpace(item)))
            metadata.Add(new AuthorizeAttribute(policy));

        return this;
    }

    /// <summary>
    /// Allows anonymous access for the generated endpoint.
    /// </summary>
    public AutomationEndpointBuilder<TRequest> AllowAnonymous()
    {
        metadata.Add(new AllowAnonymousAttribute());
        return this;
    }

    internal AutomationEndpointOptions Build()
        => new(route, method, name, bindingFactory, requestBinder, metadata);
}

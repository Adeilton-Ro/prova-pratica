using FluentResults;
using FluentValidation;
using Mediator;

namespace Application.Behaviours;

public class RequestValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : ResultBase<TResponse>, new()
{
    private readonly IEnumerable<IValidator<TRequest>> validators;

    public RequestValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        this.validators = validators;
    }

    public async ValueTask<TResponse> Handle(TRequest message, CancellationToken cancellationToken, MessageHandlerDelegate<TRequest, TResponse> next)
    {
        if (!validators.Any())
            return await next(message, cancellationToken);

        var validationContext = new ValidationContext<TRequest>(message);

        var validatinResults = await Task.WhenAll(validators.Select(v => v.ValidateAsync(validationContext, cancellationToken)));

        var fieldReasonDictionary = validatinResults
                                    .SelectMany(result => result.Errors)
                                    .GroupBy(error => error.PropertyName)
                                    .ToDictionary(
                                        errorGroup => errorGroup.Key,
                                        errorGroup => errorGroup.Select(e => e.ErrorMessage).ToArray()
                                    );

        if (fieldReasonDictionary.Count != 0)
            return new TResponse().WithError(new RequestValidationError { FieldReasonDictionary = fieldReasonDictionary });

        return await next(message, cancellationToken);
    }
}


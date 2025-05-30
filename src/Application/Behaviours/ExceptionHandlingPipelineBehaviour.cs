using FluentResults;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Behaviours;

public class ExceptionHandlingPipelineBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : ResultBase<TResponse>, new()
{
    private readonly ILogger<ExceptionHandlingPipelineBehaviour<TRequest, TResponse>> logger;

    public ExceptionHandlingPipelineBehaviour(ILogger<ExceptionHandlingPipelineBehaviour<TRequest, TResponse>> logger)
    {
        this.logger = logger;
    }

    public async ValueTask<TResponse> Handle(TRequest message, CancellationToken cancellationToken, MessageHandlerDelegate<TRequest, TResponse> next)
    {
        try
        {
            return await next(message, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Untreated exception");
            return new TResponse().WithError("Erro desconhecido");
        }
    }
}
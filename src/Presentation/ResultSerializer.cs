using Application;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace Presentation;

public static class ResultSerializer
{
    public static IResult Serialize(this Result result)
    {
        if (result.IsFailed)
            return HandlerError(result.Errors[0]);

        return Results.NoContent();
    }

    public static IResult Serialize<T>(this Result<T> result)
    {
        if (result.IsFailed)
            return HandlerError(result.Errors[0]);

        return Results.Ok(result.Value);
    }

    private static IResult HandlerError(IError error)
    {
        if (error is ResourceNotFoundError)
        {
            return TypedResults.NotFound(
                new ProblemDetails
                {
                    Detail = error.Message,
                    Status = StatusCodes.Status404NotFound,
                    Title = "Resource not found",
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4"
                }
            );
        }

        if (error is BusinessLogicError)
        {
            return TypedResults.Json(
                new ProblemDetails
                {
                    Detail = error.Message,
                    Status = StatusCodes.Status403Forbidden,
                    Title = "The action was forbidden",
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3"
                }
            );
        }

        if (error is RequestValidationError requestValidationError)
            return Results.ValidationProblem(
                errors: requestValidationError.FieldReasonDictionary, 
                detail: requestValidationError.Message, 
                statusCode: StatusCodes.Status400BadRequest,
                title: "The request was invalid format",
                type: "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"
            );

        return TypedResults.InternalServerError("");
    }
}

using Application.Behaviours;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Mediator;

namespace Application.Test.Behaviours;

public class FakeResult : ResultBase<FakeResult> { }

public class FakeRequest : IRequest<FakeResult>
{
    public string? Name { get; set; }
}

public class TestingRequestValidationBehaviour
{

    [Fact]
    public async Task Handle_ShouldCallNext_WhenNoValidators()
    {
        var cancellationToken = CancellationToken.None;
        var request = new FakeRequest { Name = "Valid" };
        var next = Substitute.For<MessageHandlerDelegate<FakeRequest, FakeResult>>();
        var expected = new FakeResult();

        next(request, cancellationToken).Returns(expected);

        var behaviour = new RequestValidationBehaviour<FakeRequest, FakeResult>([]);

        var result = await behaviour.Handle(request, cancellationToken, next);

        Assert.Equal(expected, result);
        await next.Received(1)(request, cancellationToken);
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenValidationSucceeds()
    {
        var cancellationToken = CancellationToken.None;
        var request = new FakeRequest { Name = "Valid" };
        var next = Substitute.For<MessageHandlerDelegate<FakeRequest, FakeResult>>();
        var expected = new FakeResult();

        var validator = Substitute.For<IValidator<FakeRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<FakeRequest>>(), cancellationToken)
                 .Returns(new ValidationResult());

        next(request, cancellationToken).Returns(expected);

        var behaviour = new RequestValidationBehaviour<FakeRequest, FakeResult>(
            [validator]
        );

        var result = await behaviour.Handle(request, cancellationToken, next);

        Assert.Equal(expected, result);
        await next.Received(1)(request, cancellationToken);
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationError_WhenValidationFails()
    {
        var cancellationToken = CancellationToken.None;
        const string ErrorMessage = "Name is required";

        var request = new FakeRequest { Name = null };
        var next = Substitute.For<MessageHandlerDelegate<FakeRequest, FakeResult>>();

        var validator = Substitute.For<IValidator<FakeRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<FakeRequest>>(), cancellationToken)
                 .Returns(new ValidationResult(
                 [
                     new ValidationFailure(nameof(FakeRequest.Name), ErrorMessage)
                 ]));

        var behaviour = new RequestValidationBehaviour<FakeRequest, FakeResult>(
            [validator]
        );

        var result = await behaviour.Handle(request, cancellationToken, next);

        Assert.True(result.IsFailed);

        var error = result.Errors[0];
        var validationError = Assert.IsType<RequestValidationError>(error);
        Assert.Single(validationError.FieldReasonDictionary);
        Assert.Equal(ErrorMessage, validationError.FieldReasonDictionary[nameof(FakeRequest.Name)][0]);

        await next.DidNotReceive()(request, cancellationToken);
    }
}

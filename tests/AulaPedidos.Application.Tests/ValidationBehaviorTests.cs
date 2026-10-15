using AulaPedidos.Application.Products;
using AulaPedidos.Application.Validation;
using FluentValidation;

namespace AulaPedidos.Application.Tests;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task ValidCommand_ValidatesBeforeCallingHandlerOnceWithSameToken()
    {
        using var cancellation = new CancellationTokenSource();
        var command = new CreateProduct(0m, "Valid", null);
        var validated = false;
        var validator = new InlineValidator<CreateProduct>();
        validator.RuleFor(message => message.Description).MustAsync((_, token) =>
        {
            Assert.Equal(cancellation.Token, token);
            validated = true;
            return Task.FromResult(true);
        });
        var behavior = new ValidationBehavior<CreateProduct, int>([validator]);
        var calls = 0;

        var response = await behavior.Handle(command, (message, token) =>
        {
            Assert.True(validated);
            Assert.Same(command, message);
            Assert.Equal(cancellation.Token, token);
            calls++;
            return ValueTask.FromResult(42);
        }, cancellation.Token);

        Assert.Equal(42, response);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task InvalidCommand_CollectsEveryFailureSequentiallyWithoutCallingHandler()
    {
        var sequence = new List<int>();
        var first = new InlineValidator<CreateProduct>();
        first.RuleFor(message => message.Description).MustAsync(async (_, _) =>
        {
            sequence.Add(1);
            await Task.Yield();
            sequence.Add(2);
            return false;
        }).WithMessage("First description error");
        var second = new InlineValidator<CreateProduct>();
        second.RuleFor(message => message.Description).Must(_ =>
        {
            Assert.Equal(new[] { 1, 2 }, sequence);
            sequence.Add(3);
            return false;
        }).WithMessage("Second description error");
        second.RuleFor(message => message.Price).GreaterThanOrEqualTo(0m);
        var behavior = new ValidationBehavior<CreateProduct, int>([first, second]);
        var calls = 0;

        var exception = await Assert.ThrowsAsync<ValidationException>(async () =>
            await behavior.Handle(new CreateProduct(-1m, "Invalid", null), (_, _) =>
            {
                calls++;
                return ValueTask.FromResult(1);
            }, default));

        Assert.Equal(0, calls);
        Assert.Equal(new[] { 1, 2, 3 }, sequence);
        Assert.Equal(3, exception.Errors.Count());
        Assert.Equal(new[] { "First description error", "Second description error" },
            exception.Errors.Where(failure => failure.PropertyName == "Description").Select(failure => failure.ErrorMessage));
        Assert.Single(exception.Errors, failure => failure.PropertyName == "Price");
    }

    [Fact]
    public async Task QueryWithoutValidators_ContinuesOnce()
    {
        using var cancellation = new CancellationTokenSource();
        var query = new GetProducts();
        var behavior = new ValidationBehavior<GetProducts, int>([]);
        var calls = 0;

        var result = await behavior.Handle(query, (message, token) =>
        {
            Assert.Same(query, message);
            Assert.Equal(cancellation.Token, token);
            calls++;
            return ValueTask.FromResult(7);
        }, cancellation.Token);

        Assert.Equal(7, result);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task InvalidQuery_DoesNotCallHandler()
    {
        var validator = new InlineValidator<GetProducts>();
        validator.RuleFor(query => query).Must(_ => false).WithMessage("Query rejected");
        var behavior = new ValidationBehavior<GetProducts, int>([validator]);
        var calls = 0;

        await Assert.ThrowsAsync<ValidationException>(async () => await behavior.Handle(new GetProducts(), (_, _) =>
        {
            calls++;
            return ValueTask.FromResult(0);
        }, default));

        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AlreadyCanceled_DoesNotValidateOrCallHandler(bool hasValidator)
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var validator = new InlineValidator<GetProducts>();
        validator.RuleFor(query => query).Must(_ => throw new InvalidOperationException("Validator must not run"));
        var behavior = new ValidationBehavior<GetProducts, int>(hasValidator ? [validator] : []);

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await behavior.Handle(new GetProducts(), (_, _) => throw new InvalidOperationException("Handler must not run"), cancellation.Token));

        Assert.Equal(cancellation.Token, exception.CancellationToken);
    }

    [Fact]
    public async Task CancellationDuringValidation_IsNotValidationFailureAndSkipsHandler()
    {
        using var cancellation = new CancellationTokenSource();
        var validator = new InlineValidator<CreateProduct>();
        validator.RuleFor(message => message.Description).MustAsync(async (_, token) =>
        {
            Assert.Equal(cancellation.Token, token);
            cancellation.Cancel();
            await Task.Delay(1, token);
            return true;
        });
        var behavior = new ValidationBehavior<CreateProduct, int>([validator]);

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await behavior.Handle(new CreateProduct(1m, "Valid", null), (_, _) => throw new InvalidOperationException("Handler must not run"), cancellation.Token));

        Assert.Equal(cancellation.Token, exception.CancellationToken);
    }
}

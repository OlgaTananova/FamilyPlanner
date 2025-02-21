
namespace ShoppingListService.Services;

public interface IRequestContextService
{
    string UserId { get; }
    string FamilyName { get; }
    string OperationId { get; }
    string RequestId { get; }
    string TraceId { get; }
    string RequestMethod { get; }
    string RequestPath { get; }
}

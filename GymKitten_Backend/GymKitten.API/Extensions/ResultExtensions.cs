using GymKitten.API.Infrastructure;
using GymKitten.Domain.Common;

namespace GymKitten.API.Extensions;

public static class ResultExtensions
{
    public static TOut Match<TOut>(this Result result, Func<TOut> onSuccess, Func<Result, TOut> onFailure) => result.IsSuccess ? onSuccess() : onFailure(result);
    public static TOut Match<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> onSuccess, Func<Result<TIn>, TOut> onFailure) => result.IsSuccess ? onSuccess(result.Value) : onFailure(result);
    public static IResult MatchOk(this Result result) => result.IsSuccess ? Microsoft.AspNetCore.Http.Results.Ok(ApiResult<object>.Success(null!)) : CustomResults.Problem(result);
    public static IResult MatchOk<T>(this Result<T> result) => result.IsSuccess ? Microsoft.AspNetCore.Http.Results.Ok(ApiResult<T>.Success(result.Value)) : CustomResults.Problem(result);
    public static IResult MatchCreated<T>(this Result<T> result, Func<T, string> urlFunc) => result.IsSuccess ? Microsoft.AspNetCore.Http.Results.Created(urlFunc(result.Value), ApiResult<T>.Success(result.Value)) : CustomResults.Problem(result);
}

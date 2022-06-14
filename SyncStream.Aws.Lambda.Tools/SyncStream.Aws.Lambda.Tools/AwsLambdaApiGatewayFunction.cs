using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

// Define our namespace
namespace SyncStream.Aws.Lambda.Tools;

/// <summary>
/// This class maintains the structure and bootstrapping of our AWS API Gateway Lambda Functions
/// </summary>
public abstract class AwsLambdaApiGatewayFunction<TInput, TOutput> where TInput : class, new() where TOutput : class, new()
{
    /// <summary>
    /// This method is responsible for executing the function
    /// </summary>
    /// <param name="context">The API Gateway Request/Response context</param>
    /// <param name="lambdaContext">The AWS Lambda execution context</param>
    /// <returns>An awaitable task containing the response payload</returns>
    protected abstract Task<APIGatewayProxyResponse> ExecuteAsync(AwsLambdaApiGatewayContext<TInput, TOutput> context,
        ILambdaContext lambdaContext);

    /// <summary>
    /// This method receives a request from API Gateway and send an appropriate response
    /// </summary>
    /// <param name="request">The input model from ApiGateway</param>
    /// <param name="lambdaContext"></param>
    /// <returns></returns>
    public Task<APIGatewayProxyResponse> FunctionHandlerAsync(APIGatewayProxyRequest request,
        ILambdaContext lambdaContext) =>
        ExecuteAsync(new(request), lambdaContext);
}

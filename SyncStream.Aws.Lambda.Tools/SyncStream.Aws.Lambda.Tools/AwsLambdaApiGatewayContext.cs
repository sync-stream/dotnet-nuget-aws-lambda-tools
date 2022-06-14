using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using SyncStream.Serializer;

// Define our namespace
namespace SyncStream.Aws.Lambda.Tools;

/// <summary>
/// This class maintains the structure our static AWS Lambda Context tools
/// </summary>
public static class AwsLambdaApiGatewayContext
{
    /// <summary>
    /// This method provides a factory for receiving requests directly
    /// </summary>
    /// <param name="request"></param>
    /// <typeparam name="TInput"></typeparam>
    /// <returns></returns>
    public static TInput ReceiveRequest<TInput>(APIGatewayProxyRequest request) where TInput : class, new() =>
        JsonSerializer.Deserialize<TInput>(request.Body);
    
    /// <summary>
    /// This method provides a factory for directly sending a response directly
    /// </summary>
    /// <param name="payload">The payload to send as the response body</param>
    /// <param name="statusCode">Optional HTTP Status Code to send in the response</param>
    /// <param name="headers">Optional headers to send in the response</param>
    /// <typeparam name="TOutput">Object type of <paramref name="payload" /></typeparam>
    /// <returns></returns>
    public static APIGatewayProxyResponse SendResponse<TOutput>(TOutput payload,
        HttpStatusCode statusCode = HttpStatusCode.OK, IDictionary<string, string>? headers = null)
        where TOutput : class, new() =>
        new APIGatewayProxyResponse
        {
            // Set the body into the response
            Body = JsonSerializer.Serialize(payload),
            // Set the headers into the response
            Headers = headers ?? new Dictionary<string, string>(),
            // Set the status code into the response
            StatusCode = (int) statusCode
        };
}

/// <summary>
/// This class maintains the structure of our AWS Lambda context
/// </summary>
/// <typeparam name="TInput">The expected type of the deserialized request body</typeparam>
/// <typeparam name="TOutput">The target type of the serialized response body</typeparam>
public class AwsLambdaApiGatewayContext<TInput, TOutput> where TInput: class, new() where TOutput: class, new()
{
    /// <summary>
    /// This method provides a factory from the Lambda ingress via API Gateway
    /// </summary>
    /// <param name="request">The input event from API Gateway</param>
    /// <returns></returns>
    public static AwsLambdaApiGatewayContext<TInput, TOutput> FromApiGateway(APIGatewayProxyRequest request) =>
        new(request);

    /// <summary>
    /// This property contains the body of the request
    /// </summary>
    public readonly TInput Body;

    /// <summary>
    /// This property contains the request object sent to the Lambda function
    /// </summary>
    public readonly APIGatewayProxyRequest Request;

    /// <summary>
    /// This property contains the response object to send back to the API gateway
    /// </summary>
    public readonly APIGatewayProxyResponse Response = new()
    {
        // Default the HTTP status code
        StatusCode = (int) HttpStatusCode.OK
    };

    /// <summary>
    /// This method instantiates our tool with an incoming request
    /// </summary>
    /// <param name="request">The input event from API Gateway</param>
    public AwsLambdaApiGatewayContext(APIGatewayProxyRequest request)
    {
        // Set the input event from API gateway into the instance
        Request = request;
        // Deserialize the request body into the instance
        Body = JsonSerializer.Deserialize<TInput>(request.Body);
    }

    /// <summary>
    /// This method adds a header to the response
    /// </summary>
    /// <param name="name">The name of the header to send</param>
    /// <param name="value">The value of the header to send</param>
    /// <returns></returns>
    public AwsLambdaApiGatewayContext<TInput, TOutput> SendHeader(string name, string value)
    {
        // Add the header to the response
        Response.Headers.Add(name, value);
        
        // We're done, return the instance
        return this;
    }

    /// <summary>
    /// This method serializes a complex value into JSON and adds it to the response as a header
    /// </summary>
    /// <param name="name">The name of the header to send</param>
    /// <param name="value">The value to serialize and send as the header value</param>
    /// <typeparam name="TValue">The type in which the value is</typeparam>
    /// <returns></returns>
    public AwsLambdaApiGatewayContext<TInput, TOutput> SendHeader<TValue>(string name, TValue value)
        where TValue : class, new() => SendHeader(name, JsonSerializer.Serialize(value));

    /// <summary>
    /// This method resets the response HTTP status code
    /// </summary>
    /// <param name="httpCode">The HTTP status code to send to the client</param>
    /// <returns>The current instance</returns>
    public AwsLambdaApiGatewayContext<TInput, TOutput> SendHttpCode(HttpStatusCode httpCode)
    {
        // Reset the HTTP status code into the response
        Response.StatusCode = (int) httpCode;
        
        // We're done, return the instance
        return this;
    }

    /// <summary>
    /// This method sends a response to API Gateway
    /// </summary>
    /// <param name="payload">The payload for the response body</param>
    /// <param name="statusCode">Optional HTTP Status Code override</param>
    /// <returns></returns>
    public APIGatewayProxyResponse SendResponse(TOutput payload, HttpStatusCode? statusCode = null)
    {
        // Check for a status code and override it
        if (statusCode.HasValue) SendHttpCode(statusCode.Value);
        
        // Set the response body into the instance
        Response.Body = JsonSerializer.Serialize(payload);

        // We're done, send the response
        return Response;
    }
}

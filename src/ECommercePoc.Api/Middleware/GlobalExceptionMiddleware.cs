using System.Net;
using System.Text.Json;
using ECommercePoc.Application.Exceptions;
using ECommercePoc.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePoc.Api.Middleware;

/// <summary>
/// Global error handling middleware.
/// Maps DomainException → 400, NotFoundException → 404, ValidationException → 400,
/// unhandled → 500. Returns RFC 7807 ProblemDetails.
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            DomainException => (HttpStatusCode.BadRequest, "Business Rule Violation", exception.Message),
            NotFoundException => (HttpStatusCode.NotFound, "Resource Not Found", exception.Message),
            ValidationException => (HttpStatusCode.BadRequest, "Validation Failed", exception.Message),
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error",
                "An unexpected error occurred. Please try again later.")
        };

        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

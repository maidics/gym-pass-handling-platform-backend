namespace FitPass.Application.Common.Exceptions;

public class BadRequestException(string message) : Exception(message) { }
namespace MegaProject.Domain.Models.Exceptions;

public class NotFoundException(string message) : Exception(message);
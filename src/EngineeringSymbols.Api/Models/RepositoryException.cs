namespace EngineeringSymbols.Api.Models;

public class RepositoryException : Exception
{
    public Exception? Exception { get; init; }
    
    public RepositoryOperationError RepositoryOperationError { get; }
    public RepositoryException(string message) : base(message)
    {

    }
    public RepositoryException(RepositoryOperationError repositoryOperationError)
    {
        RepositoryOperationError = repositoryOperationError;
    }
    
    public RepositoryException(RepositoryOperationError repositoryOperationError, Exception e)
    {
        RepositoryOperationError = repositoryOperationError;
        Exception = e;
    }
    
    public RepositoryException(string message, RepositoryOperationError repositoryOperationError) : base(message)
    {
        RepositoryOperationError = repositoryOperationError;
    }
}
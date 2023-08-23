namespace EngineeringSymbols.Api.Models;

public class RepositoryException : Exception
{
    public RepositoryOperationError RepositoryOperationError { get; }
    
    public RepositoryException(string message) : base(message)
    {

    }
    public RepositoryException(RepositoryOperationError repositoryOperationError)
    {
        RepositoryOperationError = repositoryOperationError;
    }
}
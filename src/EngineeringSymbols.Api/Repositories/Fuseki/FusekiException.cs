namespace EngineeringSymbols.Api.Repositories.Fuseki;

public class FusekiException : Exception
{
    public FusekiException()
    {
        
    }
    
    public FusekiException(string message) : base(message)
    {

    }

    public FusekiException(string message, Exception inner) : base(message, inner)
    {

    }
}
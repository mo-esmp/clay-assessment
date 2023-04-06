using SmartLock.Implementation.Data;

namespace SmartLock.WebApi.Swagger;

public class UserLoginExampleValue : IExamplesProvider<TokenRequest>
{
    public TokenRequest GetExamples()
    {
        return new TokenRequest(DefaultData.EmployeeEmail, DefaultData.UserPassword);
    }
}
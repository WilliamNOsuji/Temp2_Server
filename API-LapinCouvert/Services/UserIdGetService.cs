using MVC_LapinCouvert.Data;
using System.Security.Claims;

namespace API_LapinCouvert.Services
{
    public class UserIdGetService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserIdGetService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public UserIdGetService()
        {
        }

        public virtual string getUserId()
        {
            return _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}

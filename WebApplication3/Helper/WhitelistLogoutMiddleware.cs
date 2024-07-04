namespace WebApplication3.Helper
{
    public class WhitelistLogoutMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly WhitelistStorage _whitelistStorage;

        public WhitelistLogoutMiddleware(RequestDelegate next, WhitelistStorage whitelistStorage)
        {
            _next = next;
            _whitelistStorage = whitelistStorage;
        }


        public async Task Invoke(HttpContext context)
        {
            var jwt = context.Request.Cookies["jwtToken"];

            if (!string.IsNullOrEmpty(jwt) && _whitelistStorage.Whitelist.ContainsKey(jwt) && _whitelistStorage.Whitelist[jwt] > DateTime.UtcNow)
            {
                // JWT có trong whitelist và còn hạn
                // Tiến hành logout và loại bỏ JWT khỏi whitelist
                _whitelistStorage.Whitelist.Remove(jwt);

                // Thực hiện các bước logout cần thiết

                // Gọi middleware tiếp theo trong pipeline
                await _next(context);
            }
            else
            {
                // JWT không có trong whitelist hoặc đã hết hạn
                // Bỏ qua và cho phép request tiếp tục xử lý bình thường
                await _next(context);
            }
        }
    }

}


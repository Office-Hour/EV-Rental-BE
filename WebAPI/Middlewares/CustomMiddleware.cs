namespace WebAPI.Middlewares
{
    public class CustomMiddleware(RequestDelegate next)
    {
        /// <summary>
        /// The next
        /// </summary>
        private readonly RequestDelegate next = next;

        /// <summary>
        /// Invokes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public async Task Invoke(HttpContext context)
        {
            await next(context);
        }
    }
}

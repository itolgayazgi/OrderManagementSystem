using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.Entity
{
    public class IdempotencyKey : BaseEntity
    {
        public string Key { get; private set; } = default!;
        public string Endpoint { get; private set; } = default!;
        public string? RequestHash { get; private set; }
        public int? ResponseStatusCode { get; private set; }
        public string? ResponseBody { get; private set; }

        private IdempotencyKey() { }

        public IdempotencyKey(string key, string endpoint, string? requestHash)
        {
            Key = key;
            Endpoint = endpoint;
            RequestHash = requestHash;
        }

        public void SetResponse(int statusCode, string body)
        {
            ResponseStatusCode = statusCode;
            ResponseBody = body;
            Update();
        }
    }
}

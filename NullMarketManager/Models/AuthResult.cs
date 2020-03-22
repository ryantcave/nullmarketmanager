using System;
using System.Collections.Generic;
using System.Text;
using static NullMarketManager.Access.AccessManager;

namespace NullMarketManager.Models
{
    class AuthResult
    {
        public AuthResult(AccessState state, int expiry)
        {
            this.state = state;
            this.expiry = expiry;
        }

        public AccessState state = AccessState.READ_ACCESS_FROM_DISK;
        public int expiry = 0;

    }
}

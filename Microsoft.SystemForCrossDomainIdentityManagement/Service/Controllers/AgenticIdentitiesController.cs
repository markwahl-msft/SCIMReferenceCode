// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM
{
    using System;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route(ServiceConstants.RouteAgenticIdentities)]
    [Authorize]
    [ApiController]
    public sealed class AgenticIdentitiesController : ControllerTemplate<AgenticIdentity>
    {
        public AgenticIdentitiesController(IProvider provider, IMonitor monitor)
            : base(provider, monitor)
        {
        }

        protected override IProviderAdapter<AgenticIdentity> AdaptProvider(IProvider provider)
        {
            if (null == provider)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            IProviderAdapter<AgenticIdentity> result =
                new AgenticIdentityProviderAdapter(provider);
            return result;
        }
    }
}
